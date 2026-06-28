using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetrolRios.Application.DTOs.Combustible;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

/// <summary>
/// Sirve y mantiene los precios oficiales de los combustibles regulados de Ecuador. La fuente de verdad
/// es la tabla <c>precios_combustible</c> (sembrada con los valores oficiales). Un administrador los
/// actualiza cuando cambia la banda mensual; si hay una fuente externa configurada, también se pueden
/// refrescar automáticamente con respaldo a los valores guardados.
/// </summary>
public sealed class PreciosCombustibleService : IPreciosCombustibleService
{
    private readonly PetrolRiosDbContext _db;
    private readonly IProveedorPreciosExterno _externo;
    private readonly ILogger<PreciosCombustibleService> _logger;

    private const string Nota =
        "Precios oficiales regulados por EP Petroecuador mediante el sistema de bandas (vigentes del 12 de " +
        "cada mes al 11 del siguiente, con ajuste máximo de +5% / −10% mensual). La gasolina Súper no se " +
        "incluye porque su precio no está regulado y varía por comercializadora y estación.";

    public PreciosCombustibleService(
        PetrolRiosDbContext db,
        IProveedorPreciosExterno externo,
        ILogger<PreciosCombustibleService> logger)
    {
        _db = db;
        _externo = externo;
        _logger = logger;
    }

    public async Task<PreciosCombustibleResponse> ObtenerVigentesAsync(CancellationToken ct = default)
    {
        var filas = await _db.PreciosCombustible.AsNoTracking().ToListAsync(ct);
        return Construir(filas);
    }

    public async Task<PreciosCombustibleResponse> ActualizarAsync(
        ActualizarPrecioCombustibleRequest req, CancellationToken ct = default)
    {
        if (!Enum.TryParse<TipoCombustible>(req.Producto, ignoreCase: true, out var producto)
            || !Enum.IsDefined(producto))
            throw new ArgumentException($"Producto no válido: '{req.Producto}'. Use Extra, Ecopais o Diesel.");
        if (req.PrecioGalon <= 0)
            throw new ArgumentException("El precio por galón debe ser mayor a 0.");
        if (req.VigenteHasta is not null && req.VigenteHasta < req.VigenteDesde)
            throw new ArgumentException("La vigencia 'hasta' no puede ser anterior a 'desde'.");

        var fuente = string.IsNullOrWhiteSpace(req.Fuente)
            ? "Actualización manual (administrador)"
            : req.Fuente!.Trim();

        var fila = await _db.PreciosCombustible.FirstOrDefaultAsync(p => p.Producto == producto, ct);
        if (fila is null)
        {
            fila = PrecioCombustible.Create(
                producto, req.PrecioGalon, req.Subsidio, req.VigenteDesde, req.VigenteHasta, fuente);
            await _db.PreciosCombustible.AddAsync(fila, ct);
        }
        else
        {
            fila.Actualizar(req.PrecioGalon, req.Subsidio, req.VigenteDesde, req.VigenteHasta, fuente);
        }
        await _db.SaveChangesAsync(ct);
        return await ObtenerVigentesAsync(ct);
    }

    public async Task<PreciosCombustibleResponse> RefrescarDesdeFuenteAsync(CancellationToken ct = default)
    {
        if (_externo.Habilitado)
        {
            try
            {
                var externos = await _externo.ObtenerAsync(ct);
                if (externos is { Count: > 0 })
                {
                    foreach (var e in externos)
                    {
                        if (!Enum.TryParse<TipoCombustible>(e.Producto, ignoreCase: true, out var producto)
                            || !Enum.IsDefined(producto))
                            continue;
                        var fila = await _db.PreciosCombustible.FirstOrDefaultAsync(p => p.Producto == producto, ct);
                        if (fila is null)
                            await _db.PreciosCombustible.AddAsync(PrecioCombustible.Create(
                                producto, e.PrecioGalon, e.Subsidio, e.VigenteDesde, e.VigenteHasta,
                                "Fuente externa configurada"), ct);
                        else
                            fila.Actualizar(e.PrecioGalon, e.Subsidio, e.VigenteDesde, e.VigenteHasta,
                                "Fuente externa configurada");
                    }
                    await _db.SaveChangesAsync(ct);
                    _logger.LogInformation("Precios de combustible refrescados desde la fuente externa.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "No se pudo refrescar precios desde la fuente externa; se mantienen los valores guardados.");
            }
        }
        return await ObtenerVigentesAsync(ct);
    }

    private PreciosCombustibleResponse Construir(IReadOnlyList<PrecioCombustible> filas)
    {
        var precios = filas
            .OrderBy(f => (int)f.Producto)
            .Select(ToItem)
            .ToList();
        return new PreciosCombustibleResponse(precios, "USD", DateTime.UtcNow, Nota);
    }

    private static PrecioCombustibleResponse ToItem(PrecioCombustible f) =>
        new(
            Producto: f.Producto.ToString(),
            Nombre: NombreDe(f.Producto),
            PrecioGalon: f.PrecioGalon,
            Subsidio: f.Subsidio,
            VigenteDesde: f.VigenteDesde,
            VigenteHasta: f.VigenteHasta,
            Fuente: f.Fuente);

    private static string NombreDe(TipoCombustible t) => t switch
    {
        TipoCombustible.Extra => "Gasolina Extra",
        TipoCombustible.Ecopais => "Gasolina Ecopaís",
        TipoCombustible.Diesel => "Diésel",
        _ => t.ToString()
    };
}
