using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Publica el manifiesto de actualización que consultan los monitores de estación. La fuente es
/// editable sin recompilar: un archivo <c>config/monitor-version.json</c> junto al servidor, o la
/// sección <c>ActualizacionesMonitor</c> de appsettings. Si no hay manifiesto, reporta la versión
/// del servidor sin URL (no hay actualización que aplicar).
/// </summary>
[ApiController]
[Route("api/v1/monitor-estacion")]
public sealed class MonitorEstacionController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ILogger<MonitorEstacionController> _logger;

    public MonitorEstacionController(IConfiguration config, ILogger<MonitorEstacionController> logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <summary>Manifiesto de la última versión del monitor. Acceso anónimo (no expone datos sensibles).</summary>
    [HttpGet("version")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Version()
    {
        var ruta = Path.Combine(AppContext.BaseDirectory, "config", "monitor-version.json");
        if (System.IO.File.Exists(ruta))
        {
            try
            {
                var doc = JsonSerializer.Deserialize<ManifiestoMonitor>(
                    System.IO.File.ReadAllText(ruta),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (doc is not null && !string.IsNullOrWhiteSpace(doc.Version))
                    return Ok(doc);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo leer {Ruta}; se usa appsettings/versión del servidor", ruta);
            }
        }

        var sec = _config.GetSection("ActualizacionesMonitor");
        var versionCfg = sec["Version"];
        if (!string.IsNullOrWhiteSpace(versionCfg))
        {
            return Ok(new ManifiestoMonitor
            {
                Version = versionCfg,
                Url = sec["Url"] ?? "",
                Sha256 = sec["Sha256"],
                Notas = sec["Notas"],
                Obligatoria = bool.TryParse(sec["Obligatoria"], out var o) && o
            });
        }

        var propia = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion?.Split('+')[0]
            ?? "0.0.0";
        return Ok(new ManifiestoMonitor { Version = propia, Url = "", Notas = "Sin actualización publicada." });
    }

    public sealed record ManifiestoMonitor
    {
        public string Version { get; init; } = "0.0.0";
        public string Url { get; init; } = "";
        public string? Sha256 { get; init; }
        public string? Notas { get; init; }
        public bool Obligatoria { get; init; }
    }
}
