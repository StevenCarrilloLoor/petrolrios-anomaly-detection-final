using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Tests;

public class CashFraudDetectorTests
{
    private readonly CashFraudDetector _sut;

    public CashFraudDetectorTests()
    {
        _sut = new CashFraudDetector(
            new RiskScoringEngine(),
            NullLogger<CashFraudDetector>.Instance);
    }

    [Fact]
    public void Type_ReturnsCashFraud()
    {
        _sut.Type.Should().Be(TipoDetector.CashFraud);
    }

    [Fact]
    public async Task DetectAsync_WithCashDifferenceAboveThreshold_GeneratesAlert()
    {
        // Arrange: turno con faltante de $100 (umbral = $50)
        var context = TestHelpers.CreateContext(
            cierresTurno: [TestHelpers.CreateCierreTurno(faltante: 100)],
            facturas: [TestHelpers.CreateFactura(codigoPago: "EF", totalNeto: 500)],
            depositosTurno: [TestHelpers.CreateDeposito(total: 400)]);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Should().Contain(a => a.Descripcion.Contains("Diferencia de efectivo"));
    }

    [Fact]
    public async Task DetectAsync_ReglaConfiguradaOperativa_LaAlertaUsaEseCarril()
    {
        // El carril es editable: si la regla de diferencia de efectivo se marca como Operativa,
        // la alerta generada debe salir en ese carril y no en Auditoría por defecto.
        var reglaOperativa = ReglaDeteccion.Create(
            TipoDetector.CashFraud, "Diferencia", "d", "DiferenciaEfectivoUmbral", 50.0, AmbitoAlerta.Operativa);

        var context = TestHelpers.CreateContext(
            reglas: [reglaOperativa],
            cierresTurno: [TestHelpers.CreateCierreTurno(faltante: 100)],
            facturas: [TestHelpers.CreateFactura(codigoPago: "EF", totalNeto: 500)],
            depositosTurno: [TestHelpers.CreateDeposito(total: 400)]);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        var dif = result.First(a => a.Descripcion.Contains("Diferencia de efectivo"));
        dif.Ambito.Should().Be(AmbitoAlerta.Operativa);
    }

    [Fact]
    public async Task DetectAsync_WithCashDifferenceBelowThreshold_NoAlert()
    {
        // Arrange: turno con faltante de $20 (umbral = $50)
        var context = TestHelpers.CreateContext(
            cierresTurno: [TestHelpers.CreateCierreTurno(faltante: 20)],
            facturas: [TestHelpers.CreateFactura(codigoPago: "EF", totalNeto: 500)],
            depositosTurno: [TestHelpers.CreateDeposito(total: 480)]);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert: no debería generar alerta de diferencia (solo evalúa FAL_TURN)
        result.Where(a => a.Descripcion.Contains("Diferencia de efectivo")).Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_WithRecurrentShortages_DetectsGineteoPattern()
    {
        // Arrange: 3+ turnos con faltante del mismo empleado
        var cierres = new List<CierreTurnoDto>
        {
            TestHelpers.CreateCierreTurno(turno: 100, vendedor: "V001", faltante: 30),
            TestHelpers.CreateCierreTurno(turno: 101, vendedor: "V001", faltante: 25),
            TestHelpers.CreateCierreTurno(turno: 102, vendedor: "V001", faltante: 40)
        };
        var context = TestHelpers.CreateContext(cierresTurno: cierres);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Should().Contain(a => a.Descripcion.Contains("gineteo"));
    }

    [Fact]
    public async Task DetectAsync_WithPreviousAlerts_AccumulatesForPattern()
    {
        // Arrange: 1 turno con faltante + 2 alertas previas = 3 total (>= umbral 3)
        var cierres = new List<CierreTurnoDto>
        {
            TestHelpers.CreateCierreTurno(turno: 100, vendedor: "V001", faltante: 30)
        };
        var alertasPrevias = new Dictionary<string, int> { ["V001"] = 2 };

        var context = TestHelpers.CreateContext(
            cierresTurno: cierres,
            alertasPrevias: alertasPrevias);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Should().Contain(a => a.Descripcion.Contains("gineteo")
            && a.EmpleadoCodigo == "V001");
    }

    [Fact]
    public async Task DetectAsync_WithNoData_ReturnsEmpty()
    {
        var context = TestHelpers.CreateContext();

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_WithDifferentEmployees_NoPatternDetected()
    {
        // Arrange: faltantes de distintos empleados (cada uno solo 1 vez)
        var cierres = new List<CierreTurnoDto>
        {
            TestHelpers.CreateCierreTurno(turno: 100, vendedor: "V001", faltante: 30),
            TestHelpers.CreateCierreTurno(turno: 101, vendedor: "V002", faltante: 25),
            TestHelpers.CreateCierreTurno(turno: 102, vendedor: "V003", faltante: 40)
        };
        var context = TestHelpers.CreateContext(cierresTurno: cierres);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert: no gineteo (cada empleado tiene solo 1 faltante)
        result.Where(a => a.Descripcion.Contains("gineteo")).Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_AlertMetadata_ContainsRelevantFields()
    {
        var context = TestHelpers.CreateContext(
            cierresTurno: [TestHelpers.CreateCierreTurno(turno: 100, faltante: 100)],
            facturas: [TestHelpers.CreateFactura(codigoPago: "EF", totalNeto: 500, turno: 100)],
            depositosTurno: [TestHelpers.CreateDeposito(turno: 100, total: 400)]);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        var alert = result.FirstOrDefault(a => a.Descripcion.Contains("Diferencia"));
        alert.Should().NotBeNull();
        alert!.Metadata.Should().ContainKey("NumeroTurno");
        alert.Metadata.Should().ContainKey("Diferencia");
        alert.Metadata.Should().ContainKey("Umbral");
        alert.TransaccionReferencia.Should().StartWith("TURN-");
    }

    [Fact]
    public async Task DetectAsync_TurnoSinCerrar_GeneraAlertaOperativa()
    {
        // Turno abierto (EST_TURN='0') desde hace 30 h (umbral 18 h) → problema operativo
        var turnos = new List<CierreTurnoDto>
        {
            TestHelpers.CreateCierreTurno(
                turno: 5, estadoTurno: "0", fechaInicio: DateTime.UtcNow.AddHours(-30))
        };
        var context = TestHelpers.CreateContext(cierresTurno: turnos);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().Contain(a =>
            a.Descripcion.Contains("sin cerrar") && a.Ambito == AmbitoAlerta.Operativa);
    }

    [Fact]
    public async Task DetectAsync_TurnoAbiertoReciente_NoGeneraAlerta()
    {
        // Turno abierto hace solo 2 h: aún es normal, no debe alertar
        var turnos = new List<CierreTurnoDto>
        {
            TestHelpers.CreateCierreTurno(
                turno: 6, estadoTurno: "0", fechaInicio: DateTime.UtcNow.AddHours(-2))
        };
        var context = TestHelpers.CreateContext(cierresTurno: turnos);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("sin cerrar")).Should().BeEmpty();
    }
}
