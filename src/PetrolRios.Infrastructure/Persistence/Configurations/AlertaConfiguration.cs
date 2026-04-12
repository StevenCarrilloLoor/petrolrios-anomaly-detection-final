using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public class AlertaConfiguration : IEntityTypeConfiguration<Alerta>
{
    public void Configure(EntityTypeBuilder<Alerta> builder)
    {
        builder.ToTable("alertas");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Descripcion).HasMaxLength(1000).IsRequired();
        builder.Property(a => a.EmpleadoCodigo).HasMaxLength(20);
        builder.Property(a => a.TransaccionReferencia).HasMaxLength(100);
        builder.Property(a => a.MetadataJson).HasColumnType("jsonb");

        // Indices para filtrado frecuente
        builder.HasIndex(a => a.FechaDeteccion);
        builder.HasIndex(a => a.EstacionId);
        builder.HasIndex(a => a.NivelRiesgo);
        builder.HasIndex(a => a.TipoDetector);
        builder.HasIndex(a => a.Estado);
        builder.HasIndex(a => new { a.EstacionId, a.FechaDeteccion });

        builder.HasOne(a => a.Estacion)
            .WithMany(e => e.Alertas)
            .HasForeignKey(a => a.EstacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.EjecucionJob)
            .WithMany(ej => ej.Alertas)
            .HasForeignKey(a => a.EjecucionJobId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
