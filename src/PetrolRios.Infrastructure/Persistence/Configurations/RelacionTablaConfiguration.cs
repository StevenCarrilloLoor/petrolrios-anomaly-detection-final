using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public class RelacionTablaConfiguration : IEntityTypeConfiguration<RelacionTabla>
{
    public void Configure(EntityTypeBuilder<RelacionTabla> builder)
    {
        builder.ToTable("relaciones_tabla");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.FuenteOrigen).HasMaxLength(50).IsRequired();
        builder.Property(r => r.FuenteDestino).HasMaxLength(50).IsRequired();
        builder.Property(r => r.CampoOrigen).HasMaxLength(100).IsRequired();
        builder.Property(r => r.CampoDestino).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Etiqueta).HasMaxLength(150).IsRequired();

        // No registrar la misma relación (mismo origen→destino por los mismos campos) dos veces.
        builder.HasIndex(r => new { r.FuenteOrigen, r.FuenteDestino, r.CampoOrigen, r.CampoDestino })
            .IsUnique();
    }
}
