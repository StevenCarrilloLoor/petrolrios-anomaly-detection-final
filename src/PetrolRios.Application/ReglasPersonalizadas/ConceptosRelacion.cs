namespace PetrolRios.Application.ReglasPersonalizadas;

/// <summary>
/// Conceptos de unión entre tablas (llaves de negocio) y sus variantes de nombre de columna —
/// tanto los nombres lógicos de las fuentes conocidas (CodigoCliente) como los códigos crudos de
/// Contaplus (COD_CLIE). Es la base del autodescubrimiento de relaciones: dos fuentes que comparten
/// un mismo concepto (p. ej. ambas tienen el "cliente") se pueden cruzar por esa llave, aunque la
/// columna se llame distinto en cada una.
///
/// Es 100% por datos: agregar un concepto o una variante = una entrada; el descubridor lo usa sin
/// tocar lógica. La validación final la hace el solapamiento de valores reales en staging.
/// </summary>
public static class ConceptosRelacion
{
    /// <summary>concepto → variantes de nombre de columna que lo representan.</summary>
    public static readonly IReadOnlyDictionary<string, string[]> Conceptos =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["cliente"] = ["CodigoCliente", "COD_CLIE", "CodigoSocio", "COD_SOCI", "COD_CLIENTE"],
            ["vendedor"] = ["CodigoVendedor", "COD_VEND", "COD_VENDEDOR"],
            ["empleado"] = ["CodigoEmpleado", "COD_EMPL", "NUM_EMPL"],
            ["chofer"] = ["CodigoChofer", "COD_CHOF"],
            ["turno"] = ["NumeroTurno", "NUM_TURN"],
            ["producto"] = ["CodigoProducto", "COD_PROD"],
            ["banco"] = ["CodigoBanco", "COD_BANC", "COD_BANCO"],
            ["manguera"] = ["CodigoManguera", "COD_MANG"],
            ["documento"] = ["NumeroDocumento", "NUM_DCTO", "SecuenciaDocumento", "SEC_DCTO"],
            ["tanque"] = ["COD_TANQ", "CodigoTanque"],
            ["placa"] = ["Placa", "PLA_DCTO", "PLA_CABE"],
            ["estacion"] = ["COD_ESTA", "CodigoEstacion"],
        };

    /// <summary>Nombre legible del concepto para construir la etiqueta de la relación.</summary>
    public static string Nombre(string concepto) => concepto.ToLowerInvariant() switch
    {
        "cliente" => "cliente",
        "vendedor" => "vendedor",
        "empleado" => "empleado",
        "chofer" => "chofer",
        "turno" => "turno",
        "producto" => "producto",
        "banco" => "banco/tarjeta",
        "manguera" => "manguera",
        "documento" => "documento/factura",
        "tanque" => "tanque",
        "placa" => "placa",
        "estacion" => "estación",
        _ => concepto
    };

    /// <summary>
    /// Para una fuente (dada su lista de nombres de campo), devuelve concepto→campo real: qué columna
    /// de esa fuente representa cada concepto que tiene. Toma la primera variante que exista.
    /// </summary>
    public static IReadOnlyDictionary<string, string> ConceptosDe(IEnumerable<string> campos)
    {
        var porNorma = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in campos)
            if (!string.IsNullOrWhiteSpace(c))
                porNorma.TryAdd(Norm(c), c.Trim());

        var resultado = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (concepto, variantes) in Conceptos)
            foreach (var v in variantes)
                if (porNorma.TryGetValue(Norm(v), out var real))
                {
                    resultado[concepto] = real;
                    break;
                }
        return resultado;
    }

    /// <summary>true si un nombre de columna "parece" una llave (para el cruce por nombre exacto).</summary>
    public static bool PareceLlave(string campo)
    {
        var n = Norm(campo);
        return n.StartsWith("COD") || n.StartsWith("NUM") || n.StartsWith("ID")
               || n.EndsWith("ID") || n.Contains("CODIGO");
    }

    /// <summary>Normaliza un nombre de columna: sin guiones bajos, sin espacios, en mayúsculas.</summary>
    public static string Norm(string s) =>
        (s ?? string.Empty).Trim().Replace("_", "").Replace(" ", "").ToUpperInvariant();
}
