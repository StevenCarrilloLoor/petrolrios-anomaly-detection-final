using PetrolRios.Domain.Enums;

namespace PetrolRios.Domain.Entities;

/// <summary>
/// Precio oficial vigente de un combustible regulado en Ecuador (Extra, Ecopaís, Diésel). Los precios
/// los fija mensualmente EP Petroecuador mediante el sistema de bandas (vigencia del 12 de cada mes al
/// 11 del siguiente). Se guardan con su período de vigencia, subsidio y fuente para mostrarlos en el
/// dashboard y poder usarlos como "precio autorizado" de referencia. Se actualizan vía API (admin) o,
/// si se configura una fuente externa, por un conector que los refresca automáticamente.
/// </summary>
public class PrecioCombustible : BaseEntity
{
    public TipoCombustible Producto { get; private set; }

    /// <summary>Precio de venta al público por galón, en USD.</summary>
    public decimal PrecioGalon { get; private set; }

    /// <summary>Subsidio estatal por galón, en USD (informativo; 0 si no aplica/desconocido).</summary>
    public decimal Subsidio { get; private set; }

    /// <summary>Inicio de la vigencia oficial del precio (normalmente el 12 del mes).</summary>
    public DateTime VigenteDesde { get; private set; }

    /// <summary>Fin de la vigencia (normalmente el 11 del mes siguiente). Null = vigente sin fecha de corte.</summary>
    public DateTime? VigenteHasta { get; private set; }

    /// <summary>Origen del precio del SISTEMA (p. ej. "EP Petroecuador — sistema de bandas", "Admin").</summary>
    public string Fuente { get; private set; } = string.Empty;

    // ── Precio observado por la API/scraper (para comparar Sistema vs API en el dashboard) ──────────
    // PrecioGalon es el precio del SISTEMA: el efectivo, el que sirve la API y usan los detectores. Si el
    // scraping trae un precio válido se "promueve" a PrecioGalon; PrecioApi guarda SIEMPRE el último valor
    // crudo observado (aunque no se promueva), para ver lado a lado lo que registra la API y el sistema.

    /// <summary>Último precio observado por el scraper, o null si aún no se ha consultado.</summary>
    public decimal? PrecioApi { get; private set; }

    /// <summary>Cuándo se observó <see cref="PrecioApi"/> por última vez.</summary>
    public DateTime? PrecioApiActualizadoEn { get; private set; }

    /// <summary>Fuente del último <see cref="PrecioApi"/> (arch, camddepe, gasolinaecuador, primicias…).</summary>
    public string FuenteApi { get; private set; } = string.Empty;

    /// <summary>Precio pendiente de confirmar (solo Súper, libre mercado: puede llegar horas después).</summary>
    public bool PrecioPendiente { get; private set; }

    public static PrecioCombustible Create(
        TipoCombustible producto, decimal precioGalon, decimal subsidio,
        DateTime vigenteDesde, DateTime? vigenteHasta, string fuente) =>
        new()
        {
            Producto = producto,
            PrecioGalon = precioGalon,
            Subsidio = subsidio,
            VigenteDesde = DateTime.SpecifyKind(vigenteDesde, DateTimeKind.Utc),
            VigenteHasta = vigenteHasta is null ? null : DateTime.SpecifyKind(vigenteHasta.Value, DateTimeKind.Utc),
            Fuente = (fuente ?? string.Empty).Trim()
        };

    /// <summary>Actualiza el precio vigente del SISTEMA (nueva banda mensual o corrección). Marca UpdatedAt.</summary>
    public void Actualizar(decimal precioGalon, decimal subsidio, DateTime vigenteDesde, DateTime? vigenteHasta, string fuente)
    {
        PrecioGalon = precioGalon;
        Subsidio = subsidio;
        VigenteDesde = DateTime.SpecifyKind(vigenteDesde, DateTimeKind.Utc);
        VigenteHasta = vigenteHasta is null ? null : DateTime.SpecifyKind(vigenteHasta.Value, DateTimeKind.Utc);
        Fuente = (fuente ?? string.Empty).Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Registra el precio observado por el scraper. Siempre guarda el valor crudo (para comparar). Si
    /// <paramref name="promover"/> es true (precio válido dentro de banda), también lo asciende a precio del
    /// SISTEMA (efectivo). Limpia <see cref="PrecioPendiente"/> al promover.
    /// </summary>
    public void RegistrarApi(decimal precioApi, string fuenteApi, DateTime cuando, bool promover)
    {
        PrecioApi = precioApi;
        FuenteApi = (fuenteApi ?? string.Empty).Trim();
        PrecioApiActualizadoEn = DateTime.SpecifyKind(cuando, DateTimeKind.Utc);
        if (promover)
        {
            PrecioGalon = precioApi;
            Fuente = FuenteApi;
            PrecioPendiente = false;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Marca/limpia el flag de precio pendiente (Súper que aún no confirma su valor del período).</summary>
    public void MarcarPendiente(bool pendiente)
    {
        PrecioPendiente = pendiente;
        UpdatedAt = DateTime.UtcNow;
    }
}
