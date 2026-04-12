using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetrolRios.StationAgent.Configuration;

namespace PetrolRios.StationAgent.Services;

/// <summary>
/// Store-and-forward: almacena lotes de transacciones localmente
/// cuando el servidor central no está disponible. Los reintenta en el siguiente ciclo.
/// </summary>
public sealed class LocalStore
{
    private readonly string _storePath;
    private readonly ILogger<LocalStore> _logger;

    public LocalStore(IOptions<AgentOptions> options, ILogger<LocalStore> logger)
    {
        _storePath = options.Value.LocalStorePath;
        _logger = logger;
        Directory.CreateDirectory(_storePath);
    }

    /// <summary>
    /// Guarda un lote de transacciones pendientes en un archivo JSON local.
    /// </summary>
    public async Task SavePendingAsync(List<TransaccionBatchItem> items, CancellationToken ct)
    {
        var fileName = $"batch_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.json";
        var filePath = Path.Combine(_storePath, fileName);
        var json = JsonSerializer.Serialize(items);
        await File.WriteAllTextAsync(filePath, json, ct);
        _logger.LogWarning("Lote de {Count} transacciones guardado localmente en {File}", items.Count, fileName);
    }

    /// <summary>
    /// Recupera y retorna todos los lotes pendientes de envío.
    /// </summary>
    public async Task<List<(string FilePath, List<TransaccionBatchItem> Items)>> GetPendingBatchesAsync(CancellationToken ct)
    {
        var batches = new List<(string, List<TransaccionBatchItem>)>();

        if (!Directory.Exists(_storePath))
            return batches;

        foreach (var file in Directory.GetFiles(_storePath, "batch_*.json").OrderBy(f => f))
        {
            try
            {
                var json = await File.ReadAllTextAsync(file, ct);
                var items = JsonSerializer.Deserialize<List<TransaccionBatchItem>>(json);
                if (items is not null && items.Count > 0)
                    batches.Add((file, items));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leyendo lote pendiente {File}", file);
            }
        }

        return batches;
    }

    /// <summary>
    /// Elimina un archivo de lote después de enviarlo exitosamente.
    /// </summary>
    public void RemovePending(string filePath)
    {
        try
        {
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando lote pendiente {File}", filePath);
        }
    }
}
