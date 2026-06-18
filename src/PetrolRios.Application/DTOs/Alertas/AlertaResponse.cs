namespace PetrolRios.Application.DTOs.Alertas;

public sealed record AlertaResponse
{
    public int Id { get; init; }
    public string TipoDetector { get; init; } = string.Empty;
    public string NivelRiesgo { get; init; } = string.Empty;
    /// <summary>Carril de la alerta: "Operativa" (estación) o "Auditoria" (fraude).</summary>
    public string Ambito { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public double Score { get; init; }
    public DateTime FechaDeteccion { get; init; }
    public string? EmpleadoCodigo { get; init; }
    public string? TransaccionReferencia { get; init; }
    public int EstacionId { get; init; }
    public string EstacionNombre { get; init; } = string.Empty;
    public string? MetadataJson { get; init; }
}
