using PetrolRios.StationMonitor.Configuration;

namespace PetrolRios.StationMonitor.Services;

public sealed class ProblemPollingWorker : BackgroundService
{
    private readonly CentralApiClient _client;
    private readonly MonitorConfigStore _config;
    private readonly MonitorState _state;
    private readonly ILogger<ProblemPollingWorker> _logger;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public ProblemPollingWorker(
        CentralApiClient client,
        MonitorConfigStore config,
        MonitorState state,
        ILogger<ProblemPollingWorker> logger)
    {
        _client = client;
        _config = config;
        _state = state;
        _logger = logger;
    }

    public async Task<(bool Ok, string Mensaje)> RefrescarAhoraAsync(CancellationToken ct)
    {
        var settings = _config.Actual;
        if (!settings.Configurado)
            return (false, "Configure el monitor antes de consultar.");

        if (!await _refreshLock.WaitAsync(0, ct))
            return (true, "Ya hay una consulta en curso.");

        try
        {
            var (identidad, problemas) = await _client.ObtenerProblemasActivosAsync(ct);
            _state.RegistrarExito(identidad, problemas);
            return (true, $"Consulta completada: {problemas.Count} problema(s) activo(s).");
        }
        catch (Exception ex)
        {
            var mensaje = CentralApiClient.MensajeAmigable(ex);
            _state.RegistrarError(mensaje);
            _logger.LogWarning(ex, "No se pudieron actualizar los problemas operativos");
            return (false, mensaje);
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var settings = _config.Actual;
            if (settings.Configurado)
                await RefrescarAhoraAsync(stoppingToken);

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(Math.Clamp(settings.IntervaloSegundos, 5, 3600)),
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
