using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using PetrolRios.Application.Interfaces;
using PetrolRios.Application.RealTime;
using PetrolRios.Infrastructure.Hubs;

namespace PetrolRios.Infrastructure.RealTime;

/// <summary>
/// Escucha (LISTEN) las notificaciones de alertas que publica cualquier instancia del central y
/// las entrega por SignalR a los clientes conectados a ESTA instancia. Así el tiempo real funciona
/// con varias instancias compartiendo una sola base, sin Redis ni infraestructura extra.
/// </summary>
public sealed class AlertasNotificacionListener : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly IConexionStore _conexion;
    private readonly IHubContext<AlertsHub> _hub;
    private readonly ILogger<AlertasNotificacionListener> _logger;

    public AlertasNotificacionListener(
        IConexionStore conexion,
        IHubContext<AlertsHub> hub,
        ILogger<AlertasNotificacionListener> logger)
    {
        _conexion = conexion;
        _hub = hub;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var cs = _conexion.ResolverActiva();
                if (string.IsNullOrWhiteSpace(cs))
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                await using var con = new NpgsqlConnection(cs);
                await con.OpenAsync(stoppingToken);
                con.Notification += OnNotification;

                await using (var cmd = new NpgsqlCommand($"LISTEN {PostgresAlertaBroadcaster.Canal}", con))
                    await cmd.ExecuteNonQueryAsync(stoppingToken);

                _logger.LogInformation(
                    "Escuchando alertas en tiempo real (canal {Canal}).", PostgresAlertaBroadcaster.Canal);

                while (!stoppingToken.IsCancellationRequested)
                    await con.WaitAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "El listener de alertas en tiempo real cayó; reintentando en 5 s.");
                try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    private void OnNotification(object? sender, NpgsqlNotificationEventArgs e)
    {
        // Fire-and-forget: WaitAsync no debe bloquearse esperando el envío por SignalR.
        _ = EntregarAsync(e.Payload);
    }

    private async Task EntregarAsync(string payload)
    {
        try
        {
            var push = JsonSerializer.Deserialize<AlertaPush>(payload, JsonOpts);
            if (push is null || string.IsNullOrWhiteSpace(push.Evento) || push.Grupos.Count == 0)
                return;

            foreach (var grupo in push.Grupos)
                await _hub.Clients.Group(grupo).SendAsync(push.Evento, push.Payload);

            _logger.LogInformation(
                "Alerta en tiempo real entregada ({Evento}) a {N} grupo(s).", push.Evento, push.Grupos.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo entregar una notificación de alerta recibida.");
        }
    }
}
