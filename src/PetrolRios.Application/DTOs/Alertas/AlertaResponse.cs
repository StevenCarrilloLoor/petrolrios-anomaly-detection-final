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
    /// <summary>Nombre del empleado resuelto desde el catálogo (null si no hay match: se muestra solo el código).</summary>
    public string? EmpleadoNombre { get; init; }
    public string? TransaccionReferencia { get; init; }
    public int EstacionId { get; init; }
    public string EstacionNombre { get; init; } = string.Empty;
    public string? MetadataJson { get; init; }

    // --- Asignación (a quién está asignada la alerta y quién la asignó). Null si nunca se asignó. ---
    /// <summary>Id del usuario al que está asignada actualmente la alerta (la última asignación).</summary>
    public int? AsignadoAId { get; init; }
    /// <summary>Nombre del responsable asignado (para mostrar en el detalle y la lista).</summary>
    public string? AsignadoANombre { get; init; }
    /// <summary>Rol del responsable asignado ("Auditor"/"Supervisor").</summary>
    public string? AsignadoARol { get; init; }
    /// <summary>Nombre de quien hizo la asignación (supervisor/administrador). Null en asignaciones antiguas.</summary>
    public string? AsignadoPorNombre { get; init; }
    /// <summary>Fecha de la última asignación.</summary>
    public DateTime? FechaAsignacion { get; init; }
}
