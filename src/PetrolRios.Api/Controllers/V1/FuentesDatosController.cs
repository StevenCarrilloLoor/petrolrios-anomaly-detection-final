using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PetrolRios.Api.Extensions;
using PetrolRios.Application.DTOs.Fuentes;
using PetrolRios.Application.Fuentes;
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
            .Select(f => new FuenteDatosAgente(
                f.Id,
                f.Nombre,
                f.Tabla,
                f.ColumnaWatermark,
                f.UpdatedAt ?? f.CreatedAt))
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
            .ToListAsync(ct);

        var estados = await _dbContext.FuentesDatosEstadosEstacion
            .AsNoTracking()
            .Include(e => e.Estacion)
            .OrderBy(e => e.Estacion.Codigo)
            .ToListAsync(ct);

        var porFuente = estados
            .GroupBy(e => e.FuenteDatosId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<FuenteDatosEstacionEstado>)g.ToList());

        return Ok(fuentes.Select(f =>
            Map(f, porFuente.GetValueOrDefault(f.Id) ?? [])).ToList());
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

        var tabla = request.Tabla.Trim().ToUpperInvariant();
        if (await _dbContext.FuentesDatos.AnyAsync(f => f.Tabla.ToLower() == tabla.ToLower(), ct))
            return Conflict(new { mensaje = $"La tabla '{tabla}' ya está registrada." });

        var errorValidacion = await ValidarDefinicionAsync(tabla, request.ColumnaWatermark, ct);
        if (errorValidacion is not null)
            return BadRequest(new { mensaje = errorValidacion });

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

        if (string.IsNullOrWhiteSpace(request.Tabla) || string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(new { mensaje = "Nombre y tabla son obligatorios." });

        var tabla = request.Tabla.Trim().ToUpperInvariant();
        if (await _dbContext.FuentesDatos.AnyAsync(f => f.Id != id && f.Tabla.ToLower() == tabla.ToLower(), ct))
            return Conflict(new { mensaje = $"La tabla '{tabla}' ya está registrada en otra fuente." });

        var errorValidacion = await ValidarDefinicionAsync(tabla, request.ColumnaWatermark, ct);
        if (errorValidacion is not null)
            return BadRequest(new { mensaje = errorValidacion });

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

    /// <summary>
    /// Recibe el resultado de validación/extracción de cada fuente desde un Station Agent.
    /// El reporte se envía incluso cuando la tabla no existe o no produjo filas.
    /// </summary>
    [HttpPost("estado-agente")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReportarEstadoAgente(
        [FromBody] ReportarEstadoFuentesRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.CodigoEstacion))
            return BadRequest(new { mensaje = "El código de estación es obligatorio." });

        var codigo = request.CodigoEstacion.Trim().ToUpperInvariant();
        var estacion = await _dbContext.Estaciones
            .FirstOrDefaultAsync(e => e.Codigo == codigo, ct);
        if (estacion is null)
            return NotFound(new { mensaje = $"La estación '{codigo}' no está registrada." });

        var ids = request.Fuentes.Select(f => f.FuenteDatosId).Distinct().ToList();
        var fuentesExistentes = await _dbContext.FuentesDatos
            .Where(f => ids.Contains(f.Id))
            .Select(f => f.Id)
            .ToListAsync(ct);
        var idsValidos = fuentesExistentes.ToHashSet();

        var existentes = await _dbContext.FuentesDatosEstadosEstacion
            .Where(e => e.EstacionId == estacion.Id && ids.Contains(e.FuenteDatosId))
            .ToDictionaryAsync(e => e.FuenteDatosId, ct);

        foreach (var reporte in request.Fuentes.Where(r => idsValidos.Contains(r.FuenteDatosId)))
        {
            if (existentes.TryGetValue(reporte.FuenteDatosId, out var estado))
            {
                estado.Actualizar(
                    reporte.Estado,
                    reporte.TablaExiste,
                    reporte.ColumnaWatermarkValida,
                    reporte.FilasLeidas,
                    reporte.FilasEnviadas,
                    reporte.UltimoError,
                    reporte.VersionFuente);
            }
            else
            {
                await _dbContext.FuentesDatosEstadosEstacion.AddAsync(
                    FuenteDatosEstacionEstado.Create(
                        reporte.FuenteDatosId,
                        estacion.Id,
                        reporte.Estado,
                        reporte.TablaExiste,
                        reporte.ColumnaWatermarkValida,
                        reporte.FilasLeidas,
                        reporte.FilasEnviadas,
                        reporte.UltimoError,
                        reporte.VersionFuente),
                    ct);
            }
        }

        await _dbContext.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<string?> ValidarDefinicionAsync(
        string tabla,
        string? columnaWatermark,
        CancellationToken ct)
    {
        var esquema = await _dbContext.EsquemasTabla
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Tabla == tabla, ct);
        if (esquema is null)
            return $"La tabla '{tabla}' todavía no está documentada. Cargue primero el esquema de una estación.";

        if (string.IsNullOrWhiteSpace(columnaWatermark))
            return null;

        var columna = columnaWatermark.Trim().ToUpperInvariant();
        IReadOnlyList<ColumnaEsquema> columnas;
        try
        {
            columnas = JsonSerializer.Deserialize<List<ColumnaEsquema>>(
                esquema.ColumnasJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch (JsonException)
        {
            return $"El esquema guardado de '{tabla}' no es válido. Vuelva a solicitarlo a la estación.";
        }

        var encontrada = columnas.FirstOrDefault(c =>
            c.Nombre.Equals(columna, StringComparison.OrdinalIgnoreCase));
        if (encontrada is null)
            return $"La columna de marca de agua '{columna}' no existe en la tabla '{tabla}'.";
        if (!FuenteDatosPolicy.EsTipoTemporal(encontrada.Tipo))
            return $"La columna '{columna}' es de tipo {encontrada.Tipo}; la marca de agua debe ser DATE, TIME o TIMESTAMP.";

        return null;
    }

    private static FuenteDatosResponse Map(
        FuenteDatos f,
        IReadOnlyList<FuenteDatosEstacionEstado>? estados = null) => new()
    {
        Id = f.Id,
        Nombre = f.Nombre,
        Tabla = f.Tabla,
        ColumnaWatermark = f.ColumnaWatermark,
        Descripcion = f.Descripcion,
        Activa = f.Activa,
        Version = f.UpdatedAt ?? f.CreatedAt,
        Sincronizaciones = estados?.Select(e => new FuenteDatosEstacionEstadoResponse
        {
            EstacionId = e.EstacionId,
            EstacionCodigo = e.Estacion.Codigo,
            EstacionNombre = e.Estacion.Nombre,
            AgenteEnLinea = e.Estacion.UltimoHeartbeat.HasValue
                && e.Estacion.UltimoHeartbeat.Value >= DateTime.UtcNow.AddMinutes(-3),
            Estado = e.Estado,
            TablaExiste = e.TablaExiste,
            ColumnaWatermarkValida = e.ColumnaWatermarkValida,
            FilasLeidas = e.FilasLeidas,
            FilasEnviadas = e.FilasEnviadas,
            TotalFilasEnviadas = e.TotalFilasEnviadas,
            UltimoError = e.UltimoError,
            VersionFuente = e.VersionFuente,
            ConfiguracionActualizada = e.VersionFuente >= (f.UpdatedAt ?? f.CreatedAt),
            UltimoReporte = e.UltimoReporte,
            UltimoExito = e.UltimoExito
        }).ToList() ?? []
    };

    private sealed record ColumnaEsquema(string Nombre, string Tipo);
}
