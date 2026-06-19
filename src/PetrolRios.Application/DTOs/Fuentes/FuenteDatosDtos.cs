namespace PetrolRios.Application.DTOs.Fuentes;

/// <summary>Fuente de datos adicional del catálogo central (vista completa para administración).</summary>
public sealed record FuenteDatosResponse
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Tabla { get; init; } = string.Empty;
    public string? ColumnaWatermark { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public bool Activa { get; init; }
    public DateTime Version { get; init; }
    public IReadOnlyList<FuenteDatosEstacionEstadoResponse> Sincronizaciones { get; init; } = [];
}

/// <summary>Definición mínima que el agente necesita para extraer una fuente.</summary>
public sealed record FuenteDatosAgente(
    int Id,
    string Nombre,
    string Tabla,
    string? ColumnaWatermark,
    DateTime Version);

public sealed record FuenteDatosEstacionEstadoResponse
{
    public int EstacionId { get; init; }
    public string EstacionCodigo { get; init; } = string.Empty;
    public string EstacionNombre { get; init; } = string.Empty;
    public bool AgenteEnLinea { get; init; }
    public string Estado { get; init; } = string.Empty;
    public bool TablaExiste { get; init; }
    public bool ColumnaWatermarkValida { get; init; }
    public int FilasLeidas { get; init; }
    public int FilasEnviadas { get; init; }
    public long TotalFilasEnviadas { get; init; }
    public string? UltimoError { get; init; }
    public DateTime VersionFuente { get; init; }
    public bool ConfiguracionActualizada { get; init; }
    public DateTime UltimoReporte { get; init; }
    public DateTime? UltimoExito { get; init; }
}

public sealed record ReportarEstadoFuentesRequest(
    string CodigoEstacion,
    IReadOnlyList<EstadoFuenteAgenteRequest> Fuentes);

public sealed record EstadoFuenteAgenteRequest(
    int FuenteDatosId,
    DateTime VersionFuente,
    string Estado,
    bool TablaExiste,
    bool ColumnaWatermarkValida,
    int FilasLeidas,
    int FilasEnviadas,
    string? UltimoError);

public sealed record CrearFuenteDatosRequest(
    string Nombre, string Tabla, string? ColumnaWatermark, string? Descripcion);

public sealed record ActualizarFuenteDatosRequest(
    string Nombre, string Tabla, string? ColumnaWatermark, string? Descripcion, bool Activa);
