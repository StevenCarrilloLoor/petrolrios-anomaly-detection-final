using PetrolRios.Application.DTOs.Usuarios;

namespace PetrolRios.Application.Interfaces;

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioResponse>> GetAllAsync(CancellationToken ct = default);
    Task<UsuarioResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<UsuarioResponse> CreateAsync(CrearUsuarioRequest request, CancellationToken ct = default);
    Task<UsuarioResponse> UpdateAsync(int id, ActualizarUsuarioRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
