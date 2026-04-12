namespace PetrolRios.Application.DTOs.Firebird;

/// <summary>
/// Mapeado desde tabla ANUL (anulaciones de comprobantes) de Contaplus Firebird.
/// </summary>
public sealed record AnulacionDto
{
    // NUMAN — número de anulación (PK)
    public double NumeroAnulacion { get; init; }

    // TIPOCOMPROBANTE — tipo de comprobante anulado
    public string TipoComprobante { get; init; } = string.Empty;

    // FECHAANULACION — fecha de la anulación
    public DateTime FechaAnulacion { get; init; }

    // ESTABLECIMIENTO — código de establecimiento
    public string Establecimiento { get; init; } = string.Empty;

    // PUNTOEMISION — punto de emisión
    public string PuntoEmision { get; init; } = string.Empty;

    // SECUENCIALINICIO — secuencial inicio
    public string SecuencialInicio { get; init; } = string.Empty;

    // SECUENCIALFIN — secuencial fin
    public string SecuencialFin { get; init; } = string.Empty;

    // AUTORIZACION — código de autorización
    public string Autorizacion { get; init; } = string.Empty;
}
