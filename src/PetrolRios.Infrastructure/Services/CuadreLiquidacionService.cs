using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Application.RealTime;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Jobs;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

/// <summary>
/// Cuadre de liquidación de turno (mejora de auditoría #3). Al cerrar un turno, todas sus facturas
/// deben quedar en la liquidación (LIQU). Una factura "colgada" — su turno cerró pero no aparece en
/// LIQU — es una anomalía de cuadre (riesgo de descuadre de caja / facturación sin cobrar).
///
/// Por qué NO es un detector de ventana: la liquidación de un turno llega DESPUÉS de su cierre (en otro
/// lote), así que un detector que solo ve la ventana del ciclo daría falsos positivos. Aquí se consulta
/// el staging DIRECTO (acumulado), se exige que el turno esté CERRADO y con cierre anterior a
/// (ahora − umbralHoras) — para dar tiempo a que llegue su liquidación — y se crea la alerta de forma
/// IDEMPOTENTE (referencia única por turno: no se re-alerta el mismo turno). El enlace
/// <c>LIQU.NUM_TURN ↔ DCTO.NUM_TURN</c> está verificado contra la base real (149/149 liquidaciones con
/// NUM_TURN; de 39 041 facturas FV solo el turno de prueba quedó sin liquidar).
/// </summary>
public sealed class CuadreLiquidacionService
{
    private readonly PetrolRiosDbContext _db;
    private readonly IAlertaBroadcaster _broadcaster;
    private readonly ILogger<CuadreLiquidacionService> _logger;

    public CuadreLiquidacionService(
        PetrolRiosDbContext db, IAlertaBroadcaster broadcaster, ILogger<CuadreLiquidacionService> logger)
    {
        _db = db;
        _broadcaster = broadcaster;
        _logger = logger;
    }

    /// <summary>Un turno cerrado cuyas facturas no quedaron liquidadas.</summary>
    public sealed record TurnoSinLiquidar(
        int NumeroTurno, DateTime FechaCierre, string Vendedor, IReadOnlyList<FacturaDto> Facturas)
    {
        public double MontoTotal => Facturas.Sum(f => f.TotalNeto);
    }

