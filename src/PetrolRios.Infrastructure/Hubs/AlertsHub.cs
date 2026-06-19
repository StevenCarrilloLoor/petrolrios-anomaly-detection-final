using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PetrolRios.Application.Security;

namespace PetrolRios.Infrastructure.Hubs;

/// <summary>Un usuario conectado al central en tiempo real (para el panel de monitoreo).</summary>
public sealed record UsuarioConectado(string UsuarioId, string Nombre, string Rol, string? EstacionId, DateTime Desde);

/// <summary>
/// Hub de SignalR para notificaciones de alertas en tiempo real.
/// Ruta: /hubs/alerts
/// Grupos: "auditores", "supervisores", "administradores", "estacion-{id}"
/// </summary>
[Authorize]
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

        // La identidad, el rol y la estación salen exclusivamente del JWT autenticado.
        var user = Context.User;
        var usuarioId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var nombre = user?.FindFirst(ClaimTypes.Name)?.Value;
        var rol = user?.FindFirst(ClaimTypes.Role)?.Value ?? "";
        var estacionId = user?.FindFirst(PetrolRiosClaimTypes.EstacionId)?.Value;
        if (string.IsNullOrWhiteSpace(usuarioId)) usuarioId = Context.ConnectionId;
        if (string.IsNullOrWhiteSpace(nombre)) nombre = "Usuario";
        _conectados[Context.ConnectionId] = new UsuarioConectado(
            usuarioId, nombre, rol,
            string.IsNullOrWhiteSpace(estacionId) ? null : estacionId, DateTime.UtcNow);

        // Una cuenta asignada a estación recibe únicamente su carril operativo. Las cuentas
        // del central (sin claim de estación) se unen al grupo correspondiente a su rol.
        foreach (var grupo in AlertHubGroupResolver.Resolve(user))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, grupo);
            _logger.LogDebug("Cliente {Id} unido al grupo {Grupo}", Context.ConnectionId, grupo);
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
