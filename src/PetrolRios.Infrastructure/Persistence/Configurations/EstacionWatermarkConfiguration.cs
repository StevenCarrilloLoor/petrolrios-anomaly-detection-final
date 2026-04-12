using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public class EstacionWatermarkConfiguration : IEntityTypeConfiguration<EstacionWatermark>
{
    public void Configure(EntityTypeBuilder<EstacionWatermark> builder)
    {
        builder.ToTable("estacion_watermarks");
        builder.HasKey(ew => ew.Id);
        builder.HasIndex(ew => ew.EstacionId).IsUnique();

        builder.HasOne(ew => ew.Estacion)
            .WithOne(e => e.Watermark)
            .HasForeignKey<EstacionWatermark>(ew => ew.EstacionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
