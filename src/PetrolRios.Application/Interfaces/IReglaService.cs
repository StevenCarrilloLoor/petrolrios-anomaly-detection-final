using PetrolRios.Application.DTOs.Reglas;

namespace PetrolRios.Application.Interfaces;

public interface IReglaService
{
    Task<IReadOnlyList<ReglaDeteccionResponse>> GetAllAsync(CancellationToken ct = default);
    Task<ReglaDeteccionResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ReglaDeteccionResponse> CreateAsync(CrearReglaRequest request, CancellationToken ct = default);
    Task<ReglaDeteccionResponse> UpdateAsync(int id, ActualizarReglaRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
