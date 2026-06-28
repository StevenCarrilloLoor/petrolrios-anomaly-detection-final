using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Application.DTOs.Combustible;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Precios oficiales de los combustibles regulados de Ecuador (Extra, Ecopaís, Diésel). Sirven al
/// dashboard y como referencia del "precio autorizado". Los fija EP Petroecuador por el sistema de
/// bandas (mensual); un administrador los actualiza al cambiar la banda, o un conector externo los
/// refresca si se configura una fuente. La Súper se excluye: su precio no es regulado.
/// </summary>
[ApiController]
[Route("api/v1/precios-combustible")]
[Authorize(Roles = "Auditor,Supervisor,Administrador", Policy = "Central")]
public sealed class PreciosCombustibleController : ControllerBase
{
    private readonly IPreciosCombustibleService _precios;

    public PreciosCombustibleController(IPreciosCombustibleService precios) => _precios = precios;

    /// <summary>Precios oficiales vigentes (para el dashboard). Cualquier usuario central.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PreciosCombustibleResponse>> Vigentes(CancellationToken ct) =>
        Ok(await _precios.ObtenerVigentesAsync(ct));

    /// <summary>Actualiza el precio de un combustible al cambiar la banda mensual. Solo administradores.</summary>
    [HttpPut]
    [Authorize(Roles = "Administrador", Policy = "Central")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PreciosCombustibleResponse>> Actualizar(
        [FromBody] ActualizarPrecioCombustibleRequest req, CancellationToken ct)
    {
        try
        {
            return Ok(await _precios.ActualizarAsync(req, ct));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>Refresca los precios desde la fuente externa configurada (si hay). Solo administradores.</summary>
    [HttpPost("refrescar")]
    [Authorize(Roles = "Administrador", Policy = "Central")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PreciosCombustibleResponse>> Refrescar(CancellationToken ct) =>
        Ok(await _precios.RefrescarDesdeFuenteAsync("manual", ct));
}
