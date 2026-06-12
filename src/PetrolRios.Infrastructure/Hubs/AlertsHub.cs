using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace PetrolRios.Infrastructure.Hubs;

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
        _logger.LogDebug("Cliente {Id} desconectado", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
