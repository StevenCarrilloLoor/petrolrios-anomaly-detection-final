namespace PetrolRios.Application.DTOs.Firebird;

/// <summary>
/// Mapeado desde tabla LIQU (liquidaciones de turno) de Contaplus Firebird. Una liquidación cierra
/// el cuadre de un turno: si un turno cerrado NO tiene su fila en LIQU, sus facturas quedaron "colgadas"
/// (sin cuadrar). El enlace con la factura es <c>LIQU.NUM_TURN ↔ DCTO.NUM_TURN</c> (verificado contra la
/// base real: las 149 liquidaciones tienen NUM_TURN poblado).
/// </summary>
public sealed record LiquidacionDto
{
    // NUM_LIQU — número de liquidación (PK)
    public double NumeroLiquidacion { get; init; }

    // NUM_TURN — turno liquidado (enlaza con DCTO.NUM_TURN)
    public int NumeroTurno { get; init; }

    // FEC_LIQU — fecha/hora de la liquidación
    public DateTime FechaLiquidacion { get; init; }

    // DIF_LIQU — diferencia de la liquidación (informativo)
    public double Diferencia { get; init; }
}
