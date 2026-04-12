using PetrolRios.Application.Interfaces;
using PetrolRios.Infrastructure.Persistence.Repositories;

namespace PetrolRios.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly PetrolRiosDbContext _context;
    private IAlertaRepository? _alertas;
    private IEstacionRepository? _estaciones;
    private IUsuarioRepository? _usuarios;
    private IReglaDeteccionRepository? _reglasDeteccion;

    public UnitOfWork(PetrolRiosDbContext context)
    {
        _context = context;
    }

    public IAlertaRepository Alertas =>
        _alertas ??= new AlertaRepository(_context);

    public IEstacionRepository Estaciones =>
        _estaciones ??= new EstacionRepository(_context);

    public IUsuarioRepository Usuarios =>
        _usuarios ??= new UsuarioRepository(_context);

    public IReglaDeteccionRepository ReglasDeteccion =>
        _reglasDeteccion ??= new ReglaDeteccionRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
