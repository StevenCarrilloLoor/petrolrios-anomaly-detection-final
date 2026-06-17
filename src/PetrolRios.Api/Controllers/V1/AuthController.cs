using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Api.Extensions;
using PetrolRios.Application.DTOs.Auth;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Api.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUsuarioService _usuarioService;
    private readonly ILogService _logService;

    public AuthController(IAuthService authService, IUsuarioService usuarioService, ILogService logService)
    {
        _authService = authService;
        _usuarioService = usuarioService;
        _logService = logService;
    }

    /// <summary>
    /// Iniciar sesión con email y contraseña.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var response = await _authService.LoginAsync(request, ct);

        // Si falta el 2FA, aún no hay usuario/token: el frontend pedirá el código.
        if (response.Requiere2Fa || response.Usuario is null)
            return Ok(response);

        await this.RegistrarAuditoriaAsync(_logService,
            "Inicio de sesión", "Usuario", response.Usuario.Id,
            new { response.Usuario.Email }, usuarioIdExplicito: response.Usuario.Id, ct: ct);

        return Ok(response);
    }

    /// <summary>
    /// Renovar el token JWT usando un refresh token válido.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var response = await _authService.RefreshAsync(request, ct);
        return Ok(response);
    }

    /// <summary>
    /// Cambiar la propia contraseña (obligatorio tras el primer ingreso con
    /// credenciales por defecto). Requiere la contraseña actual.
    /// </summary>
    [HttpPost("cambiar-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequest request, CancellationToken ct)
    {
        var usuarioId = int.TryParse(
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
        if (usuarioId == 0) return Unauthorized();

        await _authService.CambiarPasswordAsync(usuarioId, request.PasswordActual, request.PasswordNueva, ct);

        await this.RegistrarAuditoriaAsync(_logService,
            "Cambio de contraseña", "Usuario", usuarioId, usuarioIdExplicito: usuarioId, ct: ct);

        return NoContent();
    }

    /// <summary>Estado del 2FA del usuario actual.</summary>
    [HttpGet("2fa/estado")]
    [Authorize]
    public async Task<IActionResult> Estado2fa(CancellationToken ct)
    {
        var id = UsuarioActual();
        if (id == 0) return Unauthorized();
        return Ok(new Estado2faResponse(await _authService.Estado2faAsync(id, ct)));
    }

    /// <summary>Inicia el enrolamiento de 2FA: devuelve el secreto y la URI para el QR.</summary>
    [HttpPost("2fa/iniciar")]
    [Authorize]
    public async Task<IActionResult> Iniciar2fa(CancellationToken ct)
    {
        var id = UsuarioActual();
        if (id == 0) return Unauthorized();
        return Ok(await _authService.Iniciar2faAsync(id, ct));
    }

    /// <summary>Confirma y activa el 2FA verificando el primer código de la app autenticadora.</summary>
    [HttpPost("2fa/confirmar")]
    [Authorize]
    public async Task<IActionResult> Confirmar2fa([FromBody] Confirmar2faRequest request, CancellationToken ct)
    {
        var id = UsuarioActual();
        if (id == 0) return Unauthorized();
        await _authService.Confirmar2faAsync(id, request.Codigo, ct);
        await this.RegistrarAuditoriaAsync(_logService, "Activación de 2FA", "Usuario", id, usuarioIdExplicito: id, ct: ct);
        return NoContent();
    }

    /// <summary>Desactiva el 2FA (requiere un código válido si estaba activo).</summary>
    [HttpPost("2fa/desactivar")]
    [Authorize]
    public async Task<IActionResult> Desactivar2fa([FromBody] Confirmar2faRequest request, CancellationToken ct)
    {
        var id = UsuarioActual();
        if (id == 0) return Unauthorized();
        await _authService.Desactivar2faAsync(id, request.Codigo, ct);
        await this.RegistrarAuditoriaAsync(_logService, "Desactivación de 2FA", "Usuario", id, usuarioIdExplicito: id, ct: ct);
        return NoContent();
    }

    /// <summary>Iniciar sesión con el código del autenticador (TOTP), sin contraseña.</summary>
    [HttpPost("login-totp")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginTotp([FromBody] LoginTotpRequest request, CancellationToken ct)
    {
        var response = await _authService.LoginConTotpAsync(request.Email, request.CodigoTotp, ct);
        await this.RegistrarAuditoriaAsync(_logService, "Inicio de sesión (autenticador)", "Usuario",
            response.Usuario.Id, usuarioIdExplicito: response.Usuario.Id, ct: ct);
        return Ok(response);
    }

    /// <summary>Solicitar recuperación de contraseña: envía un enlace al correo.</summary>
    [HttpPost("olvide-password")]
    [AllowAnonymous]
    public async Task<IActionResult> OlvidePassword([FromBody] OlvidePasswordRequest request, CancellationToken ct)
    {
        await _authService.SolicitarResetPasswordAsync(request.Email, ct);
        return Ok(new { ok = true, mensaje = "Si la cuenta existe, te enviamos un enlace para restablecer la contraseña." });
    }

    /// <summary>Restablecer la contraseña usando el token recibido por correo.</summary>
    [HttpPost("restablecer-password")]
    [AllowAnonymous]
    public async Task<IActionResult> RestablecerPassword([FromBody] RestablecerPasswordRequest request, CancellationToken ct)
    {
        var ok = await _authService.RestablecerPasswordAsync(request.Token, request.NuevaPassword, ct);
        return ok
            ? Ok(new { ok = true, mensaje = "Contraseña actualizada. Ya puedes iniciar sesión." })
            : BadRequest(new { ok = false, mensaje = "El enlace es inválido o expiró." });
    }

    // ─── Verificación de correo ───

    /// <summary>Verifica el correo del usuario a partir del token del enlace recibido por email.</summary>
    [HttpPost("verificar-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerificarEmail([FromBody] VerificarEmailRequest request, CancellationToken ct)
    {
        var ok = await _usuarioService.VerificarEmailAsync(request.Token, ct);
        return ok
            ? Ok(new { ok = true, mensaje = "Correo verificado correctamente." })
            : BadRequest(new { ok = false, mensaje = "El enlace es inválido o expiró." });
    }

    /// <summary>Reenvía el correo de verificación a una cuenta no verificada.</summary>
    [HttpPost("reenviar-verificacion")]
    [AllowAnonymous]
    public async Task<IActionResult> ReenviarVerificacion([FromBody] ReenviarVerificacionRequest request, CancellationToken ct)
    {
        await _usuarioService.ReenviarVerificacionAsync(request.Email, ct);
        // Respuesta neutra para no revelar si el correo existe.
        return Ok(new { ok = true, mensaje = "Si la cuenta existe y no está verificada, se envió un correo." });
    }

    // ─── Login por QR (estilo Steam) ───

    /// <summary>La pantalla que quiere entrar genera un código para mostrar como QR.</summary>
    [HttpPost("qr/iniciar")]
    [AllowAnonymous]
    public IActionResult QrIniciar() => Ok(_authService.QrIniciar());

    /// <summary>Polling del estado del código. Cuando está aprobado, devuelve el login.</summary>
    [HttpGet("qr/estado")]
    [AllowAnonymous]
    public async Task<IActionResult> QrEstado([FromQuery] string codigo, CancellationToken ct)
        => Ok(await _authService.QrEstadoAsync(codigo, ct));

    /// <summary>Un usuario ya autenticado aprueba el inicio de sesión escaneado.</summary>
    [HttpPost("qr/aprobar")]
    [Authorize]
    public async Task<IActionResult> QrAprobar([FromBody] QrAprobarRequest request, CancellationToken ct)
    {
        var id = UsuarioActual();
        if (id == 0) return Unauthorized();
        var ok = await _authService.QrAprobarAsync(request.Codigo, id, ct);
        if (!ok) return BadRequest(new { mensaje = "Código inválido o expirado." });
        await this.RegistrarAuditoriaAsync(_logService, "Aprobación de inicio por QR", "Usuario", id, usuarioIdExplicito: id, ct: ct);
        return NoContent();
    }

    private int UsuarioActual() =>
        int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    /// <summary>
    /// Cerrar sesión revocando el refresh token.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request, CancellationToken ct)
    {
        await _authService.LogoutAsync(request.RefreshToken, ct);

        await this.RegistrarAuditoriaAsync(_logService,
            "Cierre de sesión", "Usuario", ct: ct);

        return NoContent();
    }
}
