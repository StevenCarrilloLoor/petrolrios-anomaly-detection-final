using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public sealed class PrecioCombustibleLogConfiguration : IEntityTypeConfiguration<PrecioCombustibleLog>
{
    public void Configure(EntityTypeBuilder<PrecioCombustibleLog> builder)
    {
        builder.ToTable("precios_combustible_log");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Producto).IsRequired();
        builder.Property(l => l.PrecioAnterior).HasColumnType("numeric(8,4)");
        builder.Property(l => l.PrecioNuevo).HasColumnType("numeric(8,4)");
        builder.Property(l => l.VariacionPorcentual).HasColumnType("numeric(8,2)");
        builder.Property(l => l.Fuente).HasMaxLength(40).IsRequired();
        builder.Property(l => l.Disparo).HasMaxLength(40).IsRequired();
        builder.Property(l => l.Resultado).HasMaxLength(40).IsRequired();
        builder.Property(l => l.FuenteDegradada).HasMaxLength(200);
        builder.Property(l => l.EtagRecibido).HasMaxLength(200);
        builder.Property(l => l.RawHtmlHash).HasMaxLength(80);

        // Consultas del historial: por producto y por fecha.
        builder.HasIndex(l => new { l.Producto, l.CreatedAt });
    }
}
