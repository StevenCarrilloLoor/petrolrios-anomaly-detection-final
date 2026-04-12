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
    private readonly IConfiguration _config;

    public AuthService(
        PetrolRiosDbContext dbContext,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IConfiguration config)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _config = config;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var usuario = await _dbContext.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Activo, ct)
            ?? throw new UnauthorizedAccessException("Credenciales inválidas.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        return await GenerateAuthResponseAsync(usuario, ct);
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
            new UsuarioInfo(usuario.Id, usuario.Email, usuario.NombreCompleto, rolNombre));
    }
}
