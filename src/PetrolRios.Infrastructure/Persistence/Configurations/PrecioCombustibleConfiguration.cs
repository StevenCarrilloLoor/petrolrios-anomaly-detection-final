using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public sealed class PrecioCombustibleConfiguration : IEntityTypeConfiguration<PrecioCombustible>
{
    public void Configure(EntityTypeBuilder<PrecioCombustible> builder)
    {
        builder.ToTable("precios_combustible");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Producto).IsRequired();
        builder.Property(p => p.PrecioGalon).HasColumnType("numeric(8,4)").IsRequired();
        builder.Property(p => p.Subsidio).HasColumnType("numeric(8,4)");
        builder.Property(p => p.Fuente).HasMaxLength(200).IsRequired();
        builder.Property(p => p.PrecioApi).HasColumnType("numeric(8,4)");
        builder.Property(p => p.FuenteApi).HasMaxLength(80).IsRequired();

        // Un registro por producto: el precio vigente. Los cambios de banda se aplican por upsert.
        builder.HasIndex(p => p.Producto).IsUnique();
    }
}
