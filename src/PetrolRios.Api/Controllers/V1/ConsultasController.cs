using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Application.DTOs.Consultas;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Consultas EN VIVO a la Firebird de una estación, sin que el central tenga que conectarse al agente.
/// El auditor encola una consulta (<c>POST</c>), el agente la recoge en su heartbeat (cada 1 s), la corre
/// SOLO LECTURA y devuelve el resultado (<c>POST …/resultado</c>), y la interfaz lo sondea (<c>GET …/{id}</c>).
/// </summary>
[ApiController]
[Route("api/v1/consultas")]
[Authorize]
public sealed class ConsultasController : ControllerBase
{
    private readonly IConsultasFirebird _consultas;

    public ConsultasController(IConsultasFirebird consultas)
    {
        _consultas = consultas;
    }

    /// <summary>Encola una consulta de documentos para una estación. Devuelve el id para sondear el resultado.</summary>
    [HttpPost]
    [Authorize(Policy = "Central")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Encolar([FromBody] SolicitudConsulta solicitud)
    {
        if (string.IsNullOrWhiteSpace(solicitud.CodigoEstacion))
            return BadRequest(new { mensaje = "Indique la estación de la consulta." });

        var limite = Math.Clamp(solicitud.Limite <= 0 ? 200 : solicitud.Limite, 1, 1000);
        var id = _consultas.Encolar(solicitud with { Limite = limite });
        return Accepted(new { id });
    }

    /// <summary>Estado/resultado de una consulta (la interfaz lo sondea hasta "Listo"/"Error").</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "Central")]
    [ProducesResponseType(typeof(ConsultaEstado), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ConsultaEstado> Estado(string id)
    {
        var estado = _consultas.Obtener(id);
        return estado is null
            ? NotFound(new { mensaje = "La consulta no existe o expiró. Vuelva a lanzarla." })
            : Ok(estado);
    }

    /// <summary>El agente reporta el resultado (o el error) de una consulta que tomó en su heartbeat.</summary>
    [HttpPost("{id}/resultado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Resultado(string id, [FromBody] ResultadoConsultaRequest request)
    {
        _consultas.Responder(id, request.Ok, request.ResultadoJson, request.Error);
        return Ok();
    }
}

/// <summary>Lo que el agente envía al reportar el resultado de una consulta.</summary>
public sealed record ResultadoConsultaRequest(bool Ok, string? ResultadoJson, string? Error);
