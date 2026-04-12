using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : RepositoryBase<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(PetrolRiosDbContext context) : base(context) { }

    public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await DbSet.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<Usuario?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default) =>
        await DbSet
            .Include(u => u.Rol)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken), ct);
}
