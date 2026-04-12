using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Hubs;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Jobs;

/// <summary>
/// Job de Hangfire que ejecuta el ciclo de detección de anomalías:
/// 1. Obtener estaciones activas y reglas
/// 2. Procesar datos de staging (enviados por agentes de estación)
/// 3. Ejecución de los 4 detectores en paralelo
/// 4. Scoring de riesgo
/// 5. Persistencia de alertas
/// 6. Notificación por SignalR
/// 7. Registro de EjecucionJob con métricas
/// </summary>
public sealed class AnomalyDetectionJob
{
    private readonly IEnumerable<IAnomalyDetector> _detectors;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PetrolRiosDbContext _dbContext;
    private readonly IHubContext<AlertsHub> _hubContext;
    private readonly ILogger<AnomalyDetectionJob> _logger;

    public AnomalyDetectionJob(
        IEnumerable<IAnomalyDetector> detectors,
        IUnitOfWork unitOfWork,
        PetrolRiosDbContext dbContext,
        IHubContext<AlertsHub> hubContext,
        ILogger<AnomalyDetectionJob> logger)
    {
        _detectors = detectors;
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var ejecucion = EjecucionJob.Create();
        ejecucion.Estado = EstadoJob.EnProgreso;
        await _dbContext.EjecucionesJob.AddAsync(ejecucion, ct);
        await _dbContext.SaveChangesAsync(ct);

        try
        {
            _logger.LogInformation("Iniciando ciclo de detección de anomalías");

            // Obtener estaciones activas y reglas
            var estaciones = await _unitOfWork.Estaciones.GetActivasAsync(ct);
            var reglas = await _unitOfWork.ReglasDeteccion.GetActivasAsync(ct);
            var totalAlertas = 0;
            var estacionesProcesadas = 0;

            foreach (var estacion in estaciones)
            {
                // Construir contexto de detección para esta estación
                var watermark = await _unitOfWork.Estaciones.GetWatermarkAsync(estacion.Id, ct);
                var context = await BuildDetectionContextAsync(estacion, watermark, reglas, ct);

                // Ejecutar los 4 detectores en paralelo
                var detectionTasks = _detectors
                    .Select(d => d.DetectAsync(context, ct))
                    .ToArray();
                var results = await Task.WhenAll(detectionTasks);

                // Persistir alertas y notificar
                foreach (var anomalies in results)
                {
                    foreach (var anomaly in anomalies)
                    {
                        var alerta = Alerta.Create(
                            anomaly.TipoDetector,
                            anomaly.NivelRiesgo,
                            anomaly.Descripcion,
                            anomaly.Score,
                            anomaly.EstacionId,
                            anomaly.EmpleadoCodigo,
                            anomaly.TransaccionReferencia,
                            JsonSerializer.Serialize(anomaly.Metadata),
                            ejecucion.Id);

                        await _dbContext.Alertas.AddAsync(alerta, ct);
                        totalAlertas++;

                        // Notificar por SignalR
                        await NotifyAlertAsync(alerta, estacion.Id);
                    }
                }

                estacionesProcesadas++;
            }

            await _dbContext.SaveChangesAsync(ct);

            sw.Stop();
            ejecucion.Completar(
                totalAlertas,
                estacionesProcesadas,
                0,
                sw.Elapsed.TotalSeconds);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Ciclo completado en {Duracion:F2}s: {Alertas} alertas generadas, " +
                "{Procesadas} estaciones procesadas",
                sw.Elapsed.TotalSeconds, totalAlertas,
                estacionesProcesadas);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Error en el ciclo de detección de anomalías");
            ejecucion.Fallar(ex.Message);
            await _dbContext.SaveChangesAsync(ct);
            throw;
        }
    }

    private async Task<DetectionContext> BuildDetectionContextAsync(
        Estacion estacion,
        EstacionWatermark? watermark,
        IReadOnlyList<ReglaDeteccion> reglas,
        CancellationToken ct)
    {
        var desde = watermark?.UltimaExtraccion ?? DateTime.UtcNow.AddHours(-1);

        // Obtener datos de staging para esta estación
        var staging = await _dbContext.TransaccionesStaging
            .Where(s => s.EstacionId == estacion.Id && !s.Procesada)
            .ToListAsync(ct);

        var facturas = DeserializeStagingByType<Application.DTOs.Firebird.FacturaDto>(staging, "Factura");
        var detalles = DeserializeStagingByType<Application.DTOs.Firebird.DetalleFacturaDto>(staging, "DetalleFactura");
        var cierres = DeserializeStagingByType<Application.DTOs.Firebird.CierreTurnoDto>(staging, "CierreTurno");
        var depositos = DeserializeStagingByType<Application.DTOs.Firebird.DepositoTurnoDto>(staging, "DepositoTurno");
        var anulaciones = DeserializeStagingByType<Application.DTOs.Firebird.AnulacionDto>(staging, "Anulacion");
        var creditos = DeserializeStagingByType<Application.DTOs.Firebird.CreditoDto>(staging, "Credito");
        var tarjetas = DeserializeStagingByType<Application.DTOs.Firebird.TarjetaTurnoDto>(staging, "TarjetaTurno");

        // Historial de alertas por empleado (últimos 30 días)
        // Se materializa primero y se agrupa en memoria para compatibilidad con todos los providers
        var hace30Dias = DateTime.UtcNow.AddDays(-30);
        var alertasPreviasList = await _dbContext.Alertas
            .Where(a => a.EstacionId == estacion.Id
                     && a.FechaDeteccion >= hace30Dias
                     && a.EmpleadoCodigo != null)
            .Select(a => a.EmpleadoCodigo!)
            .ToListAsync(ct);
        var alertasPrevias = alertasPreviasList
            .GroupBy(codigo => codigo)
            .ToDictionary(g => g.Key, g => g.Count());

        // Marcar staging como procesada
        foreach (var s in staging)
            s.Procesada = true;

        return new DetectionContext
        {
            EstacionId = estacion.Id,
            EstacionNombre = estacion.Nombre,
            FromWatermark = desde,
            ToWatermark = DateTime.UtcNow,
            Facturas = facturas,
            Detalles = detalles,
            CierresTurno = cierres,
            DepositosTurno = depositos,
            Anulaciones = anulaciones,
            Creditos = creditos,
            TarjetasTurno = tarjetas,
            Reglas = reglas,
            AlertasPreviasPorEmpleado = alertasPrevias,
            HoraApertura = estacion.HoraApertura,
            HoraCierre = estacion.HoraCierre
        };
    }

    private static IReadOnlyList<T> DeserializeStagingByType<T>(
        IEnumerable<TransaccionStaging> staging, string tipoTransaccion)
    {
        return staging
            .Where(s => s.TipoTransaccion == tipoTransaccion)
            .Select(s =>
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(s.DataJson);
                }
                catch
                {
                    return default;
                }
            })
            .Where(item => item is not null)
            .Cast<T>()
            .ToList();
    }

    private async Task NotifyAlertAsync(Alerta alerta, int estacionId)
    {
        var payload = new
        {
            alerta.Id,
            TipoDetector = alerta.TipoDetector.ToString(),
            NivelRiesgo = alerta.NivelRiesgo.ToString(),
            alerta.Descripcion,
            alerta.Score,
            alerta.FechaDeteccion,
            EstacionId = estacionId
        };

        // Notificar a todos los grupos relevantes
        await Task.WhenAll(
            _hubContext.Clients.Group("auditores").SendAsync("NuevaAlerta", payload),
            _hubContext.Clients.Group("supervisores").SendAsync("NuevaAlerta", payload),
            _hubContext.Clients.Group("administradores").SendAsync("NuevaAlerta", payload),
            _hubContext.Clients.Group($"estacion-{estacionId}").SendAsync("NuevaAlerta", payload));
    }
}
