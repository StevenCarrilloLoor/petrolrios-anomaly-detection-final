using System.Text.Json;
using PetrolRios.Application.DTOs.Consultas;
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
    private readonly UpdateService _updateService;
    private readonly FirebirdExtractor _extractor;
    private readonly AgentState _state;
    private readonly AgentConfigStore _config;
    private readonly ILogger<Worker> _logger;
    private DateTime _ultimaRevisionUpdate = DateTime.MinValue;
    private DateTime _ultimoReporteEsquema = DateTime.MinValue;
    private DateTime _ultimoSyncEmpleados = DateTime.MinValue;

    public Worker(
        CycleRunner cycleRunner,
        ServerClient serverClient,
        UpdateService updateService,
        FirebirdExtractor extractor,
        AgentState state,
        AgentConfigStore config,
        ILogger<Worker> logger)
    {
        _cycleRunner = cycleRunner;
        _serverClient = serverClient;
        _updateService = updateService;
        _extractor = extractor;
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
                    if (latido.Ok)
                        _state.UltimaConexionServidor = DateTime.UtcNow;
                    else
                        _state.UltimaDesconexionServidor = DateTime.UtcNow;

                    if (_state.ModoAutomatico)
                        await _cycleRunner.RunCycleAsync(stoppingToken);

                    // Reportar el esquema de Firebird al central: poco después de arrancar, cada ~6 h,
                    // o de inmediato si el central lo solicitó (botón "Cargar esquema" de una estación).
                    if (latido.ReportarEsquema || DateTime.UtcNow - _ultimoReporteEsquema > TimeSpan.FromHours(6))
                        await ReportarEsquemaAsync(stoppingToken);

                    // Consultas EN VIVO pedidas desde el central (documentos de Firebird): correrlas SOLO
                    // LECTURA y devolver el resultado de cada una. Llegan en el heartbeat (casi en vivo a 1 s).
                    if (latido.Consultas.Count > 0)
                        await EjecutarConsultasAsync(latido.Consultas, stoppingToken);

                    // Sincronizar el catálogo de empleados (código → nombre) cada ~6 h.
                    if (DateTime.UtcNow - _ultimoSyncEmpleados > TimeSpan.FromHours(6))
                        await SincronizarEmpleadosAsync(stoppingToken);

                    // Revisar el feed de actualización cada ~5 minutos (no en cada vuelta)
                    if (DateTime.UtcNow - _ultimaRevisionUpdate > TimeSpan.FromMinutes(5))
                    {
                        _ultimaRevisionUpdate = DateTime.UtcNow;
                        await RevisarActualizacionAsync(stoppingToken);
                    }
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

            // Mínimo 1 s (casi tiempo real); máximo 1 h. El default de fábrica es 1 s.
            var intervalo = Math.Clamp(settings.IntervaloSegundos, 1, 3600);
            await Task.Delay(TimeSpan.FromSeconds(intervalo), stoppingToken);
        }

        _logger.LogInformation("Agente de estación {Codigo} detenido", _config.Actual.CodigoEstacion);
    }

    /// <summary>
    /// Lee el esquema completo de Firebird (tablas + columnas) y lo reporta al central. Tolerante a
    /// fallos: si Firebird o el central no responden, se reintenta en la próxima vuelta.
    /// </summary>
    private async Task ReportarEsquemaAsync(CancellationToken ct)
    {
        try
        {
            var tablas = await _extractor.ObtenerEsquemaCompletoAsync(ct);
            if (tablas.Count == 0) return;

            if (await _serverClient.EnviarEsquemaAsync(tablas, ct))
            {
                _ultimoReporteEsquema = DateTime.UtcNow;
                _state.RegistrarEvento("INFO", $"Esquema reportado al central ({tablas.Count} tablas)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "No se pudo reportar el esquema (se reintenta luego)");
        }
    }

    /// <summary>
    /// Corre las consultas EN VIVO que pidió el central (documentos de DCTO, SOLO LECTURA) y devuelve el
    /// resultado de cada una. Tolerante a fallos: un error en una consulta se reporta como tal y no rompe el ciclo.
    /// </summary>
    private async Task EjecutarConsultasAsync(IReadOnlyList<ConsultaPendiente> consultas, CancellationToken ct)
    {
        foreach (var c in consultas)
        {
            try
            {
                var filas = await _extractor.ConsultarDocumentosAsync(
                    c.TipoDocumento, c.FechaDesde, c.FechaHasta, c.Codigo, c.Limite, ct);
                var json = JsonSerializer.Serialize(new { documentos = filas, total = filas.Count });
                await _serverClient.EnviarResultadoConsultaAsync(c.Id, true, json, null, ct);
                _state.RegistrarEvento("INFO", $"Consulta en vivo → {filas.Count} documento(s)");
            }
            catch (Exception ex)
            {
                await _serverClient.EnviarResultadoConsultaAsync(c.Id, false, null, ex.Message, ct);
                _logger.LogDebug(ex, "Error corriendo la consulta {Id}", c.Id);
            }
        }
    }

    /// <summary>
    /// Lee el catálogo de empleados de Firebird (VEND/EMPL) y lo sincroniza con el central para que
    /// las alertas muestren el NOMBRE junto al código. Tolerante a fallos: se reintenta en la próxima vuelta.
    /// </summary>
    private async Task SincronizarEmpleadosAsync(CancellationToken ct)
    {
        try
        {
            var empleados = await _extractor.ObtenerEmpleadosAsync(ct);
            if (empleados.Count == 0) return;
            if (await _serverClient.EnviarEmpleadosAsync(empleados, ct))
            {
                _ultimoSyncEmpleados = DateTime.UtcNow;
                _state.RegistrarEvento("INFO", $"Catálogo de empleados sincronizado ({empleados.Count})");
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "No se pudo sincronizar el catálogo de empleados (se reintenta luego)");
        }
    }

    /// <summary>
    /// Consulta el feed de actualización y, si hay una versión más nueva, lo
    /// refleja en el estado para que el panel (local y central) lo muestre.
    /// No descarga ni aplica nada: la aplicación es con un clic desde el panel.
    /// </summary>
    private async Task RevisarActualizacionAsync(CancellationToken ct)
    {
        try
        {
            var manifiesto = await _updateService.ConsultarAsync(ct);
            if (manifiesto is null) return;

            if (UpdateService.EsMasNueva(manifiesto.Version, VersionAgente.Actual))
            {
                if (!_state.ActualizacionDisponible || _state.VersionDisponible != manifiesto.Version)
                    _state.RegistrarEvento("INFO",
                        $"Actualización {manifiesto.Version} disponible (instalada {VersionAgente.Actual})");

                _state.ActualizacionDisponible = true;
                _state.VersionDisponible = manifiesto.Version;
                _state.NotasActualizacion = manifiesto.Notas;
                _state.UrlActualizacion = manifiesto.Url;
                _state.Sha256Actualizacion = manifiesto.Sha256;
                _state.ActualizacionObligatoria = manifiesto.Obligatoria;
            }
            else
            {
                _state.ActualizacionDisponible = false;
                _state.VersionDisponible = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "No se pudo revisar el feed de actualización");
        }
    }
}
