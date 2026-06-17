using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PetrolRios.Application.DTOs.Usuarios;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

public sealed class UsuarioService : IUsuarioService
{
    private readonly PetrolRiosDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailNotificacionService _email;
    private readonly IConfiguration _config;

    public UsuarioService(
        PetrolRiosDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEmailNotificacionService email,
        IConfiguration config)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _email = email;
        _config = config;
    }

    public async Task<IReadOnlyList<UsuarioResponse>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.Usuarios
            .Include(u => u.Rol)
            .OrderBy(u => u.NombreCompleto)
            .Select(u => MapToResponse(u))
            .ToListAsync(ct);
    }

    public async Task<UsuarioResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var usuario = await _dbContext.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        return usuario is null ? null : MapToResponse(usuario);
    }

    public async Task<UsuarioResponse> CreateAsync(CrearUsuarioRequest request, CancellationToken ct = default)
    {
        var existente = await _unitOfWork.Usuarios.GetByEmailAsync(request.Email, ct);
        if (existente is not null)
            throw new InvalidOperationException($"Ya existe un usuario con email '{request.Email}'.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var usuario = Usuario.Create(request.Email, request.NombreCompleto, passwordHash, request.RolId);
        var token = usuario.GenerarTokenVerificacion();

        await _dbContext.Usuarios.AddAsync(usuario, ct);
        await _dbContext.SaveChangesAsync(ct);

        // Enviar el correo de verificación (si el SMTP está configurado).
        await EnviarCorreoVerificacionAsync(usuario, token, ct);

        // Recargar con rol
        await _dbContext.Entry(usuario).Reference(u => u.Rol).LoadAsync(ct);
        return MapToResponse(usuario);
    }

    public async Task<bool> VerificarEmailAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;
        var usuario = await _dbContext.Usuarios
            .FirstOrDefaultAsync(u => u.TokenVerificacionEmail == token, ct);
        if (usuario is null) return false;

        var ok = usuario.VerificarEmail(token);
        if (ok) await _dbContext.SaveChangesAsync(ct);
        return ok;
    }

    public async Task ReenviarVerificacionAsync(string email, CancellationToken ct = default)
    {
        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (usuario is null || usuario.EmailVerificado) return;

        var token = usuario.GenerarTokenVerificacion();
        await _dbContext.SaveChangesAsync(ct);
        await EnviarCorreoVerificacionAsync(usuario, token, ct);
    }

    private async Task EnviarCorreoVerificacionAsync(Usuario usuario, string token, CancellationToken ct)
    {
        if (!_email.Habilitado) return; // sin SMTP configurado no se envía

        var frontendUrl = (_config["App:FrontendUrl"] ?? "http://localhost:5173").TrimEnd('/');
        var enlace = $"{frontendUrl}/verificar-correo?token={token}";

        var asunto = "Verifica tu correo electrónico — PetrolRíos";
        var cuerpo =
            $"<div style='font-family:Segoe UI,Arial,sans-serif;color:#0f172a'>" +
            $"<h2>Bienvenido a PetrolRíos</h2>" +
            $"<p>Hola {usuario.NombreCompleto}, confirma tu correo para activar tu cuenta.</p>" +
            $"<p style='margin:24px 0'>" +
            $"<a href='{enlace}' style='background:#2563eb;color:#fff;text-decoration:none;" +
            $"padding:12px 22px;border-radius:8px;font-weight:600'>Verificar correo electrónico</a></p>" +
            $"<p style='font-size:12px;color:#64748b'>Si el botón no funciona, copia este enlace:<br>{enlace}</p>" +
            $"<p style='font-size:12px;color:#64748b'>Este enlace caduca en 48 horas. Si no creaste esta cuenta, ignora este correo.</p>" +
            $"</div>";

        await _email.EnviarAsync(asunto, cuerpo, new[] { usuario.Email }, ct);
    }

    public async Task<UsuarioResponse> UpdateAsync(int id, ActualizarUsuarioRequest request, CancellationToken ct = default)
    {
        var usuario = await _dbContext.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

        if (request.Activo.HasValue)
            usuario.Activo = request.Activo.Value;

        // Actualizar nombre y rol si vienen en la solicitud.
        usuario.ActualizarPerfil(request.NombreCompleto, request.RolId);

        await _dbContext.SaveChangesAsync(ct);
        // Recargar el rol por si cambió
        await _dbContext.Entry(usuario).Reference(u => u.Rol).LoadAsync(ct);
        return MapToResponse(usuario);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var usuario = await _dbContext.Usuarios.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

        usuario.Activo = false; // Soft delete
        await _dbContext.SaveChangesAsync(ct);
    }

    private static UsuarioResponse MapToResponse(Usuario u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        NombreCompleto = u.NombreCompleto,
        Rol = u.Rol.Nombre,
        RolId = u.RolId,
        Activo = u.Activo,
        EmailVerificado = u.EmailVerificado,
        CreatedAt = u.CreatedAt
    };
}
