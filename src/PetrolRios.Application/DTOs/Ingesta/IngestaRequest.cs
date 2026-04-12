namespace PetrolRios.Application.DTOs.Ingesta;

/// <summary>
/// Lote de transacciones enviado por un agente de estación.
/// </summary>
public sealed record IngestaRequest
{
    public required string CodigoEstacion { get; init; }
    public required List<TransaccionIngestaItem> Transacciones { get; init; }
}

public sealed record TransaccionIngestaItem
{
    public required string TipoTransaccion { get; init; }
    public required string DataJson { get; init; }
    public required DateTime FechaOriginal { get; init; }
}

public sealed record IngestaResponse
{
    public int TransaccionesRecibidas { get; init; }
    public DateTime FechaRecepcion { get; init; }
}
