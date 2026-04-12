namespace PetrolRios.Domain.Entities;

public class AsignacionAlerta : BaseEntity
{
    public int AlertaId { get; private set; }
    public Alerta Alerta { get; private set; } = null!;

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;

    public string? Comentario { get; set; }
    public DateTime? FechaResolucion { get; set; }

    public static AsignacionAlerta Create(int alertaId, int usuarioId, string? comentario = null) =>
        new() { AlertaId = alertaId, UsuarioId = usuarioId, Comentario = comentario };
}
