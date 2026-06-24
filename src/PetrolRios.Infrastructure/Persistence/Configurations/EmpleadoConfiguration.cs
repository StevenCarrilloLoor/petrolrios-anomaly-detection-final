using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public sealed class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("empleados");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Codigo).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Nombre).HasMaxLength(150).IsRequired();

        // Un código por estación (la misma estación no repite COD_VEND).
        builder.HasIndex(e => new { e.EstacionId, e.Codigo }).IsUnique();

        builder.HasOne(e => e.Estacion)
            .WithMany()
            .HasForeignKey(e => e.EstacionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
