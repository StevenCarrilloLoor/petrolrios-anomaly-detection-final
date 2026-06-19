using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public class EsquemaTablaConfiguration : IEntityTypeConfiguration<EsquemaTabla>
{
    public void Configure(EntityTypeBuilder<EsquemaTabla> builder)
    {
        builder.ToTable("esquemas_tabla");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Tabla).HasMaxLength(100).IsRequired();
        builder.Property(e => e.ColumnasJson).HasColumnType("jsonb").IsRequired();
        builder.Property(e => e.EstacionCodigo).HasMaxLength(50);

        builder.HasIndex(e => e.Tabla).IsUnique();
    }
}
