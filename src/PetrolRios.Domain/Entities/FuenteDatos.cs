namespace PetrolRios.Domain.Entities;

/// <summary>
/// Fuente de datos adicional registrada de forma CENTRAL: una tabla extra de la base
/// Firebird (Contaplus) que todos los agentes deben extraer y enviar, además de las
/// estándar. El ingeniero la registra una sola vez aquí (en el central) y los agentes
/// la reciben automáticamente; no hay que configurar estación por estación.
///
/// Cada agente verifica que la tabla y la columna existan en SU base antes de extraer,
/// así que registrar una tabla que falte en alguna estación no rompe nada: esa estación
/// simplemente la omite.
/// </summary>
public class FuenteDatos : BaseEntity
{
    /// <summary>Nombre lógico con el que llegan los registros (p. ej. "Tanques").</summary>
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>Tabla real de Firebird a extraer (p. ej. "TANQ_REPO").</summary>
    public string Tabla { get; private set; } = string.Empty;

    /// <summary>
    /// Columna de fecha que actúa como marca de agua (solo se extrae lo nuevo). Si está
    /// vacía, el agente envía un tope de filas por ciclo.
    /// </summary>
    public string? ColumnaWatermark { get; set; }

    /// <summary>Descripción/uso de la fuente (documentación para el ingeniero).</summary>
    public string Descripcion { get; set; } = string.Empty;

    public bool Activa { get; set; } = true;

    public static FuenteDatos Create(
        string nombre, string tabla, string? columnaWatermark, string descripcion) =>
        new()
        {
            Nombre = nombre.Trim(),
            Tabla = tabla.Trim(),
            ColumnaWatermark = string.IsNullOrWhiteSpace(columnaWatermark) ? null : columnaWatermark.Trim(),
            Descripcion = descripcion?.Trim() ?? string.Empty,
            Activa = true
        };

    public void Actualizar(string nombre, string tabla, string? columnaWatermark, string descripcion, bool activa)
    {
        Nombre = nombre.Trim();
        Tabla = tabla.Trim();
        ColumnaWatermark = string.IsNullOrWhiteSpace(columnaWatermark) ? null : columnaWatermark.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        Activa = activa;
        UpdatedAt = DateTime.UtcNow;
    }
}
