using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Infrastructure.Persistence.Repositories;

public class ReglaDeteccionRepository : RepositoryBase<ReglaDeteccion>, IReglaDeteccionRepository
{
    public ReglaDeteccionRepository(PetrolRiosDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ReglaDeteccion>> GetByTipoDetectorAsync(TipoDetector tipo, CancellationToken ct = default) =>
        await DbSet.Where(r => r.TipoDetector == tipo && r.Activa).ToListAsync(ct);

    public async Task<IReadOnlyList<ReglaDeteccion>> GetActivasAsync(CancellationToken ct = default) =>
        await DbSet.Where(r => r.Activa).OrderBy(r => r.TipoDetector).ThenBy(r => r.Nombre).ToListAsync(ct);
}
