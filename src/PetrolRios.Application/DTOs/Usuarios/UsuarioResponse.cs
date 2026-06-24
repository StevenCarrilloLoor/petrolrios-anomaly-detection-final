namespace PetrolRios.Application.DTOs.Usuarios;

public sealed record UsuarioResponse
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string NombreCompleto { get; init; } = string.Empty;
    public string Rol { get; init; } = string.Empty;
    public int RolId { get; init; }
    /// <summary>Estación a la que está adscrito el usuario (admin de estación). Null = central.</summary>
    public int? EstacionId { get; init; }
    public bool Activo { get; init; }
    public bool EmailVerificado { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed record CrearUsuarioRequest(
    string Email,
    string NombreCompleto,
    string Password,
    int RolId,
    int? EstacionId = null,
    /// <summary>
    /// Código de una estación NUEVA a crear y asignar en el momento (p. ej. "EST-011"). Si se
    /// indica y la estación no existe, se crea; tiene prioridad sobre <see cref="EstacionId"/>.
    /// Permite escalar a más estaciones desde el alta de usuarios sin un paso previo.
    /// </summary>
    string? CodigoEstacionNueva = null);

public sealed record ActualizarUsuarioRequest(
    string? NombreCompleto,
    int? RolId,
    bool? Activo,
    int? EstacionId = null,
    bool ActualizarEstacion = false);
