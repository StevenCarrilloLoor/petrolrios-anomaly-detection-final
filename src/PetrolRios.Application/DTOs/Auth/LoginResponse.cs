namespace PetrolRios.Application.DTOs.Auth;

public sealed record LoginResponse(
    string Token,
    string RefreshToken,
    DateTime Expiration,
    UsuarioInfo Usuario,
    bool DebeCambiarPassword = false);

public sealed record CambiarPasswordRequest(string PasswordActual, string PasswordNueva);

public sealed record UsuarioInfo(
    int Id,
    string Email,
    string NombreCompleto,
    string Rol);
