using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public class ReglaDeteccionConfiguration : IEntityTypeConfiguration<ReglaDeteccion>
{
    public void Configure(EntityTypeBuilder<ReglaDeteccion> builder)
    {
        builder.ToTable("reglas_deteccion");
        builder.HasKey(rd => rd.Id);
        builder.Property(rd => rd.Nombre).HasMaxLength(150).IsRequired();
        builder.Property(rd => rd.Descripcion).HasMaxLength(500).IsRequired();
        builder.Property(rd => rd.ParametroNombre).HasMaxLength(100).IsRequired();

        builder.HasIndex(rd => rd.TipoDetector);
        builder.HasIndex(rd => new { rd.TipoDetector, rd.ParametroNombre }).IsUnique();
    }
}
