using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public class FuenteDatosConfiguration : IEntityTypeConfiguration<FuenteDatos>
{
    public void Configure(EntityTypeBuilder<FuenteDatos> builder)
    {
        builder.ToTable("fuentes_datos");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(f => f.Tabla).HasMaxLength(100).IsRequired();
        builder.Property(f => f.ColumnaWatermark).HasMaxLength(100);
        builder.Property(f => f.Descripcion).HasMaxLength(500);

        // Una tabla no se registra dos veces en el catálogo central.
        builder.HasIndex(f => f.Tabla).IsUnique();
    }
}
