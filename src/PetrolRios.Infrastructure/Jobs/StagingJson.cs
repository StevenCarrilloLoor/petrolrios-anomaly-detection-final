using System.Text.Json;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Infrastructure.Jobs;

/// <summary>
/// Deserialización compartida del staging. La usan el job de detección (ciclo real) y el backtest
/// de reglas (vista previa), de modo que ambos interpretan los datos exactamente igual. Convierte
/// las filas <see cref="TransaccionStaging"/> en DTOs tipados o en diccionarios campo→valor para
/// las fuentes configurables (tablas arbitrarias del agente).
/// </summary>
public static class StagingJson
{
    /// <summary>Deserializa las filas de un tipo concreto a su DTO; descarta las malformadas.</summary>
    public static IReadOnlyList<T> DeserializarPorTipo<T>(
        IEnumerable<TransaccionStaging> staging, string tipoTransaccion) =>
        staging
            .Where(s => s.TipoTransaccion == tipoTransaccion)
            .Select(s =>
            {
                try { return JsonSerializer.Deserialize<T>(s.DataJson); }
                catch { return default; }
            })
            .Where(item => item is not null)
            .Cast<T>()
            .ToList();

    /// <summary>Convierte el JSON de una fila de fuente configurable en un diccionario campo→valor.</summary>
    public static IDictionary<string, object>? DeserializarDiccionario(string json)
    {
        try
        {
            var crudo = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            if (crudo is null) return null;
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var (clave, valor) in crudo)
                dict[clave] = ConvertirJsonElement(valor);
            return dict;
        }
        catch
        {
            return null;
        }
    }

    private static object ConvertirJsonElement(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.Number => e.GetDouble(),
        JsonValueKind.String => e.GetString() ?? "",
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => "",
        _ => e.ToString()
    };
}
