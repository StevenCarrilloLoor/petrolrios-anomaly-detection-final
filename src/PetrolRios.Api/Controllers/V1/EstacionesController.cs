using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Api.Extensions;
using PetrolRios.Application.DTOs.Estaciones;
using PetrolRios.Application.Interfaces;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Gestión de estaciones. Las estaciones se auto-registran cuando su agente se
/// conecta por primera vez; desde aquí se les asigna nombre/zona, se desactivan
/// o se eliminan (por ejemplo, una estación que dejó de ser parte del sistema).
/// </summary>
[ApiController]
[Route("api/v1/estaciones")]
[Authorize(Policy = "Central")]
public sealed class EstacionesController : ControllerBase
{
    private readonly PetrolRiosDbContext _dbContext;
    private readonly ILogService _logService;

    public EstacionesController(PetrolRiosDbContext dbContext, ILogService logService)
    {
        _dbContext = dbContext;
        _logService = logService;
    }

    /// <summary>Listar todas las estaciones registradas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EstacionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var estaciones = await _dbContext.Estaciones
            .OrderBy(e => e.Codigo)
            .Select(e => new EstacionResponse
            {
                Id = e.Id,
                Codigo = e.Codigo,
                Nombre = e.Nombre,
                Direccion = e.Direccion,
                Zona = e.Zona,
                Activa = e.Activa,
                UltimoHeartbeat = e.UltimoHeartbeat,
                VersionAgente = e.VersionAgente,
                HoraApertura = e.HoraApertura.ToString("HH:mm"),
                HoraCierre = e.HoraCierre.ToString("HH:mm"),
                CorreoContacto = e.CorreoContacto
            })
            .ToListAsync(ct);
        return Ok(estaciones);
    }

    /// <summary>Actualizar nombre, dirección y zona de una estación.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Supervisor,Administrador")]
    [ProducesResponseType(typeof(EstacionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] ActualizarEstacionRequest request, CancellationToken ct)
    {
        var estacion = await _dbContext.Estaciones.FindAsync([id], ct);
        if (estacion is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(new { mensaje = "El nombre no puede estar vacío." });

        estacion.Actualizar(request.Nombre.Trim(), request.Direccion?.Trim(), request.Zona?.Trim());

        // Configuración avanzada (horario, correo de contacto, activa): solo Administrador.
        if (User.IsInRole("Administrador"))
        {
            if (!string.IsNullOrWhiteSpace(request.HoraApertura)
                && TimeOnly.TryParse(request.HoraApertura, out var apertura))
                estacion.HoraApertura = apertura;
            if (!string.IsNullOrWhiteSpace(request.HoraCierre)
                && TimeOnly.TryParse(request.HoraCierre, out var cierre))
                estacion.HoraCierre = cierre;
            if (request.CorreoContacto is not null)
                estacion.CorreoContacto = string.IsNullOrWhiteSpace(request.CorreoContacto)
                    ? null : request.CorreoContacto.Trim();
            if (request.Activa.HasValue)
                estacion.Activa = request.Activa.Value;
        }

        await _dbContext.SaveChangesAsync(ct);

        await this.RegistrarAuditoriaAsync(_logService,
            "Actualización de estación", "Estacion", id,
            new { estacion.Codigo, estacion.Nombre, estacion.Zona, estacion.Activa }, ct: ct);

        return Ok(new EstacionResponse
        {
            Id = estacion.Id,
            Codigo = estacion.Codigo,
            Nombre = estacion.Nombre,
            Direccion = estacion.Direccion,
            Zona = estacion.Zona,
            Activa = estacion.Activa,
            UltimoHeartbeat = estacion.UltimoHeartbeat,
            VersionAgente = estacion.VersionAgente,
            HoraApertura = estacion.HoraApertura.ToString("HH:mm"),
            HoraCierre = estacion.HoraCierre.ToString("HH:mm"),
            CorreoContacto = estacion.CorreoContacto
        });
    }

    /// <summary>
    /// Eliminar una estación que ya no es parte del sistema. Si tiene historial
    /// (alertas o transacciones), se desactiva en lugar de eliminarse para no
    /// perder la trazabilidad de auditoría.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(EliminarEstacionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var estacion = await _dbContext.Estaciones.FindAsync([id], ct);
        if (estacion is null) return NotFound();

        var tieneAlertas = await _dbContext.Alertas.AnyAsync(a => a.EstacionId == id, ct);
        var tieneTransacciones = await _dbContext.TransaccionesStaging.AnyAsync(s => s.EstacionId == id, ct);

        if (tieneAlertas || tieneTransacciones)
        {
            estacion.Activa = false;
            await _dbContext.SaveChangesAsync(ct);

            await this.RegistrarAuditoriaAsync(_logService,
                "Desactivación de estación (con historial)", "Estacion", id,
                new { estacion.Codigo }, ct: ct);

            return Ok(new EliminarEstacionResponse(false, true,
                $"La estación {estacion.Codigo} tiene historial de alertas/transacciones; " +
                "se desactivó para conservar la trazabilidad."));
        }

        var watermark = await _dbContext.EstacionWatermarks
            .FirstOrDefaultAsync(w => w.EstacionId == id, ct);
        if (watermark is not null) _dbContext.EstacionWatermarks.Remove(watermark);

        _dbContext.Estaciones.Remove(estacion);
        await _dbContext.SaveChangesAsync(ct);

        await this.RegistrarAuditoriaAsync(_logService,
            "Eliminación de estación", "Estacion", id,
            new { estacion.Codigo }, ct: ct);

        return Ok(new EliminarEstacionResponse(true, false,
            $"Estación {estacion.Codigo} eliminada del sistema."));
    }
}
