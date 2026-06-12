namespace PetrolRios.Application.DTOs.Alertas;

/// <summary>Comentario de auditoría sobre una alerta (CU-07).</summary>
public sealed record ComentarioResponse
{
    public int Id { get; init; }
    public int AlertaId { get; init; }
    public int UsuarioId { get; init; }
    public string UsuarioNombre { get; init; } = string.Empty;
    public string Texto { get; init; } = string.Empty;
    public DateTime Fecha { get; init; }
}

public sealed record AgregarComentarioRequest(string Texto);
