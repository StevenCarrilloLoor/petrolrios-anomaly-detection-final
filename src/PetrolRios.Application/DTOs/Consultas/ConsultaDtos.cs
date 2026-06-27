namespace PetrolRios.Application.DTOs.Consultas;

/// <summary>
/// Solicitud de consulta EN VIVO a la base Firebird de una estación. La encola el central (a pedido de
/// un auditor/supervisor) y la corre el agente en su próximo heartbeat, en modo SOLO LECTURA. Busca
/// documentos (DCTO) por tipo + rango de fechas + un código libre que coincide con RUC, placa, cliente
/// o número de documento.
/// </summary>
public sealed record SolicitudConsulta(
    string CodigoEstacion,
    string? TipoDocumento,     // "FV" / "EB" / "DV" … o null = todos
    DateTime? FechaDesde,
    DateTime? FechaHasta,
    string? Codigo,            // coincide con RUC, placa, cliente, despachador o n.º de documento
    int Limite = 200,
    string Tabla = "DCTO");    // "DCTO" = documentos (defecto); "DESP" = líneas de surtidor por NUM_DESP

/// <summary>Consulta pendiente que el agente recoge en su heartbeat (sin el código de estación, ya es suyo).</summary>
public sealed record ConsultaPendiente(
    string Id,
    string? TipoDocumento,
    DateTime? FechaDesde,
    DateTime? FechaHasta,
    string? Codigo,
    int Limite,
    string Tabla = "DCTO");

/// <summary>Estado/resultado de una consulta, que la interfaz sondea hasta que esté "Listo" o "Error".</summary>
public sealed record ConsultaEstado(
    string Id,
    string Estado,             // "Pendiente" | "Listo" | "Error"
    string? ResultadoJson,
    string? Error);
