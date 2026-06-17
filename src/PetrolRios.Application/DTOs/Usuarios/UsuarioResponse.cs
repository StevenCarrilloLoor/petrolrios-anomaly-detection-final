namespace PetrolRios.Application.DTOs.Usuarios;

public sealed record UsuarioResponse
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string NombreCompleto { get; init; } = string.Empty;
    public string Rol { get; init; } = string.Empty;
    public int RolId { get; init; }
    public bool Activo { get; init; }
    public bool EmailVerificado { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed record CrearUsuarioRequest(
    string Email,
    string NombreCompleto,
    string Password,
    int RolId);

public sealed record ActualizarUsuarioRequest(
    string? NombreCompleto,
    int? RolId,
    bool? Activo);
