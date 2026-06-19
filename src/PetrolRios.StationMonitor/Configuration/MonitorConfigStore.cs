using System.Text.Json;

namespace PetrolRios.StationMonitor.Configuration;

/// <summary>Configuración editable del monitor, persistida junto al ejecutable.</summary>
public sealed class MonitorConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly object _sync = new();
    private readonly string _path;
    private MonitorSettings _actual;

    public MonitorConfigStore(IConfiguration configuration)
    {
        _path = Path.Combine(AppContext.BaseDirectory, "config", "station-monitor.json");
        _actual = Cargar(configuration);
    }

    public MonitorSettings Actual
    {
        get
        {
            lock (_sync)
                return _actual.Clonar();
        }
    }

    public void Guardar(MonitorSettings settings)
    {
        settings.CodigoEstacion = settings.CodigoEstacion.Trim().ToUpperInvariant();
        settings.ServerUrl = settings.ServerUrl.Trim().TrimEnd('/');
        settings.Email = settings.Email.Trim();
        settings.IntervaloSegundos = Math.Clamp(settings.IntervaloSegundos, 5, 3600);
        settings.DiasConsulta = Math.Clamp(settings.DiasConsulta, 1, 365);

        var directory = Path.GetDirectoryName(_path)!;
        Directory.CreateDirectory(directory);
        var temp = _path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(settings, JsonOptions));
        File.Move(temp, _path, true);

        lock (_sync)
            _actual = settings.Clonar();
    }

    private MonitorSettings Cargar(IConfiguration configuration)
    {
        if (File.Exists(_path))
        {
            try
            {
                var json = File.ReadAllText(_path);
                var persisted = JsonSerializer.Deserialize<MonitorSettings>(json, JsonOptions);
                if (persisted is not null)
                    return persisted;
            }
            catch
            {
                // Si el archivo quedó corrupto, se conserva para diagnóstico y se usan defaults.
            }
        }

        return configuration
            .GetSection(MonitorSettings.SectionName)
            .Get<MonitorSettings>() ?? new MonitorSettings();
    }
}
