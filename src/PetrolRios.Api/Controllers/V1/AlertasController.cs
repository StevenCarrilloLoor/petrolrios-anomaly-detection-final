using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Application.DTOs.Alertas;
using PetrolRios.Application.DTOs.Common;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Api.Controllers.V1;

[ApiController]
[Route("api/v1/alertas")]
[Authorize]
public sealed class AlertasController : ControllerBase
{
    private readonly IAlertaService _alertaService;

    public AlertasController(IAlertaService alertaService)
    {
        _alertaService = alertaService;
    }

    /// <summary>
    /// Listar alertas con filtros, paginación y ordenamiento.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<AlertaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] TipoDetector? tipo,
        [FromQuery] NivelRiesgo? nivelRiesgo,
        [FromQuery] EstadoAlerta? estado,
        [FromQuery] int? estacionId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _alertaService.GetFilteredAsync(
            tipo, nivelRiesgo, estado, estacionId, fechaDesde, fechaHasta, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener detalle de una alerta por su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AlertaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _alertaService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Cambiar el estado de una alerta (revisada, falso positivo, confirmada, etc.).
    /// </summary>
    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(typeof(AlertaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoRequest request, CancellationToken ct)
    {
        var result = await _alertaService.CambiarEstadoAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Asignar una alerta a un auditor para revisión.
    /// </summary>
    [HttpPost("{id:int}/asignar")]
    [Authorize(Roles = "Supervisor,Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Asignar(int id, [FromBody] AsignarAlertaRequest request, CancellationToken ct)
    {
        await _alertaService.AsignarAsync(id, request, ct);
        return NoContent();
    }
}
