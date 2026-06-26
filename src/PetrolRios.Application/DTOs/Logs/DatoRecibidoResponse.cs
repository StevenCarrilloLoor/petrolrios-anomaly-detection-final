namespace PetrolRios.Application.DTOs.Logs;

/// <summary>
/// Una fila CRUDA recibida de un agente (transacciones_staging), tal como llegó — sea anomalía o no.
/// Sirve para auditar qué está enviando cada estación y confirmar que las tablas registradas en el
/// selector realmente fluyen hasta el central.
/// </summary>
public sealed record DatoRecibidoResponse
{
    public int Id { get; init; }
    /// <summary>Tipo/fuente con que llegó (built-in "Factura"… o el nombre de la tabla configurable).</summary>
    public string TipoTransaccion { get; init; } = string.Empty;
    /// <summary>Nombre natural en español del tipo (p. ej. "Factura", "Anulación").</summary>
    public string TipoNatural { get; init; } = string.Empty;
    /// <summary>Tabla técnica de Contaplus de la que proviene (p. ej. "DCTO"). Vacío si se desconoce.</summary>
    public string Tabla { get; init; } = string.Empty;
    public int EstacionId { get; init; }
    public string EstacionCodigo { get; init; } = string.Empty;
    public string EstacionNombre { get; init; } = string.Empty;
    /// <summary>Fecha original del registro en Firebird (reloj de la estación).</summary>
    public DateTime FechaOriginal { get; init; }
    /// <summary>true si el motor de detección ya la consumió.</summary>
    public bool Procesada { get; init; }
    /// <summary>Contenido crudo (JSON con los nombres de columna reales de Firebird).</summary>
    public string DataJson { get; init; } = string.Empty;
    /// <summary>Cuándo la recibió el central.</summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Opción del desplegable de tipos en "Datos recibidos": el valor crudo con que se filtra
/// (<see cref="Tipo"/>) y la etiqueta legible "Natural (TABLA)" (<see cref="Etiqueta"/>).
/// </summary>
public sealed record TipoRecibidoOption(string Tipo, string Etiqueta);
