using PetrolRios.Application.DTOs.Firebird;

namespace PetrolRios.Application.ReglasPersonalizadas;

/// <summary>Condición de filtrado de una regla personalizada (se combinan con AND).</summary>
public sealed record CondicionRegla(string Campo, string Operador, string Valor);

/// <summary>Agregación opcional: agrupa los registros filtrados y compara el agregado con un umbral.</summary>
public sealed record AgregacionRegla(
    string AgruparPor,
    string Funcion,
    string? Campo,
    string Operador,
    double Umbral);

/// <summary>
/// Catálogo del motor de reglas personalizadas: fuentes de datos disponibles,
/// campos por fuente (con tipo y etiqueta), operadores y funciones de agregación.
/// Es la única fuente de verdad: la usan el builder de la interfaz (vía API),
/// la validación al guardar y el detector al evaluar.
/// </summary>
public static class CatalogoReglasPersonalizadas
{
    public sealed record CampoInfo(string Nombre, string Etiqueta, string Tipo); // Tipo: "numero" | "texto"

    public const string TipoNumero = "numero";
    public const string TipoTexto = "texto";

    public static readonly string[] OperadoresNumero = [">", ">=", "<", "<=", "=", "!="];
    public static readonly string[] OperadoresTexto = ["=", "!=", "contiene", "noContiene", "vacio", "noVacio"];
    public static readonly string[] Funciones = ["Conteo", "Suma", "Promedio"];

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<CampoInfo>> Fuentes =
        new Dictionary<string, IReadOnlyList<CampoInfo>>
        {
            ["Factura"] =
            [
                new("TotalNeto", "Total de la factura ($)", TipoNumero),
                new("Subtotal", "Subtotal ($)", TipoNumero),
                new("Descuento", "Descuento ($)", TipoNumero),
                new("Iva", "IVA ($)", TipoNumero),
                new("NumeroTurno", "Número de turno", TipoNumero),
                new("CodigoPago", "Forma de pago (EF/TC/CR…)", TipoTexto),
                new("Placa", "Placa del vehículo", TipoTexto),
                new("RucCliente", "RUC / cédula", TipoTexto),
                new("CodigoCliente", "Código de cliente", TipoTexto),
                new("CodigoVendedor", "Código de vendedor", TipoTexto)
            ],
            ["CierreTurno"] =
            [
                new("Faltante", "Faltante de caja ($)", TipoNumero),
                new("Sobrante", "Sobrante de caja ($)", TipoNumero),
                new("Ingresos", "Ingresos del turno ($)", TipoNumero),
                new("Egresos", "Egresos del turno ($)", TipoNumero),
                new("Creditos", "Créditos del turno ($)", TipoNumero),
                new("NumeroTurno", "Número de turno", TipoNumero),
                new("CodigoVendedor", "Código de vendedor", TipoTexto)
            ],
            ["DetalleFactura"] =
            [
                new("Cantidad", "Galones despachados", TipoNumero),
                new("ValorUnitario", "Precio por galón ($)", TipoNumero),
                new("VolumenTotal", "Valor total del despacho ($)", TipoNumero),
                new("CodigoProducto", "Código de producto", TipoTexto),
                new("NombreProducto", "Nombre de producto", TipoTexto),
                new("CodigoCliente", "Código de cliente", TipoTexto)
            ],
            ["Credito"] =
            [
                new("TotalCredito", "Monto del crédito ($)", TipoNumero),
                new("TotalInteres", "Interés total ($)", TipoNumero),
                new("PlazoCabecera", "Plazo (días)", TipoNumero),
                new("NumeroComprobante", "Número de comprobante", TipoNumero),
                new("CodigoSocio", "Código de socio/cliente", TipoTexto),
                new("CodigoBanco", "Código de banco", TipoTexto)
            ],
            ["TarjetaTurno"] =
            [
                new("Valor", "Valor de la transacción ($)", TipoNumero),
                new("Cantidad", "Cantidad de transacciones", TipoNumero),
                new("NumeroTurno", "Número de turno", TipoNumero),
                new("CodigoBanco", "Código de banco/tarjeta", TipoTexto)
            ]
        };

