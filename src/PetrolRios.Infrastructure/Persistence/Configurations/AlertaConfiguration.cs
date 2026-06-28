using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Persistence.Configurations;

public class AlertaConfiguration : IEntityTypeConfiguration<Alerta>
{
    public void Configure(EntityTypeBuilder<Alerta> builder)
    {
        builder.ToTable("alertas");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Descripcion).HasMaxLength(1000).IsRequired();
        builder.Property(a => a.EmpleadoCodigo).HasMaxLength(20);
        builder.Property(a => a.TransaccionReferencia).HasMaxLength(100);
        // Evidencia de la alerta. Se guarda y se lee siempre como cadena JSON completa (nada la
        // consulta con operadores jsonb), y necesitamos poder BUSCARLA como texto (placa/RUC/cliente/
        // nº de factura viven aquí). Por eso es `text`: `lower(text)` SÍ se traduce; `lower(jsonb)` no.
        builder.Property(a => a.MetadataJson).HasColumnType("text");

        // Alertas acumulables/escalables: conteo (default 1 para filas existentes) y fecha de actualización
        // (default ahora; las filas previas se igualan a FechaDeteccion en la migración para no re-ordenarlas).
        builder.Property(a => a.EventosAcumulados).HasDefaultValue(1);
        builder.Property(a => a.FechaActualizacion).HasDefaultValueSql("now()");

        // Indices para filtrado frecuente
        builder.HasIndex(a => a.FechaDeteccion);
        builder.HasIndex(a => a.FechaActualizacion);
        builder.HasIndex(a => new { a.EstacionId, a.TransaccionReferencia });
        builder.HasIndex(a => a.EstacionId);
        builder.HasIndex(a => a.NivelRiesgo);
        builder.HasIndex(a => a.TipoDetector);
        builder.HasIndex(a => a.Estado);
        builder.HasIndex(a => a.Ambito);
        builder.HasIndex(a => new { a.EstacionId, a.FechaDeteccion });
        builder.HasIndex(a => new { a.Ambito, a.EstacionId, a.FechaDeteccion });

        builder.HasOne(a => a.Estacion)
            .WithMany(e => e.Alertas)
            .HasForeignKey(a => a.EstacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.EjecucionJob)
            .WithMany(ej => ej.Alertas)
            .HasForeignKey(a => a.EjecucionJobId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
