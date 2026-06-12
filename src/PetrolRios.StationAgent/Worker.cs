using Microsoft.Extensions.Options;
using PetrolRios.StationAgent.Configuration;
using PetrolRios.StationAgent.Services;

namespace PetrolRios.StationAgent;

/// <summary>
/// Worker del agente de estación. En modo automático ejecuta un ciclo de
/// sincronización cada N segundos; en modo manual queda a la espera de que el
/// operador dispare la sincronización desde el panel local.
/// </summary>
public sealed class Worker : BackgroundService
{
    private readonly CycleRunner _cycleRunner;
    private readonly AgentState _state;
    private readonly AgentOptions _options;
    private readonly ILogger<Worker> _logger;

    public Worker(
        CycleRunner cycleRunner,
        AgentState state,
        IOptions<AgentOptions> options,
        ILogger<Worker> logger)
    {
        _cycleRunner = cycleRunner;
        _state = state;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Agente de estación {Codigo} iniciado. Intervalo: {Intervalo}s. Panel: http://localhost:{Puerto}",
            _options.CodigoEstacion, _options.IntervaloSegundos, _options.PanelPuerto);
        _state.RegistrarEvento("INFO",
            $"Agente {_options.CodigoEstacion} iniciado (intervalo {_options.IntervaloSegundos}s)");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_state.ModoAutomatico)
                {
                    await _cycleRunner.RunCycleAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el ciclo del agente");
                _state.RegistrarEvento("ERROR", ex.Message);
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.IntervaloSegundos), stoppingToken);
        }

        _logger.LogInformation("Agente de estación {Codigo} detenido", _options.CodigoEstacion);
    }
}
