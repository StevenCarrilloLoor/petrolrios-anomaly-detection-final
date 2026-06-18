using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public class EstacionConfiguration : IEntityTypeConfiguration<Estacion>
{
    public void Configure(EntityTypeBuilder<Estacion> builder)
    {
        builder.ToTable("estaciones");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Nombre).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Codigo).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Direccion).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Zona).HasMaxLength(50);
        builder.Property(e => e.CorreoContacto).HasMaxLength(200);
        builder.HasIndex(e => e.Codigo).IsUnique();
    }
}
