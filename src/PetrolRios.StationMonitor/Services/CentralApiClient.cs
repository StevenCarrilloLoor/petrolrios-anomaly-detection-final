using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PetrolRios.StationMonitor.Configuration;
using PetrolRios.StationMonitor.Models;

namespace PetrolRios.StationMonitor.Services;

/// <summary>Cliente de solo lectura para los problemas operativos del servidor central.</summary>
public sealed class CentralApiClient
{
    private readonly HttpClient _httpClient;
    private readonly MonitorConfigStore _config;
    private readonly ILogger<CentralApiClient> _logger;
    private readonly SemaphoreSlim _authLock = new(1, 1);
    private string? _token;
    private DateTime _expiration;
    private UsuarioCentral? _identidad;

    public CentralApiClient(
        HttpClient httpClient,
        MonitorConfigStore config,
        ILogger<CentralApiClient> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public void ReiniciarAutenticacion()
    {
        _token = null;
        _identidad = null;
        _expiration = default;
    }

    public async Task<(UsuarioCentral Identidad, IReadOnlyList<ProblemaOperativo> Problemas)>
        ObtenerProblemasActivosAsync(CancellationToken ct)
    {
        var settings = _config.Actual;
        var identidad = await AsegurarAutenticacionAsync(settings, ct);
        var response = await EnviarConsultaAsync(settings, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            ReiniciarAutenticacion();
            identidad = await AsegurarAutenticacionAsync(settings, ct);
            response = await EnviarConsultaAsync(settings, ct);
        }

        using (response)
        {
            response.EnsureSuccessStatusCode();
            var grupos = await response.Content.ReadFromJsonAsync<List<ProblemaEstacionGrupo>>(
                cancellationToken: ct) ?? [];

            var problemas = grupos
                .SelectMany(g => g.Problemas)
                .Where(p => p.EstacionId == identidad.EstacionId)
                .Where(p => p.Ambito.Equals("Operativa", StringComparison.OrdinalIgnoreCase))
                .Where(p => p.Estado is "Nueva" or "EnRevision")
                .DistinctBy(p => p.Id)
                .OrderByDescending(p => p.FechaDeteccion)
                .ToList();

            return (identidad, problemas);
        }
    }

    public async Task<(bool Ok, string Mensaje)> ProbarAsync(CancellationToken ct)
    {
        try
        {
            ReiniciarAutenticacion();
            var (identidad, problemas) = await ObtenerProblemasActivosAsync(ct);
            return (true,
                $"Conectado como {identidad.Email} a {identidad.EstacionCodigo} " +
                $"({problemas.Count} problema(s) activo(s)).");
        }
        catch (Exception ex)
        {
            return (false, MensajeAmigable(ex));
        }
    }

    private async Task<UsuarioCentral> AsegurarAutenticacionAsync(
        MonitorSettings settings,
        CancellationToken ct)
    {
        if (_token is not null
            && _identidad is not null
            && DateTime.UtcNow < _expiration.AddMinutes(-2))
        {
            return _identidad;
        }

        await _authLock.WaitAsync(ct);
        try
        {
            if (_token is not null
                && _identidad is not null
                && DateTime.UtcNow < _expiration.AddMinutes(-2))
            {
                return _identidad;
            }

            using var response = await _httpClient.PostAsJsonAsync(
                Url(settings.ServerUrl, "/api/v1/auth/login"),
                new { settings.Email, settings.Password },
                ct);
            response.EnsureSuccessStatusCode();

            var login = await response.Content.ReadFromJsonAsync<LoginCentralResponse>(
                cancellationToken: ct)
                ?? throw new InvalidOperationException("El central devolvió una sesión vacía.");

            if (login.Requiere2Fa || string.IsNullOrWhiteSpace(login.Token))
                throw new InvalidOperationException(
                    "La cuenta del monitor no debe tener 2FA; use una cuenta técnica de estación.");
            if (!login.Usuario.EstacionId.HasValue
                || string.IsNullOrWhiteSpace(login.Usuario.EstacionCodigo))
            {
                throw new InvalidOperationException(
                    "La cuenta no está asignada a una estación. Asígnela desde Usuarios en el central.");
            }
            if (!login.Usuario.EstacionCodigo.Equals(
                    settings.CodigoEstacion,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"La cuenta pertenece a {login.Usuario.EstacionCodigo}, no a {settings.CodigoEstacion}.");
            }

            _token = login.Token;
            _expiration = login.Expiration;
            _identidad = login.Usuario;
            return login.Usuario;
        }
        finally
        {
            _authLock.Release();
        }
    }

    private async Task<HttpResponseMessage> EnviarConsultaAsync(
        MonitorSettings settings,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            Url(
                settings.ServerUrl,
                $"/api/v1/alertas/problemas-estacion?dias={settings.DiasConsulta}&soloActivos=true"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        return await _httpClient.SendAsync(request, ct);
    }

    private static string Url(string baseUrl, string path) =>
        $"{baseUrl.TrimEnd('/')}{path}";

    public static string MensajeAmigable(Exception ex)
    {
        if (ex is HttpRequestException { StatusCode: HttpStatusCode.Unauthorized })
            return "Credenciales inválidas o cuenta sin acceso.";
        if (ex is HttpRequestException { StatusCode: HttpStatusCode.Forbidden })
            return "El servidor rechazó el acceso a esa estación.";
        if (ex is HttpRequestException)
            return "No se pudo contactar al servidor central.";
        if (ex is TaskCanceledException)
            return "El servidor central no respondió a tiempo.";

        return ex.Message;
    }
}
