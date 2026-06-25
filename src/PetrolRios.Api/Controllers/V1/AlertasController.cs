using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Api.Extensions;
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
    private readonly ILogService _logService;

    public AlertasController(IAlertaService alertaService, ILogService logService)
    {
        _alertaService = alertaService;
        _logService = logService;
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
        if (User.GetEstacionId().HasValue)
            return Forbid();

        var result = await _alertaService.GetFilteredAsync(
            tipo, nivelRiesgo, estado, estacionId, fechaDesde, fechaHasta, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Problemas operativos (carril Operativa) agrupados por estación y día, para la pestaña
    /// "Problemas de estación".
    /// </summary>
    [HttpGet("problemas-estacion")]
    [ProducesResponseType(typeof(IReadOnlyList<ProblemaEstacionGrupo>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProblemasEstacion(
        [FromQuery] int? estacionId,
        [FromQuery] int dias = 7,
        [FromQuery] bool soloActivos = false,
        CancellationToken ct = default)
    {
        var estacionEfectiva = User.GetEstacionId() ?? estacionId;
        var result = await _alertaService.GetProblemasEstacionAsync(
            estacionEfectiva,
            dias,
            soloActivos,
            ct);
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
        var estacionAsignada = User.GetEstacionId();
        if (estacionAsignada.HasValue
            && (result is null
                || result.EstacionId != estacionAsignada.Value
                || !result.Ambito.Equals(nameof(AmbitoAlerta.Operativa), StringComparison.OrdinalIgnoreCase)))
        {
            return NotFound();
        }

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
        if (User.GetEstacionId().HasValue)
            return Forbid();

        var result = await _alertaService.CambiarEstadoAsync(id, request, ct);

        await this.RegistrarAuditoriaAsync(_logService,
            $"Cambio de estado de alerta a '{result.Estado}'", "Alerta", id,
            new { result.Estado, result.TipoDetector }, ct: ct);

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
        if (User.GetEstacionId().HasValue)
            return Forbid();

        await _alertaService.AsignarAsync(id, request, ct);

        await this.RegistrarAuditoriaAsync(_logService,
            "Asignación de alerta a auditor", "Alerta", id,
            new { request.AuditorId }, ct: ct);

        return NoContent();
    }

    /// <summary>
    /// Listar los comentarios de auditoría de una alerta (CU-07).
    /// </summary>
    [HttpGet("{id:int}/comentarios")]
    [ProducesResponseType(typeof(IReadOnlyList<ComentarioResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComentarios(int id, CancellationToken ct)
    {
        if (User.GetEstacionId().HasValue)
            return Forbid();

        var result = await _alertaService.GetComentariosAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Agregar un comentario de auditoría a una alerta (CU-07).
    /// </summary>
    [HttpPost("{id:int}/comentarios")]
    [ProducesResponseType(typeof(ComentarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AgregarComentario(
        int id, [FromBody] AgregarComentarioRequest request, CancellationToken ct)
    {
        if (User.GetEstacionId().HasValue)
            return Forbid();

        var usuarioIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var result = await _alertaService.AgregarComentarioAsync(id, usuarioId, request, ct);

        await this.RegistrarAuditoriaAsync(_logService,
            "Comentario de auditoría agregado", "Alerta", id, ct: ct);

        return CreatedAtAction(nameof(GetComentarios), new { id }, result);
    }

    /// <summary>
    /// Marca esta alerta como vista por el usuario actual (estado leído/no leído POR USUARIO; no
    /// afecta a lo que ven los demás). Idempotente.
    /// </summary>
    [HttpPost("{id:int}/marcar-vista")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarcarVista(int id, CancellationToken ct)
    {
        var usuarioIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        await _alertaService.MarcarVistaAsync(id, usuarioId, ct);
        return NoContent();
    }

    /// <summary>IDs de las alertas que el usuario actual ya vio (para resaltar las "nuevas para ti").</summary>
    [HttpGet("vistas")]
    [ProducesResponseType(typeof(IReadOnlyList<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVistas(CancellationToken ct)
    {
        var usuarioIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var vistas = await _alertaService.GetVistasAsync(usuarioId, ct);
        return Ok(vistas);
    }
}
