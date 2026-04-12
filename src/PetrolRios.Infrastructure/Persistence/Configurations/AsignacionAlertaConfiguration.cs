using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public class AsignacionAlertaConfiguration : IEntityTypeConfiguration<AsignacionAlerta>
{
    public void Configure(EntityTypeBuilder<AsignacionAlerta> builder)
    {
        builder.ToTable("asignaciones_alerta");
        builder.HasKey(aa => aa.Id);
        builder.Property(aa => aa.Comentario).HasMaxLength(2000);

        builder.HasOne(aa => aa.Alerta)
            .WithMany(a => a.Asignaciones)
            .HasForeignKey(aa => aa.AlertaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(aa => aa.Usuario)
            .WithMany(u => u.Asignaciones)
            .HasForeignKey(aa => aa.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
