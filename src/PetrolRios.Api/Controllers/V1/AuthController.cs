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
    private readonly ILogService _logService;

    public AuthController(IAuthService authService, ILogService logService)
    {
        _authService = authService;
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
