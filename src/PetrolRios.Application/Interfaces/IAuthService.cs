using PetrolRios.Application.DTOs.Auth;

namespace PetrolRios.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<LoginResponse> RefreshAsync(RefreshRequest request, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNueva, CancellationToken ct = default);

    // 2FA (TOTP)
    Task<Iniciar2faResponse> Iniciar2faAsync(int usuarioId, CancellationToken ct = default);
    Task Confirmar2faAsync(int usuarioId, string codigo, CancellationToken ct = default);
    Task Desactivar2faAsync(int usuarioId, string codigo, CancellationToken ct = default);
    Task<bool> Estado2faAsync(int usuarioId, CancellationToken ct = default);

    // Login por QR (estilo Steam)
    QrIniciarResponse QrIniciar();
    Task<bool> QrAprobarAsync(string codigo, int usuarioId, CancellationToken ct = default);
    Task<QrEstadoResponse> QrEstadoAsync(string codigo, CancellationToken ct = default);
}
