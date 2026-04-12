using PetrolRios.Domain.Entities;

namespace PetrolRios.Application.Interfaces;

public interface IEstacionRepository : IRepository<Estacion>
{
    Task<IReadOnlyList<Estacion>> GetActivasAsync(CancellationToken ct = default);
    Task<Estacion?> GetByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<EstacionWatermark?> GetWatermarkAsync(int estacionId, CancellationToken ct = default);
    Task UpsertWatermarkAsync(int estacionId, DateTime ultimaExtraccion, CancellationToken ct = default);
}
