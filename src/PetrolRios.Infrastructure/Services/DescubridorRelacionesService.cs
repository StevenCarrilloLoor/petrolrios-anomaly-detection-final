using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.Interfaces;
using PetrolRios.Application.ReglasPersonalizadas;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

/// <summary>
/// Autodescubridor de relaciones entre fuentes. Sigue el método estándar de detección de claves
/// foráneas (data profiling): combina <b>similitud de nombre de columna</b> (mismo concepto de llave
/// o mismo nombre crudo) con <b>inclusión/solapamiento de valores</b> reales en staging. Así, cuando
/// se registra una tabla nueva, sus relaciones aparecen solas sin que nadie las defina.
/// </summary>
public sealed class DescubridorRelacionesService(PetrolRiosDbContext db) : IDescubridorRelaciones
{
    private const int MuestraFilas = 400;

    public async Task<int> DescubrirAsync(CancellationToken ct)
    {
        // 1) Inventario de fuentes -> nombres de campo (5 conocidas + configurables del staging).
        var fuentes = await InventarioFuentesAsync(ct);
        if (fuentes.Count < 2) return 0;

        // 2) Genera candidatos por concepto de llave compartido y por nombre de columna llave igual.
        var candidatos = GenerarCandidatos(fuentes);
        if (candidatos.Count == 0) return 0;

        // 3) Descarta los que ya existen (sembrados, manuales o auto previos).
        var existentes = await db.RelacionesTabla.AsNoTracking()
            .Select(r => new { r.FuenteOrigen, r.FuenteDestino, r.CampoOrigen, r.CampoDestino })
            .ToListAsync(ct);
        var yaHay = new HashSet<string>(
            existentes.Select(e => Clave(e.FuenteOrigen, e.FuenteDestino, e.CampoOrigen, e.CampoDestino)),
            StringComparer.OrdinalIgnoreCase);

        var nuevos = candidatos
            .GroupBy(c => Clave(c.O, c.D, c.CampoO, c.CampoD), StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .Where(c => !yaHay.Contains(Clave(c.O, c.D, c.CampoO, c.CampoD)))
            .ToList();
        if (nuevos.Count == 0) return 0;

        // 4) Valida cada candidato con el solapamiento de valores reales en staging. Si ambos lados
        //    tienen datos pero NO se solapan, es un falso positivo y se descarta. Si falta data, se
        //    crea por nombre (se confirmará cuando lleguen datos).
        var cache = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var creadas = 0;
        foreach (var c in nuevos)
        {
            var valoresO = await ValoresAsync(c.O, c.CampoO, cache, ct);
            var valoresD = await ValoresAsync(c.D, c.CampoD, cache, ct);
            if (valoresO.Count > 0 && valoresD.Count > 0 && !valoresO.Overlaps(valoresD)) continue;

            db.RelacionesTabla.Add(
                RelacionTabla.Create(c.O, c.D, c.CampoO, c.CampoD, c.Etiqueta, esAutomatica: true));
            creadas++;
        }
        if (creadas > 0) await db.SaveChangesAsync(ct);
        return creadas;
    }

    private sealed record Candidato(string O, string CampoO, string D, string CampoD, string Etiqueta);

    private static List<Candidato> GenerarCandidatos(Dictionary<string, List<string>> fuentes)
    {
        var candidatos = new List<Candidato>();
        var conceptosPorFuente = fuentes.ToDictionary(
            f => f.Key, f => ConceptosRelacion.ConceptosDe(f.Value), StringComparer.OrdinalIgnoreCase);

        // (a) Por concepto de llave compartido (cruza nombres lógicos con crudos).
        var conceptos = conceptosPorFuente.SelectMany(kv => kv.Value.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase);
        foreach (var concepto in conceptos)
        {
            var lista = conceptosPorFuente
                .Where(kv => kv.Value.ContainsKey(concepto))
                .Select(kv => (Fuente: kv.Key, Campo: kv.Value[concepto]))
                .ToList();
            for (var i = 0; i < lista.Count; i++)
                for (var j = 0; j < lista.Count; j++)
                    if (i != j)
                        candidatos.Add(new Candidato(lista[i].Fuente, lista[i].Campo, lista[j].Fuente,
                            lista[j].Campo,
                            $"{lista[j].Fuente} (relacionada por {ConceptosRelacion.Nombre(concepto)})"));
        }

        // (b) Por nombre de columna llave idéntico (cubre tablas nuevas con códigos propios compartidos).
        var porNombre = new Dictionary<string, List<(string Fuente, string Campo)>>();
        foreach (var (fuente, campos) in fuentes)
            foreach (var campo in campos)
                if (ConceptosRelacion.PareceLlave(campo))
                {
                    var n = ConceptosRelacion.Norm(campo);
                    if (!porNombre.TryGetValue(n, out var l)) porNombre[n] = l = [];
                    l.Add((fuente, campo));
                }
        foreach (var grupo in porNombre.Values)
        {
            var fuentesDistintas = grupo.Select(x => x.Fuente).Distinct(StringComparer.OrdinalIgnoreCase).Count();
            if (fuentesDistintas < 2) continue;
            for (var i = 0; i < grupo.Count; i++)
                for (var j = 0; j < grupo.Count; j++)
                    if (i != j && !string.Equals(grupo[i].Fuente, grupo[j].Fuente, StringComparison.OrdinalIgnoreCase))
                        candidatos.Add(new Candidato(grupo[i].Fuente, grupo[i].Campo, grupo[j].Fuente,
                            grupo[j].Campo, $"{grupo[j].Fuente} (relacionada por {grupo[j].Campo})"));
        }
        return candidatos;
    }

    private static string Clave(string o, string d, string co, string cd) =>
        $"{o.Trim()}|{d.Trim()}|{co.Trim()}|{cd.Trim()}".ToUpperInvariant();

    private async Task<Dictionary<string, List<string>>> InventarioFuentesAsync(CancellationToken ct)
    {
        var res = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (fuente, campos) in CatalogoReglasPersonalizadas.Fuentes)
            res[fuente] = campos.Select(c => c.Nombre).ToList();

        var conocidas = new HashSet<string>(res.Keys, StringComparer.OrdinalIgnoreCase);
        var tipos = await db.TransaccionesStaging.AsNoTracking()
            .Select(s => s.TipoTransaccion).Distinct().ToListAsync(ct);
        foreach (var tipo in tipos.Where(t => !conocidas.Contains(t)))
        {
            var muestra = await db.TransaccionesStaging.AsNoTracking()
                .Where(s => s.TipoTransaccion == tipo)
                .Select(s => s.DataJson).FirstOrDefaultAsync(ct);
            var campos = CamposDeJson(muestra);
            if (campos.Count > 0) res[tipo] = campos;
        }
        return res;
    }

