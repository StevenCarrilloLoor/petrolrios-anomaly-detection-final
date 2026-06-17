using PetrolRios.Application.DTOs.Auth;

namespace PetrolRios.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<LoginResponse> RefreshAsync(RefreshRequest request, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNueva, CancellationToken ct = default);
}
