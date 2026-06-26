using System.Diagnostics;
using PetrolRios.StationAgent.Configuration;

namespace PetrolRios.StationAgent.Services;

/// <summary>
/// Ejecuta un ciclo de sincronización: reintenta pendientes → extrae de Firebird
/// desde la marca de agua → envía al servidor central → actualiza la marca de agua.
/// Usado por el Worker (modo automático) y por el panel local (sincronización manual).
/// </summary>
public sealed class CycleRunner
{
    private readonly FirebirdExtractor _extractor;
    private readonly ServerClient _serverClient;
    private readonly LocalStore _localStore;
    private readonly SentMemory _sentMemory;
    private readonly SourceWatermarkStore _sourceWatermarks;
    private readonly AgentState _state;
    private readonly AgentConfigStore _config;
    private readonly ILogger<CycleRunner> _logger;
    private readonly string _watermarkFile;
    private DateTime _lastWatermark;

    public DateTime Watermark => _lastWatermark;

    /// <summary>
    /// Reinicia la marca de agua a una fecha dada para volver a extraer y enviar las
    /// transacciones desde ese momento (utilidad de mantenimiento del panel).
    /// </summary>
    public void ReiniciarWatermark(DateTime fechaUtc)
    {
        // La fecha viene del panel (hora de la estación); se trata como reloj de Firebird (naive),
        // que es contra lo que se compara FEC_DCTO/FIN_DESP.
        _lastWatermark = DateTime.SpecifyKind(fechaUtc, DateTimeKind.Unspecified);
        SaveWatermark(_lastWatermark);
        _state.RegistrarEvento("INFO", $"Marca de agua reiniciada a {_lastWatermark:yyyy-MM-dd HH:mm} — se reenviarán datos desde esa fecha");
    }

    public CycleRunner(
        FirebirdExtractor extractor,
        ServerClient serverClient,
        LocalStore localStore,
        SentMemory sentMemory,
        SourceWatermarkStore sourceWatermarks,
        AgentState state,
        AgentConfigStore config,
        ILogger<CycleRunner> logger)
    {
        _extractor = extractor;
        _serverClient = serverClient;
        _localStore = localStore;
        _sentMemory = sentMemory;
        _sourceWatermarks = sourceWatermarks;
        _state = state;
        _config = config;
        _logger = logger;
        _watermarkFile = Path.Combine(_config.Actual.LocalStorePath, "watermark.txt");
        _lastWatermark = LoadWatermark();
    }

