using PetrolRios.Domain.Enums;

namespace PetrolRios.Domain.Entities;

/// <summary>
/// Bitácora de auditoría de cada intento de actualización de precios (scraping, refresco manual o
/// corrección de admin). Registro de solo escritura: permite ver el historial, depurar el scraping y
/// detectar patrones (fuentes que fallan, valores fuera de rango). <c>CreatedAt</c> (de BaseEntity) es
/// el instante del evento.
/// </summary>
public class PrecioCombustibleLog : BaseEntity
{
    public TipoCombustible Producto { get; private set; }
    public decimal? PrecioAnterior { get; private set; }
    public decimal? PrecioNuevo { get; private set; }
    public decimal? VariacionPorcentual { get; private set; }

    /// <summary>Origen del dato: arch | camddepe | gasolinaecuador | primicias | admin_manual |
    /// sin_cambio_oficial | fallback | sistema.</summary>
    public string Fuente { get; private set; } = string.Empty;

    /// <summary>Disparo que originó el evento: modo_normal_08h | modo_alerta_horario | manual.</summary>
    public string Disparo { get; private set; } = string.Empty;

    /// <summary>Resultado: actualizado | sin_cambio | error | cache_304 | invalido | pendiente.</summary>
    public string Resultado { get; private set; } = string.Empty;

    public bool PrecioPendiente { get; private set; }

    /// <summary>Fuentes saltadas por estar degradadas (bloqueo previo), separadas por coma.</summary>
    public string? FuenteDegradada { get; private set; }

    /// <summary>Id del administrador, si fue corrección manual.</summary>
    public int? AdminId { get; private set; }

    public string? EtagRecibido { get; private set; }

    /// <summary>Hash del HTML parseado (depuración: detectar si la página cambió de formato).</summary>
    public string? RawHtmlHash { get; private set; }

    /// <summary>Segundos que el job durmió (jitter) antes del request.</summary>
    public int? JitterSegundos { get; private set; }

    public static PrecioCombustibleLog Create(
        TipoCombustible producto,
        string fuente,
        string disparo,
        string resultado,
        decimal? precioAnterior = null,
        decimal? precioNuevo = null,
        decimal? variacionPorcentual = null,
        bool precioPendiente = false,
        string? fuenteDegradada = null,
        int? adminId = null,
        string? etagRecibido = null,
        string? rawHtmlHash = null,
        int? jitterSegundos = null) =>
        new()
        {
            Producto = producto,
            Fuente = (fuente ?? string.Empty).Trim(),
            Disparo = (disparo ?? string.Empty).Trim(),
            Resultado = (resultado ?? string.Empty).Trim(),
            PrecioAnterior = precioAnterior,
            PrecioNuevo = precioNuevo,
            VariacionPorcentual = variacionPorcentual,
            PrecioPendiente = precioPendiente,
            FuenteDegradada = fuenteDegradada,
            AdminId = adminId,
            EtagRecibido = etagRecibido,
            RawHtmlHash = rawHtmlHash,
            JitterSegundos = jitterSegundos
        };
}
