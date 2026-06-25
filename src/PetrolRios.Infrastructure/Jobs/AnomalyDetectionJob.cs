using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Application.RealTime;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;
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
    private readonly IAlertaBroadcaster _broadcaster;
    private readonly IEmailNotificacionService _emailService;
    private readonly ILogger<AnomalyDetectionJob> _logger;

    public AnomalyDetectionJob(
        IEnumerable<IAnomalyDetector> detectors,
        IUnitOfWork unitOfWork,
        PetrolRiosDbContext dbContext,
        IAlertaBroadcaster broadcaster,
        IEmailNotificacionService emailService,
        ILogger<AnomalyDetectionJob> logger)
    {
        _detectors = detectors;
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _broadcaster = broadcaster;
        _emailService = emailService;
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

            // Obtener estaciones activas y TODAS las reglas (incluidas las inactivas):
            // los detectores necesitan ver una regla desactivada para NO ejecutarla
            // (si no la ven, aplicarían su umbral por defecto).
            var estaciones = await _unitOfWork.Estaciones.GetActivasAsync(ct);
            var reglas = await _unitOfWork.ReglasDeteccion.GetAllAsync(ct);
            var reglasPersonalizadas = await _dbContext.ReglasPersonalizadas
                .AsNoTracking()
                .ToListAsync(ct);
            // Relaciones entre tablas (para enriquecer las alertas de reglas personalizadas con campos
            // de tablas relacionadas). Solo las activas; se cargan una vez por ciclo.
            var relacionesTabla = await _dbContext.RelacionesTabla
                .AsNoTracking()
                .Where(r => r.Activa)
                .ToListAsync(ct);
            var totalAlertas = 0;
            var estacionesProcesadas = 0;

            foreach (var estacion in estaciones)
            {
                // Construir contexto de detección para esta estación
                var watermark = await _unitOfWork.Estaciones.GetWatermarkAsync(estacion.Id, ct);
                var context = await BuildDetectionContextAsync(
                    estacion, watermark, reglas, reglasPersonalizadas, relacionesTabla, ct);

                // Ejecutar los 4 detectores en paralelo
                var detectionTasks = _detectors
                    .Select(d => d.DetectAsync(context, ct))
                    .ToArray();
                var results = await Task.WhenAll(detectionTasks);

                // Persistir alertas y notificar
                var operativasDeEstacion = new List<Alerta>();
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
                            ejecucion.Id,
                            anomaly.Ambito);

                        await _dbContext.Alertas.AddAsync(alerta, ct);
                        totalAlertas++;
                        if (alerta.Ambito == AmbitoAlerta.Operativa)
                            operativasDeEstacion.Add(alerta);

                        // Notificar por SignalR
                        await NotifyAlertAsync(alerta, estacion.Id);

                        // Notificación por correo para alertas críticas (opcional, como en la tesis)
                        if (alerta.NivelRiesgo == NivelRiesgo.Critico)
                            await NotificarCriticaPorCorreoAsync(alerta, estacion, ct);
                    }
                }

                // Digest de problemas operativos al contacto de la estación (carril Operativa)
                await NotificarOperativasEstacionAsync(estacion, operativasDeEstacion, ct);

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
        IReadOnlyList<ReglaPersonalizada> reglasPersonalizadas,
        IReadOnlyList<RelacionTabla> relaciones,
        CancellationToken ct)
    {
        var desde = watermark?.UltimaExtraccion ?? DateTime.UtcNow.AddHours(-1);

        // Obtener datos de staging para esta estación
        var stagingTodos = await _dbContext.TransaccionesStaging
            .Where(s => s.EstacionId == estacion.Id && !s.Procesada)
            .ToListAsync(ct);

        // Deduplicar lotes reenviados por el agente (store-and-forward puede
        // reenviar un lote ya recibido si el primer envío falló a mitad de camino)
        var staging = stagingTodos
            .GroupBy(s => new { s.TipoTransaccion, s.DataJson })
            .Select(g => g.First())
            .ToList();

        var facturas = StagingJson.DeserializarPorTipo<Application.DTOs.Firebird.FacturaDto>(staging, "Factura");
        var detalles = StagingJson.DeserializarPorTipo<Application.DTOs.Firebird.DetalleFacturaDto>(staging, "DetalleFactura");
        var cierres = StagingJson.DeserializarPorTipo<Application.DTOs.Firebird.CierreTurnoDto>(staging, "CierreTurno");
        var depositos = StagingJson.DeserializarPorTipo<Application.DTOs.Firebird.DepositoTurnoDto>(staging, "DepositoTurno");
        var anulaciones = StagingJson.DeserializarPorTipo<Application.DTOs.Firebird.AnulacionDto>(staging, "Anulacion");
        var creditos = StagingJson.DeserializarPorTipo<Application.DTOs.Firebird.CreditoDto>(staging, "Credito");
        var tarjetas = StagingJson.DeserializarPorTipo<Application.DTOs.Firebird.TarjetaTurnoDto>(staging, "TarjetaTurno");

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

        // Fuentes genéricas: staging de tipos NO conocidos = tablas configurables del agente.
        // Se exponen como diccionarios para que las reglas personalizadas operen sobre ellas.
        var tiposConocidos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Factura", "DetalleFactura", "CierreTurno", "DepositoTurno",
            "Anulacion", "Credito", "TarjetaTurno"
        };
        var fuentesGenericas = staging
            .Where(s => !tiposConocidos.Contains(s.TipoTransaccion))
            .GroupBy(s => s.TipoTransaccion)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<IDictionary<string, object>>)g
                    .Select(s => StagingJson.DeserializarDiccionario(s.DataJson))
                    .Where(d => d is not null)
                    .Cast<IDictionary<string, object>>()
                    .ToList());

        // Marcar TODO el staging (incluidos duplicados) como procesado
        foreach (var s in stagingTodos)
            s.Procesada = true;

        return new DetectionContext
        {
            FuentesGenericas = fuentesGenericas,
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
            ReglasPersonalizadas = reglasPersonalizadas,
            Relaciones = relaciones,
            AlertasPreviasPorEmpleado = alertasPrevias,
            HoraApertura = estacion.HoraApertura,
            HoraCierre = estacion.HoraCierre
        };
    }

    // Cache de destinatarios de correo por ciclo (supervisores y administradores activos).
    private IReadOnlyList<string>? _destinatariosCorreo;

    private async Task NotificarCriticaPorCorreoAsync(Alerta alerta, Estacion estacion, CancellationToken ct)
    {
        if (!_emailService.Habilitado) return;

        _destinatariosCorreo ??= await _dbContext.Usuarios
            .AsNoTracking()
            .Where(u => u.Activo
                && (u.Rol.Nombre == "Supervisor" || u.Rol.Nombre == "Administrador")
                && u.Email != "")
            .Select(u => u.Email)
            .ToListAsync(ct);

        if (_destinatariosCorreo.Count == 0) return;

        var asunto = $"[PetrolRíos] Alerta CRÍTICA en {estacion.Nombre} (score {alerta.Score})";
        var cuerpo =
            $"<h2 style='color:#b91c1c'>Alerta crítica detectada</h2>" +
            $"<p><b>Estación:</b> {estacion.Nombre} ({estacion.Codigo})</p>" +
            $"<p><b>Detector:</b> {alerta.TipoDetector}</p>" +
            $"<p><b>Nivel de riesgo:</b> {alerta.NivelRiesgo} — score {alerta.Score}/100</p>" +
            $"<p><b>Descripción:</b> {alerta.Descripcion}</p>" +
            $"<p><b>Fecha:</b> {alerta.FechaDeteccion:yyyy-MM-dd HH:mm} UTC</p>" +
            $"<hr><p style='color:#64748b;font-size:12px'>Revise el detalle en el panel de PetrolRíos. " +
            $"Este es un aviso automático; no responda a este correo.</p>";

        await _emailService.EnviarAsync(asunto, cuerpo, _destinatariosCorreo, ct);
    }

    /// <summary>
    /// Envía un resumen de los problemas operativos de la estación a su contacto
    /// (Estacion.CorreoContacto) y a los usuarios adscritos a esa estación. Un solo correo
    /// por ciclo para no saturar; solo si hay problemas operativos y destinatarios.
    /// </summary>
    private async Task NotificarOperativasEstacionAsync(
        Estacion estacion, IReadOnlyList<Alerta> operativas, CancellationToken ct)
    {
        if (!_emailService.Habilitado || operativas.Count == 0) return;

        var destinatarios = new List<string>();
        if (!string.IsNullOrWhiteSpace(estacion.CorreoContacto))
            destinatarios.Add(estacion.CorreoContacto.Trim());

        var usuariosEstacion = await _dbContext.Usuarios
            .AsNoTracking()
            .Where(u => u.Activo && u.EstacionId == estacion.Id && u.Email != "")
            .Select(u => u.Email)
            .ToListAsync(ct);
        destinatarios.AddRange(usuariosEstacion);

        destinatarios = destinatarios
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (destinatarios.Count == 0) return;

        var filas = string.Join("", operativas.Select(a =>
            $"<li style='margin-bottom:6px'>{a.Descripcion}" +
            (string.IsNullOrWhiteSpace(a.EmpleadoCodigo) ? "" : $" <span style='color:#64748b'>(empleado {a.EmpleadoCodigo})</span>") +
            "</li>"));

        var asunto = $"[PetrolRíos] {operativas.Count} problema(s) operativo(s) en {estacion.Nombre}";
        var cuerpo =
            $"<h2 style='color:#b45309'>Problemas operativos detectados</h2>" +
            $"<p><b>Estación:</b> {estacion.Nombre} ({estacion.Codigo})</p>" +
            $"<p>Se detectaron los siguientes problemas operativos que conviene revisar y corregir:</p>" +
            $"<ul>{filas}</ul>" +
            $"<hr><p style='color:#64748b;font-size:12px'>Aviso automático del sistema de detección de PetrolRíos. " +
            $"Revise el detalle en la pestaña \"Problemas de estación\". No responda a este correo.</p>";

        await _emailService.EnviarAsync(asunto, cuerpo, destinatarios, ct);
    }

    private async Task NotifyAlertAsync(Alerta alerta, int estacionId)
    {
        // El push se difunde por pg_notify a TODAS las instancias del central (cada una lo entrega
        // a sus propios clientes SignalR). Así el tiempo real funciona con varias instancias
        // compartiendo una sola base, sin Redis.
        //
        // La entidad todavía puede no tener Id (SaveChanges ocurre al finalizar el lote); el
        // NotificationId permite al cliente deduplicar sin confundir dos alertas con Id = 0.
        var payload = new AlertaNotificacionPayload(
            NotificationId: Guid.NewGuid().ToString("N"),
            Id: alerta.Id,
            TipoDetector: alerta.TipoDetector.ToString(),
            NivelRiesgo: alerta.NivelRiesgo.ToString(),
            Ambito: alerta.Ambito.ToString(),
            Descripcion: alerta.Descripcion,
            Score: alerta.Score,
            FechaDeteccion: alerta.FechaDeteccion,
            EstacionId: estacionId);

        // Operativa = problema de estación (pestaña "Problemas de estación" + Monitor de estación);
        // NUNCA "NuevaAlerta", para no confundir a los auditores. Auditoría = bandeja del central.
        var (evento, grupos) = alerta.Ambito == AmbitoAlerta.Operativa
            ? ("ProblemaEstacion", new[] { "auditores", "supervisores", "administradores", $"estacion-{estacionId}" })
            : ("NuevaAlerta", new[] { "auditores", "supervisores", "administradores" });

        await _broadcaster.PublicarAsync(new AlertaPush(evento, grupos, payload));
    }
}
