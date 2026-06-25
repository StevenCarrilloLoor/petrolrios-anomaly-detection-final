using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Api.Extensions;
using PetrolRios.Application.DTOs.ReglasPersonalizadas;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Relaciones entre tablas/fuentes para enriquecer las alertas de las reglas personalizadas
/// (estilo "lookup/linked fields"): p. ej. un Despacho se relaciona con su Factura por el código de
/// cliente, y así una regla sobre despachos puede mostrar la placa, el vendedor y el cliente en la
/// alerta. El Administrador las gestiona aquí; el builder de reglas y el detector las usan.
/// </summary>
[ApiController]
[Route("api/v1/relaciones-tabla")]
[Authorize(Policy = "Central")]
public sealed class RelacionesTablaController : ControllerBase
{
    private readonly PetrolRiosDbContext _dbContext;
    private readonly ILogService _logService;

    public RelacionesTablaController(PetrolRiosDbContext dbContext, ILogService logService)
    {
        _dbContext = dbContext;
        _logService = logService;
    }

    /// <summary>Listar todas las relaciones (visible para los roles del central).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RelacionTablaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var relaciones = await _dbContext.RelacionesTabla
            .AsNoTracking()
            .OrderBy(r => r.FuenteOrigen).ThenBy(r => r.FuenteDestino)
            .Select(r => new RelacionTablaResponse(
                r.Id, r.FuenteOrigen, r.FuenteDestino, r.CampoOrigen, r.CampoDestino, r.Etiqueta, r.Activa))
            .ToListAsync(ct);
        return Ok(relaciones);
    }

    /// <summary>Crear una relación (solo Administrador).</summary>
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(RelacionTablaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] GuardarRelacionTablaRequest request, CancellationToken ct)
    {
        var error = Validar(request);
        if (error is not null) return BadRequest(new { mensaje = error });

        var existe = await _dbContext.RelacionesTabla.AnyAsync(r =>
            r.FuenteOrigen == request.FuenteOrigen.Trim() && r.FuenteDestino == request.FuenteDestino.Trim()
            && r.CampoOrigen == request.CampoOrigen.Trim() && r.CampoDestino == request.CampoDestino.Trim(), ct);
        if (existe) return BadRequest(new { mensaje = "Ya existe una relación igual." });

        var rel = RelacionTabla.Create(
            request.FuenteOrigen, request.FuenteDestino, request.CampoOrigen, request.CampoDestino, request.Etiqueta);
        rel.Activa = request.Activa;
        await _dbContext.RelacionesTabla.AddAsync(rel, ct);
        await _dbContext.SaveChangesAsync(ct);

        await this.RegistrarAuditoriaAsync(_logService, "Creación de relación de tablas", "RelacionTabla", rel.Id,
            new { rel.FuenteOrigen, rel.FuenteDestino }, ct: ct);

        return CreatedAtAction(nameof(GetAll), MapToResponse(rel));
    }

    /// <summary>Actualizar una relación (solo Administrador).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(RelacionTablaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] GuardarRelacionTablaRequest request, CancellationToken ct)
    {
        var rel = await _dbContext.RelacionesTabla.FindAsync([id], ct);
        if (rel is null) return NotFound();

        var error = Validar(request);
        if (error is not null) return BadRequest(new { mensaje = error });

        rel.Actualizar(request.FuenteOrigen, request.FuenteDestino, request.CampoOrigen,
            request.CampoDestino, request.Etiqueta, request.Activa);
        await _dbContext.SaveChangesAsync(ct);

        await this.RegistrarAuditoriaAsync(_logService, "Actualización de relación de tablas", "RelacionTabla", id,
            new { rel.FuenteOrigen, rel.FuenteDestino, rel.Activa }, ct: ct);
        return Ok(MapToResponse(rel));
    }

    /// <summary>Eliminar una relación (solo Administrador).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var rel = await _dbContext.RelacionesTabla.FindAsync([id], ct);
        if (rel is null) return NotFound();

        _dbContext.RelacionesTabla.Remove(rel);
        await _dbContext.SaveChangesAsync(ct);

        await this.RegistrarAuditoriaAsync(_logService, "Eliminación de relación de tablas", "RelacionTabla", id,
            new { rel.FuenteOrigen, rel.FuenteDestino }, ct: ct);
        return NoContent();
    }

    private static string? Validar(GuardarRelacionTablaRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.FuenteOrigen)) return "La fuente de origen es obligatoria.";
        if (string.IsNullOrWhiteSpace(r.FuenteDestino)) return "La fuente de destino es obligatoria.";
        if (string.IsNullOrWhiteSpace(r.CampoOrigen)) return "El campo de origen es obligatorio.";
        if (string.IsNullOrWhiteSpace(r.CampoDestino)) return "El campo de destino es obligatorio.";
        if (string.Equals(r.FuenteOrigen.Trim(), r.FuenteDestino.Trim(), StringComparison.OrdinalIgnoreCase))
            return "El origen y el destino deben ser fuentes distintas.";
        return null;
    }

    private static RelacionTablaResponse MapToResponse(RelacionTabla r) =>
        new(r.Id, r.FuenteOrigen, r.FuenteDestino, r.CampoOrigen, r.CampoDestino, r.Etiqueta, r.Activa);
}
