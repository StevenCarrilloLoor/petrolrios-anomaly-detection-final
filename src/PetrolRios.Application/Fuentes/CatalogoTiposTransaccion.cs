namespace PetrolRios.Application.Fuentes;

/// <summary>
/// Traduce el "tipo" con que una transacción llega al staging a un par
/// (nombre natural en español, tabla técnica de Contaplus), para mostrarlo en los logs como
/// "Factura (DCTO)". Cubre las 7 fuentes built-in (y tolera variantes de nombre que enviaron
/// agentes antiguos, p. ej. "Anulaciones"); para las fuentes configurables del selector el tipo
/// ES el nombre de la fuente y la tabla se resuelve con el catálogo (parámetro <c>tablaConfigurable</c>).
/// </summary>
public static class CatalogoTiposTransaccion
{
    /// <summary>tipo (como llega al staging) → (nombre natural, tabla técnica).</summary>
    private static readonly IReadOnlyDictionary<string, (string Natural, string Tabla)> BuiltIns =
        new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase)
        {
            ["Factura"] = ("Factura", "DCTO"),
            ["DetalleFactura"] = ("Detalle de factura", "DESP"),
            ["CierreTurno"] = ("Cierre de turno", "TURN"),
            ["DepositoTurno"] = ("Depósito de turno", "TURN_DEPO"),
            ["Anulacion"] = ("Anulación", "ANUL"),
            ["Anulaciones"] = ("Anulación", "ANUL"),   // variante enviada por agentes antiguos
            ["Credito"] = ("Crédito", "CRED_CABE"),
            ["TarjetaTurno"] = ("Tarjeta de turno", "TURN_TARJ"),
            ["Dcto"] = ("Documento", "DCTO"),          // fuente "Dcto" del selector (duplicaba a Factura)
        };

    /// <summary>
    /// Devuelve (nombre natural, tabla técnica) de un tipo del staging. Si no es built-in, el tipo se
    /// usa como nombre natural y la tabla se toma de <paramref name="tablaConfigurable"/> (catálogo del
    /// selector) cuando se conoce; si no, queda vacía.
    /// </summary>
    public static (string Natural, string Tabla) Resolver(string? tipo, string? tablaConfigurable = null)
    {
        var t = (tipo ?? string.Empty).Trim();
        if (t.Length == 0) return (string.Empty, string.Empty);
        if (BuiltIns.TryGetValue(t, out var info)) return info;
        return (t, (tablaConfigurable ?? string.Empty).Trim());
    }

    /// <summary>Etiqueta lista para mostrar: "Natural (TABLA)" — o solo "Natural" si no hay tabla.</summary>
    public static string Etiqueta(string? tipo, string? tablaConfigurable = null)
    {
        var (natural, tabla) = Resolver(tipo, tablaConfigurable);
        return string.IsNullOrWhiteSpace(tabla) ? natural : $"{natural} ({tabla})";
    }
}
