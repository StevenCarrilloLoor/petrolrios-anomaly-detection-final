using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Application.Interfaces;

public interface IReglaDeteccionRepository : IRepository<ReglaDeteccion>
{
    Task<IReadOnlyList<ReglaDeteccion>> GetByTipoDetectorAsync(TipoDetector tipo, CancellationToken ct = default);
    Task<IReadOnlyList<ReglaDeteccion>> GetActivasAsync(CancellationToken ct = default);
}
