using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.Interfaces;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

/// <summary>
/// Resuelve (estación, código) → nombre contra el catálogo central <c>Empleado</c>. El catálogo es
/// pequeño (los despachadores de cada estación); se consulta solo por las claves necesarias.
/// </summary>
public sealed class EmpleadoDirectorio : IEmpleadoDirectorio
{
    private readonly PetrolRiosDbContext _db;

    public EmpleadoDirectorio(PetrolRiosDbContext db) => _db = db;

    public async Task<DirectorioEmpleados> CargarAsync(
        IEnumerable<(int EstacionId, string? Codigo)> claves, CancellationToken ct = default)
    {
        var normalizadas = claves
            .Where(k => !string.IsNullOrWhiteSpace(k.Codigo))
            .Select(k => (k.EstacionId, Codigo: k.Codigo!.Trim().ToUpperInvariant()))
            .Distinct()
            .ToList();
        if (normalizadas.Count == 0) return DirectorioEmpleados.Vacio;

        var estaciones = normalizadas.Select(k => k.EstacionId).Distinct().ToList();
        var codigos = normalizadas.Select(k => k.Codigo).Distinct().ToList();

        // Se sobre-consulta por (estaciones × códigos) y se filtra el par exacto en memoria.
        var filas = await _db.Empleados
            .AsNoTracking()
            .Where(e => estaciones.Contains(e.EstacionId) && codigos.Contains(e.Codigo))
            .Select(e => new { e.EstacionId, e.Codigo, e.Nombre })
            .ToListAsync(ct);

        var pares = new HashSet<(int, string)>(normalizadas.Select(k => (k.EstacionId, k.Codigo)));
        var dict = new Dictionary<(int, string), string>();
        foreach (var f in filas)
            if (pares.Contains((f.EstacionId, f.Codigo)))
                dict[(f.EstacionId, f.Codigo)] = f.Nombre;

        return new DirectorioEmpleados(dict);
    }
}
