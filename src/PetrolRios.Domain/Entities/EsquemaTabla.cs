namespace PetrolRios.Domain.Entities;

/// <summary>
/// Esquema de una tabla de Firebird (nombre + columnas) reportado por los agentes al central.
/// Permite que el sistema central conozca qué tablas existen y qué columnas tienen, para:
///   1) Documentar automáticamente los campos de una fuente registrada (Reglas → Fuentes de datos).
///   2) Ofrecer un navegador/buscador de tablas en el central (quien no conoce la base puede
///      encontrar el nombre de la tabla y ver sus columnas sin abrir el panel del agente).
///
/// Como las 10 estaciones comparten el mismo esquema de Contaplus (CONTAC.FDB), se guarda un
/// catálogo global con una fila por nombre de tabla (se actualiza con el último agente que reporta).
/// </summary>
public class EsquemaTabla : BaseEntity
{
    /// <summary>Nombre real de la tabla en Firebird (clave única, en mayúsculas).</summary>
    public string Tabla { get; set; } = string.Empty;

    /// <summary>Columnas de la tabla en JSON: [{"nombre","tipo","longitud","nullable"}].</summary>
    public string ColumnasJson { get; set; } = "[]";

    /// <summary>Código de la estación cuyo agente reportó por última vez este esquema.</summary>
    public string? EstacionCodigo { get; set; }

    public static EsquemaTabla Create(string tabla, string columnasJson, string? estacionCodigo) =>
        new()
        {
            Tabla = tabla.Trim().ToUpperInvariant(),
            ColumnasJson = columnasJson,
            EstacionCodigo = estacionCodigo
        };
}