    private static List<string> CamposDeJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Object
                ? doc.RootElement.EnumerateObject().Select(p => p.Name).ToList()
                : [];
        }
        catch { return []; }
    }

    private async Task<HashSet<string>> ValoresAsync(
        string fuente, string campo, Dictionary<string, HashSet<string>> cache, CancellationToken ct)
    {
        var clave = $"{fuente}|{campo}".ToUpperInvariant();
        if (cache.TryGetValue(clave, out var cached)) return cached;

        var filas = await db.TransaccionesStaging.AsNoTracking()
            .Where(s => s.TipoTransaccion == fuente)
            .OrderByDescending(s => s.Id)
            .Select(s => s.DataJson)
            .Take(MuestraFilas)
            .ToListAsync(ct);

        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var json in filas)
        {
            if (string.IsNullOrWhiteSpace(json)) continue;
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) continue;
                foreach (var prop in doc.RootElement.EnumerateObject())
                    if (string.Equals(prop.Name, campo, StringComparison.OrdinalIgnoreCase))
                    {
                        var v = prop.Value.ValueKind == JsonValueKind.String
                            ? prop.Value.GetString()
                            : prop.Value.ToString();
                        if (!string.IsNullOrWhiteSpace(v)) set.Add(v.Trim().ToUpperInvariant());
                        break;
                    }
            }
            catch { /* fila inválida: se ignora */ }
        }
        cache[clave] = set;
        return set;
    }
}
