using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public sealed class FuenteDatosEstacionEstadoConfiguration
    : IEntityTypeConfiguration<FuenteDatosEstacionEstado>
{
    public void Configure(EntityTypeBuilder<FuenteDatosEstacionEstado> builder)
    {
        builder.ToTable("fuentes_datos_estados_estacion");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Estado).HasMaxLength(40).IsRequired();
        builder.Property(e => e.UltimoError).HasMaxLength(1000);
        builder.HasIndex(e => new { e.FuenteDatosId, e.EstacionId }).IsUnique();

        builder.HasOne(e => e.FuenteDatos)
            .WithMany()
            .HasForeignKey(e => e.FuenteDatosId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Estacion)
            .WithMany()
            .HasForeignKey(e => e.EstacionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
