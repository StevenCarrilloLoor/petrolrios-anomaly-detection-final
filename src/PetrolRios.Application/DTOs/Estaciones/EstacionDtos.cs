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
}

public sealed record ActualizarEstacionRequest(
    string Nombre,
    string? Direccion,
    string? Zona);

public sealed record EliminarEstacionResponse(
    bool Eliminada,
    bool Desactivada,
    string Mensaje);
