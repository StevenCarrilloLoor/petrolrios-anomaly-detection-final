using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetrolRios.StationAgent.Configuration;

namespace PetrolRios.StationAgent.Services;

/// <summary>
/// Cliente HTTP que envía lotes de transacciones al servidor central.
/// Maneja autenticación JWT con refresh automático.
/// </summary>
public sealed class ServerClient
{
    private readonly HttpClient _httpClient;
    private readonly AgentOptions _options;
    private readonly ILogger<ServerClient> _logger;
    private string? _token;
    private DateTime _tokenExpiration;

    public ServerClient(HttpClient httpClient, IOptions<AgentOptions> options, ILogger<ServerClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Envía un lote de transacciones al endpoint de ingesta del servidor.
    /// Retorna true si el servidor confirmó la recepción.
    /// </summary>
    public async Task<bool> SendBatchAsync(List<TransaccionBatchItem> items, CancellationToken ct)
    {
        await EnsureAuthenticatedAsync(ct);

        var payload = new
        {
            CodigoEstacion = _options.CodigoEstacion,
            Transacciones = items.Select(i => new
            {
                i.TipoTransaccion,
                i.DataJson,
                i.FechaOriginal
            })
        };

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/ingesta", payload, ct);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Lote de {Count} transacciones enviado exitosamente", items.Count);
            return true;
        }

        _logger.LogWarning("Error enviando lote: {Status} {Reason}",
            (int)response.StatusCode, response.ReasonPhrase);
        return false;
    }

    /// <summary>
    /// Prueba la conexión y autenticación con el servidor central (panel del agente).
    /// Mide la latencia del login JWT.
    /// </summary>
    public async Task<(bool Ok, string Mensaje, double? LatenciaMs)> ProbarConexionAsync(CancellationToken ct)
    {
        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            _token = null; // Forzar autenticación real
            await EnsureAuthenticatedAsync(ct);
            sw.Stop();
            return (true,
                $"Autenticado contra {_httpClient.BaseAddress} en {sw.Elapsed.TotalMilliseconds:F0} ms",
                Math.Round(sw.Elapsed.TotalMilliseconds, 1));
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    private async Task EnsureAuthenticatedAsync(CancellationToken ct)
    {
        if (_token is not null && DateTime.UtcNow < _tokenExpiration.AddMinutes(-5))
            return;

        _logger.LogInformation("Autenticando agente {Estacion} contra el servidor", _options.CodigoEstacion);

        var loginPayload = new { Email = _options.Email, Password = _options.Password };
        var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/login", loginPayload, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        _token = body.GetProperty("token").GetString();
        _tokenExpiration = body.GetProperty("expiration").GetDateTime();

        _logger.LogInformation("Autenticación exitosa, token válido hasta {Expiration}", _tokenExpiration);
    }
}
