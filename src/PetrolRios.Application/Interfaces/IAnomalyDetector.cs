using PetrolRios.Domain.Enums;

namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Contrato para cada detector de anomalías (Strategy Pattern).
/// </summary>
public interface IAnomalyDetector
{
    TipoDetector Type { get; }
    Task<IReadOnlyList<DetectedAnomaly>> DetectAsync(DetectionContext context, CancellationToken ct);
}

/// <summary>
/// Contexto que reciben los detectores con datos de staging y reglas vigentes.
/// </summary>
public sealed class DetectionContext
{
    public required int EstacionId { get; init; }
    public required string EstacionNombre { get; init; }
    public required DateTime FromWatermark { get; init; }
    public required DateTime ToWatermark { get; init; }
}

/// <summary>
/// Resultado de detección antes de persistirse como Alerta.
/// </summary>
public sealed class DetectedAnomaly
{
    public required TipoDetector TipoDetector { get; init; }
    public required string Descripcion { get; init; }
    public required double Score { get; init; }
    public required NivelRiesgo NivelRiesgo { get; init; }
    public required int EstacionId { get; init; }
    public string? EmpleadoCodigo { get; init; }
    public string? TransaccionReferencia { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = [];
}
