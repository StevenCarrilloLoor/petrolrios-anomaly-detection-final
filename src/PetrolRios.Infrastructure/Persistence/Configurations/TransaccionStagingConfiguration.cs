using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public class TransaccionStagingConfiguration : IEntityTypeConfiguration<TransaccionStaging>
{
    public void Configure(EntityTypeBuilder<TransaccionStaging> builder)
    {
        builder.ToTable("transacciones_staging");
        builder.HasKey(ts => ts.Id);
        builder.Property(ts => ts.TipoTransaccion).HasMaxLength(50).IsRequired();
        builder.Property(ts => ts.DataJson).HasColumnType("jsonb").IsRequired();

        builder.HasIndex(ts => ts.EstacionId);
        builder.HasIndex(ts => ts.Procesada);
        builder.HasIndex(ts => new { ts.EstacionId, ts.Procesada });
    }
}
