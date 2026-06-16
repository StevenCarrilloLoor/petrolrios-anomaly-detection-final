using PetrolRios.StationAgent.Configuration;
using PetrolRios.StationAgent.Services;

namespace PetrolRios.StationAgent;

/// <summary>
/// Worker del agente de estación. En modo automático ejecuta un ciclo de
/// sincronización cada N segundos; en modo manual queda a la espera de que el
/// operador dispare la sincronización desde el panel local. El intervalo, el
/// modo y demás parámetros se leen del <see cref="AgentConfigStore"/> en cada
/// vuelta, de modo que los cambios desde la interfaz aplican sin reiniciar.
/// </summary>
public sealed class Worker : BackgroundService
{
    private readonly CycleRunner _cycleRunner;
    private readonly ServerClient _serverClient;
    private readonly AgentState _state;
    private readonly AgentConfigStore _config;
    private readonly ILogger<Worker> _logger;

    public Worker(
        CycleRunner cycleRunner,
        ServerClient serverClient,
        AgentState state,
        AgentConfigStore config,
        ILogger<Worker> logger)
    {
        _cycleRunner = cycleRunner;
        _serverClient = serverClient;
        _state = state;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var inicial = _config.Actual;
        _logger.LogInformation(
            "Agente de estación {Codigo} iniciado. Intervalo: {Intervalo}s. Panel: http://localhost:{Puerto}",
            inicial.CodigoEstacion, inicial.IntervaloSegundos, inicial.PanelPuerto);
        _state.RegistrarEvento("INFO",
            $"Agente {inicial.CodigoEstacion} iniciado (intervalo {inicial.IntervaloSegundos}s)");

        while (!stoppingToken.IsCancellationRequested)
        {
            var settings = _config.Actual;
            try
            {
                // El agente solo opera si está configurado (evita golpear el
                // servidor con credenciales por defecto antes del primer setup).
                if (settings.Configurado)
                {
                    // Heartbeat SIEMPRE (también en modo manual): el panel central
                    // debe saber que el agente está vivo aunque no haya datos nuevos.
                    var latido = await _serverClient.SendHeartbeatAsync(stoppingToken);
                    if (latido)
                        _state.UltimaConexionServidor = DateTime.UtcNow;
                    else
                        _state.UltimaDesconexionServidor = DateTime.UtcNow;

                    if (_state.ModoAutomatico)
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

            var intervalo = Math.Clamp(settings.IntervaloSegundos, 5, 3600);
            await Task.Delay(TimeSpan.FromSeconds(intervalo), stoppingToken);
        }

        _logger.LogInformation("Agente de estación {Codigo} detenido", _config.Actual.CodigoEstacion);
    }
}
