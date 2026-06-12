using PetrolRios.Application.DTOs.Dashboard;

namespace PetrolRios.Application.Interfaces;

public interface IDashboardService
{
    Task<KpiResponse> GetKpisAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AlertasPorTipoResponse>> GetAlertasPorTipoAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AlertasPorEstacionResponse>> GetAlertasPorEstacionAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AlertasPorNivelResponse>> GetAlertasPorNivelAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TendenciaDiaResponse>> GetTendenciaAsync(int dias, CancellationToken ct = default);
    Task<IReadOnlyList<TopEmpleadoResponse>> GetTopEmpleadosAsync(int top, CancellationToken ct = default);
    Task<MetricasResolucionResponse> GetMetricasResolucionAsync(CancellationToken ct = default);
}
