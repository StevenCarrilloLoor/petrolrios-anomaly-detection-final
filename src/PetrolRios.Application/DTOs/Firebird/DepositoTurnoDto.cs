namespace PetrolRios.Application.DTOs.Firebird;

/// <summary>
/// Mapeado desde tabla TURN_DEPO (depósitos de turno) de Contaplus Firebird.
/// </summary>
public sealed record DepositoTurnoDto
{
    // NUM_TUDP — número de depósito (PK)
    public int NumeroDeposito { get; init; }

    // COD_VEND — código del vendedor
    public string CodigoVendedor { get; init; } = string.Empty;

    // NUM_TURN — número de turno asociado
    public int NumeroTurno { get; init; }

    // FEC_TUDP — fecha del depósito
    public DateTime FechaDeposito { get; init; }

    // TIP_TUDP — tipo de depósito (EF = efectivo, CH = cheque, etc.)
    public string TipoDeposito { get; init; } = string.Empty;

    // DET_TUDP — detalle del depósito
    public string Detalle { get; init; } = string.Empty;

    // CAN_TUDP — cantidad
    public int Cantidad { get; init; }

    // VAL_TUDP — valor unitario
    public decimal Valor { get; init; }

    // TOT_TUDP — total del depósito
    public decimal Total { get; init; }
}
