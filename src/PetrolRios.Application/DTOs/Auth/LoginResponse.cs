namespace PetrolRios.Application.DTOs.Auth;

public sealed record LoginResponse(
    string Token,
    string RefreshToken,
    DateTime Expiration,
    UsuarioInfo Usuario,
    bool DebeCambiarPassword = false,
    bool Requiere2Fa = false);

public sealed record CambiarPasswordRequest(string PasswordActual, string PasswordNueva);

// ─── 2FA (TOTP) ───
public sealed record Iniciar2faResponse(string Secreto, string UriOtpauth);
public sealed record Confirmar2faRequest(string Codigo);
public sealed record Estado2faResponse(bool Habilitado);

public sealed record UsuarioInfo(
    int Id,
    string Email,
    string NombreCompleto,
    string Rol);
