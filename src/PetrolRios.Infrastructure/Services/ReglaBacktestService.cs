using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.DTOs.ReglasPersonalizadas;
using PetrolRios.Application.Interfaces;
using PetrolRios.Application.ReglasPersonalizadas;
using PetrolRios.Application.ReglasPersonalizadas.Expresiones;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Jobs;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

/// <summary>
/// Backtest (vista previa) de una regla personalizada borrador. Corre la regla contra los datos
/// reales de staging de los últimos N días <b>en solo lectura</b> (no marca nada como procesado ni
/// guarda alertas), reutilizando el mismo <see cref="CustomRuleDetector"/> del ciclo real para que
/// el resultado sea fiel a lo que pasaría en producción.
/// </summary>
public sealed class ReglaBacktestService : IReglaBacktestService
{
    private readonly PetrolRiosDbContext _db;
    private readonly IAnomalyDetector _detectorPersonalizado;

    public ReglaBacktestService(PetrolRiosDbContext db, IEnumerable<IAnomalyDetector> detectores)
    {
        _db = db;
        _detectorPersonalizado = detectores.First(d => d.Type == TipoDetector.Personalizada);
    }

    public async Task<BacktestReglaResponse> EjecutarAsync(BacktestReglaRequest request, CancellationToken ct = default)
    {
        var r = request.Regla;

        // 1. Validar el borrador con las mismas reglas que al guardar: si es inválido, no se corre.
        var errores = ValidarBorrador(r);
        if (errores.Count > 0)
            return new BacktestReglaResponse { Valida = false, Errores = errores };

        var dias = Math.Clamp(request.Dias <= 0 ? 7 : request.Dias, 1, 90);
        var desde = DateTime.UtcNow.AddDays(-dias);

        // 2. Regla transitoria (NO se guarda en la base).
        var regla = ReglaPersonalizada.Create(
            string.IsNullOrWhiteSpace(r.Nombre) ? "(prueba)" : r.Nombre.Trim(),
            r.Descripcion?.Trim() ?? "",
            r.FuenteDatos,
            CatalogoReglasPersonalizadas.SerializarCondiciones(r.CombinadorCondiciones, r.Condiciones),
            r.Agregacion is null ? null : JsonSerializer.Serialize(r.Agregacion),
            r.RiesgoBase,
            r.Ambito);
        regla.Activa = true;
        regla.ExpresionAvanzada = string.IsNullOrWhiteSpace(r.ExpresionAvanzada) ? null : r.ExpresionAvanzada.Trim();

        // 3. Staging de la ventana (solo lectura). Se deduplican reenvíos como en el ciclo real.
        var staging = await _db.TransaccionesStaging
            .AsNoTracking()
            .Where(s => s.FechaOriginal >= desde)
            .ToListAsync(ct);
        staging = staging
            .GroupBy(s => new { s.EstacionId, s.TipoTransaccion, s.DataJson })
            .Select(g => g.First())
            .ToList();

        var estaciones = (await _db.Estaciones.AsNoTracking().Where(e => e.Activa).ToListAsync(ct))
            .ToDictionary(e => e.Id);

        var alertasPreviasPorEstacion = await CargarAlertasPreviasAsync(ct);

        // 4. Evaluar por estación con el detector real y acumular las coincidencias.
        var coincidencias = new List<(DetectedAnomaly Anomalia, string Estacion)>();
        var registrosEvaluados = 0;

        foreach (var grupo in staging.GroupBy(s => s.EstacionId))
        {
            if (!estaciones.TryGetValue(grupo.Key, out var estacion)) continue;

            var filas = grupo.ToList();
            registrosEvaluados += ContarRegistrosFuente(filas, r.FuenteDatos);

            var contexto = ConstruirContexto(
                estacion, filas, regla,
                alertasPreviasPorEstacion.GetValueOrDefault(estacion.Id) ?? new Dictionary<string, int>());

            var resultado = await _detectorPersonalizado.DetectAsync(contexto, ct);
            coincidencias.AddRange(resultado.Select(a => (a, estacion.Nombre)));
        }

        // 5. Agregar resultados.
        return new BacktestReglaResponse
        {
            Valida = true,
            VentanaDias = dias,
            RegistrosEvaluados = registrosEvaluados,
            TotalCoincidencias = coincidencias.Count,
            Bajo = coincidencias.Count(c => c.Anomalia.NivelRiesgo == NivelRiesgo.Bajo),
            Medio = coincidencias.Count(c => c.Anomalia.NivelRiesgo == NivelRiesgo.Medio),
            Alto = coincidencias.Count(c => c.Anomalia.NivelRiesgo == NivelRiesgo.Alto),
            Critico = coincidencias.Count(c => c.Anomalia.NivelRiesgo == NivelRiesgo.Critico),
            Muestra = coincidencias
                .OrderByDescending(c => c.Anomalia.Score)
                .Take(12)
                .Select(c => new BacktestCoincidencia(
                    c.Anomalia.NivelRiesgo.ToString(),
                    c.Anomalia.Score,
                    c.Anomalia.Descripcion,
                    c.Anomalia.EmpleadoCodigo,
                    c.Estacion))
                .ToList()
        };
    }

