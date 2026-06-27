using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Application.DTOs.Dashboard;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Api.Controllers.V1;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize(Policy = "Central")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Obtener KPIs generales del sistema.
    /// </summary>
    [HttpGet("kpis")]
    [ProducesResponseType(typeof(KpiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKpis([FromQuery] int? estacionId, CancellationToken ct)
    {
        var result = await _dashboardService.GetKpisAsync(estacionId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener conteo de alertas agrupadas por tipo de detector.
    /// </summary>
    [HttpGet("alertas-por-tipo")]
    [ProducesResponseType(typeof(IReadOnlyList<AlertasPorTipoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlertasPorTipo([FromQuery] int? estacionId, CancellationToken ct)
    {
        var result = await _dashboardService.GetAlertasPorTipoAsync(estacionId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener conteo de alertas agrupadas por estación.
    /// </summary>
    [HttpGet("alertas-por-estacion")]
    [ProducesResponseType(typeof(IReadOnlyList<AlertasPorEstacionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlertasPorEstacion(CancellationToken ct)
    {
        var result = await _dashboardService.GetAlertasPorEstacionAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener conteo de alertas agrupadas por nivel de riesgo.
    /// </summary>
    [HttpGet("alertas-por-nivel")]
    [ProducesResponseType(typeof(IReadOnlyList<AlertasPorNivelResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlertasPorNivel([FromQuery] int? estacionId, CancellationToken ct)
    {
        var result = await _dashboardService.GetAlertasPorNivelAsync(estacionId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener la serie temporal de alertas por día (CU-13).
    /// </summary>
    [HttpGet("tendencia")]
    [ProducesResponseType(typeof(IReadOnlyList<TendenciaDiaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTendencia(
        [FromQuery] int dias = 14, [FromQuery] int? estacionId = null, CancellationToken ct = default)
    {
        var result = await _dashboardService.GetTendenciaAsync(dias, estacionId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener el ranking de empleados con más alertas (CU-13).
    /// </summary>
    [HttpGet("top-empleados")]
    [ProducesResponseType(typeof(IReadOnlyList<TopEmpleadoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopEmpleados(
        [FromQuery] int top = 10, [FromQuery] int? estacionId = null, CancellationToken ct = default)
    {
        var result = await _dashboardService.GetTopEmpleadosAsync(top, estacionId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener métricas de resolución y efectividad (CU-13).
    /// </summary>
    [HttpGet("metricas-resolucion")]
    [ProducesResponseType(typeof(MetricasResolucionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetricasResolucion([FromQuery] int? estacionId, CancellationToken ct)
    {
        var result = await _dashboardService.GetMetricasResolucionAsync(estacionId, ct);
        return Ok(result);
    }
}
