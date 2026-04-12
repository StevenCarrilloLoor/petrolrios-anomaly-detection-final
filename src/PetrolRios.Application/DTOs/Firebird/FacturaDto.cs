namespace PetrolRios.Application.DTOs.Firebird;

/// <summary>
/// Mapeado desde tabla DCTO (documentos/facturas) de Contaplus Firebird.
/// </summary>
public sealed record FacturaDto
{
    // SEC_DCTO — secuencia del documento (PK)
    public double SecuenciaDocumento { get; init; }

    // TIP_DCTO — tipo de documento
    public string TipoDocumento { get; init; } = string.Empty;

    // NUM_DCTO — número de documento
    public string NumeroDocumento { get; init; } = string.Empty;

    // FEC_DCTO — fecha del documento
    public DateTime FechaDocumento { get; init; }

    // COD_CLIE — código de cliente
    public string CodigoCliente { get; init; } = string.Empty;

    // TNI_DCTO — total neto con IVA
    public double TotalNeto { get; init; }

    // TSI_DCTO — total sin IVA
    public double TotalSinIva { get; init; }

    // DSC_DCTO — descuento aplicado
    public double Descuento { get; init; }

    // IVA_DCTO — monto IVA
    public double Iva { get; init; }

    // COD_VEND — código de vendedor/empleado
    public string CodigoVendedor { get; init; } = string.Empty;

    // COD_PAGO — código de forma de pago
    public string CodigoPago { get; init; } = string.Empty;

    // PLA_DCTO — placa del vehículo
    public string Placa { get; init; } = string.Empty;

    // RUC_DCTO — RUC del cliente en el documento
    public string RucCliente { get; init; } = string.Empty;

    // NUM_TURN — número de turno asociado
    public int NumeroTurno { get; init; }

    // SUB_DCTO — subtotal
    public double Subtotal { get; init; }

    // NUM_CONS — número consecutivo
    public int NumeroConsecutivo { get; init; }

    // COD_CHOF — código de chofer
    public string CodigoChofer { get; init; } = string.Empty;

    // COD_MANG — código de manguera
    public string CodigoManguera { get; init; } = string.Empty;
}