    /// <summary>Validación idéntica a la del guardado (catálogo + expresión avanzada).</summary>
    private static List<string> ValidarBorrador(GuardarReglaPersonalizadaRequest r)
    {
        var errores = new List<string>();
        var fuente = r.FuenteDatos ?? "";

        if (string.IsNullOrWhiteSpace(fuente))
            errores.Add("Debe indicar una fuente de datos.");

        if (r.RiesgoBase is < 1 or > 100)
            errores.Add("El riesgo base debe estar entre 1 y 100.");

        if (!string.IsNullOrWhiteSpace(r.ExpresionAvanzada))
        {
            errores.AddRange(EvaluadorExpresion.Validar(r.ExpresionAvanzada, fuente));
            if (r.Agregacion is not null)
                errores.AddRange(CatalogoReglasPersonalizadas.ValidarDefinicion(fuente, [], r.Agregacion, r.RiesgoBase));
        }
        else
        {
            errores.AddRange(CatalogoReglasPersonalizadas.ValidarDefinicion(fuente, r.Condiciones, r.Agregacion, r.RiesgoBase));
        }

        return errores;
    }

    /// <summary>Reincidencias por (estación, empleado) de los últimos 30 días, como en el ciclo real.</summary>
    private async Task<Dictionary<int, IReadOnlyDictionary<string, int>>> CargarAlertasPreviasAsync(CancellationToken ct)
    {
        var hace30Dias = DateTime.UtcNow.AddDays(-30);
        var previas = await _db.Alertas
            .AsNoTracking()
            .Where(a => a.FechaDeteccion >= hace30Dias && a.EmpleadoCodigo != null)
            .Select(a => new { a.EstacionId, a.EmpleadoCodigo })
            .ToListAsync(ct);

        return previas
            .GroupBy(a => a.EstacionId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<string, int>)g
                    .GroupBy(x => x.EmpleadoCodigo!)
                    .ToDictionary(x => x.Key, x => x.Count()));
    }

    /// <summary>
    /// Construye un <see cref="DetectionContext"/> de solo lectura para una estación a partir de su
    /// staging de la ventana. Replica el armado del ciclo real (sin marcar nada como procesado).
    /// </summary>
    private static DetectionContext ConstruirContexto(
        Estacion estacion,
        IReadOnlyList<TransaccionStaging> staging,
        ReglaPersonalizada regla,
        IReadOnlyDictionary<string, int> alertasPrevias)
    {
        var tiposConocidos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Factura", "DetalleFactura", "CierreTurno", "DepositoTurno",
            "Anulacion", "Credito", "TarjetaTurno"
        };

        var fuentesGenericas = staging
            .Where(s => !tiposConocidos.Contains(s.TipoTransaccion))
            .GroupBy(s => s.TipoTransaccion)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<IDictionary<string, object>>)g
                    .Select(s => StagingJson.DeserializarDiccionario(s.DataJson))
                    .Where(d => d is not null)
                    .Cast<IDictionary<string, object>>()
                    .ToList());

        return new DetectionContext
        {
            FuentesGenericas = fuentesGenericas,
            EstacionId = estacion.Id,
            EstacionNombre = estacion.Nombre,
            FromWatermark = DateTime.UtcNow.AddDays(-90),
            ToWatermark = DateTime.UtcNow,
            Facturas = StagingJson.DeserializarPorTipo<Application.DTOs.Firebird.FacturaDto>(staging, "Factura"),
            Detalles = StagingJson.DeserializarPorTipo<Application.DTOs.Firebird.DetalleFacturaDto>(staging, "DetalleFactura"),
            CierresTurno = StagingJson.DeserializarPorTipo<Application.DTOs.Firebird.CierreTurnoDto>(staging, "CierreTurno"),
            DepositosTurno = StagingJson.DeserializarPorTipo<Application.DTOs.Firebird.DepositoTurnoDto>(staging, "DepositoTurno"),
            Anulaciones = StagingJson.DeserializarPorTipo<Application.DTOs.Firebird.AnulacionDto>(staging, "Anulacion"),
            Creditos = StagingJson.DeserializarPorTipo<Application.DTOs.Firebird.CreditoDto>(staging, "Credito"),
            TarjetasTurno = StagingJson.DeserializarPorTipo<Application.DTOs.Firebird.TarjetaTurnoDto>(staging, "TarjetaTurno"),
            Reglas = [],
            ReglasPersonalizadas = [regla],
            AlertasPreviasPorEmpleado = alertasPrevias,
            HoraApertura = estacion.HoraApertura,
            HoraCierre = estacion.HoraCierre
        };
    }

    /// <summary>Cuenta cuántas filas del staging pertenecen a la fuente de la regla (para "evaluados").</summary>
    private static int ContarRegistrosFuente(IEnumerable<TransaccionStaging> staging, string fuente) =>
        staging.Count(s => string.Equals(s.TipoTransaccion, fuente, StringComparison.OrdinalIgnoreCase));
}
