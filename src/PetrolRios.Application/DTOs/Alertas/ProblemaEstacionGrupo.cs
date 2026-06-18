namespace PetrolRios.Application.DTOs.Alertas;

/// <summary>
/// Agrupación de problemas operativos de una estación en un día, para la pestaña
/// "Problemas de estación" (carril Operativa). Permite mostrar estación + conteo del día
/// y, al expandir, la lista de problemas para documentación.
/// </summary>
public sealed record ProblemaEstacionGrupo
{
    public int EstacionId { get; init; }
    public string EstacionNombre { get; init; } = string.Empty;
    public DateTime Fecha { get; init; }
    public int Total { get; init; }
    public IReadOnlyList<AlertaResponse> Problemas { get; init; } = [];
}
