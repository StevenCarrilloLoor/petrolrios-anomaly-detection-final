using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public class ComentarioAlertaConfiguration : IEntityTypeConfiguration<ComentarioAlerta>
{
    public void Configure(EntityTypeBuilder<ComentarioAlerta> builder)
    {
        builder.ToTable("comentarios_alerta");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Texto).HasMaxLength(2000).IsRequired();

        builder.HasIndex(c => c.AlertaId);

        builder.HasOne(c => c.Alerta)
            .WithMany(a => a.Comentarios)
            .HasForeignKey(c => c.AlertaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Usuario)
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
