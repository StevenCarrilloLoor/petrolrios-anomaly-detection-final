using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Application.DTOs.Configuracion;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Gestión de la conexión a la base de datos central desde el sistema (solo Administrador).
/// Permite ver, probar y guardar la conexión sin tocar el código ni recompilar; los cambios
/// se persisten en config/connection.json y se aplican al reiniciar.
/// </summary>
[ApiController]
[Route("api/v1/conexion-base")]
[Authorize(Roles = "Administrador", Policy = "Central")]
public sealed class ConexionBaseController : ControllerBase
{
    private readonly IConexionStore _store;

    public ConexionBaseController(IConexionStore store)
    {
        _store = store;
    }

    /// <summary>Estado actual de la conexión (enmascarada) y su origen.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ConexionActiva), StatusCodes.Status200OK)]
    public ActionResult<ConexionActiva> Estado() => Ok(_store.DescribirActiva());

    /// <summary>Prueba una conexión candidata sin persistir nada.</summary>
    [HttpPost("probar")]
    [ProducesResponseType(typeof(ProbarConexionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProbarConexionResponse>> Probar(
        [FromBody] ProbarConexionRequest request,
        CancellationToken ct)
    {
        var cadena = ResolverCadena(request);
        if (string.IsNullOrWhiteSpace(cadena))
            return Ok(new ProbarConexionResponse(false, "Indique una cadena de conexión o los campos del servidor.", null));

        var (ok, mensaje, version) = await _store.ProbarAsync(cadena, ct);
        return Ok(new ProbarConexionResponse(ok, mensaje, version));
    }

    /// <summary>Prueba y, si funciona, persiste la conexión. No guarda una conexión que falla.</summary>
    [HttpPost("guardar")]
    [ProducesResponseType(typeof(GuardarConexionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(GuardarConexionResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GuardarConexionResponse>> Guardar(
        [FromBody] ProbarConexionRequest request,
        CancellationToken ct)
    {
        var cadena = ResolverCadena(request);
        if (string.IsNullOrWhiteSpace(cadena))
            return BadRequest(new GuardarConexionResponse(false, "Indique una cadena de conexión o los campos del servidor."));

        var (ok, mensaje, _) = await _store.ProbarAsync(cadena, ct);
        if (!ok)
            return BadRequest(new GuardarConexionResponse(false, $"No se guardó: la conexión falló ({mensaje})."));

        _store.Guardar(cadena);
        return Ok(new GuardarConexionResponse(true, "Conexión guardada. Reinicie el sistema central para aplicarla."));
    }

    private string? ResolverCadena(ProbarConexionRequest request)
    {
        string? cadena = null;

        if (!string.IsNullOrWhiteSpace(request.Cadena))
            cadena = request.Cadena.Trim();
        else if (!string.IsNullOrWhiteSpace(request.Servidor)
            && !string.IsNullOrWhiteSpace(request.BaseDatos)
            && !string.IsNullOrWhiteSpace(request.Usuario))
        {
            cadena = _store.ConstruirCadena(
                request.Servidor!.Trim(),
                request.Puerto ?? 5432,
                request.BaseDatos!.Trim(),
                request.Usuario!.Trim(),
                request.Password,
                request.ModoSsl ?? "Prefer");
        }

        // Si no se reescribió la contraseña pero es la misma base, reusa la activa.
        return cadena is null ? null : _store.CompletarPassword(cadena);
    }
}
