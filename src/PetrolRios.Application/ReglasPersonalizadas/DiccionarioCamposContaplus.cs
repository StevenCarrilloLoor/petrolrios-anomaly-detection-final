namespace PetrolRios.Application.ReglasPersonalizadas;

/// <summary>
/// Rol semántico de un campo, para documentarlo en lenguaje natural y mostrar un ícono.
/// Es independiente del tipo de operador (número/texto): un mismo "número" puede ser un
/// monto ($), una cantidad (galones) o un identificador.
/// </summary>
public enum RolCampo
{
    Fecha,
    Monto,
    Cantidad,
    Numero,
    Codigo,
    Nombre,
    Identificacion,
    Placa,
    Estado,
    Texto
}

/// <summary>Campo de una tabla, documentado en lenguaje natural para usuarios no técnicos.</summary>
public sealed record CampoDocumentado(
    string Nombre,
    string Etiqueta,
    string Tipo,
    string Rol,
    string Descripcion,
    string Icono);

/// <summary>
/// Diccionario de datos + glosario de negocio para los campos de Contaplus (CONTAC/CONTAB.FDB).
/// Traduce los códigos crípticos del esquema (p. ej. <c>FEC_DCTO</c>, <c>CAN_DESP</c>) a un
/// nombre legible, un rol semántico, una descripción en español y un ícono — todo de forma
/// AUTOMÁTICA, combinando tres fuentes en orden de prioridad:
///   1. Glosario curado (los campos comunes y de más valor, con texto exacto).
///   2. Comentario del campo en Firebird (RDB$DESCRIPTION) si la base lo trae.
///   3. Inferencia por prefijo de Contaplus (FEC_=fecha, COD_=código, VAL_=monto, …).
/// Es 100% por datos (no hay lógica cableada por campo): para cubrir un campo nuevo basta
/// agregar una entrada al glosario o un prefijo; nada más cambia. Por eso escala a cualquier tabla.
/// </summary>
public static class DiccionarioCamposContaplus
{
    /// <summary>Glosario curado: código exacto → (etiqueta legible, rol, descripción).</summary>
    private static readonly IReadOnlyDictionary<string, (string Etiqueta, RolCampo Rol, string Desc)> Glosario =
        new Dictionary<string, (string, RolCampo, string)>(StringComparer.OrdinalIgnoreCase)
        {
            // ---- DCTO (facturas / documentos) ----
            ["NUM_DCTO"] = ("Número de documento", RolCampo.Numero, "Número de la factura o comprobante."),
            ["SEC_DCTO"] = ("Secuencia del documento", RolCampo.Numero, "Identificador interno único del documento."),
            ["FEC_DCTO"] = ("Fecha del documento", RolCampo.Fecha, "Fecha en que se registró la factura."),
            ["TIP_DCTO"] = ("Tipo de documento", RolCampo.Codigo, "Tipo de comprobante (FV=factura de venta, etc.)."),
            ["COD_CLIE"] = ("Código de cliente", RolCampo.Codigo, "Cliente al que se facturó."),
            ["COD_VEND"] = ("Código del vendedor", RolCampo.Codigo, "Despachador que realizó la venta."),
            ["COD_CHOF"] = ("Código del chofer", RolCampo.Codigo, "Chofer asociado a la venta."),
            ["COD_PAGO"] = ("Forma de pago", RolCampo.Codigo, "EF=efectivo, TC=tarjeta, CR=crédito…"),
            ["COD_MANG"] = ("Código de manguera", RolCampo.Codigo, "Manguera/surtidor del despacho."),
            ["PLA_DCTO"] = ("Placa del vehículo", RolCampo.Placa, "Placa del vehículo al que se despachó."),
            ["RUC_DCTO"] = ("RUC / cédula", RolCampo.Identificacion, "Identificación tributaria del cliente."),
            ["NUM_TURN"] = ("Número de turno", RolCampo.Numero, "Turno en que se hizo la venta."),
            ["NUM_CONS"] = ("Número de consecutivo", RolCampo.Numero, "Consecutivo interno del documento."),
            ["TNI_DCTO"] = ("Total con IVA ($)", RolCampo.Monto, "Total de la factura incluyendo IVA."),
            ["TSI_DCTO"] = ("Subtotal sin IVA ($)", RolCampo.Monto, "Base imponible antes de IVA."),
            ["SUB_DCTO"] = ("Subtotal ($)", RolCampo.Monto, "Subtotal del documento."),
            ["IVA_DCTO"] = ("IVA ($)", RolCampo.Monto, "Impuesto al valor agregado."),
            ["DSC_DCTO"] = ("Descuento ($)", RolCampo.Monto, "Descuento aplicado al documento."),
            ["OBS_DCTO"] = ("Observación", RolCampo.Texto, "Comentario libre del documento."),
            ["FUL_CABE"] = ("Fecha real de inserción", RolCampo.Fecha, "Momento en que el sistema guardó el registro (sirve para detectar backdating)."),

            // ---- DESP (despachos de combustible) ----
            ["NUM_DESP"] = ("Número de despacho", RolCampo.Numero, "Identificador del despacho."),
            ["CAN_DESP"] = ("Galones despachados", RolCampo.Cantidad, "Cantidad de combustible despachado por la manguera."),
            ["VTO_DESP"] = ("Valor del despacho ($)", RolCampo.Monto, "Importe en dólares del despacho."),
            ["VUN_DESP"] = ("Precio por galón ($)", RolCampo.Monto, "Precio unitario del combustible."),
            ["COD_PROD"] = ("Código de producto", RolCampo.Codigo, "Producto/combustible despachado."),
            ["NOM_PROD"] = ("Nombre de producto", RolCampo.Nombre, "Nombre del combustible (DIESEL, EXTRA…)."),
            ["FIN_DESP"] = ("Fecha del despacho", RolCampo.Fecha, "Fecha y hora del despacho."),
            ["FAC_DESP"] = ("¿Facturado?", RolCampo.Estado, "Indica si el despacho ya fue facturado."),
            ["EST_DESP"] = ("Estado del despacho", RolCampo.Estado, "Estado del despacho."),
            ["SUR_DESP"] = ("Surtidor", RolCampo.Codigo, "Surtidor que realizó el despacho."),

            // ---- TURN / TURN_DEPO (turnos y depósitos) ----
            ["FIN_TURN"] = ("Apertura del turno", RolCampo.Fecha, "Fecha/hora de apertura del turno."),
            ["FFI_TURN"] = ("Cierre del turno", RolCampo.Fecha, "Fecha/hora de cierre del turno."),
            ["EST_TURN"] = ("Estado del turno", RolCampo.Estado, "'0' = turno abierto, otro = cerrado."),
            ["FAL_TURN"] = ("Faltante de caja ($)", RolCampo.Monto, "Dinero faltante al cerrar el turno."),
            ["SOB_TURN"] = ("Sobrante de caja ($)", RolCampo.Monto, "Dinero sobrante al cerrar el turno."),
            ["ING_TURN"] = ("Ingresos del turno ($)", RolCampo.Monto, "Total de ingresos del turno."),
            ["EGR_TURN"] = ("Egresos del turno ($)", RolCampo.Monto, "Total de egresos del turno."),
            ["CRE_TURN"] = ("Créditos del turno ($)", RolCampo.Monto, "Ventas a crédito del turno."),

            // ---- CRED / CRED_CABE (créditos) ----
            ["FEC_CABE"] = ("Fecha del crédito", RolCampo.Fecha, "Fecha en que se otorgó el crédito."),
            ["COD_GARA"] = ("Código del garante", RolCampo.Codigo, "Garante/avalista del crédito (vacío = sin garante)."),
            ["COD_SOCI"] = ("Código de socio/cliente", RolCampo.Codigo, "Socio o cliente del crédito."),
            ["TOT_CABE"] = ("Monto del crédito ($)", RolCampo.Monto, "Importe total del crédito."),
            ["PLA_CABE"] = ("Plazo (días)", RolCampo.Numero, "Plazo del crédito en días."),
            ["COD_AUTO"] = ("Código de autorización", RolCampo.Codigo, "Autorización del crédito (vacío = sin autorizar)."),

            // ---- ANUL (anulaciones) ----
            ["FECHAANULACION"] = ("Fecha de anulación", RolCampo.Fecha, "Cuándo se anuló el comprobante."),
            ["NUMAN"] = ("Número de anulaciones", RolCampo.Numero, "Cantidad de anulaciones por punto de emisión."),

            // ---- CLIE / VEND / EMPL (catálogos) ----
            ["NOM_CLIE"] = ("Nombre del cliente", RolCampo.Nombre, "Nombre o razón social del cliente."),
            ["LIM_CLIE"] = ("Límite de crédito ($)", RolCampo.Monto, "Cupo de crédito del cliente."),
            ["NOM_VEND"] = ("Nombre del vendedor", RolCampo.Nombre, "Nombre del despachador."),
            ["NOM_EMPL"] = ("Nombre del empleado", RolCampo.Nombre, "Nombre del empleado."),

            // ---- TANQ_REPO (reportes de tanque) ----
            ["DIFERENCIA"] = ("Diferencia de tanque", RolCampo.Cantidad, "Diferencia entre lo medido y lo esperado en el tanque."),
            ["VENTAS_TANQ"] = ("Ventas del tanque", RolCampo.Cantidad, "Galones vendidos según el tanque."),
            ["FEC_FIN_REPO"] = ("Fecha del reporte", RolCampo.Fecha, "Fecha del cuadre/reporte de tanque."),
        };

