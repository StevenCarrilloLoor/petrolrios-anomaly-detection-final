using System.Globalization;
using System.Text.RegularExpressions;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Infrastructure.Services.Precios;

/// <summary>
/// Extrae precios de combustible del HTML de una fuente pública (arch, camddepe, gasolinaecuador,
/// primicias…). Es defensivo a propósito: busca, cerca de cada palabra clave del combustible, un importe
/// "$X,XX(X)" (en Ecuador la coma es separador decimal) y se queda con el valor más frecuente dentro de un
/// rango plausible. Así tolera cambios de maquetación sin romperse, y descarta porcentajes y basura.
/// </summary>
public static partial class ParserPreciosHtml
{
    // palabra-clave del combustible, hasta 35 caracteres que NO sean '$' ni salto de línea, y luego "$precio".
    [GeneratedRegex(@"(?<fuel>Ecopa[ií]s|Di[eé]sel|S[uú]per|Extra)[^$\r\n]{0,35}\$\s*(?<precio>\d{1,2}[.,]\d{2,3})",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RegexPrecio();

    /// <summary>Parsea el HTML y devuelve el precio por combustible que pudo extraer (puede faltar alguno).</summary>
    public static IReadOnlyDictionary<TipoCombustible, decimal> Parsear(string? html)
    {
        var resultado = new Dictionary<TipoCombustible, decimal>();
        if (string.IsNullOrWhiteSpace(html)) return resultado;

        // Por combustible, todos los precios plausibles observados → nos quedamos con el más frecuente (moda).
        var candidatos = new Dictionary<TipoCombustible, List<decimal>>();
        foreach (Match m in RegexPrecio().Matches(html))
        {
            var fuel = MapearCombustible(m.Groups["fuel"].Value);
            if (fuel is null) continue;
            if (!TryParsePrecio(m.Groups["precio"].Value, out var precio)) continue;
            if (!EnRango(fuel.Value, precio)) continue;

            if (!candidatos.TryGetValue(fuel.Value, out var lista))
                candidatos[fuel.Value] = lista = [];
            lista.Add(precio);
        }

        foreach (var (fuel, lista) in candidatos)
        {
            // Moda (valor más repetido); ante empate, el menor (conservador).
            var moda = lista
                .GroupBy(p => p)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .First().Key;
            resultado[fuel] = moda;
        }
        return resultado;
    }

    private static TipoCombustible? MapearCombustible(string clave)
    {
        var k = clave.ToLowerInvariant();
        if (k.StartsWith("ecopa")) return TipoCombustible.Ecopais;
        if (k.StartsWith("di")) return TipoCombustible.Diesel;     // diésel/diesel
        if (k.StartsWith("s")) return TipoCombustible.Super;        // súper/super
        if (k.StartsWith("extra")) return TipoCombustible.Extra;
        return null;
    }

    private static bool TryParsePrecio(string crudo, out decimal precio)
    {
        // En Ecuador la coma es el separador decimal ("3,310" = 3.310). Los precios son < $10 (sin miles).
        var normal = crudo.Replace(",", ".");
        return decimal.TryParse(normal, NumberStyles.Number, CultureInfo.InvariantCulture, out precio);
    }

    private static bool EnRango(TipoCombustible fuel, decimal precio) => fuel switch
    {
        TipoCombustible.Super => precio is >= 2.00m and <= 10.00m,
        _ => precio is >= 1.50m and <= 6.00m
    };
}
