using System.Reflection;

namespace PetrolRios.Detectors;

/// <summary>
/// Enriquecedor central de evidencia. Dada la <c>Fuente</c> de una anomalía (un DTO de Firebird:
/// <c>FacturaDto</c>, <c>CierreTurnoDto</c>, <c>CreditoDto</c>, <c>DespachoDto</c>…), copia a la evidencia
/// (<c>Metadata</c>) los campos IDENTIFICABLES estándar — RUC, número de documento, placa, cliente, turno,
/// fecha, monto, forma de pago — para que TODA alerta los traiga sin que cada regla los repita a mano.
///
/// Es la pieza que vuelve la evidencia "automática y escalable": una regla nueva solo fija <c>Fuente</c> y
/// hereda estos campos. Mapea por NOMBRE de propiedad del DTO y NUNCA pisa una clave que la regla ya puso
/// (la regla manda). Los nombres de clave coinciden con las etiquetas e hipervínculos del frontend.
/// </summary>
public static class EvidenciaEnriquecida
{
    // Nombre de propiedad del DTO → clave de evidencia (la que el frontend etiqueta / hace buscable).
    private static readonly IReadOnlyDictionary<string, string> Mapa =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["RucCliente"] = "Ruc",
            ["NumeroDocumento"] = "NumeroDocumento",
            ["Placa"] = "Placa",
            ["CodigoCliente"] = "Cliente",
            ["NumeroTurno"] = "NumeroTurno",
            ["CodigoPago"] = "FormaPago",
            ["FechaDocumento"] = "Fecha",
            ["FechaFin"] = "Fecha",
            ["FechaCabecera"] = "Fecha",
            ["FechaAnulacion"] = "Fecha",
            ["FechaDespacho"] = "Fecha",
            ["TotalNeto"] = "Monto",
        };

    /// <summary>
    /// Refleja los campos identificables de <paramref name="fuente"/> hacia <paramref name="metadata"/>.
    /// No hace nada si la fuente es nula. No pisa claves existentes ni copia valores vacíos/cero.
    /// </summary>
    public static void Enriquecer(Dictionary<string, object> metadata, object? fuente)
    {
        if (fuente is null) return;

        foreach (var prop in fuente.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!Mapa.TryGetValue(prop.Name, out var clave)) continue;
            if (metadata.ContainsKey(clave)) continue;          // la regla ya lo puso: respetar
            var valor = prop.GetValue(fuente);
            if (EsVacio(valor)) continue;                        // no ensuciar con vacíos/cero
            metadata[clave] = Normalizar(valor!);
        }
    }

    private static bool EsVacio(object? v) => v switch
    {
        null => true,
        string s => string.IsNullOrWhiteSpace(s),
        int i => i == 0,
        double d => d == 0,
        decimal m => m == 0,
        DateTime dt => dt == default,
        _ => false,
    };

    private static object Normalizar(object v) => v switch
    {
        string s => s.Trim(),
        DateTime dt => dt.ToString("yyyy-MM-dd HH:mm"),
        _ => v,
    };
}
