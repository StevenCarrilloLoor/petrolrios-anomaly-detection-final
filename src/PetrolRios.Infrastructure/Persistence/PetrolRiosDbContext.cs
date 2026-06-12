using Microsoft.EntityFrameworkCore;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence;

public class PetrolRiosDbContext : DbContext
{
    public PetrolRiosDbContext(DbContextOptions<PetrolRiosDbContext> options) : base(options) { }

    public DbSet<Alerta> Alertas => Set<Alerta>();
    public DbSet<AsignacionAlerta> AsignacionesAlerta => Set<AsignacionAlerta>();
    public DbSet<ComentarioAlerta> ComentariosAlerta => Set<ComentarioAlerta>();
    public DbSet<EjecucionJob> EjecucionesJob => Set<EjecucionJob>();
    public DbSet<Estacion> Estaciones => Set<Estacion>();
    public DbSet<EstacionWatermark> EstacionWatermarks => Set<EstacionWatermark>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ReglaDeteccion> ReglasDeteccion => Set<ReglaDeteccion>();
    public DbSet<ReglaPersonalizada> ReglasPersonalizadas => Set<ReglaPersonalizada>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<TransaccionStaging> TransaccionesStaging => Set<TransaccionStaging>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PetrolRiosDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
