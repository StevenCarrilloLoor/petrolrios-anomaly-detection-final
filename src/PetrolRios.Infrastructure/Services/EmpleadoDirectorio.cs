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

        // El catálogo por estación es pequeño (los despachadores); se trae COMPLETO y se cruza en memoria.
        // Se cruza por código EXACTO y, si no, por código NORMALIZADO (núcleo numérico) para puentear
        // formatos distintos del mismo código entre el documento y el catálogo: en la Contaplus real el
        // despachador llega en la factura como DCTO.COD_VEND="DD0000010" pero VEND.COD_VEND="010" (o "10").
        // Normalizar ambos a su valor numérico ("DD0000010"→"10", "010"→"10") los empareja.
        var filas = await _db.Empleados
            .AsNoTracking()
            .Where(e => estaciones.Contains(e.EstacionId))
            .Select(e => new { e.EstacionId, e.Codigo, e.Nombre })
            .ToListAsync(ct);

        var exacto = new Dictionary<(int, string), string>();
        var porNucleo = new Dictionary<(int, string), string>();
        foreach (var f in filas)
        {
            var cod = (f.Codigo ?? string.Empty).Trim().ToUpperInvariant();
            exacto.TryAdd((f.EstacionId, cod), f.Nombre);
            var nucleo = NucleoCodigo(f.Codigo);
            if (nucleo is not null) porNucleo.TryAdd((f.EstacionId, nucleo), f.Nombre);
        }

        var dict = new Dictionary<(int, string), string>();
        foreach (var (estacionId, codigo) in normalizadas)
        {
            if (exacto.TryGetValue((estacionId, codigo), out var nombre))
            {
                dict[(estacionId, codigo)] = nombre;
                continue;
            }
            var nucleo = NucleoCodigo(codigo);
            if (nucleo is not null && porNucleo.TryGetValue((estacionId, nucleo), out var nombreNucleo))
                dict[(estacionId, codigo)] = nombreNucleo;
        }

        return new DirectorioEmpleados(dict);
    }

    /// <summary>Núcleo numérico de un código de empleado: sus dígitos sin ceros a la izquierda
    /// ("DD0000010"→"10", "010"→"10", "11"→"11"). Devuelve null si el código no tiene dígitos (se cruza
    /// solo por código exacto). Permite emparejar el COD_VEND del documento con el del catálogo VEND
    /// aunque vengan en formatos distintos.</summary>
    private static string? NucleoCodigo(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return null;
        var digitos = new string(codigo.Where(char.IsDigit).ToArray()).TrimStart('0');
        return digitos.Length > 0 ? digitos : null;
    }
}
