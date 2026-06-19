namespace PetrolRios.Application.DTOs.Esquema;

/// <summary>Una columna de una tabla de Firebird (documentación automática).</summary>
public sealed record ColumnaEsquema(string Nombre, string Tipo, int Longitud, bool Nullable);

/// <summary>Una tabla con sus columnas, tal como la reporta el agente.</summary>
public sealed record TablaEsquema(string Tabla, IReadOnlyList<ColumnaEsquema> Columnas);

/// <summary>Payload que el agente envía al central con el esquema de su Firebird.</summary>
public sealed record ReportarEsquemaRequest(
    string CodigoEstacion, IReadOnlyList<TablaEsquema> Tablas);

/// <summary>Resumen de una tabla para el navegador del central (nombre + nº de columnas).</summary>
public sealed record TablaResumen(string Tabla, int Columnas);

/// <summary>Detalle de una tabla para el central (columnas + cuándo se reportó).</summary>
public sealed record TablaDetalle(
    string Tabla, IReadOnlyList<ColumnaEsquema> Columnas, string? EstacionCodigo, DateTime? Actualizado);
