using System.Text.Json;
using PetrolRios.Application.Fuentes;
using PetrolRios.StationAgent.Configuration;

namespace PetrolRios.StationAgent.Services;

/// <summary>
/// Persiste una marca de agua independiente por fuente central. Una tabla recién agregada
/// no hereda el cursor global del agente y puede cargar su muestra inicial de forma segura.
/// </summary>
public sealed class SourceWatermarkStore
{
    private readonly string _filePath;
    private readonly object _lock = new();
    private Dictionary<int, SourceWatermarkEntry> _watermarks;

    public SourceWatermarkStore(AgentConfigStore config)
    {
        _filePath = Path.Combine(config.Actual.LocalStorePath, "source-watermarks.json");
        _watermarks = Load();
    }

    public DateTime? Get(FuenteExtraccion fuente)
    {
        if (fuente.Id <= 0)
            return null;

        lock (_lock)
        {
            var entry = _watermarks.GetValueOrDefault(fuente.Id);
            if (entry is null || entry.VersionFuente != fuente.Version)
                return null;
            return entry.Watermark;
        }
    }

    public void Save(int fuenteDatosId, DateTime versionFuente, DateTime watermark)
    {
        if (fuenteDatosId <= 0)
            return;

        lock (_lock)
        {
            _watermarks[fuenteDatosId] = new SourceWatermarkEntry(
                // Firebird TIMESTAMP no contiene zona horaria. El cursor debe volver a
                // consultarse con el mismo reloj local/naive que existe en la columna.
                FuenteDatosPolicy.NormalizarCursorFirebird(watermark),
                versionFuente.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(versionFuente, DateTimeKind.Utc)
                    : versionFuente.ToUniversalTime());
            Persist();
        }
    }

    public void Remove(int fuenteDatosId)
    {
        lock (_lock)
        {
            if (_watermarks.Remove(fuenteDatosId))
                Persist();
        }
    }

    private Dictionary<int, SourceWatermarkEntry> Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return [];

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<Dictionary<int, SourceWatermarkEntry>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private void Persist()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        var temp = $"{_filePath}.tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(_watermarks));
        File.Move(temp, _filePath, true);
    }

    private sealed record SourceWatermarkEntry(DateTime Watermark, DateTime VersionFuente);
}
