namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Orquestador ETL: extrae datos de Firebird por estación usando watermark,
/// carga a staging en PostgreSQL y actualiza la marca de agua.
/// </summary>
public interface IEtlOrchestrator
{
    Task<EtlResult> ExecuteAsync(CancellationToken ct = default);
}

/// <summary>
/// Resultado de una ejecución ETL.
/// </summary>
public sealed record EtlResult
{
    public int EstacionesProcesadas { get; init; }
    public int EstacionesConError { get; init; }
    public int TransaccionesExtraidas { get; init; }
    public Dictionary<string, string> Errores { get; init; } = [];
    public DateTime WatermarkMaxima { get; init; }
}
