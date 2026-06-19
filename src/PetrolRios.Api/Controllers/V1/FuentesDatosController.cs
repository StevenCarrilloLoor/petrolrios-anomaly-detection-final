using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Api.Extensions;
using PetrolRios.Application.DTOs.Fuentes;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Catálogo CENTRAL de fuentes de datos adicionales (tablas extra de Firebird que todos
/// los agentes deben extraer). El ingeniero registra una tabla una sola vez aquí y los
/// agentes la reciben automáticamente vía <c>GET /activas</c>; no se configura estación
/// por estación. Cada agente verifica que la tabla exista en SU base antes de extraer.
/// </summary>
[ApiController]
[Route("api/v1/fuentes-datos")]
[Authorize]
public sealed class FuentesDatosController : ControllerBase
{
    private readonly PetrolRiosDbContext _dbContext;
    private readonly ILogService _logService;

    public FuentesDatosController(PetrolRiosDbContext dbContext, ILogService logService)
    {
        _dbContext = dbContext;
        _logService = logService;
    }

    /// <summary>
    /// Fuentes ACTIVAS para los agentes (definición mínima). Cualquier usuario autenticado
    /// (incluido el usuario del agente) puede consultarla.
    /// </summary>
    [HttpGet("activas")]
    [ProducesResponseType(typeof(IReadOnlyList<FuenteDatosAgente>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivas(CancellationToken ct)
    {
        var fuentes = await _dbContext.FuentesDatos
            .AsNoTracking()
            .Where(f => f.Activa)
            .OrderBy(f => f.Nombre)
            .Select(f => new FuenteDatosAgente(f.Nombre, f.Tabla, f.ColumnaWatermark))
            .ToListAsync(ct);
        return Ok(fuentes);
    }

    /// <summary>Listado completo para administración (Supervisor/Administrador).</summary>
    [HttpGet]
    [Authorize(Roles = "Supervisor,Administrador")]
    [ProducesResponseType(typeof(IReadOnlyList<FuenteDatosResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var fuentes = await _dbContext.FuentesDatos
            .AsNoTracking()
            .OrderBy(f => f.Nombre)
            .Select(f => Map(f))
            .ToListAsync(ct);
        return Ok(fuentes);
    }

    /// <summary>Registrar una nueva fuente en el catálogo central (solo Administrador).</summary>
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(FuenteDatosResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearFuenteDatosRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Tabla) || string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(new { mensaje = "Nombre y tabla son obligatorios." });

        var tabla = request.Tabla.Trim();
        if (await _dbContext.FuentesDatos.AnyAsync(f => f.Tabla.ToLower() == tabla.ToLower(), ct))
            return Conflict(new { mensaje = $"La tabla '{tabla}' ya está registrada." });

        var fuente = FuenteDatos.Create(request.Nombre, tabla, request.ColumnaWatermark, request.Descripcion ?? "");
        await _dbContext.FuentesDatos.AddAsync(fuente, ct);
        await _dbContext.SaveChangesAsync(ct);

        await this.RegistrarAuditoriaAsync(_logService,
            "Registro de fuente de datos central", "FuenteDatos", fuente.Id,
            new { fuente.Nombre, fuente.Tabla, fuente.ColumnaWatermark }, ct: ct);

        return StatusCode(StatusCodes.Status201Created, Map(fuente));
    }

    /// <summary>Actualizar una fuente (solo Administrador).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(FuenteDatosResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarFuenteDatosRequest request, CancellationToken ct)
    {
        var fuente = await _dbContext.FuentesDatos.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (fuente is null) return NotFound();

        var tabla = request.Tabla.Trim();
        if (await _dbContext.FuentesDatos.AnyAsync(f => f.Id != id && f.Tabla.ToLower() == tabla.ToLower(), ct))
            return Conflict(new { mensaje = $"La tabla '{tabla}' ya está registrada en otra fuente." });

        fuente.Actualizar(request.Nombre, tabla, request.ColumnaWatermark, request.Descripcion ?? "", request.Activa);
        await _dbContext.SaveChangesAsync(ct);

        await this.RegistrarAuditoriaAsync(_logService,
            "Actualización de fuente de datos central", "FuenteDatos", fuente.Id,
            new { fuente.Nombre, fuente.Tabla, fuente.Activa }, ct: ct);

        return Ok(Map(fuente));
    }

    /// <summary>Eliminar una fuente del catálogo (solo Administrador).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        var fuente = await _dbContext.FuentesDatos.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (fuente is null) return NotFound();

        _dbContext.FuentesDatos.Remove(fuente);
        await _dbContext.SaveChangesAsync(ct);

        await this.RegistrarAuditoriaAsync(_logService,
            "Eliminación de fuente de datos central", "FuenteDatos", id,
            new { fuente.Nombre, fuente.Tabla }, ct: ct);

        return NoContent();
    }

    private static FuenteDatosResponse Map(FuenteDatos f) => new()
    {
        Id = f.Id,
        Nombre = f.Nombre,
        Tabla = f.Tabla,
        ColumnaWatermark = f.ColumnaWatermark,
        Descripcion = f.Descripcion,
        Activa = f.Activa
    };
}
