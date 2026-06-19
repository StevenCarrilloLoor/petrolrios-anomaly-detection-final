namespace PetrolRios.StationMonitor.Models;

public sealed record LoginCentralResponse(
    string Token,
    DateTime Expiration,
    UsuarioCentral Usuario,
    bool Requiere2Fa = false);

public sealed record UsuarioCentral(
    int Id,
    string Email,
    string NombreCompleto,
    string Rol,
    int? EstacionId,
    string? EstacionCodigo,
    string? EstacionNombre);

public sealed record ProblemaEstacionGrupo(
    int EstacionId,
    string EstacionNombre,
    DateTime Fecha,
    int Total,
    IReadOnlyList<ProblemaOperativo> Problemas);

public sealed record ProblemaOperativo(
    int Id,
    string TipoDetector,
    string NivelRiesgo,
    string Ambito,
    string Estado,
    string Descripcion,
    double Score,
    DateTime FechaDeteccion,
    string? EmpleadoCodigo,
    string? TransaccionReferencia,
    int EstacionId,
    string EstacionNombre,
    string? MetadataJson);
