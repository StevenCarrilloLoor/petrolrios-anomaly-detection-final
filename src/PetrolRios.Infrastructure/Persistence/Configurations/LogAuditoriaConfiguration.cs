using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public class LogAuditoriaConfiguration : IEntityTypeConfiguration<LogAuditoria>
{
    public void Configure(EntityTypeBuilder<LogAuditoria> builder)
    {
        builder.ToTable("logs_auditoria");
        builder.HasKey(la => la.Id);
        builder.Property(la => la.Accion).HasMaxLength(100).IsRequired();
        builder.Property(la => la.Entidad).HasMaxLength(100).IsRequired();
        builder.Property(la => la.DetalleJson).HasColumnType("jsonb");
        builder.Property(la => la.DireccionIp).HasMaxLength(50);

        builder.HasIndex(la => la.CreatedAt);

        builder.HasOne(la => la.Usuario)
            .WithMany(u => u.Logs)
            .HasForeignKey(la => la.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
