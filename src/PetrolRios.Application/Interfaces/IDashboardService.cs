using PetrolRios.Application.DTOs.Dashboard;

namespace PetrolRios.Application.Interfaces;

public interface IDashboardService
{
    Task<KpiResponse> GetKpisAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AlertasPorTipoResponse>> GetAlertasPorTipoAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AlertasPorEstacionResponse>> GetAlertasPorEstacionAsync(CancellationToken ct = default);
}
