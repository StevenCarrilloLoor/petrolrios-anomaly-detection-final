using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Api.Extensions;
using PetrolRios.Application.DTOs.Ingesta;
using PetrolRios.Application.Interfaces;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Api.Controllers.V1;

[ApiController]
[Route("api/v1/ingesta")]
[Authorize]
public sealed class IngestaController : ControllerBase
{
    private readonly IIngestaService _ingestaService;
    private readonly ISolicitudesEsquema _solicitudesEsquema;
    private readonly IConsultasFirebird _consultas;
    private readonly PetrolRiosDbContext _dbContext;

    public IngestaController(
        IIngestaService ingestaService,
        ISolicitudesEsquema solicitudesEsquema,
        IConsultasFirebird consultas,
        PetrolRiosDbContext dbContext)
    {
        _ingestaService = ingestaService;
        _solicitudesEsquema = solicitudesEsquema;
        _consultas = consultas;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Recibe un lote de transacciones de un agente de estación.
    /// Los agentes se autentican con JWT y envían datos cada 5 minutos.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(IngestaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecibirLote(
        [FromBody] IngestaRequest request,
        CancellationToken ct)
    {
        if (!await User.PuedeUsarEstacionAsync(_dbContext, request.CodigoEstacion, ct))
            return Forbid();

        var result = await _ingestaService.RecibirLoteAsync(request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Heartbeat del agente: señal de vida periódica aunque no haya datos nuevos.
    /// Si la estación no existe, se auto-registra en el sistema. La respuesta puede pedirle al
    /// agente que reporte su esquema (cuando un administrador solicitó "cargar esquema" de la estación).
    /// </summary>
    [HttpPost("heartbeat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Heartbeat(
        [FromBody] HeartbeatRequest request,
        CancellationToken ct)
    {
        if (!await User.PuedeUsarEstacionAsync(_dbContext, request.CodigoEstacion, ct))
            return Forbid();

        await _ingestaService.HeartbeatAsync(request, ct);
        var reportarEsquema = _solicitudesEsquema.TomarPendiente(request.CodigoEstacion);
        // Consultas en vivo pendientes para esta estación: el agente las correrá SOLO LECTURA y devolverá
        // el resultado a /api/v1/consultas/{id}/resultado. Se entregan una sola vez (quedan "tomadas").
        var consultas = _consultas.TomarPendientes(request.CodigoEstacion);
        return Ok(new { reportarEsquema, consultas });
    }
}
