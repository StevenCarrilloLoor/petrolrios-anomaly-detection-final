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
            throw new ArgumentException($"Producto no válido: '{req.Producto}'. Use Extra, Ecopais, Diesel o Super.");
        if (req.PrecioGalon <= 0)
            throw new ArgumentException("El precio por galón debe ser mayor a 0.");
        if (!RangoValido(producto, req.PrecioGalon))
            throw new ArgumentException($"El precio ${req.PrecioGalon} está fuera del rango plausible para {producto}.");
        if (req.VigenteHasta is not null && req.VigenteHasta < req.VigenteDesde)
            throw new ArgumentException("La vigencia 'hasta' no puede ser anterior a 'desde'.");

        var fuente = string.IsNullOrWhiteSpace(req.Fuente)
            ? "Admin"
            : req.Fuente!.Trim();

        var fila = await _db.PreciosCombustible.FirstOrDefaultAsync(p => p.Producto == producto, ct);
        var precioAnterior = fila?.PrecioGalon;
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

        // Bitácora de auditoría del cambio manual.
        var variacion = precioAnterior is null or 0
            ? (decimal?)null
            : Math.Round((req.PrecioGalon - precioAnterior.Value) / precioAnterior.Value * 100m, 2);
        await _db.PreciosCombustibleLog.AddAsync(PrecioCombustibleLog.Create(
            producto, "admin_manual", "manual", "actualizado",
            precioAnterior: precioAnterior, precioNuevo: req.PrecioGalon, variacionPorcentual: variacion), ct);

        await _db.SaveChangesAsync(ct);
        return await ObtenerVigentesAsync(ct);
    }

    public async Task<PreciosCombustibleResponse> RefrescarDesdeFuenteAsync(
        string disparo = "manual", CancellationToken ct = default)
    {
        if (!_externo.Habilitado) return await ObtenerVigentesAsync(ct);

        IReadOnlyList<PrecioCombustibleExterno>? externos = null;
        try { externos = await _externo.ObtenerAsync(ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "La cascada de precios falló."); }

        var fuente = _externo.UltimaFuente ?? "fuente";
        var degradadas = _externo.FuentesDegradadas.Count > 0
            ? string.Join(",", _externo.FuentesDegradadas) : null;

        if (externos is { Count: > 0 })
        {
            foreach (var e in externos)
            {
                if (!Enum.TryParse<TipoCombustible>(e.Producto, ignoreCase: true, out var producto)
                    || !Enum.IsDefined(producto))
                    continue;

                var fila = await _db.PreciosCombustible.FirstOrDefaultAsync(p => p.Producto == producto, ct);
                var anterior = fila?.PrecioGalon;
                var enRango = RangoValido(producto, e.PrecioGalon);
                var plausible = VariacionPlausible(producto, anterior, e.PrecioGalon);
                var promover = enRango && plausible;     // solo se asciende al SISTEMA un valor válido
                var variacion = anterior is null or 0
                    ? (decimal?)null
                    : Math.Round((e.PrecioGalon - anterior.Value) / anterior.Value * 100m, 2);
                var resultado = !enRango || !plausible ? "invalido"
                    : anterior == e.PrecioGalon ? "sin_cambio" : "actualizado";

                if (fila is null)
                {
                    fila = PrecioCombustible.Create(producto, e.PrecioGalon, 0m, e.VigenteDesde, e.VigenteHasta, fuente);
                    fila.RegistrarApi(e.PrecioGalon, fuente, DateTime.UtcNow, promover);
                    await _db.PreciosCombustible.AddAsync(fila, ct);
                }
                else
                {
                    fila.RegistrarApi(e.PrecioGalon, fuente, DateTime.UtcNow, promover);
                }

                await _db.PreciosCombustibleLog.AddAsync(PrecioCombustibleLog.Create(
                    producto, fuente, disparo, resultado,
                    precioAnterior: anterior, precioNuevo: e.PrecioGalon, variacionPorcentual: variacion,
                    fuenteDegradada: degradadas), ct);
            }
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Precios refrescados desde '{Fuente}' ({Disparo}).", fuente, disparo);
        }
        else
        {
            // Todas las fuentes fallaron: se conservan los precios del sistema; se registra el intento.
            await _db.PreciosCombustibleLog.AddAsync(PrecioCombustibleLog.Create(
                TipoCombustible.Extra, "fallback", disparo, "error", fuenteDegradada: degradadas), ct);
            await _db.SaveChangesAsync(ct);
            _logger.LogWarning("Ninguna fuente devolvió precios ({Disparo}); se conservan los del sistema.", disparo);
        }

        return await ObtenerVigentesAsync(ct);
    }

    public async Task<SaludPreciosResponse> ObtenerSaludAsync(CancellationToken ct = default)
    {
        var modo = Precios.PlanificadorPrecios
            .Modo(Precios.PlanificadorPrecios.AhoraEcuador(DateTime.UtcNow)).ToString();
        var degradadas = _externo.FuentesDegradadas;

        var ultimoOk = await _db.PreciosCombustibleLog.AsNoTracking()
            .Where(l => l.Resultado == "actualizado")
            .OrderByDescending(l => l.CreatedAt).FirstOrDefaultAsync(ct);
        var ultimoErr = await _db.PreciosCombustibleLog.AsNoTracking()
            .Where(l => l.Resultado == "error")
            .OrderByDescending(l => l.CreatedAt).FirstOrDefaultAsync(ct);

        var errorReciente = ultimoErr is not null && ultimoErr.CreatedAt >= DateTime.UtcNow.AddHours(-2);
        var sinActualizar = ultimoOk is null ? (TimeSpan?)null : DateTime.UtcNow - ultimoOk.CreatedAt;

        var estado = "OK";
        string? detalle = null;
        if (errorReciente && modo == nameof(Precios.ModoScrapePrecios.Alerta))
        { estado = "Critico"; detalle = "Falló el scraping durante la ventana de cambio de precio."; }
        else if (errorReciente)
        { estado = "Error"; detalle = "El último intento de scraping falló; se reintentará."; }
        else if (sinActualizar is { TotalDays: > 40 })
        { estado = "Urgente"; detalle = "Sin actualización de precio en más de 40 días; conviene revisar."; }
        else if (degradadas.Count > 0)
        { estado = "Warning"; detalle = $"Fuente(s) degradada(s): {string.Join(", ", degradadas)}."; }

        return new SaludPreciosResponse(
            modo, estado, detalle,
            ultimoOk?.CreatedAt,
            ultimoOk?.Fuente ?? _externo.UltimaFuente,
            ultimoErr is not null ? $"{ultimoErr.CreatedAt:yyyy-MM-dd HH:mm} UTC ({ultimoErr.Fuente})" : null,
            degradadas);
    }

    public async Task<IReadOnlyList<HistorialPrecioItem>> ObtenerHistorialAsync(
        int meses = 12, CancellationToken ct = default)
    {
        var desde = DateTime.UtcNow.AddMonths(-Math.Clamp(meses, 1, 60));
        var filas = await _db.PreciosCombustibleLog.AsNoTracking()
            .Where(l => l.CreatedAt >= desde)
            .OrderByDescending(l => l.CreatedAt)
            .Take(500)
            .ToListAsync(ct);
        return filas.Select(l => new HistorialPrecioItem(
            l.CreatedAt, l.Producto.ToString(), l.PrecioAnterior, l.PrecioNuevo,
            l.VariacionPorcentual, l.Fuente, l.Disparo, l.Resultado)).ToList();
    }

    private PreciosCombustibleResponse Construir(IReadOnlyList<PrecioCombustible> filas)
    {
        var precios = filas
            .OrderBy(f => (int)f.Producto)
            .Select(ToItem)
            .ToList();
        return new PreciosCombustibleResponse(
            precios, "USD", DateTime.UtcNow, Nota, _externo.FuentesDegradadas);
    }

    private static PrecioCombustibleResponse ToItem(PrecioCombustible f) =>
        new(
            Producto: f.Producto.ToString(),
            Nombre: NombreDe(f.Producto),
            EsRegulado: f.Producto.EsRegulado(),
            PrecioGalon: f.PrecioGalon,
            PrecioApi: f.PrecioApi,
            FuenteApi: string.IsNullOrWhiteSpace(f.FuenteApi) ? null : f.FuenteApi,
            ApiActualizadoEn: f.PrecioApiActualizadoEn,
            Subsidio: f.Subsidio,
            PrecioPendiente: f.PrecioPendiente,
            VigenteDesde: f.VigenteDesde,
            VigenteHasta: f.VigenteHasta,
            Fuente: f.Fuente);

    private static string NombreDe(TipoCombustible t) => t switch
    {
        TipoCombustible.Extra => "Gasolina Extra",
        TipoCombustible.Ecopais => "Gasolina Ecopaís",
        TipoCombustible.Diesel => "Diésel",
        TipoCombustible.Super => "Gasolina Súper",
        _ => t.ToString()
    };

    // ── Validación de precios (sistema de bandas) ───────────────────────────────────────────────────
    // Rango absoluto plausible por producto y variación máxima vs el valor anterior (regulados ±10% por
    // la banda; Súper ±40% por ser libre mercado). Lo usa el admin (rango) y, más adelante, el scraper.

    /// <summary>True si el precio está dentro del rango absoluto plausible del producto.</summary>
    public static bool RangoValido(TipoCombustible producto, decimal precio) => producto switch
    {
        TipoCombustible.Super => precio is >= 2.00m and <= 10.00m,
        _ => precio is >= 1.50m and <= 6.00m
    };

    /// <summary>True si la variación respecto al precio anterior es plausible (banda). Sin anterior → true.</summary>
    public static bool VariacionPlausible(TipoCombustible producto, decimal? anterior, decimal nuevo)
    {
        if (anterior is null or 0) return true;
        var max = producto == TipoCombustible.Super ? 0.40m : 0.10m;
        var variacion = Math.Abs(nuevo - anterior.Value) / anterior.Value;
        return variacion <= max;
    }
}
