namespace PetrolRios.Application.DTOs.Ingesta;

/// <summary>
/// Lote de transacciones enviado por un agente de estación.
/// </summary>
public sealed record IngestaRequest
{
    public required string CodigoEstacion { get; init; }
    public string? NombreEstacion { get; init; }
    public string? ZonaEstacion { get; init; }
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

/// <summary>Latido del agente: señal de vida aunque no haya transacciones nuevas.</summary>
public sealed record HeartbeatRequest
{
    public required string CodigoEstacion { get; init; }
    public string? NombreEstacion { get; init; }
    public string? ZonaEstacion { get; init; }
    public string? VersionAgente { get; init; }
}
