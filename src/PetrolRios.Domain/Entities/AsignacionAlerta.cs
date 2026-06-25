namespace PetrolRios.Domain.Entities;

public class AsignacionAlerta : BaseEntity
{
    public int AlertaId { get; private set; }
    public Alerta Alerta { get; private set; } = null!;

    /// <summary>Usuario al que se asignó la alerta (el auditor/supervisor responsable de revisarla).</summary>
    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;

    /// <summary>
    /// Quién hizo la asignación (el supervisor/administrador que la asignó). Nullable porque las
    /// asignaciones históricas anteriores a esta mejora no lo registraban.
    /// </summary>
    public int? AsignadoPorId { get; private set; }
    public Usuario? AsignadoPor { get; private set; }

    public string? Comentario { get; set; }
    public DateTime? FechaResolucion { get; set; }

    public static AsignacionAlerta Create(
        int alertaId, int usuarioId, int? asignadoPorId = null, string? comentario = null) =>
        new()
        {
            AlertaId = alertaId,
            UsuarioId = usuarioId,
            AsignadoPorId = asignadoPorId,
            Comentario = comentario
        };
}
