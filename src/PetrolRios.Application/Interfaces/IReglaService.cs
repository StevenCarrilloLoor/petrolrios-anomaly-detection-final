using PetrolRios.Application.DTOs.Reglas;

namespace PetrolRios.Application.Interfaces;

public interface IReglaService
{
    Task<IReadOnlyList<ReglaDeteccionResponse>> GetAllAsync(CancellationToken ct = default);
    Task<ReglaDeteccionResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ReglaDeteccionResponse> CreateAsync(CrearReglaRequest request, CancellationToken ct = default);
    Task<ReglaDeteccionResponse> UpdateAsync(int id, ActualizarReglaRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Restablece todas las reglas de un detector a sus valores predeterminados de fábrica
    /// (umbral, carril, activa y aviso por correo), por si se editaron y se quiere volver al estado
    /// original. Devuelve las reglas actualizadas.
    /// </summary>
    Task<IReadOnlyList<ReglaDeteccionResponse>> RestablecerDetectorAsync(string tipoDetector, CancellationToken ct = default);
}