    /// <summary>Inferencia por prefijo (3 letras antes del '_') cuando el campo no está en el glosario.</summary>
    private static readonly (string Prefijo, RolCampo Rol, string Desc)[] Prefijos =
    [
        ("FEC", RolCampo.Fecha, "Fecha."),
        ("FIN", RolCampo.Fecha, "Fecha/hora de fin."),
        ("FFI", RolCampo.Fecha, "Fecha/hora."),
        ("FUL", RolCampo.Fecha, "Fecha real de inserción."),
        ("FUM", RolCampo.Fecha, "Fecha de modificación."),
        ("FUE", RolCampo.Fecha, "Fecha."),
        ("NOM", RolCampo.Nombre, "Nombre."),
        ("PLA", RolCampo.Placa, "Placa."),
        ("RUC", RolCampo.Identificacion, "Identificación (RUC/cédula)."),
        ("DNI", RolCampo.Identificacion, "Identificación."),
        ("CED", RolCampo.Identificacion, "Cédula."),
        ("VAL", RolCampo.Monto, "Valor ($)."),
        ("VTO", RolCampo.Monto, "Valor total ($)."),
        ("VUN", RolCampo.Monto, "Valor unitario ($)."),
        ("TNI", RolCampo.Monto, "Total con impuesto ($)."),
        ("TSI", RolCampo.Monto, "Subtotal sin impuesto ($)."),
        ("SUB", RolCampo.Monto, "Subtotal ($)."),
        ("IVA", RolCampo.Monto, "IVA ($)."),
        ("DSC", RolCampo.Monto, "Descuento ($)."),
        ("IMP", RolCampo.Monto, "Importe ($)."),
        ("TOT", RolCampo.Monto, "Total ($)."),
        ("PRE", RolCampo.Monto, "Precio ($)."),
        ("CAN", RolCampo.Cantidad, "Cantidad."),
        ("EST", RolCampo.Estado, "Estado."),
        ("DET", RolCampo.Texto, "Detalle."),
        ("OBS", RolCampo.Texto, "Observación."),
        ("DIR", RolCampo.Texto, "Dirección."),
        ("COD", RolCampo.Codigo, "Código."),
        ("TIP", RolCampo.Codigo, "Tipo."),
        ("NUM", RolCampo.Numero, "Número."),
        ("SEC", RolCampo.Numero, "Secuencia."),
    ];

