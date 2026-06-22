using System.Diagnostics;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PetrolRios.StationMonitor.Configuration;

namespace PetrolRios.StationMonitor.Services;

/// <summary>Manifiesto de actualización publicado por el central.</summary>
public sealed record ManifiestoActualizacion
{
    [JsonPropertyName("version")] public string Version { get; init; } = "0.0.0";
    [JsonPropertyName("url")] public string Url { get; init; } = "";
    [JsonPropertyName("sha256")] public string? Sha256 { get; init; }
    [JsonPropertyName("notas")] public string? Notas { get; init; }
    [JsonPropertyName("obligatoria")] public bool Obligatoria { get; init; }
}

/// <summary>
/// Auto-actualización del monitor de estación. Consulta el manifiesto que sirve el central
/// (<c>{ServerUrl}/api/v1/monitor-estacion/version</c>), descarga el nuevo ejecutable, verifica el
/// checksum SHA256, lo intercambia y reinicia el servicio. Mismo mecanismo probado del agente, para
/// poder actualizar las estaciones de forma remota (sin visitarlas).
/// </summary>
public sealed class UpdateService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };
    private const string ServicioWindows = "PetrolRios Station Monitor";

    private readonly IHttpClientFactory _httpFactory;
    private readonly MonitorConfigStore _config;
    private readonly ILogger<UpdateService> _logger;

    public UpdateService(IHttpClientFactory httpFactory, MonitorConfigStore config, ILogger<UpdateService> logger)
    {
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
    }

    private string? FeedUrl()
    {
        var server = _config.Actual.ServerUrl?.TrimEnd('/');
        return string.IsNullOrWhiteSpace(server) ? null : $"{server}/api/v1/monitor-estacion/version";
    }

    /// <summary>Descarga el manifiesto desde el central. Null si no hay feed o falla.</summary>
    public async Task<ManifiestoActualizacion?> ConsultarAsync(CancellationToken ct)
    {
        var url = FeedUrl();
        if (string.IsNullOrWhiteSpace(url)) return null;
        try
        {
            using var http = _httpFactory.CreateClient("actualizaciones");
            http.Timeout = TimeSpan.FromSeconds(15);
            var m = await http.GetFromJsonAsync<ManifiestoActualizacion>(url, JsonOpts, ct);
            return m is not null && !string.IsNullOrWhiteSpace(m.Version) ? m : null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "No se pudo leer el feed de actualización del monitor en {Url}", url);
            return null;
        }
    }

    /// <summary>true si la versión del manifiesto es mayor a la instalada.</summary>
    public static bool EsMasNueva(string disponible, string instalada) => Comparar(disponible, instalada) > 0;

    private static int Comparar(string a, string b)
    {
        static int[] Partes(string v) => (v ?? "0")
            .Split('+', '-')[0]
            .Split('.')
            .Select(p => int.TryParse(p, out var n) ? n : 0)
            .Concat(new[] { 0, 0, 0 })
            .Take(3)
            .ToArray();

        var pa = Partes(a);
        var pb = Partes(b);
        for (var i = 0; i < 3; i++)
            if (pa[i] != pb[i]) return pa[i].CompareTo(pb[i]);
        return 0;
    }

    /// <summary>Descarga, verifica checksum y deja programado el reemplazo + reinicio.</summary>
    public async Task<(bool Ok, string Mensaje)> AplicarAsync(ManifiestoActualizacion manifiesto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(manifiesto.Url))
            return (false, "El manifiesto no incluye la URL del ejecutable.");

        var dirApp = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        var exeActual = Path.Combine(dirApp, "PetrolRios.StationMonitor.exe");
        var exeNuevo = Path.Combine(dirApp, "PetrolRios.StationMonitor.nuevo.exe");

        try
        {
            using var http = _httpFactory.CreateClient("actualizaciones");
            http.Timeout = TimeSpan.FromMinutes(5);
            var bytes = await http.GetByteArrayAsync(manifiesto.Url, ct);

            if (!string.IsNullOrWhiteSpace(manifiesto.Sha256))
            {
                var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                if (!hash.Equals(manifiesto.Sha256.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                    return (false, "El archivo descargado no coincide con el checksum esperado (descarga corrupta o manipulada).");
            }

            await File.WriteAllBytesAsync(exeNuevo, bytes, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falló la descarga de la actualización del monitor");
            return (false, $"No se pudo descargar la actualización: {ex.Message}");
        }

        try
        {
            var script = Path.Combine(dirApp, "aplicar_actualizacion.bat");
            await File.WriteAllTextAsync(script, ConstruirScript(exeActual, exeNuevo, ServicioWindows), ct);

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{script}\"",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });

            return (true, $"Descargada la versión {manifiesto.Version}. El monitor se reiniciará para aplicarla.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falló al programar el reemplazo del ejecutable del monitor");
            return (false, $"No se pudo iniciar el actualizador: {ex.Message}");
        }
    }

    private static string ConstruirScript(string exeActual, string exeNuevo, string servicio) =>
        $"""
        @echo off
        rem Actualizador del Monitor PetrolRios — generado automaticamente
        timeout /t 4 /nobreak >nul
        net stop "{servicio}" >nul 2>&1
        :reintento
        move /Y "{exeNuevo}" "{exeActual}" >nul 2>&1
        if errorlevel 1 (
          timeout /t 2 /nobreak >nul
          goto reintento
        )
        sc query "{servicio}" >nul 2>&1
        if not errorlevel 1 (
          net start "{servicio}" >nul 2>&1
        ) else (
          start "" "{exeActual}"
        )
        del "%~f0" >nul 2>&1
        """;
}
