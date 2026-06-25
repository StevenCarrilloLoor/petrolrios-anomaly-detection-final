using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public class AlertaVistaConfiguration : IEntityTypeConfiguration<AlertaVista>
{
    public void Configure(EntityTypeBuilder<AlertaVista> builder)
    {
        builder.ToTable("alertas_vistas");
        builder.HasKey(v => v.Id);

        // Un usuario marca una alerta como vista una sola vez.
        builder.HasIndex(v => new { v.AlertaId, v.UsuarioId }).IsUnique();
        builder.HasIndex(v => v.UsuarioId);
    }
}
