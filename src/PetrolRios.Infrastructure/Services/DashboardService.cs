using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.DTOs.Dashboard;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly PetrolRiosDbContext _dbContext;

    public DashboardService(PetrolRiosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<KpiResponse> GetKpisAsync(CancellationToken ct = default)
    {
        var alertas = _dbContext.Alertas.AsQueryable();

        return new KpiResponse
        {
            TotalAlertas = await alertas.CountAsync(ct),
            AlertasNuevas = await alertas.CountAsync(a => a.Estado == EstadoAlerta.Nueva, ct),
            AlertasCriticas = await alertas.CountAsync(a => a.NivelRiesgo == NivelRiesgo.Critico, ct),
            AlertasEnRevision = await alertas.CountAsync(a => a.Estado == EstadoAlerta.EnRevision, ct),
            AlertasConfirmadas = await alertas.CountAsync(a => a.Estado == EstadoAlerta.Confirmada, ct),
            AlertasFalsoPositivo = await alertas.CountAsync(a => a.Estado == EstadoAlerta.FalsoPositivo, ct),
            ScorePromedio = await alertas.AnyAsync(ct) ? await alertas.AverageAsync(a => a.Score, ct) : 0,
            EstacionesActivas = await _dbContext.Estaciones.CountAsync(e => e.Activa, ct)
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
}
