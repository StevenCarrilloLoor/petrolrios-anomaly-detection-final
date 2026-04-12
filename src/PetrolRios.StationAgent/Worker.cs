using Microsoft.Extensions.Options;
using PetrolRios.StationAgent.Configuration;
using PetrolRios.StationAgent.Services;

namespace PetrolRios.StationAgent;

/// <summary>
/// Worker principal del agente de estación.
/// Cada ciclo: extrae datos de Firebird → envía al servidor → actualiza watermark.
/// Si el servidor no está disponible, almacena localmente (store-and-forward).
/// </summary>
public sealed class Worker : BackgroundService
{
    private readonly FirebirdExtractor _extractor;
    private readonly ServerClient _serverClient;
    private readonly LocalStore _localStore;
    private readonly AgentOptions _options;
    private readonly ILogger<Worker> _logger;
    private DateTime _lastWatermark;
    private readonly string _watermarkFile;

    public Worker(
        FirebirdExtractor extractor,
        ServerClient serverClient,
        LocalStore localStore,
        IOptions<AgentOptions> options,
        ILogger<Worker> logger)
    {
        _extractor = extractor;
        _serverClient = serverClient;
        _localStore = localStore;
        _options = options.Value;
        _logger = logger;
        _watermarkFile = Path.Combine(_options.LocalStorePath, "watermark.txt");
        _lastWatermark = LoadWatermark();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Agente de estación {Codigo} iniciado. Intervalo: {Intervalo}s. Watermark: {Watermark:O}",
            _options.CodigoEstacion, _options.IntervaloSegundos, _lastWatermark);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el ciclo del agente");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.IntervaloSegundos), stoppingToken);
        }

        _logger.LogInformation("Agente de estación {Codigo} detenido", _options.CodigoEstacion);
    }

    private async Task RunCycleAsync(CancellationToken ct)
    {
        _logger.LogInformation("Ciclo iniciado — extrayendo desde {Watermark:O}", _lastWatermark);

        // 1. Reintentar lotes pendientes (store-and-forward)
        await RetrySendPendingBatchesAsync(ct);

        // 2. Extraer nuevas transacciones desde Firebird
        var items = await _extractor.ExtractSinceAsync(_lastWatermark, ct);

        if (items.Count == 0)
        {
            _logger.LogInformation("Sin transacciones nuevas");
            return;
        }

        // 3. Enviar al servidor central
        var sent = await _serverClient.SendBatchAsync(items, ct);

        if (sent)
        {
            // 4. Actualizar watermark local
            _lastWatermark = DateTime.UtcNow;
            SaveWatermark(_lastWatermark);
            _logger.LogInformation("Ciclo completado — {Count} transacciones enviadas", items.Count);
        }
        else
        {
            // 5. Store-and-forward: guardar localmente para reintento
            await _localStore.SavePendingAsync(items, ct);
        }
    }

    private async Task RetrySendPendingBatchesAsync(CancellationToken ct)
    {
        var pending = await _localStore.GetPendingBatchesAsync(ct);
        if (pending.Count == 0) return;

        _logger.LogInformation("Reintentando {Count} lotes pendientes", pending.Count);

        foreach (var (filePath, items) in pending)
        {
            var sent = await _serverClient.SendBatchAsync(items, ct);
            if (sent)
                _localStore.RemovePending(filePath);
            else
                break; // Si falla uno, dejar los demás para el próximo ciclo
        }
    }

    private DateTime LoadWatermark()
    {
        try
        {
            if (File.Exists(_watermarkFile))
                return DateTime.Parse(File.ReadAllText(_watermarkFile).Trim());
        }
        catch { /* Si no se puede leer, empezar desde hace 1 hora */ }

        return DateTime.UtcNow.AddHours(-1);
    }

    private void SaveWatermark(DateTime watermark)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_watermarkFile)!);
            File.WriteAllText(_watermarkFile, watermark.ToString("O"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando watermark");
        }
    }
}
