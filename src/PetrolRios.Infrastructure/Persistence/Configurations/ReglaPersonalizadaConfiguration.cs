using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public class ReglaPersonalizadaConfiguration : IEntityTypeConfiguration<ReglaPersonalizada>
{
    public void Configure(EntityTypeBuilder<ReglaPersonalizada> builder)
    {
        builder.ToTable("reglas_personalizadas");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Nombre).HasMaxLength(150).IsRequired();
        builder.Property(r => r.Descripcion).HasMaxLength(500).IsRequired();
        builder.Property(r => r.FuenteDatos).HasMaxLength(50).IsRequired();
        builder.Property(r => r.CondicionesJson).HasColumnType("jsonb").IsRequired();
        builder.Property(r => r.AgregacionJson).HasColumnType("jsonb");
        builder.Property(r => r.ExpresionAvanzada).HasMaxLength(2000);
        builder.Property(r => r.Ambito).HasMaxLength(20).IsRequired().HasDefaultValue("Auditoria");

        builder.HasIndex(r => r.Activa);
        builder.HasIndex(r => r.Nombre).IsUnique();
    }
}
