using PetrolRios.Application.DTOs.Dashboard;

namespace PetrolRios.Application.Interfaces;

public interface IDashboardService
{
    // estacionId (opcional) acota el dashboard a una estación ("no mezclar estaciones", lo pidió
    // auditoría). La comparativa "alertas por estación" es la vista cruzada, así que NO se filtra.
    Task<KpiResponse> GetKpisAsync(int? estacionId = null, CancellationToken ct = default);
    Task<IReadOnlyList<AlertasPorTipoResponse>> GetAlertasPorTipoAsync(int? estacionId = null, CancellationToken ct = default);
    Task<IReadOnlyList<AlertasPorEstacionResponse>> GetAlertasPorEstacionAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AlertasPorNivelResponse>> GetAlertasPorNivelAsync(int? estacionId = null, CancellationToken ct = default);
    Task<IReadOnlyList<TendenciaDiaResponse>> GetTendenciaAsync(int dias, int? estacionId = null, CancellationToken ct = default);
    Task<IReadOnlyList<TopEmpleadoResponse>> GetTopEmpleadosAsync(int top, int? estacionId = null, CancellationToken ct = default);
    Task<MetricasResolucionResponse> GetMetricasResolucionAsync(int? estacionId = null, CancellationToken ct = default);
}
