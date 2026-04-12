namespace PetrolRios.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IAlertaRepository Alertas { get; }
    IEstacionRepository Estaciones { get; }
    IUsuarioRepository Usuarios { get; }
    IReglaDeteccionRepository ReglasDeteccion { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
