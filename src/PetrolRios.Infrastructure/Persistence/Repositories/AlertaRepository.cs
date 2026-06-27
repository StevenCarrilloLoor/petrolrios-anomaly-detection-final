using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Infrastructure.Persistence.Repositories;

public class AlertaRepository : RepositoryBase<Alerta>, IAlertaRepository
{
    public AlertaRepository(PetrolRiosDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Alerta>> GetByEstacionAsync(int estacionId, CancellationToken ct = default) =>
        await DbSet
            .Where(a => a.EstacionId == estacionId)
            .OrderByDescending(a => a.FechaDeteccion)
            .Include(a => a.Estacion)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Alerta>> GetFilteredAsync(
        TipoDetector? tipo, NivelRiesgo? nivel, EstadoAlerta? estado,
        int? estacionId, DateTime? desde, DateTime? hasta,
        int page, int pageSize, string? buscar, CancellationToken ct)
    {
        var query = ApplyFilters(tipo, nivel, estado, estacionId, desde, hasta, buscar);
        return await query
            .OrderByDescending(a => a.FechaDeteccion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(a => a.Estacion)
            .ToListAsync(ct);
    }

    public async Task<int> GetFilteredCountAsync(
        TipoDetector? tipo, NivelRiesgo? nivel, EstadoAlerta? estado,
        int? estacionId, DateTime? desde, DateTime? hasta, string? buscar, CancellationToken ct)
    {
        return await ApplyFilters(tipo, nivel, estado, estacionId, desde, hasta, buscar).CountAsync(ct);
    }

    public async Task<int> CountByEmpleadoAndTipoAsync(
        string empleadoCodigo, TipoDetector tipo, DateTime desde, CancellationToken ct) =>
        await DbSet.CountAsync(
            a => a.EmpleadoCodigo == empleadoCodigo
                 && a.TipoDetector == tipo
                 && a.FechaDeteccion >= desde, ct);

    private IQueryable<Alerta> ApplyFilters(
        TipoDetector? tipo, NivelRiesgo? nivel, EstadoAlerta? estado,
        int? estacionId, DateTime? desde, DateTime? hasta, string? buscar)
    {
        // La bandeja de auditoría muestra SOLO alertas de ámbito Auditoría (fraude).
        // Los problemas operativos (turno sin cerrar, despacho no facturado, campos
        // faltantes) van exclusivamente a "Problemas de estación" y al Monitor de estación,
        // para no confundir a los auditores con incidencias que resuelve la propia estación.
        IQueryable<Alerta> query = DbSet.Where(a => a.Ambito == AmbitoAlerta.Auditoria);
        if (tipo.HasValue) query = query.Where(a => a.TipoDetector == tipo.Value);
        if (nivel.HasValue) query = query.Where(a => a.NivelRiesgo == nivel.Value);
        if (estado.HasValue) query = query.Where(a => a.Estado == estado.Value);
        if (estacionId.HasValue) query = query.Where(a => a.EstacionId == estacionId.Value);
        if (desde.HasValue) query = query.Where(a => a.FechaDeteccion >= desde.Value);
        if (hasta.HasValue) query = query.Where(a => a.FechaDeteccion <= hasta.Value);

        // Búsqueda libre (la pidió auditoría): por placa, RUC, nº de factura, cliente o código de
        // empleado. Esos datos viven en la descripción, la referencia, la evidencia (MetadataJson) o
        // el código de empleado, así que se busca en esos campos con coincidencia parcial e
        // insensible a mayúsculas. `ToLower().Contains` se traduce a un LIKE en PostgreSQL.
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim().ToLower();
            query = query.Where(a =>
                a.Descripcion.ToLower().Contains(termino)
                || (a.TransaccionReferencia != null && a.TransaccionReferencia.ToLower().Contains(termino))
                || (a.MetadataJson != null && a.MetadataJson.ToLower().Contains(termino))
                || (a.EmpleadoCodigo != null && a.EmpleadoCodigo.ToLower().Contains(termino)));
        }
        return query;
    }
}
