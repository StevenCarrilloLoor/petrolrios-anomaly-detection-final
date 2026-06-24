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

/// <summary>
/// Alta de una estación nueva. Además de la estación, crea su usuario-agente
/// (<c>agent-{codigo}@petrolrios.com</c>, rol Auditor, ligado a la estación) para que el
/// agente de campo pueda autenticarse. Si <c>PasswordAgente</c> viene vacía, se genera una.
/// </summary>
public sealed record CrearEstacionRequest(
    string Codigo,
    string Nombre,
    string? Zona = null,
    string? Direccion = null,
    string? PasswordAgente = null);

/// <summary>
/// Resultado del alta: la estación creada y las credenciales del usuario-agente. La contraseña
/// se devuelve EN CLARO una sola vez (al crearse) para configurarla en el agente de esa estación.
/// </summary>
public sealed record ProvisionarEstacionResponse(
    EstacionResponse Estacion,
    string AgenteEmail,
    string? AgentePassword,
    bool AgenteCreado);
