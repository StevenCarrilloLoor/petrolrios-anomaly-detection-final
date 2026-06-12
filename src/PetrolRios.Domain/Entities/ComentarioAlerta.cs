namespace PetrolRios.Domain.Entities;

/// <summary>
/// Comentario de un auditor sobre una alerta (CU-07: cada comentario queda
/// registrado con fecha, hora y autor).
/// </summary>
public class ComentarioAlerta : BaseEntity
{
    public int AlertaId { get; private set; }
    public Alerta Alerta { get; private set; } = null!;

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;

    public string Texto { get; private set; } = string.Empty;

    public static ComentarioAlerta Create(int alertaId, int usuarioId, string texto) =>
        new() { AlertaId = alertaId, UsuarioId = usuarioId, Texto = texto };
}