    /// <summary>Ícono (emoji) representativo de cada rol.</summary>
    public static string IconoDe(RolCampo rol) => rol switch
    {
        RolCampo.Fecha => "📅",
        RolCampo.Monto => "💲",
        RolCampo.Cantidad => "⛽",
        RolCampo.Numero => "🔢",
        RolCampo.Codigo => "🏷️",
        RolCampo.Nombre => "🔤",
        RolCampo.Identificacion => "🪪",
        RolCampo.Placa => "🚗",
        RolCampo.Estado => "🔘",
        _ => "📝"
    };

    /// <summary>Tipo de operador (número/texto) que corresponde a un rol, para el builder de condiciones.</summary>
    public static string TipoOperadorDe(RolCampo rol) => rol switch
    {
        RolCampo.Monto or RolCampo.Cantidad or RolCampo.Numero => "numero",
        _ => "texto"
    };

    /// <summary>
    /// Documenta un campo: combina glosario curado, descripción de Firebird (si llega) e inferencia
    /// por prefijo. Nunca lanza; ante un campo desconocido devuelve una documentación razonable.
    /// </summary>
    public static (string Etiqueta, RolCampo Rol, string Descripcion) Documentar(
        string campo, string? descripcionFirebird = null)
    {
        var c = (campo ?? string.Empty).Trim();
        var clave = c.ToUpperInvariant();

        if (Glosario.TryGetValue(clave, out var g))
            return (g.Etiqueta, g.Rol, Preferir(descripcionFirebird, g.Desc));

        var prefijo = clave.Split('_')[0];
        foreach (var p in Prefijos)
            if (prefijo == p.Prefijo)
                return (Humanizar(c), p.Rol, Preferir(descripcionFirebird, p.Desc));

        return (Humanizar(c), RolCampo.Texto, Preferir(descripcionFirebird, "Campo de la tabla."));
    }

