namespace PetrolRios.Application.DTOs.Auth;

public sealed record LoginResponse(
    string Token,
    string RefreshToken,
    DateTime Expiration,
    UsuarioInfo Usuario);

public sealed record UsuarioInfo(
    int Id,
    string Email,
    string NombreCompleto,
    string Rol);
