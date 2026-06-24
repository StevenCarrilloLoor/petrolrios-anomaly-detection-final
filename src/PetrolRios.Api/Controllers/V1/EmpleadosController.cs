using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Api.Extensions;
using PetrolRios.Application.DTOs.Empleados;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Catálogo de empleados/despachadores por estación. El agente lo sincroniza desde Firebird
/// (VEND/EMPL) para que las alertas muestren el NOMBRE junto al código y se pueda actuar de
/// inmediato. La cuenta de estación solo puede sincronizar SU propia estación.
/// </summary>
[ApiController]
[Route("api/v1/empleados")]
[Authorize]
public sealed class EmpleadosController : ControllerBase
{
    private readonly PetrolRiosDbContext _dbContext;

    public EmpleadosController(PetrolRiosDbContext dbContext) => _dbContext = dbContext;

    /// <summary>El agente envía el catálogo de empleados de SU estación (upsert por código).</summary>
    [HttpPost("sync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Sincronizar(
        [FromBody] SincronizarEmpleadosRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.CodigoEstacion))
            return BadRequest(new { mensaje = "El código de estación es obligatorio." });
        if (!await User.PuedeUsarEstacionAsync(_dbContext, request.CodigoEstacion, ct))
            return Forbid();

        var codigoEstacion = request.CodigoEstacion.Trim().ToUpperInvariant();
        var estacion = await _dbContext.Estaciones
            .FirstOrDefaultAsync(e => e.Codigo == codigoEstacion, ct);
        if (estacion is null)
            return NotFound(new { mensaje = $"La estación '{codigoEstacion}' no está registrada." });

        // Normaliza, descarta vacíos y deduplica por código (el último gana).
        var entrantes = (request.Empleados ?? [])
            .Where(e => !string.IsNullOrWhiteSpace(e.Codigo) && !string.IsNullOrWhiteSpace(e.Nombre))
            .GroupBy(e => Empleado.Normalizar(e.Codigo))
            .ToDictionary(g => g.Key, g => g.Last().Nombre.Trim());
        if (entrantes.Count == 0) return Ok(new { sincronizados = 0 });

        var existentes = await _dbContext.Empleados
            .Where(e => e.EstacionId == estacion.Id)
            .ToDictionaryAsync(e => e.Codigo, ct);

        foreach (var (cod, nombre) in entrantes)
        {
            if (existentes.TryGetValue(cod, out var emp))
            {
                if (emp.Nombre != nombre) emp.Actualizar(nombre);
            }
            else
            {
                await _dbContext.Empleados.AddAsync(Empleado.Create(estacion.Id, cod, nombre), ct);
            }
        }

        await _dbContext.SaveChangesAsync(ct);
        return Ok(new { sincronizados = entrantes.Count });
    }
}
