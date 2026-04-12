using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Application.DTOs.Dashboard;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Api.Controllers.V1;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
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
    public async Task<IActionResult> GetKpis(CancellationToken ct)
    {
        var result = await _dashboardService.GetKpisAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener conteo de alertas agrupadas por tipo de detector.
    /// </summary>
    [HttpGet("alertas-por-tipo")]
    [ProducesResponseType(typeof(IReadOnlyList<AlertasPorTipoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlertasPorTipo(CancellationToken ct)
    {
        var result = await _dashboardService.GetAlertasPorTipoAsync(ct);
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
}