    /// <summary>
    /// Ejecuta un ciclo completo. Devuelve un resumen legible del resultado.
    /// </summary>
    public async Task<string> RunCycleAsync(CancellationToken ct)
    {
        if (!await _state.CandadoCiclo.WaitAsync(0, ct))
            return "Ya hay un ciclo en ejecución";

        var sw = Stopwatch.StartNew();
        List<FuenteExtraccion>? fuentesCentrales = null;
        try
        {
            _logger.LogInformation("Ciclo iniciado — extrayendo desde {Watermark:O}", _lastWatermark);
            _state.RegistrarEvento("INFO", $"Ciclo iniciado (watermark {_lastWatermark:HH:mm:ss})");

            // 1. Reintentar lotes pendientes (store-and-forward). Lo reenviado SÍ cuenta como
            //    transacciones enviadas (antes el contador "Transacciones enviadas" ignoraba la cola
            //    y mostraba 0 aunque se vaciaran cientos de lotes — confuso).
            var pendientesEnviados = await RetrySendPendingBatchesAsync(ct);
            if (pendientesEnviados > 0)
                _state.TotalTransaccionesEnviadas += pendientesEnviados;

            // 2. Extraer nuevas transacciones desde Firebird. Antes pedimos al central el
            //    catálogo de fuentes extra (registradas una sola vez por el ingeniero). Si el
            //    central no responde, se devuelve null y se sigue con las fuentes locales.
            fuentesCentrales = await _serverClient.ObtenerFuentesCentralAsync(ct);
            if (fuentesCentrales is not null)
                _state.ActualizarFuentesCentrales(fuentesCentrales);
            var extraccion = await _extractor.ExtractSinceAsync(
                _lastWatermark, fuentesCentrales, ct);
            var items = extraccion.Items;

            // 2b. Filtrar lo ya enviado: algunos registros "vivos" reaparecen ciclo a ciclo
            //     (p. ej. un turno aún sin cerrar, EST_TURN='0') aunque su marca de agua sea
            //     anterior. La memoria de envío evita reenviarlos una y otra vez.
            var nuevos = _sentMemory.FiltrarNuevos(items);
            var omitidos = items.Count - nuevos.Count;

            if (nuevos.Count == 0)
            {
                GuardarCursores(extraccion.CursoresFuentes);
                await ReportarEstadosAsync(
                    extraccion.EstadosFuentes,
                    nuevos,
                    "Sincronizada",
                    fuentesCentrales,
                    ct);

                var partes = new List<string>();
                if (omitidos > 0) partes.Add($"{omitidos} ya enviados (omitidos)");
                if (pendientesEnviados > 0) partes.Add($"{pendientesEnviados} pendientes reenviados");
                var resumenVacio = partes.Count > 0
                    ? string.Join("; ", partes)
                    : "Sin transacciones nuevas";
                Completar(true, resumenVacio, 0);
                return resumenVacio;
            }

            // 3. Enviar al servidor central
            var sent = await _serverClient.SendBatchAsync(nuevos, ct);

            if (sent)
            {
                _sentMemory.MarcarEnviados(nuevos);
                GuardarCursores(extraccion.CursoresFuentes);
                await ReportarEstadosAsync(
                    extraccion.EstadosFuentes,
                    nuevos,
                    "Sincronizada",
                    fuentesCentrales,
                    ct);
                // Avanzar la marca con el RELOJ DE FIREBIRD (no DateTime.UtcNow): así vive en el mismo
                // reloj que FEC_DCTO/FIN_DESP y no se desfasa en estaciones fuera de UTC (antes una
                // estación en UTC-5 saltaba 5 h al futuro y dejaba de extraer lo nuevo).
                _lastWatermark = extraccion.RelojFirebird;
                SaveWatermark(_lastWatermark);
                _state.TotalTransaccionesEnviadas += nuevos.Count;
                _state.UltimaConexionServidor = DateTime.UtcNow;

                var sufijo = omitidos > 0 ? $" ({omitidos} ya enviados, omitidos)" : "";
                var resumen = $"{nuevos.Count} transacciones enviadas en {sw.Elapsed.TotalSeconds:F1}s{sufijo}";
                Completar(true, resumen, nuevos.Count);
                return resumen;
            }

            // 4. Store-and-forward: guardar localmente para reintento
            await _localStore.SavePendingAsync(nuevos, ct);
            // El lote ya quedó persistido localmente: avanzar el cursor de las fuentes
            // evita crear el mismo archivo pendiente en cada ciclo mientras vuelve la red.
            GuardarCursores(extraccion.CursoresFuentes);
            await ReportarEstadosAsync(
                extraccion.EstadosFuentes,
                nuevos,
                "PendienteEnvio",
                fuentesCentrales,
                ct);
            _state.UltimaDesconexionServidor = DateTime.UtcNow;

            var resumenFallo = $"Servidor no disponible — {nuevos.Count} transacciones guardadas localmente";
            Completar(false, resumenFallo, nuevos.Count);
            return resumenFallo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en el ciclo del agente");
            if (fuentesCentrales is { Count: > 0 })
            {
                var estadosError = fuentesCentrales.Select(f => new EstadoFuenteAgente(
                    f.Id,
                    f.Version,
                    "Error",
                    false,
                    false,
                    0,
                    0,
                    ex.Message)).ToList();
                _state.ActualizarFuentesCentrales(fuentesCentrales, estadosError);
                await _serverClient.ReportarEstadoFuentesAsync(estadosError, ct);
            }
            var resumenError = $"Error: {ex.Message}";
            Completar(false, resumenError, 0);
            return resumenError;
        }
        finally
        {
            _state.CandadoCiclo.Release();
        }
    }

