using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Application.DTOs.Monitoreo;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Monitoreo de conexiones del aplicativo: estado de los agentes por estación,
/// base de datos, SignalR y motor de detección.
/// </summary>
[ApiController]
[Route("api/v1/monitoreo")]
[Authorize]
public sealed class MonitoreoController : ControllerBase
{
    private readonly IMonitoreoService _monitoreoService;

    public MonitoreoController(IMonitoreoService monitoreoService)
    {
        _monitoreoService = monitoreoService;
    }

    /// <summary>
    /// Estado de conexión de cada estación (agente): última ingesta, volumen y pendientes.
    /// </summary>
    [HttpGet("conexiones")]
    [ProducesResponseType(typeof(IReadOnlyList<ConexionEstacionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConexiones(CancellationToken ct)
    {
        var result = await _monitoreoService.GetConexionesEstacionesAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Estado general del sistema: API, base de datos, SignalR y último ciclo de detección.
    /// </summary>
    [HttpGet("sistema")]
    [ProducesResponseType(typeof(EstadoSistemaResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstadoSistema(CancellationToken ct)
    {
        var result = await _monitoreoService.GetEstadoSistemaAsync(ct);
        return Ok(result);
    }
}
