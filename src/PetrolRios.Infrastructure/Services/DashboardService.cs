using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.DTOs.Dashboard;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private static readonly EstadoAlerta[] EstadosResueltos =
        [EstadoAlerta.Confirmada, EstadoAlerta.FalsoPositivo, EstadoAlerta.Cerrada];

    private readonly PetrolRiosDbContext _dbContext;
    private readonly IEmpleadoDirectorio _empleados;

    public DashboardService(PetrolRiosDbContext dbContext, IEmpleadoDirectorio empleados)
    {
        _dbContext = dbContext;
        _empleados = empleados;
    }

    /// <summary>
    /// Alertas del carril de auditoría (anomalías a revisar). El dashboard del central es la vista
    /// de los auditores: los problemas operativos de estación NO se cuentan aquí (tienen su propia
    /// pestaña "Problemas de estación"), para no inflar las métricas de auditoría.
    /// </summary>
    private IQueryable<Alerta> AlertasAuditoria =>
        _dbContext.Set<Alerta>().Where(a => a.Ambito == AmbitoAlerta.Auditoria);

    public async Task<KpiResponse> GetKpisAsync(CancellationToken ct = default)
    {
        var alertas = AlertasAuditoria.AsQueryable();

        // Estaciones en línea de verdad: heartbeat del agente dentro de la ventana
        var limiteConexion = DateTime.UtcNow - MonitoreoService.VentanaConexion;
        var conectadas = await _dbContext.Estaciones
            .CountAsync(e => e.Activa && e.UltimoHeartbeat >= limiteConexion, ct);

        return new KpiResponse
        {
            TotalAlertas = await alertas.CountAsync(ct),
            AlertasNuevas = await alertas.CountAsync(a => a.Estado == EstadoAlerta.Nueva, ct),
            AlertasCriticas = await alertas.CountAsync(a => a.NivelRiesgo == NivelRiesgo.Critico, ct),
            AlertasEnRevision = await alertas.CountAsync(a => a.Estado == EstadoAlerta.EnRevision, ct),
            AlertasConfirmadas = await alertas.CountAsync(a => a.Estado == EstadoAlerta.Confirmada, ct),
            AlertasFalsoPositivo = await alertas.CountAsync(a => a.Estado == EstadoAlerta.FalsoPositivo, ct),
            ScorePromedio = await alertas.AnyAsync(ct) ? await alertas.AverageAsync(a => a.Score, ct) : 0,
            EstacionesConectadas = conectadas,
            EstacionesTotales = await _dbContext.Estaciones.CountAsync(e => e.Activa, ct)
        };
    }

    public async Task<IReadOnlyList<AlertasPorTipoResponse>> GetAlertasPorTipoAsync(CancellationToken ct = default)
    {
        return await AlertasAuditoria
            .GroupBy(a => a.TipoDetector)
            .Select(g => new AlertasPorTipoResponse(g.Key.ToString(), g.Count()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AlertasPorEstacionResponse>> GetAlertasPorEstacionAsync(CancellationToken ct = default)
    {
        return await AlertasAuditoria
            .Include(a => a.Estacion)
            .GroupBy(a => new { a.EstacionId, a.Estacion.Nombre })
            .Select(g => new AlertasPorEstacionResponse(g.Key.EstacionId, g.Key.Nombre, g.Count()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AlertasPorNivelResponse>> GetAlertasPorNivelAsync(CancellationToken ct = default)
    {
        return await AlertasAuditoria
            .GroupBy(a => a.NivelRiesgo)
            .Select(g => new AlertasPorNivelResponse(g.Key.ToString(), g.Count()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TendenciaDiaResponse>> GetTendenciaAsync(int dias, CancellationToken ct = default)
    {
        dias = Math.Clamp(dias, 1, 90);
        var desde = DateTime.UtcNow.Date.AddDays(-(dias - 1));

        var agrupadas = await AlertasAuditoria
            .Where(a => a.FechaDeteccion >= desde)
            .GroupBy(a => a.FechaDeteccion.Date)
            .Select(g => new
            {
                Fecha = g.Key,
                Total = g.Count(),
                Criticas = g.Count(a => a.NivelRiesgo == NivelRiesgo.Critico),
                Altas = g.Count(a => a.NivelRiesgo == NivelRiesgo.Alto)
            })
            .ToListAsync(ct);

        var porFecha = agrupadas.ToDictionary(x => x.Fecha);

        // Serie completa con días sin alertas en cero (para gráficos continuos)
        return Enumerable.Range(0, dias)
            .Select(i => desde.AddDays(i))
            .Select(fecha => porFecha.TryGetValue(fecha, out var dato)
                ? new TendenciaDiaResponse(fecha, dato.Total, dato.Criticas, dato.Altas)
                : new TendenciaDiaResponse(fecha, 0, 0, 0))
            .ToList();
    }

    public async Task<IReadOnlyList<TopEmpleadoResponse>> GetTopEmpleadosAsync(int top, CancellationToken ct = default)
    {
        top = Math.Clamp(top, 1, 50);

        var resultado = await AlertasAuditoria
            .Where(a => a.EmpleadoCodigo != null && a.EmpleadoCodigo != "")
            .GroupBy(a => new { a.EmpleadoCodigo, a.EstacionId, a.Estacion.Nombre })
            .Select(g => new
            {
                g.Key.EmpleadoCodigo,
                g.Key.EstacionId,
                EstacionNombre = g.Key.Nombre,
                Cantidad = g.Count(),
                ScorePromedio = g.Average(a => a.Score),
                Criticas = g.Count(a => a.NivelRiesgo == NivelRiesgo.Critico)
            })
            .OrderByDescending(t => t.Cantidad)
            .ThenByDescending(t => t.ScorePromedio)
            .Take(top)
            .ToListAsync(ct);

        // Resolver el nombre del empleado para mostrarlo junto al código en el ranking.
        var empleados = await _empleados.CargarAsync(
            resultado.Select(r => (r.EstacionId, (string?)r.EmpleadoCodigo)), ct);

        return resultado
            .Select(r => new TopEmpleadoResponse(
                r.EmpleadoCodigo!,
                r.Cantidad,
                Math.Round(r.ScorePromedio, 1),
                r.Criticas,
                r.EstacionNombre,
                empleados.Nombre(r.EstacionId, r.EmpleadoCodigo)))
            .ToList();
    }

    public async Task<MetricasResolucionResponse> GetMetricasResolucionAsync(CancellationToken ct = default)
    {
        var resueltas = await AlertasAuditoria
            .Where(a => EstadosResueltos.Contains(a.Estado))
            .Select(a => new { a.Estado, a.FechaDeteccion, a.FechaResolucion })
            .ToListAsync(ct);

        var totalResueltas = resueltas.Count;
        var falsosPositivos = resueltas.Count(a => a.Estado == EstadoAlerta.FalsoPositivo);
        var confirmadas = resueltas.Count(a => a.Estado == EstadoAlerta.Confirmada);

        var horasResolucion = resueltas
            .Where(a => a.FechaResolucion.HasValue)
            .Select(a => (a.FechaResolucion!.Value - a.FechaDeteccion).TotalHours)
            .Where(h => h >= 0)
            .ToList();

        var hace24Horas = DateTime.UtcNow.AddHours(-24);

        return new MetricasResolucionResponse
        {
            TiempoMedioResolucionHoras = horasResolucion.Count > 0
                ? Math.Round(horasResolucion.Average(), 1)
                : 0,
            TasaFalsosPositivos = totalResueltas > 0
                ? Math.Round((double)falsosPositivos / totalResueltas * 100, 1)
                : 0,
            TasaAlertasValidas = totalResueltas > 0
                ? Math.Round((double)confirmadas / totalResueltas * 100, 1)
                : 0,
            AlertasUltimas24Horas = await AlertasAuditoria
                .CountAsync(a => a.FechaDeteccion >= hace24Horas, ct),
            TotalResueltas = totalResueltas,
            TotalPendientes = await AlertasAuditoria
                .CountAsync(a => a.Estado == EstadoAlerta.Nueva || a.Estado == EstadoAlerta.EnRevision, ct)
        };
    }
}
