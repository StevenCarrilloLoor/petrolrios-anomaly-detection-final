namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Servicio de autenticación de dos factores (2FA) basado en TOTP (RFC 6238),
/// compatible con Google Authenticator, Authy, Microsoft Authenticator, etc.
/// </summary>
public interface ITotpService
{
    /// <summary>Genera un secreto nuevo en Base32 para enrolar una cuenta.</summary>
    string GenerarSecreto();

    /// <summary>
    /// Construye la URI <c>otpauth://</c> que se codifica en el QR para escanear
    /// con la app autenticadora.
    /// </summary>
    string ConstruirUriOtpauth(string secreto, string cuenta, string emisor = "PetrolRios");

    /// <summary>Valida un código de 6 dígitos contra el secreto (con tolerancia de ±1 ventana).</summary>
    bool Validar(string secreto, string codigo);
}
