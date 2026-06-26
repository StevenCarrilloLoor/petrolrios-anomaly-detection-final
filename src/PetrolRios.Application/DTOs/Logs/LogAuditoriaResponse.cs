namespace PetrolRios.Application.DTOs.Logs;

public sealed record LogAuditoriaResponse
{
    public int Id { get; init; }
    public string Accion { get; init; } = string.Empty;
    public string Entidad { get; init; } = string.Empty;
    public int? EntidadId { get; init; }
    public string? DetalleJson { get; init; }
    public string DireccionIp { get; init; } = string.Empty;
    public int? UsuarioId { get; init; }
    public string? UsuarioEmail { get; init; }
    public DateTime CreatedAt { get; init; }
}
