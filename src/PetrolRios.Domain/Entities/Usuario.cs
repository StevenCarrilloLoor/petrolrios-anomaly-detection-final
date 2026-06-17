namespace PetrolRios.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string NombreCompleto { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public int RolId { get; private set; }
    public Rol Rol { get; private set; } = null!;

    // ─── Seguridad ───
    /// <summary>Obliga a cambiar la contraseña en el próximo inicio de sesión (p. ej. credenciales por defecto).</summary>
    public bool DebeCambiarPassword { get; set; }

    /// <summary>Intentos de login fallidos consecutivos (para bloqueo por fuerza bruta).</summary>
    public int AccessFailedCount { get; private set; }

    /// <summary>Hasta cuándo está bloqueada la cuenta (UTC). Null = no bloqueada.</summary>
    public DateTime? LockoutEnd { get; private set; }

    /// <summary>Secreto TOTP (Base32) para 2FA. Null = sin 2FA configurado.</summary>
    public string? TotpSecret { get; private set; }

    /// <summary>true cuando el usuario confirmó y activó el 2FA.</summary>
    public bool TotpHabilitado { get; private set; }

    // ─── Verificación de correo ───
    /// <summary>true cuando el usuario confirmó su correo con el enlace recibido.</summary>
    public bool EmailVerificado { get; private set; }

    /// <summary>Token de un solo uso enviado por correo para verificar la cuenta.</summary>
    public string? TokenVerificacionEmail { get; private set; }

    /// <summary>Hasta cuándo es válido el token de verificación (UTC).</summary>
    public DateTime? TokenVerificacionExpira { get; private set; }

    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];
    public ICollection<AsignacionAlerta> Asignaciones { get; private set; } = [];
    public ICollection<LogAuditoria> Logs { get; private set; } = [];

    public static Usuario Create(string email, string nombreCompleto, string passwordHash, int rolId) =>
        new() { Email = email, NombreCompleto = nombreCompleto, PasswordHash = passwordHash, RolId = rolId };

    public void UpdatePassword(string newHash)
    {
        PasswordHash = newHash;
        DebeCambiarPassword = false;
        // Cambiar la contraseña limpia cualquier bloqueo previo.
        ResetearFallos();
    }

    /// <summary>Actualiza el nombre y/o el rol del usuario (edición desde el panel).</summary>
    public void ActualizarPerfil(string? nombreCompleto, int? rolId)
    {
        if (!string.IsNullOrWhiteSpace(nombreCompleto))
            NombreCompleto = nombreCompleto.Trim();
        if (rolId.HasValue && rolId.Value > 0)
            RolId = rolId.Value;
    }

    // ─── Bloqueo por intentos fallidos ───
    public bool EstaBloqueado() => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;

    /// <summary>Registra un intento fallido; bloquea la cuenta al superar el máximo.</summary>
    public void RegistrarFalloLogin(int maxIntentos, int minutosBloqueo)
    {
        AccessFailedCount++;
        if (AccessFailedCount >= maxIntentos)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(minutosBloqueo);
            AccessFailedCount = 0;
        }
    }

    public void ResetearFallos()
    {
        AccessFailedCount = 0;
        LockoutEnd = null;
    }

    // ─── 2FA (TOTP) ───
    /// <summary>Guarda el secreto TOTP (aún sin activar; se activa al confirmar el primer código).</summary>
    public void ConfigurarTotp(string secret)
    {
        TotpSecret = secret;
        TotpHabilitado = false;
    }

    public void ActivarTotp() => TotpHabilitado = true;

    public void DeshabilitarTotp()
    {
        TotpSecret = null;
        TotpHabilitado = false;
    }

    // ─── Verificación de correo ───
    /// <summary>Genera un token de verificación (válido por las horas indicadas) y lo devuelve.</summary>
    public string GenerarTokenVerificacion(int horasValido = 48)
    {
        TokenVerificacionEmail = Guid.NewGuid().ToString("N");
        TokenVerificacionExpira = DateTime.UtcNow.AddHours(horasValido);
        EmailVerificado = false;
        return TokenVerificacionEmail;
    }

    /// <summary>Marca el correo como verificado directamente (cuentas del sistema/seed).</summary>
    public void MarcarEmailVerificado()
    {
        EmailVerificado = true;
        TokenVerificacionEmail = null;
        TokenVerificacionExpira = null;
    }

    /// <summary>Marca el correo como verificado si el token coincide y no expiró.</summary>
    public bool VerificarEmail(string token)
    {
        if (EmailVerificado) return true;
        if (string.IsNullOrWhiteSpace(TokenVerificacionEmail)) return false;
        if (TokenVerificacionEmail != token) return false;
        if (TokenVerificacionExpira is null || TokenVerificacionExpira < DateTime.UtcNow) return false;

        EmailVerificado = true;
        TokenVerificacionEmail = null;
        TokenVerificacionExpira = null;
        return true;
    }
}
