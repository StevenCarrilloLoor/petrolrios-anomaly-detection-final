using System.Text.Json;
using Microsoft.Extensions.Options;

namespace PetrolRios.StationAgent.Configuration;

/// <summary>
/// Fuente única de configuración del agente en tiempo de ejecución. Carga la
/// configuración desde un archivo JSON escribible junto al ejecutable
/// (config/agent-config.json); si no existe, parte de los valores de
/// appsettings.json. La interfaz del agente la edita y guarda en caliente, de
/// modo que cambiar el Firebird o el servidor central NO requiere reiniciar el
/// proceso (cada ciclo crea sus conexiones a partir de este store).
/// </summary>
public sealed class AgentConfigStore
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };
    private readonly string _rutaArchivo;
    private readonly ILogger<AgentConfigStore> _logger;
    private readonly object _candado = new();
    private AgentSettings _settings;

    public AgentConfigStore(IOptions<AgentOptions> defaults, ILogger<AgentConfigStore> logger)
    {
        _logger = logger;
        _rutaArchivo = Path.Combine(AppContext.BaseDirectory, "config", "agent-config.json");
        _settings = Cargar(defaults.Value);
    }

    /// <summary>Copia inmutable de la configuración actual (para lectura).</summary>
    public AgentSettings Actual
    {
        get { lock (_candado) return _settings.Clonar(); }
    }

    /// <summary>Reemplaza la configuración y la persiste a disco.</summary>
    public void Guardar(AgentSettings nueva)
    {
        lock (_candado)
        {
            nueva.Configurado = true;
            _settings = nueva;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_rutaArchivo)!);
                File.WriteAllText(_rutaArchivo, JsonSerializer.Serialize(nueva, JsonOpts));
                _logger.LogInformation("Configuración del agente guardada en {Ruta}", _rutaArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo guardar la configuración del agente");
            }
        }
    }

    private AgentSettings Cargar(AgentOptions defaults)
    {
        // Si hay archivo de configuración previo, tiene prioridad
        try
        {
            if (File.Exists(_rutaArchivo))
            {
                var json = File.ReadAllText(_rutaArchivo);
                var cargada = JsonSerializer.Deserialize<AgentSettings>(json, JsonOpts);
                if (cargada is not null)
                {
                    _logger.LogInformation("Configuración del agente cargada desde {Ruta}", _rutaArchivo);
                    return cargada;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo leer {Ruta}; se usan los valores de appsettings", _rutaArchivo);
        }

        // Valores iniciales tomados de appsettings.json (primer arranque)
        var settings = new AgentSettings
        {
            CodigoEstacion = defaults.CodigoEstacion,
            ServerUrl = defaults.ServerUrl,
            Email = defaults.Email,
            Password = defaults.Password,
            IntervaloSegundos = defaults.IntervaloSegundos,
            LocalStorePath = defaults.LocalStorePath,
            PanelPuerto = defaults.PanelPuerto,
            InicioAutomatico = defaults.InicioAutomatico,
            Configurado = false
        };

        // Descomponer el connection string por defecto en campos estructurados
        AplicarConnectionString(settings, defaults.FirebirdConnectionString);
        return settings;
    }

    private static void AplicarConnectionString(AgentSettings settings, string? cs)
    {
        if (string.IsNullOrWhiteSpace(cs)) return;
        var partes = cs.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var parte in partes)
        {
            var kv = parte.Split('=', 2);
            if (kv.Length != 2) continue;
            var clave = kv[0].Trim().ToLowerInvariant();
            var valor = kv[1].Trim();
            switch (clave)
            {
                case "user": settings.FirebirdUser = valor; break;
                case "password": settings.FirebirdPassword = valor; break;
                case "database": settings.FirebirdDatabase = valor; break;
                case "datasource": settings.FirebirdHost = valor; break;
                case "port" when int.TryParse(valor, out var p): settings.FirebirdPort = p; break;
                case "dialect" when int.TryParse(valor, out var d): settings.FirebirdDialect = d; break;
                case "charset": settings.FirebirdCharset = valor; break;
                case "wirecrypt": settings.FirebirdWireCrypt = valor; break;
            }
        }
    }
}
