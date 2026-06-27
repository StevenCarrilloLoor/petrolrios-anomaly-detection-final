namespace PetrolRios.Application.Fuentes;

public static class FuenteDatosPolicy
{
    /// <summary>
    /// Tablas de Contaplus que el agente YA extrae automáticamente como fuentes "built-in"
    /// (tabla cruda → nombre de la fuente integrada). Registrarlas otra vez en el selector de
    /// tablas las DUPLICA (entran dos veces al staging, con nombres de campo distintos: crudos
    /// vs amigables) y confunde a quien crea reglas. Por eso el central rechaza registrarlas y el
    /// agente las omite si aparecen en el catálogo configurable.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> TablasBuiltIn =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["DCTO"] = "Factura",
            ["DESP"] = "DetalleFactura",
            ["TURN"] = "CierreTurno",
            ["TURN_DEPO"] = "DepositoTurno",
            ["ANUL"] = "Anulacion",
            ["CRED_CABE"] = "Credito",
            ["TURN_TARJ"] = "TarjetaTurno",
            ["LIQU"] = "Liquidacion",
        };

    /// <summary>¿La tabla ya se procesa automáticamente como una fuente built-in?</summary>
    public static bool TablaCubiertaPorBuiltIn(string? tabla) =>
        !string.IsNullOrWhiteSpace(tabla) && TablasBuiltIn.ContainsKey(tabla.Trim());

    /// <summary>Nombre de la fuente built-in que cubre esa tabla (null si ninguna).</summary>
    public static string? FuenteBuiltInDe(string? tabla) =>
        !string.IsNullOrWhiteSpace(tabla) && TablasBuiltIn.TryGetValue(tabla.Trim(), out var f) ? f : null;


    public static bool EsTipoTemporal(string? tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            return false;

        var normalizado = tipo.Trim().ToUpperInvariant();
        return normalizado.StartsWith("DATE", StringComparison.Ordinal)
               || normalizado.StartsWith("TIME", StringComparison.Ordinal)
               || normalizado.StartsWith("TIMESTAMP", StringComparison.Ordinal);
    }

    /// <summary>
    /// Firebird TIMESTAMP no almacena zona. Conserva exactamente los ticks del valor
    /// leído y elimina el Kind para reutilizarlo como parámetro del próximo SELECT.
    /// </summary>
    public static DateTime NormalizarCursorFirebird(DateTime fecha) =>
        DateTime.SpecifyKind(fecha, DateTimeKind.Unspecified);
}
