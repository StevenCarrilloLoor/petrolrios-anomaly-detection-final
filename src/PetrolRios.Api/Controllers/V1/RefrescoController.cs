using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Expone SOLO la tasa de refresco de la interfaz (en segundos) a CUALQUIER usuario autenticado del
/// central (Auditor, Supervisor o Administrador), para que todas las pantallas la usen como intervalo
/// de actualización. El resto de parámetros de operación (nivel de correo, cron) siguen siendo solo de
/// Administrador en <see cref="ParametrosOperacionController"/>.
/// </summary>
[ApiController]
[Route("api/v1/refresco")]
[Authorize(Policy = "Central")]
public sealed class RefrescoController : ControllerBase
{
    private readonly IParametrosOperacion _store;

    public RefrescoController(IParametrosOperacion store)
    {
        _store = store;
    }

    /// <summary>Tasa de refresco (segundos) con la que las pantallas vuelven a consultar al servidor.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Actual() => Ok(new { refrescoSegundos = _store.Actual().RefrescoSegundos });
}
