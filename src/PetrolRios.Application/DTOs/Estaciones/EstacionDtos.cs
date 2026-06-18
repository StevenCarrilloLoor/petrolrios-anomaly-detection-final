namespace PetrolRios.Application.DTOs.Estaciones;

public sealed record EstacionResponse
{
    public int Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Direccion { get; init; } = string.Empty;
    public string? Zona { get; init; }
    public bool Activa { get; init; }
    public DateTime? UltimoHeartbeat { get; init; }
    public string? VersionAgente { get; init; }

    // Configuración (Admin): horario de operación y correo de contacto de la estación.
    public string HoraApertura { get; init; } = string.Empty;
    public string HoraCierre { get; init; } = string.Empty;
    public string? CorreoContacto { get; init; }
}

public sealed record ActualizarEstacionRequest(
    string Nombre,
    string? Direccion,
    string? Zona,
    string? HoraApertura = null,
    string? HoraCierre = null,
    string? CorreoContacto = null,
    bool? Activa = null);

public sealed record EliminarEstacionResponse(
    bool Eliminada,
    bool Desactivada,
    string Mensaje);
