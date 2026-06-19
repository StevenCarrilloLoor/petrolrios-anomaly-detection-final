using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PetrolRios.StationAgent.Configuration;

namespace PetrolRios.StationAgent.Services;

/// <summary>
/// Cliente HTTP que envía lotes de transacciones al servidor central.
/// Maneja autenticación JWT con refresh automático. Lee la configuración del
/// <see cref="AgentConfigStore"/> en cada operación, de modo que cambiar la URL
/// del servidor o las credenciales desde la interfaz aplica sin reiniciar.
/// </summary>
public sealed class ServerClient
{
    private readonly HttpClient _httpClient;
    private readonly AgentConfigStore _config;
    private readonly ILogger<ServerClient> _logger;
    private string? _token;
    private DateTime _tokenExpiration;

    public ServerClient(HttpClient httpClient, AgentConfigStore config, ILogger<ServerClient> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    private static string Url(string baseUrl, string path) =>
        $"{baseUrl.TrimEnd('/')}{path}";

    /// <summary>
    /// Envía un lote de transacciones al endpoint de ingesta del servidor.
    /// Retorna true si el servidor confirmó la recepción.
    /// </summary>
    public async Task<bool> SendBatchAsync(List<TransaccionBatchItem> items, CancellationToken ct)
    {
        var settings = _config.Actual;
        await EnsureAuthenticatedAsync(settings, ct);

        var payload = new
        {
            CodigoEstacion = settings.CodigoEstacion,
            NombreEstacion = settings.NombreEstacion,
            ZonaEstacion = settings.ZonaEstacion,
            Transacciones = items.Select(i => new
            {
                i.TipoTransaccion,
                i.DataJson,
                i.FechaOriginal
            })
        };

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);

        var response = await _httpClient.PostAsJsonAsync(Url(settings.ServerUrl, "/api/v1/ingesta"), payload, ct);

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
    /// Envía un heartbeat al servidor: señal de vida del agente aunque no haya
    /// transacciones nuevas. Reporta también el nombre y la zona de la estación.
    /// </summary>
    public async Task<bool> SendHeartbeatAsync(CancellationToken ct)
    {
        var settings = _config.Actual;
        try
        {
            await EnsureAuthenticatedAsync(settings, ct);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            var payload = new
            {
                CodigoEstacion = settings.CodigoEstacion,
                NombreEstacion = settings.NombreEstacion,
                ZonaEstacion = settings.ZonaEstacion,
                VersionAgente = PetrolRios.StationAgent.VersionAgente.Actual
            };
            var response = await _httpClient.PostAsJsonAsync(
                Url(settings.ServerUrl, "/api/v1/ingesta/heartbeat"), payload, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Heartbeat no entregado (servidor no disponible)");
            return false;
        }
    }

    /// <summary>
    /// Prueba la conexión y autenticación con el servidor central (panel del agente).
    /// Mide la latencia del login JWT.
    /// </summary>
    public async Task<(bool Ok, string Mensaje, double? LatenciaMs)> ProbarConexionAsync(CancellationToken ct)
    {
        var settings = _config.Actual;
        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            _token = null; // Forzar autenticación real
            await EnsureAuthenticatedAsync(settings, ct);
            sw.Stop();
            return (true,
                $"Autenticado contra {settings.ServerUrl} en {sw.Elapsed.TotalMilliseconds:F0} ms",
                Math.Round(sw.Elapsed.TotalMilliseconds, 1));
        }
        catch (Exception ex)
        {
            return (false, MensajeError(ex), null);
        }
    }

    /// <summary>
    /// Verifica unas credenciales arbitrarias contra el servidor central y devuelve
    /// el rol del usuario. Se usa para autenticar el panel del agente con RBAC real.
    /// <c>Alcanzable</c> indica si se pudo contactar al servidor (para decidir si
    /// recurrir al respaldo local cuando está caído).
    /// </summary>
    public async Task<(bool Ok, string? Rol, bool Alcanzable, string Mensaje)> VerificarUsuarioAsync(
        string email, string password, CancellationToken ct)
    {
        var settings = _config.Actual;
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(Math.Clamp(settings.ServerTimeoutSegundos, 5, 30)) };
            var resp = await http.PostAsJsonAsync(
                Url(settings.ServerUrl, "/api/v1/auth/login"),
                new { Email = email, Password = password }, ct);

            if (!resp.IsSuccessStatusCode)
                return (false, null, true, "Credenciales inválidas.");

            var body = await resp.Content.ReadFromJsonAsync<JsonElement>(ct);
            var rol = body.TryGetProperty("usuario", out var u) && u.TryGetProperty("rol", out var r)
                ? r.GetString()
                : null;
            return (true, rol, true, "OK");
        }
        catch (Exception ex)
        {
            // No alcanzable (red/servidor caído) → permite el respaldo local
            _logger.LogDebug(ex, "No se pudo verificar el usuario contra el central");
            return (false, null, false, "Servidor central no disponible.");
        }
    }

    /// <summary>
    /// Descarga del servidor central el catálogo de fuentes de datos ACTIVAS (tablas extra
    /// que todos los agentes deben extraer). Devuelve null si el servidor no respondió, de
    /// modo que el ciclo siga con las fuentes locales sin fallar.
    /// </summary>
    public async Task<List<FuenteExtraccion>?> ObtenerFuentesCentralAsync(CancellationToken ct)
    {
        var settings = _config.Actual;
        try
        {
            await EnsureAuthenticatedAsync(settings, ct);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            var resp = await _httpClient.GetAsync(
                Url(settings.ServerUrl, "/api/v1/fuentes-datos/activas"), ct);
            if (!resp.IsSuccessStatusCode) return null;

            var arr = await resp.Content.ReadFromJsonAsync<List<FuenteCentralDto>>(cancellationToken: ct);
            return arr?.Select(a => new FuenteExtraccion
            {
                Nombre = a.Nombre,
                Tabla = a.Tabla,
                ColumnaWatermark = a.ColumnaWatermark,
                Activa = true
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "No se pudo obtener el catálogo central de fuentes");
            return null;
        }
    }

    private sealed record FuenteCentralDto(string Nombre, string Tabla, string? ColumnaWatermark);

    private async Task EnsureAuthenticatedAsync(AgentSettings settings, CancellationToken ct)
    {
        if (_token is not null && DateTime.UtcNow < _tokenExpiration.AddMinutes(-5))
            return;

        _logger.LogInformation("Autenticando agente {Estacion} contra el servidor", settings.CodigoEstacion);

        // El timeout se aplica POR PETICIÓN con un CancellationToken, no mutando
        // HttpClient.Timeout (eso lanza "instance has already started a request"
        // una vez que el cliente ya se usó para los heartbeats).
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(settings.ServerTimeoutSegundos, 5, 120)));

        var loginPayload = new { Email = settings.Email, Password = settings.Password };
        var response = await _httpClient.PostAsJsonAsync(
            Url(settings.ServerUrl, "/api/v1/auth/login"), loginPayload, cts.Token);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        _token = body.GetProperty("token").GetString();
        _tokenExpiration = body.GetProperty("expiration").GetDateTime();

        _logger.LogInformation("Autenticación exitosa, token válido hasta {Expiration}", _tokenExpiration);
    }

    private static string MensajeError(Exception ex) => ex switch
    {
        HttpRequestException => "No se pudo contactar al servidor central. Verifique la URL y que la API esté encendida.",
        TaskCanceledException => "El servidor no respondió a tiempo (timeout). Revise la red o aumente el timeout.",
        _ => ex.Message
    };
}
