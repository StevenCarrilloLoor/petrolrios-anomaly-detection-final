namespace PetrolRios.Application.DTOs.Firebird;

/// <summary>
/// Mapeado desde tabla TURN (turnos/cierres de turno) de Contaplus Firebird.
/// </summary>
public sealed record CierreTurnoDto
{
    // NUM_TURN — número de turno (PK)
    public int NumeroTurno { get; init; }

    // COD_VEND — código del vendedor/despachador
    public string CodigoVendedor { get; init; } = string.Empty;

    // FIN_TURN — fecha/hora de inicio del turno
    public DateTime FechaInicio { get; init; }

    // FFI_TURN — fecha/hora de fin del turno
    public DateTime FechaFin { get; init; }

    // SIN_TURN — saldo inicial
    public double SaldoInicial { get; init; }

    // ING_TURN — ingresos del turno
    public double Ingresos { get; init; }

    // EGR_TURN — egresos del turno
    public double Egresos { get; init; }

    // SFI_TURN — saldo final
    public double SaldoFinal { get; init; }

    // FAL_TURN — faltante de caja
    public double Faltante { get; init; }

    // SOB_TURN — sobrante de caja
    public double Sobrante { get; init; }

    // CRE_TURN — créditos del turno
    public double Creditos { get; init; }
}
