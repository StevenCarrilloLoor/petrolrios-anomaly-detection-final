using System.Text.Json;
using Microsoft.Extensions.Logging;
using Npgsql;
using PetrolRios.Application.Interfaces;
using PetrolRios.Application.RealTime;

namespace PetrolRios.Infrastructure.RealTime;

/// <summary>
/// Difunde los push de alertas a TODAS las instancias del central mediante <c>pg_notify</c> sobre
/// la base compartida. Cada instancia los recibe con <see cref="AlertasNotificacionListener"/> y
/// los entrega a sus clientes SignalR. No requiere Redis ni infraestructura extra.
/// </summary>
public sealed class PostgresAlertaBroadcaster : IAlertaBroadcaster
{
    /// <summary>Canal de PostgreSQL usado para el fan-out de alertas.</summary>
    public const string Canal = "petrolrios_alertas";

    private readonly IConexionStore _conexion;
    private readonly ILogger<PostgresAlertaBroadcaster> _logger;

    public PostgresAlertaBroadcaster(IConexionStore conexion, ILogger<PostgresAlertaBroadcaster> logger)
    {
        _conexion = conexion;
        _logger = logger;
    }

    public async Task PublicarAsync(AlertaPush push, CancellationToken ct = default)
    {
        try
        {
            var cs = _conexion.ResolverActiva();
            if (string.IsNullOrWhiteSpace(cs)) return;

            var json = JsonSerializer.Serialize(push);
            // pg_notify tiene un límite de 8000 bytes; el payload de una alerta es muy pequeño.
            if (json.Length > 7000)
            {
                _logger.LogWarning("Notificación demasiado grande para pg_notify ({Len} bytes); se omite.", json.Length);
                return;
            }

            await using var con = new NpgsqlConnection(cs);
            await con.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand("SELECT pg_notify(@canal, @payload)", con);
            cmd.Parameters.AddWithValue("canal", Canal);
            cmd.Parameters.AddWithValue("payload", json);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch (Exception ex)
        {
            // Nunca tumbar el ciclo de detección por un fallo de notificación en tiempo real.
            _logger.LogWarning(ex, "No se pudo publicar la notificación de alerta en tiempo real.");
        }
    }
}
