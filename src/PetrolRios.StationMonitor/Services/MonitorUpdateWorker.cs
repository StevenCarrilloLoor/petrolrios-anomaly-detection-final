using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PetrolRios.StationMonitor.Configuration;

namespace PetrolRios.StationMonitor.Services;

/// <summary>
/// Revisa periódicamente si el central publicó una versión nueva del monitor y la aplica sola.
/// Así una estación se actualiza de forma remota, sin que nadie tenga que ir a reinstalar.
/// </summary>
public sealed class MonitorUpdateWorker : BackgroundService
{
    private static readonly TimeSpan EsperaInicial = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan Intervalo = TimeSpan.FromHours(6);

    private readonly UpdateService _update;
    private readonly MonitorConfigStore _config;
    private readonly ILogger<MonitorUpdateWorker> _logger;

    public MonitorUpdateWorker(UpdateService update, MonitorConfigStore config, ILogger<MonitorUpdateWorker> logger)
    {
        _update = update;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(EsperaInicial, stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_config.Actual.Configurado)
                {
                    var m = await _update.ConsultarAsync(stoppingToken);
                    if (m is not null && !string.IsNullOrWhiteSpace(m.Url)
                        && UpdateService.EsMasNueva(m.Version, VersionMonitor.Actual))
                    {
                        _logger.LogInformation("Actualización del monitor disponible: {Version}. Aplicando...", m.Version);
                        var (ok, mensaje) = await _update.AplicarAsync(m, stoppingToken);
                        _logger.LogInformation("Resultado de la actualización del monitor: {Ok} — {Mensaje}", ok, mensaje);
                        if (ok) return; // el script detendrá/reiniciará el servicio
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallo al revisar actualizaciones del monitor");
            }

            try { await Task.Delay(Intervalo, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
}
