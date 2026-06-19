using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.DTOs.Esquema;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Catálogo de esquemas de Firebird: los agentes reportan aquí qué tablas y columnas existen en su
/// base. El central lo usa para documentar fuentes automáticamente y para ofrecer un navegador de
/// tablas con búsqueda (sin que el usuario tenga que conocer los nombres de las tablas).
/// </summary>
[ApiController]
[Route("api/v1/esquema")]
[Authorize]
public sealed class EsquemaController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly PetrolRiosDbContext _dbContext;
    private readonly PetrolRios.Application.Interfaces.ISolicitudesEsquema _solicitudes;

    public EsquemaController(
        PetrolRiosDbContext dbContext,
        PetrolRios.Application.Interfaces.ISolicitudesEsquema solicitudes)
    {
        _dbContext = dbContext;
        _solicitudes = solicitudes;
    }

    /// <summary>
    /// Solicita que una estación conectada cargue/reporte su esquema. En su próximo heartbeat
    /// (segundos) el agente recibirá la señal y enviará sus tablas y columnas al central.
    /// </summary>
    [HttpPost("solicitar/{codigoEstacion}")]
    [Authorize(Roles = "Supervisor,Administrador")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult Solicitar(string codigoEstacion)
    {
        _solicitudes.Solicitar(codigoEstacion);
        return Accepted(new { mensaje = $"Se pedirá el esquema a {codigoEstacion} en su próximo latido." });
    }

    /// <summary>El agente reporta el esquema de su Firebird (upsert por nombre de tabla).</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reportar([FromBody] ReportarEsquemaRequest request, CancellationToken ct)
    {
        if (request.Tablas is null || request.Tablas.Count == 0)
            return Ok(new { recibidas = 0 });

        var nombres = request.Tablas
            .Select(t => t.Tabla.Trim().ToUpperInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existentes = await _dbContext.EsquemasTabla
            .Where(e => nombres.Contains(e.Tabla))
            .ToDictionaryAsync(e => e.Tabla, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var tabla in request.Tablas)
        {
            var nombre = tabla.Tabla.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(nombre)) continue;
            var columnasJson = JsonSerializer.Serialize(tabla.Columnas);

            if (existentes.TryGetValue(nombre, out var fila))
            {
                fila.ColumnasJson = columnasJson;
                fila.EstacionCodigo = request.CodigoEstacion;
                fila.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                await _dbContext.EsquemasTabla.AddAsync(
                    EsquemaTabla.Create(nombre, columnasJson, request.CodigoEstacion), ct);
            }
        }

        await _dbContext.SaveChangesAsync(ct);
        return Ok(new { recibidas = request.Tablas.Count });
    }

    /// <summary>Buscar tablas por nombre (navegador del central). Devuelve nombre + nº de columnas.</summary>
    [HttpGet("tablas")]
    [Authorize(Roles = "Supervisor,Administrador")]
    [ProducesResponseType(typeof(IReadOnlyList<TablaResumen>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar([FromQuery] string? buscar, CancellationToken ct)
    {
        var query = _dbContext.EsquemasTabla.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var b = buscar.Trim().ToUpperInvariant();
            query = query.Where(e => e.Tabla.Contains(b));
        }

        var filas = await query.OrderBy(e => e.Tabla).Take(100).ToListAsync(ct);
        var resumen = filas.Select(e => new TablaResumen(e.Tabla, ContarColumnas(e.ColumnasJson))).ToList();
        return Ok(resumen);
    }

    /// <summary>Columnas de una tabla (documentación automática para el central).</summary>
    [HttpGet("tabla/{nombre}")]
    [Authorize(Roles = "Supervisor,Administrador")]
    [ProducesResponseType(typeof(TablaDetalle), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Detalle(string nombre, CancellationToken ct)
    {
        var t = nombre.Trim().ToUpperInvariant();
        var fila = await _dbContext.EsquemasTabla.AsNoTracking().FirstOrDefaultAsync(e => e.Tabla == t, ct);
        if (fila is null) return NotFound();

        var columnas = JsonSerializer.Deserialize<List<ColumnaEsquema>>(fila.ColumnasJson, JsonOpts) ?? [];
        return Ok(new TablaDetalle(fila.Tabla, columnas, fila.EstacionCodigo, fila.UpdatedAt ?? fila.CreatedAt));
    }

    private static int ContarColumnas(string columnasJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<ColumnaEsquema>>(columnasJson, JsonOpts)?.Count ?? 0;
        }
        catch
        {
            return 0;
        }
    }
}
