using PetrolRios.Domain.Entities;

namespace PetrolRios.Application.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Usuario?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}
