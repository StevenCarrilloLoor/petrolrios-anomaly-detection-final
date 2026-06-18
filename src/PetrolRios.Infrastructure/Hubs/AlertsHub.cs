using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace PetrolRios.Infrastructure.Hubs;

/// <summary>Un usuario conectado al central en tiempo real (para el panel de monitoreo).</summary>
public sealed record UsuarioConectado(string UsuarioId, string Nombre, string Rol, string? EstacionId, DateTime Desde);

/// <summary>
/// Hub de SignalR para notificaciones de alertas en tiempo real.
/// Ruta: /hubs/alerts
/// Grupos: "auditores", "supervisores", "administradores", "estacion-{id}"
/// </summary>
public sealed class AlertsHub : Hub
{
    private static int _conexionesActivas;

    /// <summary>Clientes SignalR conectados en este momento (para el panel de monitoreo).</summary>
    public static int ConexionesActivas => Volatile.Read(ref _conexionesActivas);

    // Registro de conexiones activas con su usuario (clave = ConnectionId).
    private static readonly ConcurrentDictionary<string, UsuarioConectado> _conectados = new();

    /// <summary>
    /// Usuarios conectados al central ahora mismo (un usuario con varias pestañas se cuenta una
    /// vez, con su conexión más antigua). Para la sección de monitoreo "Usuarios conectados".
    /// </summary>
    public static IReadOnlyList<UsuarioConectado> UsuariosConectados =>
        _conectados.Values
            .GroupBy(u => u.UsuarioId)
            .Select(g => g.OrderBy(u => u.Desde).First())
            .OrderBy(u => u.Nombre)
            .ToList();

    private readonly ILogger<AlertsHub> _logger;

    public AlertsHub(ILogger<AlertsHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        Interlocked.Increment(ref _conexionesActivas);
        var httpContext = Context.GetHttpContext();
        var rol = httpContext?.Request.Query["rol"].ToString();
        var estacionId = httpContext?.Request.Query["estacionId"].ToString();

        // Registrar el usuario conectado (de los claims del JWT; respaldo en query).
        var user = Context.User;
        var usuarioId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? httpContext?.Request.Query["usuarioId"].ToString();
        var nombre = user?.FindFirst(ClaimTypes.Name)?.Value
                     ?? httpContext?.Request.Query["nombre"].ToString();
        if (string.IsNullOrWhiteSpace(usuarioId)) usuarioId = Context.ConnectionId;
        if (string.IsNullOrWhiteSpace(nombre)) nombre = "Usuario";
        var rolEfectivo = user?.FindFirst(ClaimTypes.Role)?.Value
                          ?? (string.IsNullOrWhiteSpace(rol) ? "" : rol);
        _conectados[Context.ConnectionId] = new UsuarioConectado(
            usuarioId, nombre, rolEfectivo,
            string.IsNullOrWhiteSpace(estacionId) ? null : estacionId, DateTime.UtcNow);

        // Unir al grupo según rol
        if (!string.IsNullOrWhiteSpace(rol))
        {
            var grupo = rol.ToLowerInvariant() switch
            {
                "auditor" => "auditores",
                "supervisor" => "supervisores",
                "administrador" => "administradores",
                _ => null
            };

            if (grupo is not null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, grupo);
                _logger.LogDebug("Cliente {Id} unido al grupo {Grupo}", Context.ConnectionId, grupo);
            }
        }

        // Unir al grupo de estación si aplica
        if (!string.IsNullOrWhiteSpace(estacionId))
        {
            var grupoEstacion = $"estacion-{estacionId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, grupoEstacion);
            _logger.LogDebug("Cliente {Id} unido al grupo {Grupo}", Context.ConnectionId, grupoEstacion);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Interlocked.Decrement(ref _conexionesActivas);
        _conectados.TryRemove(Context.ConnectionId, out _);
        _logger.LogDebug("Cliente {Id} desconectado", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
