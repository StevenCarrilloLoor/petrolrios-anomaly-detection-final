using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Application.Programacion;
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
    private readonly IParametrosOperacion _parametros;
    private readonly Services.CuadreLiquidacionService _cuadre;
    private readonly ILogger<AnomalyDetectionJob> _logger;

    /// <summary>Parámetro de la regla del cuadre de liquidación (mejora #3). No es un detector de
    /// ventana: lo evalúa <see cref="Services.CuadreLiquidacionService"/> sobre el staging acumulado y
    /// de forma idempotente, por eso se gestiona aparte del gate de detectores.</summary>
    private const string ParametroCuadre = "FacturaSinLiquidacionHorasUmbral";

    /// <summary>Días de staging hacia atrás que mira el cuadre (acota el costo; los turnos son diarios).</summary>
    private const int DiasLookbackCuadre = 30;

    public AnomalyDetectionJob(
        IEnumerable<IAnomalyDetector> detectors,
        IUnitOfWork unitOfWork,
        PetrolRiosDbContext dbContext,
        IAlertaBroadcaster broadcaster,
        IEmailNotificacionService emailService,
        IParametrosOperacion parametros,
        Services.CuadreLiquidacionService cuadre,
        ILogger<AnomalyDetectionJob> logger)
    {
        _detectors = detectors;
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _broadcaster = broadcaster;
        _emailService = emailService;
        _parametros = parametros;
        _cuadre = cuadre;
        _logger = logger;
    }

    /// <summary>
    /// Zona horaria de las estaciones (Ecuador, UTC-5, sin horario de verano). Se usa para el modo
    /// Calendario de la programación: "el día 29 a las 00:00" significa medianoche hora local, y Cronos
    /// la traduce a UTC. Se construye a mano para ser idéntica en Windows y Linux (los IDs de TZ difieren).
    /// </summary>
    private static readonly TimeZoneInfo ZonaEstacion =
        TimeZoneInfo.CreateCustomTimeZone("PetrolRios-EC", TimeSpan.FromHours(-5), "Ecuador (UTC-5)", "Ecuador (UTC-5)");

    /// <summary>
    /// Calcula la próxima ejecución de una programación y la normaliza a UTC (Npgsql exige
    /// <c>DateTimeKind.Utc</c> para columnas <c>timestamptz</c>). Cronos devuelve instantes UTC; el
    /// modo Intervalo suma sobre <c>DateTime.UtcNow</c>. El guardia cubre cualquier <c>Unspecified</c>.
    /// </summary>
    private static DateTime CalcularProximaUtc(ProgramacionEjecucion prog, DateTime desdeUtc)
    {
        // CalcularProxima solo devuelve null en "cada ciclo" (que aquí nunca se programa); el ?? es
        // un guardia defensivo. Para Intervalo/Calendario siempre hay una próxima fecha.
        var prox = CalculadoraProgramacion.CalcularProxima(prog, desdeUtc, ZonaEstacion) ?? desdeUtc.AddDays(1);
        return prox.Kind switch
        {
            DateTimeKind.Utc => prox,
            DateTimeKind.Local => prox.ToUniversalTime(),
            _ => DateTime.SpecifyKind(prox, DateTimeKind.Utc)
        };
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
            // Sin tracking: el gate de programación alterna Activa en memoria para "apagar" las reglas
            // que no les toca este ciclo; al no estar rastreadas, esos cambios nunca tocan la BD.
            var reglas = await _dbContext.ReglasDeteccion
                .AsNoTracking()
                .ToListAsync(ct);
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

            // ── Gate de programación por regla ──────────────────────────────────────────────────
            // Cada regla declara su cadencia en ProgramacionJson (vacío = "cada ciclo", el default).
            // La Pasada A corre las reglas "cada ciclo" sobre el lote incremental no procesado. Las
            // reglas programadas (intervalo/calendario) NO corren en A: cuando les toca, corren en la
            // Pasada B sobre su VENTANA de datos (FechaOriginal en (UltimaEjecucion, ahora]), sin marcar
            // Procesada, y luego avanzamos su próxima ejecución. Como la ventana no se solapa entre
            // corridas, cada transacción la ve la regla una sola vez → sin alertas duplicadas.
            var ahora = DateTime.UtcNow;
            var activaOriginalBi = reglas.ToDictionary(r => r.Id, r => r.Activa);
            var biCadaCiclo = new HashSet<int>();
            var cuCadaCiclo = new HashSet<int>();
            var corridasProgramadas = new List<(ReglaDeteccion? BuiltIn, ReglaPersonalizada? Custom, ProgramacionEjecucion Prog)>();
            var anclajes = new List<(bool EsBuiltIn, int Id, DateTime Prox)>();

            foreach (var r in reglas)
            {
                if (r.ParametroNombre == ParametroCuadre) continue;  // el cuadre no es un detector de ventana (ver abajo)
                var prog = ProgramacionEjecucion.Leer(r.ProgramacionJson);
                if (prog.Modo == ModoProgramacion.CadaCiclo) { biCadaCiclo.Add(r.Id); continue; }
                if (!activaOriginalBi[r.Id]) continue;               // programada pero desactivada: ni corre ni agenda
                if (r.ProximaEjecucion is null)
                    anclajes.Add((true, r.Id, CalcularProximaUtc(prog, ahora)));   // primer anclaje: espera su turno
                else if (ahora >= r.ProximaEjecucion.Value)
                    corridasProgramadas.Add((r, null, prog));        // le toca: se evalúa por ventana en la Pasada B
            }

            // ── Cuadre de liquidación (mejora #3): se gestiona aparte del gate de detectores ──
            // No es un detector de ventana: lo evalúa CuadreLiquidacionService sobre el staging acumulado,
            // de forma idempotente. Solo programable (nunca "cada ciclo": es costoso y el cuadre es diario).
            var reglaCuadre = reglas.FirstOrDefault(r => r.ParametroNombre == ParametroCuadre);
            var progCuadre = reglaCuadre is null ? null : ProgramacionEjecucion.Leer(reglaCuadre.ProgramacionJson);
            var cuadreActivo = reglaCuadre is not null && activaOriginalBi[reglaCuadre.Id];
            var cuadreAnclar = cuadreActivo && reglaCuadre!.ProximaEjecucion is null;         // primer anclaje: espera su turno
            var cuadreToca = cuadreActivo && reglaCuadre!.ProximaEjecucion is not null
                          && ahora >= reglaCuadre.ProximaEjecucion.Value;
            foreach (var r in reglasPersonalizadas)
            {
                var prog = ProgramacionEjecucion.Leer(r.ProgramacionJson);
                if (prog.Modo == ModoProgramacion.CadaCiclo) { cuCadaCiclo.Add(r.Id); continue; }
                if (!r.Activa) continue;
                if (r.ProximaEjecucion is null)
                    anclajes.Add((false, r.Id, CalcularProximaUtc(prog, ahora)));
                else if (ahora >= r.ProximaEjecucion.Value)
                    corridasProgramadas.Add((null, r, prog));
            }

            // Reglas personalizadas del carril incremental (cada ciclo). El resto va por ventana.
            var reglasPersCadaCiclo = reglasPersonalizadas.Where(r => cuCadaCiclo.Contains(r.Id)).ToList();

            // Ajusta el flag Activa de las reglas del motor (en memoria, sin tracking) preservando el
            // "desactivada por el usuario": una regla solo se evalúa si estaba activa Y le toca esta pasada.
            void AplicarActivaBi(Func<ReglaDeteccion, bool> leToca)
            {
                foreach (var r in reglas)
                    r.Activa = activaOriginalBi[r.Id] && leToca(r);
            }

            // Nivel mínimo de alerta que dispara aviso por correo (configurable en Ajustes → Operación
            // del sistema). Por defecto Crítico; si baja a Alto, también avisará por correo las Altas.
            var nivelMinCorreo = Enum.TryParse<NivelRiesgo>(_parametros.Actual().NivelMinimoCorreo, true, out var nm)
                ? nm
                : NivelRiesgo.Critico;

            foreach (var estacion in estaciones)
            {
                var watermark = await _unitOfWork.Estaciones.GetWatermarkAsync(estacion.Id, ct);
                var operativasDeEstacion = new List<Alerta>();

                // Historial de alertas por empleado (30 días): se calcula una vez por estación y se
                // reutiliza en ambas pasadas para el scoring de reincidencia.
                var alertasPrevias = await CargarHistorialEmpleadoAsync(estacion.Id, ct);

                // ── Pasada A: ciclo incremental (reglas "cada ciclo") sobre el lote no procesado ──
                AplicarActivaBi(r => biCadaCiclo.Contains(r.Id));
                var lote = await _dbContext.TransaccionesStaging
                    .Where(s => s.EstacionId == estacion.Id && !s.Procesada)
                    .ToListAsync(ct);
                var contextoA = ConstruirContexto(
                    estacion, watermark, lote, reglas, reglasPersCadaCiclo, relacionesTabla, alertasPrevias);
                totalAlertas += await ProcesarContextoAsync(
                    contextoA, estacion, ejecucion, nivelMinCorreo, operativasDeEstacion, ct);
                foreach (var s in lote) s.Procesada = true;   // el lote queda consumido por el carril incremental

                // ── Pasada B: reglas programadas a las que les toca, cada una sobre su VENTANA ──
                foreach (var corrida in corridasProgramadas)
                {
                    var dias = CalculadoraProgramacion.DiasVentanaSugerida(corrida.Prog);
                    var ultima = corrida.BuiltIn?.UltimaEjecucion ?? corrida.Custom?.UltimaEjecucion;
                    var desdeVentana = ultima ?? ahora.AddDays(-dias);
                    var ventana = await _dbContext.TransaccionesStaging
                        .AsNoTracking()
                        .Where(s => s.EstacionId == estacion.Id
                                 && s.FechaOriginal > desdeVentana && s.FechaOriginal <= ahora)
                        .ToListAsync(ct);
                    if (ventana.Count == 0) continue;

                    List<ReglaPersonalizada> persB;
                    if (corrida.BuiltIn is not null)
                    {
                        var idBi = corrida.BuiltIn.Id;
                        AplicarActivaBi(r => r.Id == idBi);   // solo esta regla del motor
                        persB = [];
                    }
                    else
                    {
                        AplicarActivaBi(_ => false);                        // ninguna regla del motor
                        persB = [corrida.Custom!];
                    }
                    var contextoB = ConstruirContexto(
                        estacion, watermark, ventana, reglas, persB, relacionesTabla, alertasPrevias);
                    totalAlertas += await ProcesarContextoAsync(
                        contextoB, estacion, ejecucion, nivelMinCorreo, operativasDeEstacion, ct);
                    // La ventana NO se marca Procesada (es re-lectura por fecha; el avance de
                    // UltimaEjecucion evita el solape entre corridas → sin alertas duplicadas).
                }

                // ── Cuadre de liquidación (#3): turnos cerrados sin liquidar (idempotente, sobre el
                // staging acumulado; no usa la ventana del Pass B porque la liquidación llega tras el cierre).
                if (cuadreToca)
                    totalAlertas += await _cuadre.EvaluarEstacionAsync(
                        estacion, reglaCuadre!.ValorUmbral, reglaCuadre.Ambito, DiasLookbackCuadre, ct);

                // Digest de problemas operativos al contacto de la estación (carril Operativa)
                await NotificarOperativasEstacionAsync(estacion, operativasDeEstacion, ct);

                estacionesProcesadas++;
            }

            // Avanzar la programación (anclar las nuevas + mover la próxima de las que corrieron), una
            // sola vez por ciclo: la regla corre para todas las estaciones, su próxima fecha es global.
            await AvanzarProgramacionAsync(anclajes, corridasProgramadas, ahora, ct);

            // Avanzar la agenda del cuadre (#3): primer anclaje (espera su turno) o avance tras correr.
            // Se carga con tracking aparte porque 'reglas' está sin tracking (ver carga arriba).
            if (reglaCuadre is not null && (cuadreAnclar || cuadreToca))
            {
                var eCuadre = await _dbContext.ReglasDeteccion.FirstAsync(r => r.Id == reglaCuadre.Id, ct);
                if (cuadreToca) eCuadre.UltimaEjecucion = ahora;
                eCuadre.ProximaEjecucion = CalcularProximaUtc(progCuadre!, ahora);
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

    /// <summary>
    /// Historial de alertas por empleado (últimos 30 días) para el scoring de reincidencia. Se
    /// materializa y agrupa en memoria para compatibilidad con todos los providers de EF.
    /// </summary>
    private async Task<Dictionary<string, int>> CargarHistorialEmpleadoAsync(int estacionId, CancellationToken ct)
    {
        var hace30Dias = DateTime.UtcNow.AddDays(-30);
        var lista = await _dbContext.Alertas
            .Where(a => a.EstacionId == estacionId
                     && a.FechaDeteccion >= hace30Dias
                     && a.EmpleadoCodigo != null)
            .Select(a => a.EmpleadoCodigo!)
            .ToListAsync(ct);
        return lista.GroupBy(codigo => codigo).ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Construye el contexto de detección a partir de un conjunto de registros de staging ya cargados
    /// (no consulta la BD ni marca <c>Procesada</c>: eso lo decide quien llama, según la pasada). Las
    /// reglas y su flag <c>Activa</c> vienen preparados por el gate de programación.
    /// </summary>
    private static DetectionContext ConstruirContexto(
        Estacion estacion,
        EstacionWatermark? watermark,
        IReadOnlyList<TransaccionStaging> stagingFuente,
        IReadOnlyList<ReglaDeteccion> reglas,
        IReadOnlyList<ReglaPersonalizada> reglasPersonalizadas,
        IReadOnlyList<RelacionTabla> relaciones,
        Dictionary<string, int> alertasPrevias)
    {
        var desde = watermark?.UltimaExtraccion ?? DateTime.UtcNow.AddHours(-1);

        // Deduplicar lotes reenviados por el agente (store-and-forward puede reenviar un lote ya
        // recibido si el primer envío falló a mitad de camino).
        var staging = stagingFuente
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

        // Fuentes genéricas: staging de tipos NO conocidos = tablas configurables del agente.
        // Se exponen como diccionarios para que las reglas personalizadas operen sobre ellas.
        var tiposConocidos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Factura", "DetalleFactura", "CierreTurno", "DepositoTurno",
            "Anulacion", "Credito", "TarjetaTurno", "Liquidacion"
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

    /// <summary>
    /// Corre los detectores sobre un contexto, persiste las alertas resultantes, las notifica por
    /// SignalR y dispara los correos según nivel/opt-in. Devuelve cuántas alertas generó.
    /// </summary>
    private async Task<int> ProcesarContextoAsync(
        DetectionContext context,
        Estacion estacion,
        EjecucionJob ejecucion,
        NivelRiesgo nivelMinCorreo,
        List<Alerta> operativasDeEstacion,
        CancellationToken ct)
    {
        var detectionTasks = _detectors
            .Select(d => d.DetectAsync(context, ct))
            .ToArray();
        var results = await Task.WhenAll(detectionTasks);

        var generadas = 0;
        foreach (var anomalies in results)
        {
            foreach (var anomaly in anomalies)
            {
                var metadataJson = JsonSerializer.Serialize(anomaly.Metadata);

                // Caso ACUMULABLE (p. ej. despachos rápidos del mismo RUC/placa): si ya hay una alerta
                // ABIERTA del mismo caso (misma referencia), se acumula y se escala en vez de crear otra.
                // Así no se inunda la bandeja y la severidad crece con la reincidencia (re-emerge arriba).
                if (anomaly.EsAcumulable && !string.IsNullOrWhiteSpace(anomaly.TransaccionReferencia))
                {
                    var abierta = await _dbContext.Alertas.FirstOrDefaultAsync(a =>
                        a.EstacionId == anomaly.EstacionId
                        && a.TransaccionReferencia == anomaly.TransaccionReferencia
                        && (a.Estado == EstadoAlerta.Nueva || a.Estado == EstadoAlerta.EnRevision), ct);

                    if (abierta is not null)
                    {
                        var aporte = Math.Max(anomaly.EventosEnLote, 1);
                        var (escScore, escNivel) = Alerta.EscalarPorConteo(abierta.EventosAcumulados + aporte);
                        abierta.Acumular(aporte, escScore, escNivel, anomaly.Descripcion, metadataJson, DateTime.UtcNow);
                        generadas++;
                        if (abierta.Ambito is AmbitoAlerta.Operativa or AmbitoAlerta.Ambos)
                            operativasDeEstacion.Add(abierta);
                        await NotifyAlertAsync(abierta, estacion.Id);
                        if ((int)abierta.NivelRiesgo >= (int)nivelMinCorreo)
                            await NotificarNivelPorCorreoAsync(abierta, estacion, ct);
                        continue;
                    }
                }

                var alerta = Alerta.Create(
                    anomaly.TipoDetector,
                    anomaly.NivelRiesgo,
                    anomaly.Descripcion,
                    anomaly.Score,
                    anomaly.EstacionId,
                    anomaly.EmpleadoCodigo,
                    anomaly.TransaccionReferencia,
                    metadataJson,
                    ejecucion.Id,
                    anomaly.Ambito,
                    eventosAcumulados: anomaly.EsAcumulable ? Math.Max(anomaly.EventosEnLote, 1) : 1);

                await _dbContext.Alertas.AddAsync(alerta, ct);
                generadas++;
                if (alerta.Ambito is AmbitoAlerta.Operativa or AmbitoAlerta.Ambos)
                    operativasDeEstacion.Add(alerta);

                // Notificar por SignalR
                await NotifyAlertAsync(alerta, estacion.Id);

                // Correo: si la alerta alcanza el nivel mínimo configurado (por defecto Crítico);
                // y si la regla que la generó pidió aviso por correo (opt-in, motor o personalizada).
                if ((int)alerta.NivelRiesgo >= (int)nivelMinCorreo)
                    await NotificarNivelPorCorreoAsync(alerta, estacion, ct);
                else if (anomaly.NotificarCorreo)
                    await NotificarReglaPorCorreoAsync(alerta, estacion, ct);
            }
        }
        return generadas;
    }

    /// <summary>
    /// Avanza la programación de las reglas tras el ciclo: ancla la próxima ejecución de las reglas
    /// recién programadas (sin <c>ProximaEjecucion</c>) y, para las que corrieron, fija
    /// <c>UltimaEjecucion = ahora</c> y calcula la siguiente. Carga las entidades con tracking (una
    /// vez cada una) para que el <c>SaveChanges</c> del cierre del ciclo persista los cambios.
    /// </summary>
    private async Task AvanzarProgramacionAsync(
        List<(bool EsBuiltIn, int Id, DateTime Prox)> anclajes,
        List<(ReglaDeteccion? BuiltIn, ReglaPersonalizada? Custom, ProgramacionEjecucion Prog)> corridas,
        DateTime ahora,
        CancellationToken ct)
    {
        if (anclajes.Count == 0 && corridas.Count == 0) return;

        // Cada regla aparece a lo sumo en UNA de las listas y una sola vez (es CadaCiclo, o se ancla, o
        // le toca), así que se carga con tracking una única vez y el SaveChanges del ciclo la persiste.
        foreach (var (esBuiltIn, id, prox) in anclajes)
        {
            if (esBuiltIn)
            {
                var e = await _dbContext.ReglasDeteccion.FirstAsync(r => r.Id == id, ct);
                e.ProximaEjecucion = prox;
            }
            else
            {
                var e = await _dbContext.ReglasPersonalizadas.FirstAsync(r => r.Id == id, ct);
                e.ProximaEjecucion = prox;
            }
        }

        foreach (var corrida in corridas)
        {
            var prox = CalcularProximaUtc(corrida.Prog, ahora);
            if (corrida.BuiltIn is not null)
            {
                var id = corrida.BuiltIn.Id;
                var e = await _dbContext.ReglasDeteccion.FirstAsync(r => r.Id == id, ct);
                e.UltimaEjecucion = ahora;
                e.ProximaEjecucion = prox;
            }
            else
            {
                var id = corrida.Custom!.Id;
                var e = await _dbContext.ReglasPersonalizadas.FirstAsync(r => r.Id == id, ct);
                e.UltimaEjecucion = ahora;
                e.ProximaEjecucion = prox;
            }
        }
    }

    // Cache de destinatarios de correo por ciclo (supervisores y administradores activos).
    private IReadOnlyList<string>? _destinatariosCorreo;

    private async Task NotificarNivelPorCorreoAsync(Alerta alerta, Estacion estacion, CancellationToken ct)
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

        var asunto = $"[PetrolRíos] Alerta {alerta.NivelRiesgo} en {estacion.Nombre} (score {alerta.Score})";
        var cuerpo =
            $"<h2 style='color:#b91c1c'>Alerta de nivel {alerta.NivelRiesgo} detectada</h2>" +
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
    /// Correo por una regla marcada con "avisar por correo" (opt-in), aunque la alerta no sea crítica.
    /// Va a supervisores y administradores (mismos destinatarios que las críticas).
    /// </summary>
    private async Task NotificarReglaPorCorreoAsync(Alerta alerta, Estacion estacion, CancellationToken ct)
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

        var asunto = $"[PetrolRíos] Alerta de regla marcada en {estacion.Nombre} ({alerta.NivelRiesgo}, score {alerta.Score})";
        var cuerpo =
            $"<h2 style='color:#b45309'>Alerta de una regla con aviso por correo</h2>" +
            $"<p>La regla que generó esta alerta está configurada para avisar por correo cuando se dispara.</p>" +
            $"<p><b>Estación:</b> {estacion.Nombre} ({estacion.Codigo})</p>" +
            $"<p><b>Detector:</b> {alerta.TipoDetector}</p>" +
            $"<p><b>Nivel de riesgo:</b> {alerta.NivelRiesgo} — score {alerta.Score}/100</p>" +
            $"<p><b>Descripción:</b> {alerta.Descripcion}</p>" +
            $"<p><b>Fecha:</b> {alerta.FechaDeteccion:yyyy-MM-dd HH:mm} UTC</p>" +
            $"<hr><p style='color:#64748b;font-size:12px'>Revise el detalle en el panel de PetrolRíos. " +
            $"Aviso automático; no responda a este correo.</p>";

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
        // Carriles: Auditoría/Ambos → bandeja del central (evento "NuevaAlerta"); Operativa/Ambos →
        // problema de estación (evento "ProblemaEstacion", incluye el grupo de la estación). Una alerta
        // de carril "Ambos" emite los DOS eventos (cada uno con su NotificationId) y aparece en ambos
        // lados. Una Operativa NUNCA emite "NuevaAlerta", para no confundir a los auditores.
        var esOperativa = alerta.Ambito is AmbitoAlerta.Operativa or AmbitoAlerta.Ambos;
        var esAuditoria = alerta.Ambito is AmbitoAlerta.Auditoria or AmbitoAlerta.Ambos;

        if (esAuditoria)
            await PublicarPushAsync(alerta, estacionId, "NuevaAlerta",
                "auditores", "supervisores", "administradores");

        if (esOperativa)
            await PublicarPushAsync(alerta, estacionId, "ProblemaEstacion",
                "auditores", "supervisores", "administradores", $"estacion-{estacionId}");
    }

    private Task PublicarPushAsync(Alerta alerta, int estacionId, string evento, params string[] grupos)
    {
        // La entidad todavía puede no tener Id (SaveChanges ocurre al finalizar el lote); el
        // NotificationId permite al cliente deduplicar sin confundir dos notificaciones (incluidos
        // los dos eventos de una alerta "Ambos").
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

        return _broadcaster.PublicarAsync(new AlertaPush(evento, grupos, payload));
    }
}