    /// <summary>Obtiene el valor de un campo de un registro de la fuente indicada.</summary>
    public static object? GetValor(string fuente, string campo, object registro) => (fuente, registro) switch
    {
        ("Factura", FacturaDto f) => campo switch
        {
            "TotalNeto" => f.TotalNeto,
            "Subtotal" => f.Subtotal,
            "Descuento" => f.Descuento,
            "Iva" => f.Iva,
            "NumeroTurno" => f.NumeroTurno,
            "CodigoPago" => f.CodigoPago.Trim(),
            "Placa" => f.Placa.Trim(),
            "RucCliente" => f.RucCliente.Trim(),
            "CodigoCliente" => f.CodigoCliente.Trim(),
            "CodigoVendedor" => f.CodigoVendedor.Trim(),
            _ => null
        },
        ("CierreTurno", CierreTurnoDto t) => campo switch
        {
            "Faltante" => t.Faltante,
            "Sobrante" => t.Sobrante,
            "Ingresos" => t.Ingresos,
            "Egresos" => t.Egresos,
            "Creditos" => t.Creditos,
            "NumeroTurno" => t.NumeroTurno,
            "CodigoVendedor" => t.CodigoVendedor.Trim(),
            _ => null
        },
        ("DetalleFactura", DetalleFacturaDto d) => campo switch
        {
            "Cantidad" => d.Cantidad,
            "ValorUnitario" => d.ValorUnitario,
            "VolumenTotal" => d.VolumenTotal,
            "CodigoProducto" => d.CodigoProducto.Trim(),
            "NombreProducto" => d.NombreProducto.Trim(),
            "CodigoCliente" => d.CodigoCliente.Trim(),
            _ => null
        },
        ("Credito", CreditoDto c) => campo switch
        {
            "TotalCredito" => c.TotalCredito,
            "TotalInteres" => c.TotalInteres,
            "PlazoCabecera" => c.PlazoCabecera,
            "NumeroComprobante" => c.NumeroComprobante,
            "CodigoSocio" => c.CodigoSocio.Trim(),
            "CodigoBanco" => c.CodigoBanco.Trim(),
            _ => null
        },
        ("TarjetaTurno", TarjetaTurnoDto tt) => campo switch
        {
            "Valor" => (double)tt.Valor,
            "Cantidad" => tt.Cantidad,
            "NumeroTurno" => tt.NumeroTurno,
            "CodigoBanco" => tt.CodigoBanco.Trim(),
            _ => null
        },
        _ => null
    };

    /// <summary>Campo que identifica al empleado en cada fuente (para asociar la alerta).</summary>
    public static string? CampoEmpleado(string fuente) => fuente switch
    {
        "Factura" or "CierreTurno" => "CodigoVendedor",
        _ => null
    };

    /// <summary>Campo monetario de referencia por fuente (alimenta el multiplicador de scoring).</summary>
    public static string? CampoMonto(string fuente) => fuente switch
    {
        "Factura" => "TotalNeto",
        "CierreTurno" => "Faltante",
        "DetalleFactura" => "VolumenTotal",
        "Credito" => "TotalCredito",
        "TarjetaTurno" => "Valor",
        _ => null
    };

    public static CampoInfo? BuscarCampo(string fuente, string campo) =>
        Fuentes.TryGetValue(fuente, out var campos)
            ? campos.FirstOrDefault(c => c.Nombre == campo)
            : null;

    /// <summary>
    /// Valida una definición completa de regla. Devuelve la lista de errores (vacía si es válida).
    /// </summary>
    public static IReadOnlyList<string> ValidarDefinicion(
        string fuente, IReadOnlyList<CondicionRegla> condiciones, AgregacionRegla? agregacion, double riesgoBase)
    {
        var errores = new List<string>();

        if (!Fuentes.ContainsKey(fuente))
            errores.Add($"Fuente de datos desconocida: '{fuente}'.");

        if (riesgoBase is < 1 or > 100)
            errores.Add("El riesgo base debe estar entre 1 y 100.");

        if (condiciones.Count == 0 && agregacion is null)
            errores.Add("La regla necesita al menos una condición o una agregación.");

        foreach (var condicion in condiciones)
        {
            var campo = BuscarCampo(fuente, condicion.Campo);
            if (campo is null)
            {
                errores.Add($"El campo '{condicion.Campo}' no existe en la fuente '{fuente}'.");
                continue;
            }

            var operadoresValidos = campo.Tipo == TipoNumero ? OperadoresNumero : OperadoresTexto;
            if (!operadoresValidos.Contains(condicion.Operador))
            {
                errores.Add($"Operador '{condicion.Operador}' no válido para el campo {campo.Tipo} '{condicion.Campo}'.");
                continue;
            }

            if (campo.Tipo == TipoNumero && !double.TryParse(
                    condicion.Valor, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out _))
            {
                errores.Add($"El valor '{condicion.Valor}' de la condición sobre '{condicion.Campo}' debe ser numérico.");
            }
        }

        if (agregacion is not null)
        {
            if (BuscarCampo(fuente, agregacion.AgruparPor) is null)
                errores.Add($"El campo de agrupación '{agregacion.AgruparPor}' no existe en la fuente '{fuente}'.");

            if (!Funciones.Contains(agregacion.Funcion))
                errores.Add($"Función de agregación desconocida: '{agregacion.Funcion}'.");

            if (agregacion.Funcion is "Suma" or "Promedio")
            {
                if (string.IsNullOrWhiteSpace(agregacion.Campo))
                    errores.Add($"La función {agregacion.Funcion} requiere un campo numérico.");
                else
                {
                    var campoAgregado = BuscarCampo(fuente, agregacion.Campo);
                    if (campoAgregado is null || campoAgregado.Tipo != TipoNumero)
                        errores.Add($"El campo '{agregacion.Campo}' de la agregación debe ser numérico de la fuente '{fuente}'.");
                }
            }

            if (!OperadoresNumero.Contains(agregacion.Operador))
                errores.Add($"Operador de umbral no válido: '{agregacion.Operador}'.");
        }

        return errores;
    }
}
