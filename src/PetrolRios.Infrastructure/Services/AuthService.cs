using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PetrolRios.Application.DTOs.Auth;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly PetrolRiosDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ITotpService _totpService;
    private readonly QrLoginService _qrLogin;
    private readonly IConfiguration _config;

    public AuthService(
        PetrolRiosDbContext dbContext,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ITotpService totpService,
        QrLoginService qrLogin,
        IConfiguration config)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _totpService = totpService;
        _qrLogin = qrLogin;
        _config = config;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var usuario = await _dbContext.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Activo, ct)
            ?? throw new UnauthorizedAccessException("Credenciales inválidas.");

        // Bloqueo por intentos fallidos (anti fuerza bruta)
        if (usuario.EstaBloqueado())
            throw new UnauthorizedAccessException(
                "La cuenta está bloqueada temporalmente por varios intentos fallidos. Intente más tarde.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
        {
            var maxIntentos = _config.GetValue("Seguridad:MaxIntentosLogin", 5);
            var minutosBloqueo = _config.GetValue("Seguridad:MinutosBloqueo", 15);
            usuario.RegistrarFalloLogin(maxIntentos, minutosBloqueo);
            await _dbContext.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        // Segundo factor (2FA / TOTP) si el usuario lo tiene activado
        if (usuario.TotpHabilitado)
        {
            if (string.IsNullOrWhiteSpace(request.CodigoTotp))
            {
                // Contraseña correcta pero falta el código: el frontend lo pedirá.
                return new LoginResponse("", "", DateTime.UtcNow, default!, false, Requiere2Fa: true);
            }
            if (!_totpService.Validar(usuario.TotpSecret!, request.CodigoTotp))
            {
                var maxIntentos = _config.GetValue("Seguridad:MaxIntentosLogin", 5);
                var minutosBloqueo = _config.GetValue("Seguridad:MinutosBloqueo", 15);
                usuario.RegistrarFalloLogin(maxIntentos, minutosBloqueo);
                await _dbContext.SaveChangesAsync(ct);
                throw new UnauthorizedAccessException("Código de verificación (2FA) inválido.");
            }
        }

        // Login correcto: limpiar contador de fallos
        if (usuario.AccessFailedCount > 0 || usuario.LockoutEnd is not null)
        {
            usuario.ResetearFallos();
            await _dbContext.SaveChangesAsync(ct);
        }

        return await GenerateAuthResponseAsync(usuario, ct);
    }

    // ─── 2FA (TOTP) ───

    public async Task<Iniciar2faResponse> Iniciar2faAsync(int usuarioId, CancellationToken ct = default)
    {
        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId && u.Activo, ct)
            ?? throw new UnauthorizedAccessException("Usuario no encontrado.");

        var secreto = _totpService.GenerarSecreto();
        usuario.ConfigurarTotp(secreto); // queda sin activar hasta confirmar el primer código
        await _dbContext.SaveChangesAsync(ct);

        var uri = _totpService.ConstruirUriOtpauth(secreto, usuario.Email);
        return new Iniciar2faResponse(secreto, uri);
    }

    public async Task Confirmar2faAsync(int usuarioId, string codigo, CancellationToken ct = default)
    {
        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId && u.Activo, ct)
            ?? throw new UnauthorizedAccessException("Usuario no encontrado.");
        if (string.IsNullOrWhiteSpace(usuario.TotpSecret))
            throw new InvalidOperationException("Primero inicie la configuración del 2FA.");
        if (!_totpService.Validar(usuario.TotpSecret, codigo))
            throw new UnauthorizedAccessException("El código no es válido. Verifique la hora del dispositivo e intente de nuevo.");

        usuario.ActivarTotp();
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Desactivar2faAsync(int usuarioId, string codigo, CancellationToken ct = default)
    {
        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId && u.Activo, ct)
            ?? throw new UnauthorizedAccessException("Usuario no encontrado.");
        if (usuario.TotpHabilitado && !_totpService.Validar(usuario.TotpSecret!, codigo))
            throw new UnauthorizedAccessException("Código inválido; no se desactivó el 2FA.");

        usuario.DeshabilitarTotp();
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<bool> Estado2faAsync(int usuarioId, CancellationToken ct = default)
    {
        return await _dbContext.Usuarios
            .Where(u => u.Id == usuarioId)
            .Select(u => u.TotpHabilitado)
            .FirstOrDefaultAsync(ct);
    }

    // ─── Login por QR (estilo Steam) ───

    public QrIniciarResponse QrIniciar()
    {
        var (codigo, expira) = _qrLogin.Iniciar();
        return new QrIniciarResponse(codigo, expira);
    }

    public async Task<bool> QrAprobarAsync(string codigo, int usuarioId, CancellationToken ct = default)
    {
        // Solo un usuario activo y existente puede aprobar.
        var existe = await _dbContext.Usuarios.AnyAsync(u => u.Id == usuarioId && u.Activo, ct);
        if (!existe) return false;
        return _qrLogin.Aprobar(codigo, usuarioId);
    }

    public async Task<QrEstadoResponse> QrEstadoAsync(string codigo, CancellationToken ct = default)
    {
        var (estado, usuarioId) = _qrLogin.Consultar(codigo);
        if (estado != QrLoginService.EstadoQr.Aprobado || usuarioId is null)
            return new QrEstadoResponse(estado.ToString().ToLowerInvariant());

        var usuario = await _dbContext.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Activo, ct);
        if (usuario is null)
            return new QrEstadoResponse("noexiste");

        _qrLogin.Consumir(codigo); // un solo uso
        var login = await GenerateAuthResponseAsync(usuario, ct);
        return new QrEstadoResponse("aprobado", login);
    }

    public async Task CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNueva, CancellationToken ct = default)
    {
        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId && u.Activo, ct)
            ?? throw new UnauthorizedAccessException("Usuario no encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(passwordActual, usuario.PasswordHash))
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");

        if (string.IsNullOrWhiteSpace(passwordNueva) || passwordNueva.Length < 8)
            throw new ArgumentException("La nueva contraseña debe tener al menos 8 caracteres.");
        if (passwordNueva == passwordActual)
            throw new ArgumentException("La nueva contraseña debe ser distinta de la actual.");

        usuario.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(passwordNueva));
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<LoginResponse> RefreshAsync(RefreshRequest request, CancellationToken ct = default)
    {
        var refreshToken = await _dbContext.RefreshTokens
            .Include(rt => rt.Usuario)
                .ThenInclude(u => u.Rol)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct)
            ?? throw new UnauthorizedAccessException("Refresh token inválido.");

        if (!refreshToken.IsActive)
            throw new UnauthorizedAccessException("Refresh token expirado o revocado.");

        // Revocar el token actual
        refreshToken.Revoked = true;

        return await GenerateAuthResponseAsync(refreshToken.Usuario, ct);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var token = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, ct);

        if (token is not null)
        {
            token.Revoked = true;
            await _dbContext.SaveChangesAsync(ct);
        }
    }

    private async Task<LoginResponse> GenerateAuthResponseAsync(Usuario usuario, CancellationToken ct)
    {
        var rolNombre = usuario.Rol.Nombre;
        var jwt = _jwtService.GenerateToken(usuario, rolNombre);
        var refreshTokenStr = _jwtService.GenerateRefreshToken();
        var refreshDays = _config.GetValue("Jwt:RefreshExpirationDays", 7);

        var refreshToken = RefreshToken.Create(
            refreshTokenStr,
            DateTime.UtcNow.AddDays(refreshDays),
            usuario.Id);

        await _dbContext.RefreshTokens.AddAsync(refreshToken, ct);
        await _dbContext.SaveChangesAsync(ct);

        var expirationMinutes = _config.GetValue("Jwt:ExpirationMinutes", 60);

        return new LoginResponse(
            jwt,
            refreshTokenStr,
            DateTime.UtcNow.AddMinutes(expirationMinutes),
            new UsuarioInfo(usuario.Id, usuario.Email, usuario.NombreCompleto, rolNombre),
            usuario.DebeCambiarPassword);
    }
}
