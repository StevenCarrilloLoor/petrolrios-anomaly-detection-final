using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.DTOs.Dashboard;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private static readonly EstadoAlerta[] EstadosResueltos =
        [EstadoAlerta.Confirmada, EstadoAlerta.FalsoPositivo, EstadoAlerta.Cerrada];

    private readonly PetrolRiosDbContext _dbContext;

    public DashboardService(PetrolRiosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<KpiResponse> GetKpisAsync(CancellationToken ct = default)
    {
        var alertas = _dbContext.Alertas.AsQueryable();

        // Estaciones conectadas de verdad: con ingesta del agente en los últimos 10 minutos
        var limiteConexion = DateTime.UtcNow - MonitoreoService.VentanaConexion;
        var conectadas = await _dbContext.TransaccionesStaging
            .Where(s => s.CreatedAt >= limiteConexion)
            .Select(s => s.EstacionId)
            .Distinct()
            .CountAsync(ct);

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
        return await _dbContext.Alertas
            .GroupBy(a => a.TipoDetector)
            .Select(g => new AlertasPorTipoResponse(g.Key.ToString(), g.Count()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AlertasPorEstacionResponse>> GetAlertasPorEstacionAsync(CancellationToken ct = default)
    {
        return await _dbContext.Alertas
            .Include(a => a.Estacion)
            .GroupBy(a => new { a.EstacionId, a.Estacion.Nombre })
            .Select(g => new AlertasPorEstacionResponse(g.Key.EstacionId, g.Key.Nombre, g.Count()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AlertasPorNivelResponse>> GetAlertasPorNivelAsync(CancellationToken ct = default)
    {
        return await _dbContext.Alertas
            .GroupBy(a => a.NivelRiesgo)
            .Select(g => new AlertasPorNivelResponse(g.Key.ToString(), g.Count()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TendenciaDiaResponse>> GetTendenciaAsync(int dias, CancellationToken ct = default)
    {
        dias = Math.Clamp(dias, 1, 90);
        var desde = DateTime.UtcNow.Date.AddDays(-(dias - 1));

        var agrupadas = await _dbContext.Alertas
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

        var resultado = await _dbContext.Alertas
            .Where(a => a.EmpleadoCodigo != null && a.EmpleadoCodigo != "")
            .GroupBy(a => new { a.EmpleadoCodigo, a.Estacion.Nombre })
            .Select(g => new
            {
                g.Key.EmpleadoCodigo,
                EstacionNombre = g.Key.Nombre,
                Cantidad = g.Count(),
                ScorePromedio = g.Average(a => a.Score),
                Criticas = g.Count(a => a.NivelRiesgo == NivelRiesgo.Critico)
            })
            .OrderByDescending(t => t.Cantidad)
            .ThenByDescending(t => t.ScorePromedio)
            .Take(top)
            .ToListAsync(ct);

        return resultado
            .Select(r => new TopEmpleadoResponse(
                r.EmpleadoCodigo!,
                r.Cantidad,
                Math.Round(r.ScorePromedio, 1),
                r.Criticas,
                r.EstacionNombre))
            .ToList();
    }

    public async Task<MetricasResolucionResponse> GetMetricasResolucionAsync(CancellationToken ct = default)
    {
        var resueltas = await _dbContext.Alertas
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
            AlertasUltimas24Horas = await _dbContext.Alertas
                .CountAsync(a => a.FechaDeteccion >= hace24Horas, ct),
            TotalResueltas = totalResueltas,
            TotalPendientes = await _dbContext.Alertas
                .CountAsync(a => a.Estado == EstadoAlerta.Nueva || a.Estado == EstadoAlerta.EnRevision, ct)
        };
    }
}
