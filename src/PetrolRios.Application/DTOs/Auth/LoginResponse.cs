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

// ─── Verificación de correo ───
public sealed record VerificarEmailRequest(string Token);
public sealed record ReenviarVerificacionRequest(string Email);

// ─── Login por QR (estilo Steam) ───
public sealed record QrIniciarResponse(string Codigo, int ExpiraSegundos);
public sealed record QrAprobarRequest(string Codigo);
/// <summary>Estado: "pendiente" | "aprobado" | "expirado" | "noexiste". Cuando es aprobado, trae el login.</summary>
public sealed record QrEstadoResponse(string Estado, LoginResponse? Login = null);

public sealed record UsuarioInfo(
    int Id,
    string Email,
    string NombreCompleto,
    string Rol);
