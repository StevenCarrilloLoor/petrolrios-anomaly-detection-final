using System.Diagnostics;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PetrolRios.StationAgent.Configuration;

namespace PetrolRios.StationAgent.Services;

/// <summary>
/// Manifiesto de actualización publicado por el feed (servidor central o GitHub).
/// </summary>
public sealed record ManifiestoActualizacion
{
    [JsonPropertyName("version")] public string Version { get; init; } = "0.0.0";
    [JsonPropertyName("url")] public string Url { get; init; } = "";
    [JsonPropertyName("sha256")] public string? Sha256 { get; init; }
    [JsonPropertyName("notas")] public string? Notas { get; init; }
    [JsonPropertyName("obligatoria")] public bool Obligatoria { get; init; }
}

/// <summary>
/// Consulta el feed de actualización (configurable) y aplica la actualización con
/// un clic: descarga el nuevo ejecutable, verifica su checksum, escribe un script
/// que intercambia el .exe y reinicia el agente (servicio o proceso).
/// </summary>
public sealed class UpdateService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };
    private readonly IHttpClientFactory _httpFactory;
    private readonly AgentConfigStore _config;
    private readonly ILogger<UpdateService> _logger;

    public UpdateService(IHttpClientFactory httpFactory, AgentConfigStore config, ILogger<UpdateService> logger)
    {
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
    }

    /// <summary>Descarga el manifiesto desde la URL primaria; si falla, prueba la de respaldo.</summary>
    public async Task<ManifiestoActualizacion?> ConsultarAsync(CancellationToken ct)
    {
        var settings = _config.Actual;
        var urls = new[] { settings.ResolverUpdateFeedUrl(), settings.UpdateFeedFallbackUrl }
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Distinct();

        foreach (var url in urls)
        {
            try
            {
                using var http = _httpFactory.CreateClient("actualizaciones");
                http.Timeout = TimeSpan.FromSeconds(15);
                var manifiesto = await http.GetFromJsonAsync<ManifiestoActualizacion>(url, JsonOpts, ct);
                if (manifiesto is not null && !string.IsNullOrWhiteSpace(manifiesto.Version))
                    return manifiesto;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "No se pudo leer el feed de actualización en {Url}", url);
            }
        }
        return null;
    }

    /// <summary>true si la versión del manifiesto es mayor a la instalada.</summary>
    public static bool EsMasNueva(string disponible, string instalada) =>
        Comparar(disponible, instalada) > 0;

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

    /// <summary>
    /// Descarga el nuevo ejecutable, verifica el checksum (si viene), y deja
    /// programado el reemplazo + reinicio. Devuelve (ok, mensaje). Si ok=true, el
    /// llamador debe detener la aplicación para liberar el .exe.
    /// </summary>
    public async Task<(bool Ok, string Mensaje)> AplicarAsync(ManifiestoActualizacion manifiesto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(manifiesto.Url))
            return (false, "El manifiesto no incluye la URL del ejecutable.");

        var dirApp = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        var esWindows = OperatingSystem.IsWindows();
        const string nombre = "PetrolRios.StationAgent";
        var exeActual = Path.Combine(dirApp, esWindows ? $"{nombre}.exe" : nombre);
        var exeNuevo = Path.Combine(dirApp, esWindows ? $"{nombre}.nuevo.exe" : $"{nombre}.nuevo");

        // 1) Descargar
        try
        {
            using var http = _httpFactory.CreateClient("actualizaciones");
            http.Timeout = TimeSpan.FromMinutes(5);
            var bytes = await http.GetByteArrayAsync(manifiesto.Url, ct);

            // 2) Verificar checksum si el manifiesto lo trae
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
            _logger.LogError(ex, "Falló la descarga de la actualización");
            return (false, $"No se pudo descargar la actualización: {ex.Message}");
        }

        // 3) Programar el reemplazo + reinicio, según el sistema operativo.
        try
        {
            if (esWindows)
            {
                var servicio = _config.Actual.NombreServicioWindows;
                var script = Path.Combine(dirApp, "aplicar_actualizacion.bat");
                await File.WriteAllTextAsync(script, ScriptWindows(exeActual, exeNuevo, servicio), ct);
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{script}\"",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            }
            else
            {
                // Linux/macOS: el script reemplaza el binario y mata el proceso; systemd (Restart=on-failure)
                // o launchd (KeepAlive) lo relanzan solos con el binario nuevo. No depende del nombre del servicio.
                var script = Path.Combine(dirApp, "aplicar_actualizacion.sh");
                await File.WriteAllTextAsync(script, ScriptUnix(exeActual, exeNuevo, Environment.ProcessId), ct);
                Process.Start(new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"nohup bash '{script}' >/dev/null 2>&1 &\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }

            return (true, $"Descargada la versión {manifiesto.Version}. El agente se reiniciará para aplicarla.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falló al programar el reemplazo del ejecutable");
            return (false, $"No se pudo iniciar el actualizador: {ex.Message}");
        }
    }

    /// <summary>Windows: detiene el servicio, intercambia el .exe y lo reinicia (o lo ejecuta directo).</summary>
    private static string ScriptWindows(string exeActual, string exeNuevo, string servicio) =>
        $"""
        @echo off
        rem Actualizador del Agente PetrolRios — generado automaticamente
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

    /// <summary>Linux/macOS: reemplaza el binario y mata el proceso; el gestor de servicios lo relanza.</summary>
    private static string ScriptUnix(string exeActual, string exeNuevo, int pid) =>
        "#!/usr/bin/env bash\n"
        + "sleep 4\n"
        + $"mv -f \"{exeNuevo}\" \"{exeActual}\"\n"
        + $"chmod +x \"{exeActual}\"\n"
        + "# Matar el proceso: systemd (Restart) o launchd (KeepAlive) relanzan con el binario nuevo.\n"
        + $"kill -9 {pid} 2>/dev/null || true\n";
}
