using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public class EjecucionJobConfiguration : IEntityTypeConfiguration<EjecucionJob>
{
    public void Configure(EntityTypeBuilder<EjecucionJob> builder)
    {
        builder.ToTable("ejecuciones_job");
        builder.HasKey(ej => ej.Id);
        builder.Property(ej => ej.ErrorDetalle).HasMaxLength(4000);
        builder.HasIndex(ej => ej.FechaInicio);
        builder.HasIndex(ej => ej.Estado);
    }
}