    /// <summary>
    /// Lógica pura y testeable (sin BD): de los cierres, liquidaciones y facturas dados, devuelve los
    /// turnos CERRADOS (EST_TURN='1') con cierre anterior a (ahora − umbralHoras) que tienen facturas
    /// FV pero NO aparecen en ninguna liquidación.
    /// </summary>
    public static IReadOnlyList<TurnoSinLiquidar> CalcularTurnosSinLiquidar(
        IEnumerable<CierreTurnoDto> cierres,
        IEnumerable<LiquidacionDto> liquidaciones,
        IEnumerable<FacturaDto> facturas,
        double umbralHoras,
        DateTime ahoraUtc)
    {
        var liquidados = liquidaciones
            .Where(l => l.NumeroTurno > 0)
            .Select(l => l.NumeroTurno)
            .ToHashSet();

        var facturasPorTurno = facturas
            .Where(f => f.NumeroTurno > 0
                     && f.TipoDocumento.Trim().Equals("FV", StringComparison.OrdinalIgnoreCase))
            .GroupBy(f => f.NumeroTurno)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<FacturaDto>)g.ToList());

        var limite = ahoraUtc.AddHours(-Math.Max(0, umbralHoras));
        var resultado = new List<TurnoSinLiquidar>();

        foreach (var grupo in cierres
            .Where(c => c.NumeroTurno > 0 && c.EstadoTurno.Trim() == "1" && c.FechaFin <= limite)
            .GroupBy(c => c.NumeroTurno))
        {
            var turno = grupo.Key;
            if (liquidados.Contains(turno)) continue;                         // ya liquidado: cuadra
            if (!facturasPorTurno.TryGetValue(turno, out var fs) || fs.Count == 0) continue; // sin facturas
            var cierre = grupo.OrderByDescending(c => c.FechaFin).First();
            resultado.Add(new TurnoSinLiquidar(turno, cierre.FechaFin, cierre.CodigoVendedor.Trim(), fs));
        }
        return resultado;
    }

    private static string Referencia(int estacionId, int turno) => $"SINLIQU-{estacionId}-{turno}";

    /// <summary>
    /// Evalúa el cuadre de una estación: lee el staging (cierres, liquidaciones, facturas) de los
    /// últimos <paramref name="diasLookback"/> días, calcula los turnos colgados y crea una alerta por
    /// turno NUEVO (idempotente: salta los que ya tienen alerta con su referencia). Devuelve cuántas creó.
    /// </summary>
    public async Task<int> EvaluarEstacionAsync(
        Estacion estacion, double umbralHoras, AmbitoAlerta ambito, int diasLookback, CancellationToken ct)
    {
        var desde = DateTime.UtcNow.AddDays(-Math.Max(1, diasLookback));
        var staging = await _db.TransaccionesStaging
            .AsNoTracking()
            .Where(s => s.EstacionId == estacion.Id && s.FechaOriginal >= desde)
            .ToListAsync(ct);
        if (staging.Count == 0) return 0;

        // Deduplicar lotes reenviados (store-and-forward) para no doblar montos/conteos.
        var unicas = staging
            .GroupBy(s => new { s.TipoTransaccion, s.DataJson })
            .Select(g => g.First())
            .ToList();

        var cierres = StagingJson.DeserializarPorTipo<CierreTurnoDto>(unicas, "CierreTurno");
        var liquidaciones = StagingJson.DeserializarPorTipo<LiquidacionDto>(unicas, "Liquidacion");
        var facturas = StagingJson.DeserializarPorTipo<FacturaDto>(unicas, "Factura");

        var colgados = CalcularTurnosSinLiquidar(cierres, liquidaciones, facturas, umbralHoras, DateTime.UtcNow);
        if (colgados.Count == 0) return 0;

        // Idempotencia: no re-alertar un turno ya alertado.
        var refs = colgados.Select(t => Referencia(estacion.Id, t.NumeroTurno)).ToList();
        var yaAlertados = (await _db.Alertas
            .AsNoTracking()
            .Where(a => a.TransaccionReferencia != null && refs.Contains(a.TransaccionReferencia))
            .Select(a => a.TransaccionReferencia!)
            .ToListAsync(ct)).ToHashSet();

        var nuevas = new List<Alerta>();
        foreach (var t in colgados)
        {
            var referencia = Referencia(estacion.Id, t.NumeroTurno);
            if (yaAlertados.Contains(referencia)) continue;

            var (score, nivel) = Puntuar(t.MontoTotal);
            var metadata = new Dictionary<string, object>
            {
                ["NumeroTurno"] = t.NumeroTurno,
                ["FechaCierre"] = t.FechaCierre.ToString("yyyy-MM-dd HH:mm"),
                ["CantidadFacturas"] = t.Facturas.Count,
                ["MontoTotal"] = Math.Round(t.MontoTotal, 2),
                ["NumerosFactura"] = t.Facturas.Select(f => f.NumeroDocumento.Trim()).Where(n => n.Length > 0).ToList(),
                ["Rucs"] = t.Facturas.Select(f => f.RucCliente.Trim()).Where(r => r.Length > 0).Distinct().ToList()
            };

            var alerta = Alerta.Create(
                TipoDetector.InvoiceAnomaly,
                nivel,
                $"Turno {t.NumeroTurno} cerrado el {t.FechaCierre:dd/MM/yyyy HH:mm} SIN liquidación: " +
                $"{t.Facturas.Count} factura(s) por ${t.MontoTotal:F2} quedaron fuera del cuadre.",
                score,
                estacion.Id,
                string.IsNullOrWhiteSpace(t.Vendedor) ? null : t.Vendedor,
                referencia,
                System.Text.Json.JsonSerializer.Serialize(metadata),
                ejecucionJobId: null,
                ambito);
            nuevas.Add(alerta);
        }

        if (nuevas.Count == 0) return 0;

        await _db.Alertas.AddRangeAsync(nuevas, ct);
        await _db.SaveChangesAsync(ct);

        foreach (var alerta in nuevas)
            await NotificarAsync(alerta, estacion.Id);

        _logger.LogInformation(
            "Cuadre de liquidación: {N} turno(s) sin liquidar en estación {Est}", nuevas.Count, estacion.Nombre);
        return nuevas.Count;
    }

    /// <summary>Score 0–100 según el monto colgado (base 55 = Alto; sube a Crítico con montos grandes).</summary>
    private static (double Score, NivelRiesgo Nivel) Puntuar(double monto)
    {
        var score = Math.Round(Math.Clamp(55 + Math.Min(40, monto / 100.0), 0, 100), 1);
        var nivel = score > 75 ? NivelRiesgo.Critico
            : score > 50 ? NivelRiesgo.Alto
            : score > 25 ? NivelRiesgo.Medio
            : NivelRiesgo.Bajo;
        return (score, nivel);
    }

    private Task NotificarAsync(Alerta alerta, int estacionId)
    {
        var payload = new AlertaNotificacionPayload(
            NotificationId: Guid.NewGuid().ToString("N"),
            Id: alerta.Id,
            TipoDetector: alerta.TipoDetector.ToString(),
            NivelRiesgo: alerta.NivelRiesgo.ToString(),
            Ambito: alerta.Ambito.ToString(),
            Descripcion: alerta.Descripcion,
            Score: alerta.Score,
            FechaDeteccion: alerta.FechaDeteccion,
            EstacionId: estacionId);
        return _broadcaster.PublicarAsync(
            new AlertaPush("NuevaAlerta", ["auditores", "supervisores", "administradores"], payload));
    }
}