    public int ContarPendientes()
    {
        try
        {
            var storePath = _config.Actual.LocalStorePath;
            if (!Directory.Exists(storePath)) return 0;
            return Directory.GetFiles(storePath, "batch_*.json").Length;
        }
        catch
        {
            return 0;
        }
    }

    private void Completar(bool exitoso, string resumen, int transacciones)
    {
        _state.UltimoCiclo = DateTime.UtcNow;
        _state.UltimoResultado = resumen;
        _state.UltimoCicloExitoso = exitoso;
        _state.UltimoLoteTransacciones = transacciones;
        _state.CiclosEjecutados++;
        _state.RegistrarEvento(exitoso ? "OK" : "ERROR", resumen);
        _logger.LogInformation("Ciclo completado — {Resumen}", resumen);
    }

    private async Task<int> RetrySendPendingBatchesAsync(CancellationToken ct)
    {
        var pending = await _localStore.GetPendingBatchesAsync(ct);
        if (pending.Count == 0) return 0;

        _logger.LogInformation("Reintentando {Count} lotes pendientes", pending.Count);
        _state.RegistrarEvento("INFO", $"Reintentando {pending.Count} lotes pendientes");

        var enviados = 0;
        foreach (var (filePath, items) in pending)
        {
            var sent = await _serverClient.SendBatchAsync(items, ct);
            if (sent)
            {
                _sentMemory.MarcarEnviados(items);
                _localStore.RemovePending(filePath);
                enviados += items.Count;
            }
            else
            {
                break; // Si falla uno, dejar los demás para el próximo ciclo
            }
        }
        return enviados;
    }

    private void GuardarCursores(IReadOnlyList<CursorFuenteExtraida> cursores)
    {
        foreach (var cursor in cursores)
            _sourceWatermarks.Save(
                cursor.FuenteDatosId,
                cursor.VersionFuente,
                cursor.CursorMaximo);
    }

    private async Task ReportarEstadosAsync(
        IReadOnlyList<EstadoFuenteAgente> estados,
        IReadOnlyList<TransaccionBatchItem> enviados,
        string estadoExitoso,
        IReadOnlyList<FuenteExtraccion>? fuentes,
        CancellationToken ct)
    {
        if (estados.Count == 0)
            return;

        var enviadosPorFuente = enviados
            .Where(i => i.FuenteDatosId.HasValue)
            .GroupBy(i => i.FuenteDatosId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var reporte = estados.Select(e =>
        {
            if (e.Estado is "TablaNoExiste" or "WatermarkInvalido" or "Error")
                return e;

            var filasEnviadas = enviadosPorFuente.GetValueOrDefault(e.FuenteDatosId);
            return e with
            {
                Estado = estadoExitoso,
                FilasEnviadas = filasEnviadas
            };
        }).ToList();

        if (fuentes is not null)
            _state.ActualizarFuentesCentrales(fuentes, reporte);

        await _serverClient.ReportarEstadoFuentesAsync(reporte, ct);
    }

    private DateTime LoadWatermark()
    {
        try
        {
            if (File.Exists(_watermarkFile))
            {
                // RoundtripKind: conserva el Kind del texto. La marca NUEVA se guarda sin zona
                // (Unspecified = reloj de Firebird) y se usa tal cual.
                var guardada = DateTime.Parse(
                    File.ReadAllText(_watermarkFile).Trim(),
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind);
                // Marca ANTIGUA guardada en UTC ('...Z', versión previa del agente): NO es del reloj
                // de Firebird → re-sembrar (default) para que el primer ciclo arranque desde el reloj
                // de Firebird y no quede 5 h desfasada tras actualizar.
                return guardada.Kind == DateTimeKind.Utc ? default : guardada;
            }
        }
        catch { /* Si no se puede leer, primera sincronización. */ }

        // Sentinela "sin marca previa": el extractor arranca desde 1 h atrás EN EL RELOJ DE FIREBIRD.
        return default;
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
