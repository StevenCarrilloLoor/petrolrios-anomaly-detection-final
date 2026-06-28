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
