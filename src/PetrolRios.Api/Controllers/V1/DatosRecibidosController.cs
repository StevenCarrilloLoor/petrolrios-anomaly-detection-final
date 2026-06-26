using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.DTOs.Common;
using PetrolRios.Application.DTOs.Logs;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Logs de DATOS CRUDOS recibidos de los agentes (tabla de staging), sin importar si generaron una
/// anomalía. Ordenados (lo más reciente primero), con filtro por tipo/estación/estado y búsqueda por
/// tipo. Permite confirmar que las tablas registradas en el selector realmente envían información.
/// Solo lectura, cuentas del central (excluye agentes).
/// </summary>
[ApiController]
[Route("api/v1/datos-recibidos")]
[Authorize(Roles = "Auditor,Supervisor,Administrador", Policy = "Central")]
public sealed class DatosRecibidosController : ControllerBase
{
    private readonly PetrolRiosDbContext _dbContext;

    public DatosRecibidosController(PetrolRiosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Lista paginada de lo recibido, con filtros y búsqueda (por tipo de transacción).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<DatoRecibidoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? tipo,
        [FromQuery] int? estacionId,
        [FromQuery] bool? procesada,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 50 : pageSize;

        var query = _dbContext.TransaccionesStaging.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(tipo))
            query = query.Where(s => s.TipoTransaccion == tipo);
        if (estacionId is > 0)
            query = query.Where(s => s.EstacionId == estacionId);
        if (procesada is not null)
            query = query.Where(s => s.Procesada == procesada);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var patron = $"%{q.Trim()}%";
            query = query.Where(s => EF.Functions.ILike(s.TipoTransaccion, patron));
        }

        var total = await query.CountAsync(ct);

        var rows = await query
            .OrderByDescending(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.TipoTransaccion,
                s.EstacionId,
                s.FechaOriginal,
                s.Procesada,
                s.DataJson,
                s.CreatedAt
            })
            .ToListAsync(ct);

        var estacionIds = rows.Select(r => r.EstacionId).Distinct().ToList();
        var estaciones = await _dbContext.Estaciones
            .AsNoTracking()
            .Where(e => estacionIds.Contains(e.Id))
            .Select(e => new { e.Id, e.Codigo, e.Nombre })
            .ToDictionaryAsync(e => e.Id, e => (e.Codigo, e.Nombre), ct);

        var items = rows.Select(r =>
        {
            var est = estaciones.GetValueOrDefault(r.EstacionId);
            return new DatoRecibidoResponse
            {
                Id = r.Id,
                TipoTransaccion = r.TipoTransaccion,
                EstacionId = r.EstacionId,
                EstacionCodigo = est.Codigo ?? "",
                EstacionNombre = est.Nombre ?? "",
                FechaOriginal = r.FechaOriginal,
                Procesada = r.Procesada,
                DataJson = r.DataJson,
                CreatedAt = r.CreatedAt
            };
        }).ToList();

        return Ok(new PaginatedResponse<DatoRecibidoResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        });
    }

    /// <summary>
    /// Tipos de transacción distintos recibidos (para el desplegable del filtro). Así se ve de un
    /// vistazo qué tablas/fuentes están llegando realmente al central.
    /// </summary>
    [HttpGet("tipos")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTipos(CancellationToken ct)
    {
        var tipos = await _dbContext.TransaccionesStaging
            .AsNoTracking()
            .Select(s => s.TipoTransaccion)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync(ct);
        return Ok(tipos);
    }
}
