namespace PetrolRios.Domain.Entities;

/// <summary>
/// Regla de negocio definida por el usuario desde la interfaz (sin tocar código).
/// La evalúa el detector genérico <c>CustomRuleDetector</c> en cada ciclo:
/// filtra los registros de la fuente con las condiciones (AND) y, opcionalmente,
/// agrupa y compara un agregado contra un umbral.
/// </summary>
public class ReglaPersonalizada : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Fuente de datos: Factura, CierreTurno, DetalleFactura, Credito o TarjetaTurno.</summary>
    public string FuenteDatos { get; set; } = string.Empty;

    /// <summary>Condiciones de filtrado (JSON): [{"campo","operador","valor"}], combinadas con AND.</summary>
    public string CondicionesJson { get; set; } = "[]";

    /// <summary>Agregación opcional (JSON): {"agruparPor","funcion","campo","operador","umbral"}.</summary>
    public string? AgregacionJson { get; set; }

    /// <summary>
    /// Modo avanzado: expresión lógica (programación) que reemplaza las condiciones
    /// simples. Si está presente, el detector filtra los registros con esta expresión
    /// (ej.: "TotalNeto > 400 &amp;&amp; (CodigoPago == 'EF' || Descuento / Subtotal > 0.1)").
    /// </summary>
    public string? ExpresionAvanzada { get; set; }

    /// <summary>Riesgo base de la alerta generada (1–100); el motor de scoring aplica multiplicadores.</summary>
    public double RiesgoBase { get; set; } = 50;

    /// <summary>
    /// Campos a mostrar en la evidencia de la alerta (JSON de lista de strings). Cada elemento es un
    /// campo propio de la fuente ("Cantidad") o uno de una tabla relacionada en formato
    /// "Fuente.Campo" ("Factura.Placa"). El detector los resuelve (cruzando en memoria con la
    /// relación definida) y los agrega a la evidencia, para que la alerta tenga más contexto
    /// (quién, qué placa, qué cliente, qué factura). Si está vacío, se usa el comportamiento previo.
    /// </summary>
    public string? CamposMostrarJson { get; set; }

    /// <summary>
    /// Carril de las alertas que genera esta regla: "Operativa" (problema de estación → avisa al
    /// administrador de la estación) o "Auditoria" (fraude → central). Por defecto Auditoría.
    /// </summary>
    public string Ambito { get; set; } = "Auditoria";

    public bool Activa { get; set; } = true;

    public static ReglaPersonalizada Create(
        string nombre, string descripcion, string fuenteDatos,
        string condicionesJson, string? agregacionJson, double riesgoBase,
        string ambito = "Auditoria", string? camposMostrarJson = null) =>
        new()
        {
            Nombre = nombre,
            Descripcion = descripcion,
            FuenteDatos = fuenteDatos,
            CondicionesJson = condicionesJson,
            AgregacionJson = agregacionJson,
            RiesgoBase = riesgoBase,
            Ambito = NormalizarAmbito(ambito),
            CamposMostrarJson = string.IsNullOrWhiteSpace(camposMostrarJson) ? null : camposMostrarJson
        };

    /// <summary>Acepta "Operativa", "Auditoria" o "Ambos" (doble carril); por defecto Auditoría.</summary>
    public static string NormalizarAmbito(string? ambito)
    {
        var a = ambito?.Trim();
        if (string.Equals(a, "Operativa", StringComparison.OrdinalIgnoreCase)) return "Operativa";
        if (string.Equals(a, "Ambos", StringComparison.OrdinalIgnoreCase)) return "Ambos";
        return "Auditoria";
    }
}
