using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Application.Interfaces;
using PetrolRios.Application.Programacion;
using PetrolRios.Application.RealTime;
using PetrolRios.Application.ReglasPersonalizadas;
using PetrolRios.Detectors;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Jobs;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Api.Tests;

/// <summary>
/// Test end-to-end del ciclo completo de detección de anomalías:
/// staging (push) -> detectores -> scoring -> persistencia -> notificación por SignalR.
/// Usa datos sintéticos diseñados para disparar reglas específicas.
/// </summary>
public sealed class AnomalyDetectionJobE2ETests : IDisposable
{
    private readonly PetrolRiosDbContext _dbContext;
    private readonly Mock<IAlertaBroadcaster> _broadcasterMock;
    private readonly List<AlertaPush> _pushes = new();
    private readonly AnomalyDetectionJob _job;

    public AnomalyDetectionJobE2ETests()
    {
        var options = new DbContextOptionsBuilder<PetrolRiosDbContext>()
            .UseInMemoryDatabase(databaseName: $"E2E_{Guid.NewGuid()}")
            .Options;
        _dbContext = new PetrolRiosDbContext(options);

        // Seed data mínima
        SeedTestData();

        // El job delega el push en IAlertaBroadcaster (fan-out por pg_notify). Capturamos los
        // push para verificar evento, grupos y payload sin necesidad de SignalR real.
        _broadcasterMock = new Mock<IAlertaBroadcaster>();
        _broadcasterMock
            .Setup(b => b.PublicarAsync(It.IsAny<AlertaPush>(), It.IsAny<CancellationToken>()))
            .Callback<AlertaPush, CancellationToken>((push, _) => _pushes.Add(push))
            .Returns(Task.CompletedTask);

        // Construir detectores reales via DI
        var services = new ServiceCollection();
        services.AddDetectors();
        services.AddLogging();

        // Mock unit of work con los datos de prueba
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var estacionRepoMock = new Mock<IEstacionRepository>();
        var reglaRepoMock = new Mock<IReglaDeteccionRepository>();
        var alertaRepoMock = new Mock<IAlertaRepository>();
        var usuarioRepoMock = new Mock<IUsuarioRepository>();

        var estaciones = _dbContext.Estaciones.ToList();
        estacionRepoMock.Setup(r => r.GetActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(estaciones);

        foreach (var est in estaciones)
        {
            estacionRepoMock.Setup(r => r.GetWatermarkAsync(est.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EstacionWatermark.Create(est.Id, DateTime.UtcNow.AddHours(-1)));
        }

        var reglas = _dbContext.ReglasDeteccion.ToList();
        reglaRepoMock.Setup(r => r.GetActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(reglas);
        // El job ahora carga TODAS las reglas (incluidas inactivas) para respetar el flag Activa
        reglaRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(reglas);

        unitOfWorkMock.Setup(u => u.Estaciones).Returns(estacionRepoMock.Object);
        unitOfWorkMock.Setup(u => u.ReglasDeteccion).Returns(reglaRepoMock.Object);
        unitOfWorkMock.Setup(u => u.Alertas).Returns(alertaRepoMock.Object);
        unitOfWorkMock.Setup(u => u.Usuarios).Returns(usuarioRepoMock.Object);

        var sp = services.BuildServiceProvider();
        var detectors = sp.GetServices<IAnomalyDetector>();

        var emailMock = new Mock<IEmailNotificacionService>();
        emailMock.SetupGet(e => e.Habilitado).Returns(false);

        var parametrosMock = new Mock<IParametrosOperacion>();
        parametrosMock.Setup(p => p.Actual())
            .Returns(new PetrolRios.Application.DTOs.Configuracion.OperacionConfig("Critico", "*/5 * * * *"));

        var cuadre = new PetrolRios.Infrastructure.Services.CuadreLiquidacionService(
            _dbContext,
            _broadcasterMock.Object,
            sp.GetRequiredService<ILogger<PetrolRios.Infrastructure.Services.CuadreLiquidacionService>>());

        _job = new AnomalyDetectionJob(
            detectors,
            unitOfWorkMock.Object,
            _dbContext,
            _broadcasterMock.Object,
            emailMock.Object,
            parametrosMock.Object,
            cuadre,
            sp.GetRequiredService<ILogger<AnomalyDetectionJob>>());
    }

    private void SeedTestData()
    {
        // Estación de prueba
        var estacion = Estacion.Create("Estacion Test E2E", "EST-E2E", "Dirección test", "Centro");
        _dbContext.Estaciones.Add(estacion);
        _dbContext.SaveChanges();

        // Reglas de detección (las 12 estándar)
        var reglas = new[]
        {
            ReglaDeteccion.Create(TipoDetector.CashFraud, "Diferencia efectivo vs sistema",
                "Genera alerta si la diferencia excede el umbral", "DiferenciaEfectivoUmbral", 50.0),
            ReglaDeteccion.Create(TipoDetector.CashFraud, "Patron de faltantes recurrentes",
                "Genera alerta por gineteo", "FaltantesRecurrentesMaximo", 3.0),
            ReglaDeteccion.Create(TipoDetector.CashFraud, "Periodo de evaluacion de faltantes",
                "Dias hacia atras para evaluar faltantes", "FaltantesRecurrentesDias", 30.0),
            ReglaDeteccion.Create(TipoDetector.InvoiceAnomaly, "Tasa de anulaciones excesivas",
                "Genera alerta si anulaciones exceden umbral", "AnulacionesPorcentajeUmbral", 5.0),
            ReglaDeteccion.Create(TipoDetector.InvoiceAnomaly, "Precio fuera de lista",
                "Genera alerta si precio excede autorizado", "PrecioFueraListaHabilitado", 1.0),
            ReglaDeteccion.Create(TipoDetector.InvoiceAnomaly, "Campos obligatorios vacios",
                "Genera alerta si faltan campos obligatorios", "CamposObligatoriosHabilitado", 1.0),
            ReglaDeteccion.Create(TipoDetector.PaymentFraud, "Reversion tarjeta tardia",
                "Genera alerta por reversion tardia", "ReversionTarjetaMinutosUmbral", 30.0),
            ReglaDeteccion.Create(TipoDetector.PaymentFraud, "Credito sin autorizacion",
                "Genera alerta por credito sin comprobante", "CreditoSinAutorizacionHabilitado", 1.0),
            ReglaDeteccion.Create(TipoDetector.PaymentFraud, "Transacciones duplicadas",
                "Genera alerta por duplicados", "DuplicadaMinutosUmbral", 5.0),
            ReglaDeteccion.Create(TipoDetector.ComplianceViolation, "Venta excesiva a placa generica",
                "Genera alerta por placa ZZZ999949", "PlacaGenericaGalonesMaximo", 5.0),
            ReglaDeteccion.Create(TipoDetector.ComplianceViolation, "Vehiculo con multiples combustibles",
                "Genera alerta por multiples combustibles", "MultipleCombustibleHabilitado", 1.0),
            ReglaDeteccion.Create(TipoDetector.ComplianceViolation, "Operacion fuera de horario",
                "Genera alerta por operacion fuera de horario", "FueraHorarioHabilitado", 1.0)
        };
        _dbContext.ReglasDeteccion.AddRange(reglas);
        _dbContext.SaveChanges();

        // Dos reglas sobre una fuente genérica: una de auditoría y otra operativa.
        // Esto prueba el recorrido completo de una tabla agregada dinámicamente.
        var condicionesTanque = JsonSerializer.Serialize(
            new[] { new CondicionRegla("DIFERENCIA", ">", "500") });
        var reglaAuditoria = ReglaPersonalizada.Create(
            "Tanque con diferencia para auditoría",
            "Prueba E2E de fuente genérica",
            "TanquesE2E",
            condicionesTanque,
            null,
            70);
        var reglaOperativa = ReglaPersonalizada.Create(
            "Tanque con diferencia operativa",
            "Prueba E2E de fuente genérica",
            "TanquesE2E",
            condicionesTanque,
            null,
            60);
        reglaOperativa.Ambito = "Operativa";
        _dbContext.ReglasPersonalizadas.AddRange(reglaAuditoria, reglaOperativa);
        _dbContext.SaveChanges();

        // --- Datos de staging sintéticos diseñados para disparar alertas ---

        // 1. CashFraud: CierreTurno con Faltante=$100 (umbral es $50)
        var cierreTurno = new CierreTurnoDto
        {
            NumeroTurno = 100,
            CodigoVendedor = "V001",
            FechaInicio = DateTime.UtcNow.AddHours(-8),
            FechaFin = DateTime.UtcNow,
            SaldoInicial = 100,
            Ingresos = 1000,
            Egresos = 0,
            SaldoFinal = 1100,
            Faltante = 100,  // > umbral 50 -> ALERTA
            Sobrante = 0,
            Creditos = 0
        };
        _dbContext.TransaccionesStaging.Add(
            TransaccionStaging.Create(estacion.Id, "CierreTurno",
                JsonSerializer.Serialize(cierreTurno), DateTime.UtcNow.AddMinutes(-5)));

        // 2. ComplianceViolation: Factura con placa genérica ZZZ999949
        var facturaGenerica = new FacturaDto
        {
            SecuenciaDocumento = 1,
            TipoDocumento = "FV",
            NumeroDocumento = "001-001-0000001",
            FechaDocumento = DateTime.UtcNow.AddMinutes(-20),
            CodigoCliente = "C001",
            TotalNeto = 50,
            TotalSinIva = 44.64,
            Descuento = 0,
            Iva = 5.36,
            CodigoVendedor = "V001",
            CodigoPago = "EF",
            Placa = "ZZZ999949",  // Placa genérica
            RucCliente = "9999999999999",
            NumeroTurno = 100,
            Subtotal = 44.64,
            NumeroConsecutivo = 1,
            CodigoChofer = "",
            CodigoManguera = "01"
        };
        _dbContext.TransaccionesStaging.Add(
            TransaccionStaging.Create(estacion.Id, "Factura",
                JsonSerializer.Serialize(facturaGenerica), DateTime.UtcNow.AddMinutes(-20)));

        // Detalle con > 5 galones (umbral es 5)
        var detalleGenerica = new DetalleFacturaDto
        {
            NumeroDespacho = 1,
            CodigoManguera = "01",
            FechaDespacho = DateTime.UtcNow.AddMinutes(-20),
            VolumenTotal = 25,
            Cantidad = 10,  // 10 galones > 5 -> ALERTA
            ValorUnitario = 2.50,
            CodigoProducto = "01",
            NombreProducto = "Diesel Premium",
            CodigoCliente = "C001"
        };
        _dbContext.TransaccionesStaging.Add(
            TransaccionStaging.Create(estacion.Id, "DetalleFactura",
                JsonSerializer.Serialize(detalleGenerica), DateTime.UtcNow.AddMinutes(-20)));

        // 3. PaymentFraud: Crédito sin autorización (NumeroComprobante = 0)
        var creditoSinAuth = new CreditoDto
        {
            NumeroCabecera = 1,
            FechaCabecera = DateTime.UtcNow.AddMinutes(-15),
            CodigoCredito = "01",
            CodigoSocio = "S001",
            PlazoCabecera = 30,
            TasaCredito = 0.15,
            CodigoGarante = "",
            TotalCredito = 500,
            TotalInteres = 75,
            CodigoBanco = "01",
            NumeroComprobante = 0  // Sin autorización -> ALERTA
        };
        _dbContext.TransaccionesStaging.Add(
            TransaccionStaging.Create(estacion.Id, "Credito",
                JsonSerializer.Serialize(creditoSinAuth), DateTime.UtcNow.AddMinutes(-15)));

        // 4. Fuente genérica recibida desde una tabla extra del agente.
        _dbContext.TransaccionesStaging.Add(
            TransaccionStaging.Create(
                estacion.Id,
                "TanquesE2E",
                """{"COD_TANQ":"T-01","DIFERENCIA":650,"VENTAS_TANQ":100}""",
                DateTime.UtcNow.AddMinutes(-2)));

        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task ExecuteAsync_FullCycle_DetectsAnomaliesAndPersists()
    {
        // Act — ejecutar el ciclo completo de detección
        await _job.ExecuteAsync();

        // Assert — las alertas deben persistirse
        var alertas = await _dbContext.Alertas.ToListAsync();
        alertas.Should().NotBeEmpty("se diseñaron datos sintéticos para disparar al menos una alerta");

        // Verificar que al menos CashFraud fue detectado (Faltante=100 > umbral 50)
        alertas.Should().Contain(a => a.TipoDetector == TipoDetector.CashFraud,
            "el cierre de turno tiene un faltante de $100 que excede el umbral de $50");
    }

    [Fact]
    public async Task ExecuteAsync_FullCycle_CreatesEjecucionJobRecord()
    {
        // Act
        await _job.ExecuteAsync();

        // Assert — EjecucionJob debe crearse y completarse
        var ejecuciones = await _dbContext.EjecucionesJob.ToListAsync();
        ejecuciones.Should().HaveCount(1);
        ejecuciones[0].Estado.Should().Be(EstadoJob.Completado);
        ejecuciones[0].AlertasGeneradas.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExecuteAsync_FullCycle_NotifiesViaSignalR()
    {
        // Act
        await _job.ExecuteAsync();

        // Assert — cada alerta de auditoría debe publicarse como "NuevaAlerta" a los grupos del central
        var alertaCount = await _dbContext.Alertas.CountAsync();
        if (alertaCount > 0)
        {
            _pushes.Should().Contain(p =>
                p.Evento == "NuevaAlerta" && p.Grupos.Contains("auditores"),
                "las alertas detectadas deben difundirse por el broadcaster en tiempo real");
        }
    }

    [Fact]
    public async Task ExecuteAsync_FuenteGenerica_CreaAlertasEnAmbosAmbitosYNotificaEstacion()
    {
        await _job.ExecuteAsync();

        var personalizadas = await _dbContext.Alertas
            .Where(a => a.TipoDetector == TipoDetector.Personalizada)
            .ToListAsync();

        personalizadas.Should().Contain(a => a.Ambito == AmbitoAlerta.Auditoria);
        personalizadas.Should().Contain(a => a.Ambito == AmbitoAlerta.Operativa);

        var estacionId = await _dbContext.Estaciones.Select(e => e.Id).SingleAsync();
        _pushes.Should().Contain(p =>
            p.Evento == "ProblemaEstacion" && p.Grupos.Contains($"estacion-{estacionId}"),
            "los problemas operativos deben difundirse como ProblemaEstacion al grupo de la estación");
    }

    [Fact]
    public async Task ExecuteAsync_FullCycle_MarksStagingAsProcessed()
    {
        // Act
        await _job.ExecuteAsync();

        // Assert — todos los registros de staging deben marcarse como procesados
        var noProcesadas = await _dbContext.TransaccionesStaging
            .Where(s => !s.Procesada)
            .CountAsync();
        noProcesadas.Should().Be(0, "todas las transacciones de staging deben marcarse como procesadas");
    }

    [Fact]
    public async Task ExecuteAsync_FullCycle_AlertasHaveValidScores()
    {
        // Act
        await _job.ExecuteAsync();

        // Assert — todas las alertas deben tener scores válidos entre 0 y 100
        var alertas = await _dbContext.Alertas.ToListAsync();
        foreach (var alerta in alertas)
        {
            alerta.Score.Should().BeGreaterThanOrEqualTo(0);
            alerta.Score.Should().BeLessThanOrEqualTo(100);
            alerta.NivelRiesgo.Should().BeOneOf(
                NivelRiesgo.Bajo, NivelRiesgo.Medio, NivelRiesgo.Alto, NivelRiesgo.Critico);
            alerta.Descripcion.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task ExecuteAsync_FullCycle_DetectsComplianceViolation()
    {
        // Act
        await _job.ExecuteAsync();

        // Assert — la violación de cumplimiento debe detectarse (ZZZ999949 con 10 galones > 5)
        var alertas = await _dbContext.Alertas.ToListAsync();
        alertas.Should().Contain(a => a.TipoDetector == TipoDetector.ComplianceViolation,
            "la factura con placa ZZZ999949 y 10 galones debe disparar alerta de cumplimiento");
    }

    [Fact]
    public async Task ExecuteAsync_ReglaProgramadaNoLeToca_NoSeEvalua()
    {
        // Arrange — ambas reglas personalizadas pasan de "cada ciclo" a estar programadas "cada 1 día"
        // con la próxima ejecución en el FUTURO: aún no les toca.
        var prog = new ProgramacionEjecucion
        {
            Modo = ModoProgramacion.Intervalo,
            IntervaloN = 1,
            IntervaloUnidad = UnidadIntervalo.Dias
        }.Serializar();
        foreach (var r in _dbContext.ReglasPersonalizadas.ToList())
        {
            r.ProgramacionJson = prog;
            r.ProximaEjecucion = DateTime.UtcNow.AddDays(1);   // futuro → no le toca
            r.UltimaEjecucion = null;
        }
        await _dbContext.SaveChangesAsync();

        // Act
        await _job.ExecuteAsync();

        // Assert — ninguna alerta personalizada: la regla está programada y no le toca este ciclo
        var personalizadas = await _dbContext.Alertas
            .CountAsync(a => a.TipoDetector == TipoDetector.Personalizada);
        personalizadas.Should().Be(0, "una regla programada cuya próxima ejecución es futura no debe evaluarse");

        // …pero las reglas del motor (cada ciclo por defecto) no se ven afectadas por el gate
        (await _dbContext.Alertas.CountAsync(a => a.TipoDetector == TipoDetector.CashFraud))
            .Should().BeGreaterThan(0, "las reglas 'cada ciclo' siguen corriendo en cada ciclo");
    }

    [Fact]
    public async Task ExecuteAsync_ReglaProgramadaLeToca_SeEvaluaPorVentanaYAvanzaProxima()
    {
        // Arrange — ambas reglas personalizadas programadas "cada 1 día" con la próxima ejecución YA
        // VENCIDA (les toca). Sin última ejecución → la ventana abarca los últimos ~31 días.
        var prog = new ProgramacionEjecucion
        {
            Modo = ModoProgramacion.Intervalo,
            IntervaloN = 1,
            IntervaloUnidad = UnidadIntervalo.Dias
        }.Serializar();
        foreach (var r in _dbContext.ReglasPersonalizadas.ToList())
        {
            r.ProgramacionJson = prog;
            r.ProximaEjecucion = DateTime.UtcNow.AddMinutes(-1);   // vencida → le toca
            r.UltimaEjecucion = null;
        }
        await _dbContext.SaveChangesAsync();

        // Act
        await _job.ExecuteAsync();

        // Assert — la regla se evaluó sobre su ventana y generó alertas en ambos carriles
        var personalizadas = await _dbContext.Alertas
            .Where(a => a.TipoDetector == TipoDetector.Personalizada)
            .ToListAsync();
        personalizadas.Should().Contain(a => a.Ambito == AmbitoAlerta.Auditoria);
        personalizadas.Should().Contain(a => a.Ambito == AmbitoAlerta.Operativa);

        // …y su programación avanzó: próxima ≈ ahora + 1 día, última ejecución registrada
        var actualizadas = await _dbContext.ReglasPersonalizadas.AsNoTracking().ToListAsync();
        actualizadas.Should().OnlyContain(r => r.ProximaEjecucion != null && r.ProximaEjecucion > DateTime.UtcNow);
        actualizadas.Should().OnlyContain(r => r.UltimaEjecucion != null);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
