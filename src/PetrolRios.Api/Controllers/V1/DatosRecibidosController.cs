using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.DTOs.Common;
using PetrolRios.Application.DTOs.Logs;
using PetrolRios.Application.Fuentes;
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

        // Tabla técnica de las fuentes configurables del selector (built-ins se resuelven solos).
        var tablaPorFuente = await TablaPorFuenteAsync(ct);

        var items = rows.Select(r =>
        {
            var est = estaciones.GetValueOrDefault(r.EstacionId);
            var (natural, tabla) = CatalogoTiposTransaccion.Resolver(
                r.TipoTransaccion, tablaPorFuente.GetValueOrDefault(r.TipoTransaccion));
            return new DatoRecibidoResponse
            {
                Id = r.Id,
                TipoTransaccion = r.TipoTransaccion,
                TipoNatural = natural,
                Tabla = tabla,
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
    [ProducesResponseType(typeof(IReadOnlyList<TipoRecibidoOption>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTipos(CancellationToken ct)
    {
        var tipos = await _dbContext.TransaccionesStaging
            .AsNoTracking()
            .Select(s => s.TipoTransaccion)
            .Distinct()
            .ToListAsync(ct);

        var tablaPorFuente = await TablaPorFuenteAsync(ct);

        var opciones = tipos
            .Select(t => new TipoRecibidoOption(
                t, CatalogoTiposTransaccion.Etiqueta(t, tablaPorFuente.GetValueOrDefault(t))))
            .OrderBy(o => o.Etiqueta, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        return Ok(opciones);
    }

    /// <summary>
    /// Mapa Nombre-de-fuente → Tabla técnica del catálogo del selector, para resolver la tabla de las
    /// fuentes configurables (las built-in se resuelven con <see cref="CatalogoTiposTransaccion"/>).
    /// </summary>
    private async Task<Dictionary<string, string>> TablaPorFuenteAsync(CancellationToken ct)
    {
        var fuentes = await _dbContext.FuentesDatos
            .AsNoTracking()
            .Select(f => new { f.Nombre, f.Tabla })
            .ToListAsync(ct);
        return fuentes
            .GroupBy(f => f.Nombre, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Tabla, StringComparer.OrdinalIgnoreCase);
    }
}
