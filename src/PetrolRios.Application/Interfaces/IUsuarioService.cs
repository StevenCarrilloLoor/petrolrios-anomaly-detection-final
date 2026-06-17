using PetrolRios.Application.DTOs.Usuarios;

namespace PetrolRios.Application.Interfaces;

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioResponse>> GetAllAsync(CancellationToken ct = default);
    Task<UsuarioResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<UsuarioResponse> CreateAsync(CrearUsuarioRequest request, CancellationToken ct = default);
    Task<UsuarioResponse> UpdateAsync(int id, ActualizarUsuarioRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Verifica el correo de un usuario a partir del token recibido por email.</summary>
    Task<bool> VerificarEmailAsync(string token, CancellationToken ct = default);

    /// <summary>Reenvía el correo de verificación a un usuario (si existe y no está verificado).</summary>
    Task ReenviarVerificacionAsync(string email, CancellationToken ct = default);
}