    /// <summary>Construye el objeto documentado completo (con tipo, ícono y rol como texto).</summary>
    public static CampoDocumentado Construir(
        string campo, string? tipoConocido = null, string? descripcionFirebird = null)
    {
        var (etiqueta, rol, descripcion) = Documentar(campo, descripcionFirebird);
        var tipo = string.IsNullOrWhiteSpace(tipoConocido) ? TipoOperadorDe(rol) : tipoConocido!;
        return new CampoDocumentado(campo.Trim(), etiqueta, tipo, rol.ToString(), descripcion, IconoDe(rol));
    }

    /// <summary>
    /// Documenta un campo "lógico" de las fuentes conocidas (Factura, DetalleFactura, …), que ya
    /// trae una etiqueta clara en español pero usa nombres lógicos (TotalNeto, Cantidad) en vez de
    /// los códigos crudos de Firebird. Infiere el rol por palabras clave del nombre/etiqueta.
    /// </summary>
    public static CampoDocumentado ConstruirLogico(string nombre, string etiqueta, string tipo)
    {
        var rol = RolDeLogico(nombre, etiqueta, tipo);
        return new CampoDocumentado(nombre.Trim(), etiqueta, tipo, rol.ToString(), etiqueta, IconoDe(rol));
    }

    /// <summary>Infiere el rol de un campo lógico por palabras clave de su nombre/etiqueta y su tipo.</summary>
    public static RolCampo RolDeLogico(string nombre, string etiqueta, string tipo)
    {
        var t = ($"{nombre} {etiqueta}").ToLowerInvariant();
        if (t.Contains("fecha") || t.Contains("hora")) return RolCampo.Fecha;
        if (t.Contains("placa")) return RolCampo.Placa;
        if (t.Contains("ruc") || t.Contains("cédula") || t.Contains("cedula") || t.Contains("identif"))
            return RolCampo.Identificacion;
        if (t.Contains("galon") || t.Contains("galón") || t.Contains("cantidad")) return RolCampo.Cantidad;
        if (t.Contains("$") || t.Contains("monto") || t.Contains("total") || t.Contains("subtotal")
            || t.Contains("precio") || t.Contains("valor") || t.Contains("iva") || t.Contains("descuento")
            || t.Contains("faltante") || t.Contains("sobrante") || t.Contains("ingreso") || t.Contains("egreso")
            || t.Contains("crédito") || t.Contains("credito") || t.Contains("interés") || t.Contains("interes"))
            return RolCampo.Monto;
        if (t.Contains("nombre")) return RolCampo.Nombre;
        if (t.Contains("forma de pago") || t.Contains("banco") || t.Contains("código") || t.Contains("codigo")
            || t.Contains("producto"))
            return RolCampo.Codigo;
        if (t.Contains("turno") || t.Contains("número") || t.Contains("numero") || t.Contains("plazo")
            || t.Contains("comprobante"))
            return RolCampo.Numero;
        return tipo == "texto" ? RolCampo.Texto : RolCampo.Numero;
    }

    /// <summary>Prefiere la descripción real de Firebird si vino; si no, la del glosario/prefijo.</summary>
    private static string Preferir(string? firebird, string fallback) =>
        string.IsNullOrWhiteSpace(firebird) ? fallback : firebird!.Trim();

    /// <summary>Convierte un código tipo "FEC_DCTO" en algo legible ("Fec dcto") como último recurso.</summary>
    private static string Humanizar(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return codigo;
        var partes = codigo.Replace('_', ' ').ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', partes.Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
    }
}
