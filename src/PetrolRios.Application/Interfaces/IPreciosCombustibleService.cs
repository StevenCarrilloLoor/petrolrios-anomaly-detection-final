using PetrolRios.Application.DTOs.Combustible;

namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Sirve los precios oficiales vigentes de los combustibles regulados de Ecuador (Extra, Ecopaís,
/// Diésel). La fuente de verdad es la tabla del central, sembrada con los valores oficiales y
/// actualizable por un administrador cada vez que cambia la banda mensual. Si se configura una fuente
/// externa, también puede refrescarlos automáticamente (con respaldo a los valores guardados).
/// </summary>
public interface IPreciosCombustibleService
{
    /// <summary>Precios vigentes hoy (o el último conocido por producto), con metadatos.</summary>
    Task<PreciosCombustibleResponse> ObtenerVigentesAsync(CancellationToken ct = default);

    /// <summary>Actualiza (upsert) el precio de un combustible y devuelve todos los vigentes. Lo usa el
    /// administrador al cambiar la banda mensual.</summary>
    Task<PreciosCombustibleResponse> ActualizarAsync(ActualizarPrecioCombustibleRequest req, CancellationToken ct = default);

    /// <summary>
    /// Intenta refrescar los precios desde la fuente externa configurada (si hay). Si la fuente
    /// responde, hace upsert y devuelve los nuevos; si no hay fuente o falla, devuelve los vigentes
    /// guardados sin error (degradación elegante).
    /// </summary>
    Task<PreciosCombustibleResponse> RefrescarDesdeFuenteAsync(CancellationToken ct = default);
}

/// <summary>
/// Conector a una fuente EXTERNA de precios (p. ej. una API/endpoint que devuelva los precios oficiales
/// de Ecuador). Implementación por defecto: HTTP a una URL configurable; si no hay URL, está deshabilitado
/// y devuelve null (el servicio usa los valores guardados). Mantiene el sistema robusto: no depende de
/// una fuente frágil, pero puede "interrogar" una cuando exista.
/// </summary>
public interface IProveedorPreciosExterno
{
    /// <summary>True si hay una fuente externa configurada.</summary>
    bool Habilitado { get; }

    /// <summary>Obtiene los precios de la fuente externa, o null si está deshabilitada o falla.</summary>
    Task<IReadOnlyList<PrecioCombustibleExterno>?> ObtenerAsync(CancellationToken ct = default);
}
