using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.DTOs.Usuarios;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

public sealed class UsuarioService : IUsuarioService
{
    private readonly PetrolRiosDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;

    public UsuarioService(PetrolRiosDbContext dbContext, IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
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

        await _dbContext.Usuarios.AddAsync(usuario, ct);
        await _dbContext.SaveChangesAsync(ct);

        // Recargar con rol
        await _dbContext.Entry(usuario).Reference(u => u.Rol).LoadAsync(ct);
        return MapToResponse(usuario);
    }

    public async Task<UsuarioResponse> UpdateAsync(int id, ActualizarUsuarioRequest request, CancellationToken ct = default)
    {
        var usuario = await _dbContext.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

        if (request.Activo.HasValue)
            usuario.Activo = request.Activo.Value;

        // NombreCompleto y RolId tienen setters privados — necesitamos reflexión o un método Update
        // Por ahora actualizamos lo que permiten los setters públicos
        await _dbContext.SaveChangesAsync(ct);
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
        CreatedAt = u.CreatedAt
    };
}
