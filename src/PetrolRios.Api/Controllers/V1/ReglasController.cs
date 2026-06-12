using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Api.Extensions;
using PetrolRios.Application.DTOs.Reglas;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Configuración de las reglas del motor de detección (CU-14 / CU-16).
/// Las reglas están definidas por los 4 detectores del motor; desde aquí se
/// parametrizan (umbral) y se activan o desactivan. No se crean reglas
/// arbitrarias: cada parámetro corresponde a lógica implementada en el motor.
/// </summary>
[ApiController]
[Route("api/v1/reglas")]
[Authorize(Roles = "Supervisor,Administrador")]
public sealed class ReglasController : ControllerBase
{
    private readonly IReglaService _reglaService;
    private readonly ILogService _logService;

    public ReglasController(IReglaService reglaService, ILogService logService)
    {
        _reglaService = reglaService;
        _logService = logService;
    }

    /// <summary>
    /// Listar todas las reglas de detección.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReglaDeteccionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _reglaService.GetAllAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener una regla por ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReglaDeteccionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _reglaService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Actualizar el umbral o el estado activo de una regla del motor.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReglaDeteccionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarReglaRequest request, CancellationToken ct)
    {
        var result = await _reglaService.UpdateAsync(id, request, ct);

        await this.RegistrarAuditoriaAsync(_logService,
            "Actualización de regla de detección", "ReglaDeteccion", id,
            new { result.Nombre, result.ValorUmbral, result.Activa }, ct: ct);

        return Ok(result);
    }
}
