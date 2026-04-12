using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Repositories;

public class EstacionRepository : RepositoryBase<Estacion>, IEstacionRepository
{
    public EstacionRepository(PetrolRiosDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Estacion>> GetActivasAsync(CancellationToken ct = default) =>
        await DbSet.Where(e => e.Activa).OrderBy(e => e.Nombre).ToListAsync(ct);

    public async Task<Estacion?> GetByCodigoAsync(string codigo, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(e => e.Codigo == codigo, ct);

    public async Task<EstacionWatermark?> GetWatermarkAsync(int estacionId, CancellationToken ct = default) =>
        await Context.EstacionWatermarks.FirstOrDefaultAsync(ew => ew.EstacionId == estacionId, ct);

    public async Task UpsertWatermarkAsync(int estacionId, DateTime ultimaExtraccion, CancellationToken ct = default)
    {
        var watermark = await Context.EstacionWatermarks
            .FirstOrDefaultAsync(ew => ew.EstacionId == estacionId, ct);

        if (watermark is null)
        {
            await Context.EstacionWatermarks.AddAsync(
                EstacionWatermark.Create(estacionId, ultimaExtraccion), ct);
        }
        else
        {
            watermark.UltimaExtraccion = ultimaExtraccion;
        }
    }
}
