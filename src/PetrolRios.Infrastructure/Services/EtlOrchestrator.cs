using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Firebird;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

/// <summary>
/// Orquesta la extracción de datos desde las bases Firebird de cada estación,
/// los carga a staging en PostgreSQL y actualiza las marcas de agua.
/// Tolerante a fallos: si una estación falla, continúa con las demás.
/// </summary>
public sealed class EtlOrchestrator : IEtlOrchestrator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PetrolRiosDbContext _dbContext;
    private readonly IFirebirdSourceClientFactory _clientFactory;
    private readonly FirebirdOptions _firebirdOptions;
    private readonly ILogger<EtlOrchestrator> _logger;
    private readonly ResiliencePipeline _resiliencePipeline;

    public EtlOrchestrator(
        IUnitOfWork unitOfWork,
        PetrolRiosDbContext dbContext,
        IFirebirdSourceClientFactory clientFactory,
        IOptions<FirebirdOptions> firebirdOptions,
        ILogger<EtlOrchestrator> logger)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _clientFactory = clientFactory;
        _firebirdOptions = firebirdOptions.Value;
        _logger = logger;

        // Polly v8: retry con backoff exponencial (2s, 4s, 8s)
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        args.Outcome.Exception,
                        "Reintento {Attempt} para extracción Firebird tras error",
                        args.AttemptNumber);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task<EtlResult> ExecuteAsync(CancellationToken ct = default)
    {
        var estaciones = await _unitOfWork.Estaciones.GetActivasAsync(ct);
        var errores = new Dictionary<string, string>();
        var totalTransacciones = 0;
        var watermarkMaxima = DateTime.MinValue;

        _logger.LogInformation("Iniciando ETL para {Count} estaciones activas", estaciones.Count);

        foreach (var estacion in estaciones)
        {
            try
            {
                if (!_firebirdOptions.Stations.TryGetValue(estacion.Codigo, out var connectionString))
                {
                    _logger.LogWarning(
                        "No se encontró connection string Firebird para estación {Codigo}",
                        estacion.Codigo);
                    errores[estacion.Codigo] = "Connection string no configurada";
                    continue;
                }

                var transacciones = await ExtractStationDataAsync(
                    estacion, connectionString, ct);

                totalTransacciones += transacciones.count;
                if (transacciones.maxDate > watermarkMaxima)
                    watermarkMaxima = transacciones.maxDate;

                _logger.LogInformation(
                    "Estación {Codigo}: {Count} registros extraídos",
                    estacion.Codigo, transacciones.count);
            }
            catch (OperationCanceledException)
            {
                throw; // Propagar cancelación
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error procesando estación {Codigo}: {Message}",
                    estacion.Codigo, ex.Message);
                errores[estacion.Codigo] = ex.Message;
            }
        }

        return new EtlResult
        {
            EstacionesProcesadas = estaciones.Count - errores.Count,
            EstacionesConError = errores.Count,
            TransaccionesExtraidas = totalTransacciones,
            Errores = errores,
            WatermarkMaxima = watermarkMaxima == DateTime.MinValue ? DateTime.UtcNow : watermarkMaxima
        };
    }

    private async Task<(int count, DateTime maxDate)> ExtractStationDataAsync(
        Estacion estacion, string connectionString, CancellationToken ct)
    {
        var watermark = await _unitOfWork.Estaciones.GetWatermarkAsync(estacion.Id, ct);
        var desde = watermark?.UltimaExtraccion ?? DateTime.UtcNow.AddHours(-1);

        var client = _clientFactory.Create(connectionString);
        var count = 0;
        var maxDate = desde;

        // Extraer cada tipo de dato con resiliencia Polly
        var facturas = await _resiliencePipeline.ExecuteAsync(
            async token => await client.GetFacturasDesdeAsync(desde, token), ct);
        count += facturas.Count;
        if (facturas.Count > 0)
        {
            var maxFecha = facturas.Max(f => f.FechaDocumento);
            if (maxFecha > maxDate) maxDate = maxFecha;
            await PersistStagingAsync(estacion.Id, "Factura", facturas, ct);
        }

        var detalles = await _resiliencePipeline.ExecuteAsync(
            async token => await client.GetDetallesFacturaAsync(desde, token), ct);
        count += detalles.Count;
        if (detalles.Count > 0)
        {
            var maxFecha = detalles.Max(d => d.FechaDespacho);
            if (maxFecha > maxDate) maxDate = maxFecha;
            await PersistStagingAsync(estacion.Id, "DetalleFactura", detalles, ct);
        }

        var cierres = await _resiliencePipeline.ExecuteAsync(
            async token => await client.GetCierresTurnoAsync(desde, token), ct);
        count += cierres.Count;
        if (cierres.Count > 0)
        {
            var maxFecha = cierres.Max(c => c.FechaFin);
            if (maxFecha > maxDate) maxDate = maxFecha;
            await PersistStagingAsync(estacion.Id, "CierreTurno", cierres, ct);
        }

        var depositos = await _resiliencePipeline.ExecuteAsync(
            async token => await client.GetDepositosTurnoAsync(desde, token), ct);
        count += depositos.Count;
        if (depositos.Count > 0)
        {
            var maxFecha = depositos.Max(d => d.FechaDeposito);
            if (maxFecha > maxDate) maxDate = maxFecha;
            await PersistStagingAsync(estacion.Id, "DepositoTurno", depositos, ct);
        }

        var anulaciones = await _resiliencePipeline.ExecuteAsync(
            async token => await client.GetAnulacionesAsync(desde, token), ct);
        count += anulaciones.Count;
        if (anulaciones.Count > 0)
        {
            var maxFecha = anulaciones.Max(a => a.FechaAnulacion);
            if (maxFecha > maxDate) maxDate = maxFecha;
            await PersistStagingAsync(estacion.Id, "Anulacion", anulaciones, ct);
        }

        var creditos = await _resiliencePipeline.ExecuteAsync(
            async token => await client.GetCreditosAsync(desde, token), ct);
        count += creditos.Count;
        if (creditos.Count > 0)
        {
            var maxFecha = creditos.Max(c => c.FechaCabecera);
            if (maxFecha > maxDate) maxDate = maxFecha;
            await PersistStagingAsync(estacion.Id, "Credito", creditos, ct);
        }

        var tarjetas = await _resiliencePipeline.ExecuteAsync(
            async token => await client.GetTarjetasTurnoAsync(desde, token), ct);
        count += tarjetas.Count;
        if (tarjetas.Count > 0)
            await PersistStagingAsync(estacion.Id, "TarjetaTurno", tarjetas, ct);

        // Actualizar watermark
        if (count > 0)
        {
            await _unitOfWork.Estaciones.UpsertWatermarkAsync(estacion.Id, maxDate, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return (count, maxDate);
    }

    private async Task PersistStagingAsync<T>(
        int estacionId, string tipoTransaccion, IReadOnlyList<T> items, CancellationToken ct)
    {
        foreach (var item in items)
        {
            var json = JsonSerializer.Serialize(item);
            var staging = TransaccionStaging.Create(
                estacionId, tipoTransaccion, json, DateTime.UtcNow);
            await _dbContext.TransaccionesStaging.AddAsync(staging, ct);
        }

        await _dbContext.SaveChangesAsync(ct);
    }
}
