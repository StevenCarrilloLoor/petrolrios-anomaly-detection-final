namespace PetrolRios.Domain.Enums;

/// <summary>
/// Combustibles cuyo precio en Ecuador es REGULADO/subsidiado por el Estado (sistema de bandas de
/// EP Petroecuador). Son los que vende PetrolRíos con precio oficial uniforme a nivel nacional. La
/// gasolina Súper se excluye a propósito: su precio NO está regulado y varía por comercializadora/estación.
/// </summary>
public enum TipoCombustible
{
    /// <summary>Gasolina Extra (84 octanos) — precio regulado por banda.</summary>
    Extra = 1,

    /// <summary>Gasolina Ecopaís (mezcla con etanol) — precio regulado por banda.</summary>
    Ecopais = 2,

    /// <summary>Diésel (Premium) — precio regulado/subsidiado.</summary>
    Diesel = 3
}
