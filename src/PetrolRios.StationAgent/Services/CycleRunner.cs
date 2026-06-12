using System.Diagnostics;
using Microsoft.Extensions.Options;
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
    private readonly AgentState _state;
    private readonly AgentOptions _options;
    private readonly ILogger<CycleRunner> _logger;
    private readonly string _watermarkFile;
    private DateTime _lastWatermark;

    public DateTime Watermark => _lastWatermark;

    public CycleRunner(
        FirebirdExtractor extractor,
        ServerClient serverClient,
        LocalStore localStore,
        AgentState state,
        IOptions<AgentOptions> options,
        ILogger<CycleRunner> logger)
    {
        _extractor = extractor;
        _serverClient = serverClient;
        _localStore = localStore;
        _state = state;
        _options = options.Value;
        _logger = logger;
        _watermarkFile = Path.Combine(_options.LocalStorePath, "watermark.txt");
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
        try
        {
            _logger.LogInformation("Ciclo iniciado — extrayendo desde {Watermark:O}", _lastWatermark);
            _state.RegistrarEvento("INFO", $"Ciclo iniciado (watermark {_lastWatermark:HH:mm:ss})");

            // 1. Reintentar lotes pendientes (store-and-forward)
            var pendientesEnviados = await RetrySendPendingBatchesAsync(ct);

            // 2. Extraer nuevas transacciones desde Firebird
            var items = await _extractor.ExtractSinceAsync(_lastWatermark, ct);

            if (items.Count == 0)
            {
                var resumenVacio = pendientesEnviados > 0
                    ? $"Sin datos nuevos; {pendientesEnviados} pendientes reenviados"
                    : "Sin transacciones nuevas";
                Completar(true, resumenVacio, 0);
                return resumenVacio;
            }

            // 3. Enviar al servidor central
            var sent = await _serverClient.SendBatchAsync(items, ct);

            if (sent)
            {
                _lastWatermark = DateTime.UtcNow;
                SaveWatermark(_lastWatermark);
                _state.TotalTransaccionesEnviadas += items.Count;
                _state.UltimaConexionServidor = DateTime.UtcNow;

                var resumen = $"{items.Count} transacciones enviadas en {sw.Elapsed.TotalSeconds:F1}s";
                Completar(true, resumen, items.Count);
                return resumen;
            }

            // 4. Store-and-forward: guardar localmente para reintento
            await _localStore.SavePendingAsync(items, ct);
            _state.UltimaDesconexionServidor = DateTime.UtcNow;

            var resumenFallo = $"Servidor no disponible — {items.Count} transacciones guardadas localmente";
            Completar(false, resumenFallo, items.Count);
            return resumenFallo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en el ciclo del agente");
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
            if (!Directory.Exists(_options.LocalStorePath)) return 0;
            return Directory.GetFiles(_options.LocalStorePath, "batch_*.json").Length;
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
