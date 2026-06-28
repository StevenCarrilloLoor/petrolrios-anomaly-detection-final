namespace PetrolRios.Domain.Enums;

/// <summary>
/// Combustibles que vende PetrolRíos. Extra, Ecopaís y Diésel tienen precio REGULADO/subsidiado por el
/// Estado (sistema de bandas de EP Petroecuador), uniforme y obligatorio a nivel nacional. La Súper es
/// LIBRE MERCADO: su precio NO está regulado y varía por comercializadora/estación; se guarda solo como
/// referencial (no entra al detector de precio fuera de lista). Usa <see cref="EsRegulado"/> para distinguir.
/// </summary>
public enum TipoCombustible
{
    /// <summary>Gasolina Extra (84 octanos) — precio regulado por banda.</summary>
    Extra = 1,

    /// <summary>Gasolina Ecopaís (mezcla con etanol) — precio regulado por banda.</summary>
    Ecopais = 2,

    /// <summary>Diésel (Premium) — precio regulado/subsidiado.</summary>
    Diesel = 3,

    /// <summary>Gasolina Súper (alto octanaje) — LIBRE MERCADO, precio referencial (no regulado).</summary>
    Super = 4
}

/// <summary>Utilidades sobre <see cref="TipoCombustible"/>.</summary>
public static class TipoCombustibleExtensions
{
    /// <summary>True si el precio es regulado (uniforme nacional). La Súper es libre mercado → false.</summary>
    public static bool EsRegulado(this TipoCombustible t) => t != TipoCombustible.Super;
}

/// <summary>
/// Mapeo entre el CÓDIGO de producto de Contaplus (DESP.COD_PROD / línea de factura) y el combustible.
/// Confirmado por precio en SanPio (cruzando el salto de banda del 12-jun con los precios oficiales):
/// 1=Súper, 2=Extra/Ecopaís (mismo código y precio), 3=Diésel. Fuente única de verdad para que tanto los
/// detectores como la factura muestren el NOMBRE real del combustible (no el número), evitando confusiones.
/// </summary>
public static class Combustibles
{
    public static TipoCombustible? PorCodigo(string? codigo) =>
        (codigo ?? string.Empty).Trim().TrimStart('0') switch   // tolera "1" y "01"
        {
            "1" => TipoCombustible.Super,
            "2" => TipoCombustible.Extra,
            "3" => TipoCombustible.Diesel,
            _ => null
        };

    /// <summary>Nombre legible del combustible por su código; si el código es desconocido, lo devuelve tal cual.</summary>
    public static string NombrePorCodigo(string? codigo) => PorCodigo(codigo) switch
    {
        TipoCombustible.Super => "Súper",
        TipoCombustible.Extra => "Extra/Ecopaís",
        TipoCombustible.Diesel => "Diésel",
        _ => (codigo ?? string.Empty).Trim()
    };
}
