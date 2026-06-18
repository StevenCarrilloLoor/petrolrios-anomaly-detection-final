using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Application.DTOs.Monitoreo;
using PetrolRios.Application.Interfaces;
using PetrolRios.Infrastructure.Hubs;

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
    /// Usuarios conectados al central en tiempo real (vía SignalR). Solo Supervisor/Administrador.
    /// </summary>
    [HttpGet("usuarios-conectados")]
    [Authorize(Roles = "Supervisor,Administrador")]
    [ProducesResponseType(typeof(IReadOnlyList<UsuarioConectadoResponse>), StatusCodes.Status200OK)]
    public IActionResult GetUsuariosConectados()
    {
        var conectados = AlertsHub.UsuariosConectados
            .Select(u => new UsuarioConectadoResponse
            {
                UsuarioId = u.UsuarioId,
                Nombre = u.Nombre,
                Rol = u.Rol,
                EstacionId = u.EstacionId,
                Desde = u.Desde
            })
            .ToList();
        return Ok(conectados);
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
