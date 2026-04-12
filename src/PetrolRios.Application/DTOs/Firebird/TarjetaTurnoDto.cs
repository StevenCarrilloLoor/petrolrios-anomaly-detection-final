namespace PetrolRios.Application.DTOs.Firebird;

/// <summary>
/// Mapeado desde tabla TURN_TARJ (tarjetas por turno) de Contaplus Firebird.
/// </summary>
public sealed record TarjetaTurnoDto
{
    // NUM_TURN_TARJ — número de registro (PK)
    public int NumeroTarjetaTurno { get; init; }

    // NUM_TURN — número de turno asociado
    public int NumeroTurno { get; init; }

    // COD_BANC — código del banco/tarjeta
    public string CodigoBanco { get; init; } = string.Empty;

    // CAN_TURN_TARJ — cantidad de transacciones con tarjeta
    public int Cantidad { get; init; }

    // VAL_TURN_TARJ — valor total en tarjeta
    public decimal Valor { get; init; }
}
