using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Api.Extensions;
using PetrolRios.Application.DTOs.Estaciones;
using PetrolRios.Application.DTOs.Usuarios;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
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
    private readonly IUsuarioService _usuarioService;

    public EstacionesController(
        PetrolRiosDbContext dbContext, ILogService logService, IUsuarioService usuarioService)
    {
        _dbContext = dbContext;
        _logService = logService;
        _usuarioService = usuarioService;
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

    /// <summary>
    /// Alta de una estación nueva CON su usuario-agente. El sistema NO está limitado a 10 estaciones;
    /// esto permite escalar a más. Crea la estación y su cuenta de servicio (rol Agente, ligada a la
    /// estación) y devuelve sus credenciales UNA sola vez para configurar el agente de esa estación.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ProvisionarEstacionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearEstacionRequest request, CancellationToken ct)
    {
        var codigo = (request.Codigo ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(codigo))
            return BadRequest(new { mensaje = "El código de estación es obligatorio." });
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(new { mensaje = "El nombre de la estación es obligatorio." });
        if (await _dbContext.Estaciones.AnyAsync(e => e.Codigo == codigo, ct))
            return Conflict(new { mensaje = $"Ya existe una estación con código '{codigo}'." });

        // 1) Crear la estación.
        var estacion = Estacion.Create(
            request.Nombre.Trim(),
            codigo,
            string.IsNullOrWhiteSpace(request.Direccion) ? "(pendiente de completar)" : request.Direccion!.Trim(),
            string.IsNullOrWhiteSpace(request.Zona) ? null : request.Zona!.Trim());
        await _dbContext.Estaciones.AddAsync(estacion, ct);
        await _dbContext.SaveChangesAsync(ct);

        // 2) Crear su usuario-agente (rol Agente — SIN acceso a la app central) reutilizando UsuarioService.
        var email = $"agent-{codigo.ToLowerInvariant()}@petrolrios.com";
        var password = string.IsNullOrWhiteSpace(request.PasswordAgente)
            ? GenerarPassword()
            : request.PasswordAgente!.Trim();
        var agenteRolId = await _dbContext.Roles
            .Where(r => r.Nombre == "Agente").Select(r => r.Id).FirstOrDefaultAsync(ct);

        var agenteCreado = false;
        if (agenteRolId > 0 && !await _dbContext.Usuarios.AnyAsync(u => u.Email == email, ct))
        {
            await _usuarioService.CreateAsync(
                new CrearUsuarioRequest(email, $"Agente Estacion {codigo}", password, agenteRolId, estacion.Id), ct);
            agenteCreado = true;
        }

        await this.RegistrarAuditoriaAsync(_logService,
            "Creación de estación", "Estacion", estacion.Id,
            new { estacion.Codigo, estacion.Nombre, AgenteEmail = email }, ct: ct);

        return CreatedAtAction(nameof(GetAll), new ProvisionarEstacionResponse(
            MapToResponse(estacion), email, agenteCreado ? password : null, agenteCreado));
    }

    /// <summary>Contraseña aleatoria legible para el usuario-agente (se muestra una sola vez).</summary>
    private static string GenerarPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var bytes = new byte[12];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        var sb = new System.Text.StringBuilder("Ag-");
        foreach (var b in bytes) sb.Append(chars[b % chars.Length]);
        return sb.ToString();
    }

    private static EstacionResponse MapToResponse(Estacion e) => new()
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
    };

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
