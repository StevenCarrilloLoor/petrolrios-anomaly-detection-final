namespace PetrolRios.Application.DTOs.Firebird;

/// <summary>
/// Mapeado desde tabla DESP (despachos/detalle de ventas) de Contaplus Firebird.
/// </summary>
public sealed record DetalleFacturaDto
{
    // NUM_DESP — número de despacho (PK)
    public double NumeroDespacho { get; init; }

    // COD_MANG — código de manguera
    public string CodigoManguera { get; init; } = string.Empty;

    // FIN_DESP — fecha/hora del despacho
    public DateTime FechaDespacho { get; init; }

    // VTO_DESP — volumen total
    public double VolumenTotal { get; init; }

    // CAN_DESP — cantidad (galones)
    public double Cantidad { get; init; }

    // VUN_DESP — valor unitario (precio aplicado)
    public double ValorUnitario { get; init; }

    // COD_PROD — código de producto
    public string CodigoProducto { get; init; } = string.Empty;

    // NOM_PROD — nombre del producto
    public string NombreProducto { get; init; } = string.Empty;

    // COD_CLIE — código de cliente
    public string CodigoCliente { get; init; } = string.Empty;

    // FAC_DESP — indicador de facturado ('1' facturado; distinto = no facturado)
    public string Facturado { get; init; } = string.Empty;
}
