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

    /// <summary>Origen del dato (p. ej. "EP Petroecuador — sistema de bandas").</summary>
    public string Fuente { get; private set; } = string.Empty;

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

    /// <summary>Actualiza el precio vigente (nueva banda mensual o corrección). Marca UpdatedAt.</summary>
    public void Actualizar(decimal precioGalon, decimal subsidio, DateTime vigenteDesde, DateTime? vigenteHasta, string fuente)
    {
        PrecioGalon = precioGalon;
        Subsidio = subsidio;
        VigenteDesde = DateTime.SpecifyKind(vigenteDesde, DateTimeKind.Utc);
        VigenteHasta = vigenteHasta is null ? null : DateTime.SpecifyKind(vigenteHasta.Value, DateTimeKind.Utc);
        Fuente = (fuente ?? string.Empty).Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
