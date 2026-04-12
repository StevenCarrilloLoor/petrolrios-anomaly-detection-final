using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Application.DTOs.Reglas;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Api.Controllers.V1;

[ApiController]
[Route("api/v1/reglas")]
[Authorize(Roles = "Supervisor,Administrador")]
public sealed class ReglasController : ControllerBase
{
    private readonly IReglaService _reglaService;

    public ReglasController(IReglaService reglaService)
    {
        _reglaService = reglaService;
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
    /// Crear una nueva regla de detección.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReglaDeteccionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CrearReglaRequest request, CancellationToken ct)
    {
        var result = await _reglaService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Actualizar umbral o estado activo de una regla.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReglaDeteccionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarReglaRequest request, CancellationToken ct)
    {
        var result = await _reglaService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Eliminar una regla de detección.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _reglaService.DeleteAsync(id, ct);
        return NoContent();
    }
}
