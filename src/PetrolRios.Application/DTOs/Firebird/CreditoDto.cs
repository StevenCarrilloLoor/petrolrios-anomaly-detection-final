namespace PetrolRios.Application.DTOs.Firebird;

/// <summary>
/// Mapeado desde tabla CRED_CABE (cabeceras de crédito) de Contaplus Firebird.
/// </summary>
public sealed record CreditoDto
{
    // NUM_CABE — número de cabecera (PK)
    public double NumeroCabecera { get; init; }

    // FEC_CABE — fecha del crédito
    public DateTime FechaCabecera { get; init; }

    // COD_CRED — código del tipo de crédito
    public string CodigoCredito { get; init; } = string.Empty;

    // COD_SOCI — código del socio/cliente
    public string CodigoSocio { get; init; } = string.Empty;

    // PLA_CABE — plazo del crédito
    public int PlazoCabecera { get; init; }

    // TAZ_CRED — tasa del crédito
    public double TasaCredito { get; init; }

    // COD_GARA — código del garante
    public string CodigoGarante { get; init; } = string.Empty;

    // TCR_CABE — total crédito
    public double TotalCredito { get; init; }

    // TIN_CABE — total interés
    public double TotalInteres { get; init; }

    // COD_BANC — código de banco
    public string CodigoBanco { get; init; } = string.Empty;

    // NUMMCOMP — número de comprobante
    public double NumeroComprobante { get; init; }
}
