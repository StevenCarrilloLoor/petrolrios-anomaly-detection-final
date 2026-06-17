using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Publica el manifiesto de actualización que consultan los agentes de estación
/// (control de versiones). La fuente es editable sin recompilar: un archivo
/// <c>config/agente-version.json</c> junto al servidor, o la sección
/// <c>Actualizaciones</c> de appsettings. Si no hay manifiesto configurado, se
/// reporta la versión actual del servidor sin URL (no hay actualización que aplicar).
/// </summary>
[ApiController]
[Route("api/v1/agente")]
public sealed class AgenteController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AgenteController> _logger;

    public AgenteController(IConfiguration config, IWebHostEnvironment env, ILogger<AgenteController> logger)
    {
        _config = config;
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Manifiesto de la última versión del agente disponible. Acceso anónimo: los
    /// agentes lo consultan antes de autenticarse y no expone datos sensibles.
    /// </summary>
    [HttpGet("version")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Version()
    {
        // 1) Archivo editable junto al ejecutable (config/agente-version.json)
        var ruta = Path.Combine(AppContext.BaseDirectory, "config", "agente-version.json");
        if (System.IO.File.Exists(ruta))
        {
            try
            {
                var json = System.IO.File.ReadAllText(ruta);
                var doc = JsonSerializer.Deserialize<ManifiestoAgente>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (doc is not null && !string.IsNullOrWhiteSpace(doc.Version))
                    return Ok(doc);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo leer {Ruta}; se usa appsettings/versión del servidor", ruta);
            }
        }

        // 2) Sección Actualizaciones de appsettings
        var sec = _config.GetSection("Actualizaciones");
        var versionCfg = sec["Version"];
        if (!string.IsNullOrWhiteSpace(versionCfg))
        {
            return Ok(new ManifiestoAgente
            {
                Version = versionCfg,
                Url = sec["Url"] ?? "",
                Sha256 = sec["Sha256"],
                Notas = sec["Notas"],
                Obligatoria = bool.TryParse(sec["Obligatoria"], out var o) && o
            });
        }

        // 3) Por defecto: versión del propio servidor, sin URL (no hay actualización)
        var v = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion?.Split('+')[0]
            ?? "0.0.0";
        return Ok(new ManifiestoAgente { Version = v, Url = "", Notas = "Sin actualización publicada." });
    }

    public sealed record ManifiestoAgente
    {
        public string Version { get; init; } = "0.0.0";
        public string Url { get; init; } = "";
        public string? Sha256 { get; init; }
        public string? Notas { get; init; }
        public bool Obligatoria { get; init; }
    }
}
