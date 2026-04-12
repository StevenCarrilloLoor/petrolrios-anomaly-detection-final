using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Tests;

public class PaymentFraudDetectorTests
{
    private readonly PaymentFraudDetector _sut;

    public PaymentFraudDetectorTests()
    {
        _sut = new PaymentFraudDetector(
            new RiskScoringEngine(),
            NullLogger<PaymentFraudDetector>.Instance);
    }

    [Fact]
    public void Type_ReturnsPaymentFraud()
    {
        _sut.Type.Should().Be(TipoDetector.PaymentFraud);
    }

    [Fact]
    public async Task DetectAsync_WithLateCardReversal_GeneratesAlert()
    {
        // Arrange: reversión de tarjeta 60 min después de la venta (umbral 30 min)
        var ahora = DateTime.UtcNow;
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(turno: 100, codigoPago: "TJ", fecha: ahora.AddMinutes(-90))
        };
        var cierres = new List<CierreTurnoDto>
        {
            TestHelpers.CreateCierreTurno(turno: 100, fechaFin: ahora.AddMinutes(-30))
        };
        var tarjetas = new List<TarjetaTurnoDto>
        {
            TestHelpers.CreateTarjetaTurno(id: 1, turno: 100, valor: -50) // Valor negativo = reversión
        };

        var context = TestHelpers.CreateContext(
            facturas: facturas, cierresTurno: cierres, tarjetasTurno: tarjetas);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Should().Contain(a => a.Descripcion.Contains("Reversión de tarjeta tardía"));
    }

    [Fact]
    public async Task DetectAsync_WithQuickReversal_NoAlert()
    {
        // Arrange: reversión rápida (10 min, umbral 30 min)
        var ahora = DateTime.UtcNow;
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(turno: 100, codigoPago: "TJ", fecha: ahora.AddMinutes(-20))
        };
        var cierres = new List<CierreTurnoDto>
        {
            TestHelpers.CreateCierreTurno(turno: 100, fechaFin: ahora.AddMinutes(-10))
        };
        var tarjetas = new List<TarjetaTurnoDto>
        {
            TestHelpers.CreateTarjetaTurno(id: 1, turno: 100, valor: -50)
        };

        var context = TestHelpers.CreateContext(
            facturas: facturas, cierresTurno: cierres, tarjetasTurno: tarjetas);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Where(a => a.Descripcion.Contains("Reversión")).Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_WithCreditNoAuthorization_GeneratesAlert()
    {
        // Arrange: crédito con NUMMCOMP = 0 (sin autorización)
        var creditos = new List<CreditoDto>
        {
            TestHelpers.CreateCredito(total: 500, comprobante: 0, socio: "S001")
        };

        var context = TestHelpers.CreateContext(creditos: creditos);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Should().Contain(a => a.Descripcion.Contains("sin código de autorización"));
    }

    [Fact]
    public async Task DetectAsync_WithAuthorizedCredit_NoAlert()
    {
        // Arrange: crédito con NUMMCOMP != 0 (tiene autorización)
        var creditos = new List<CreditoDto>
        {
            TestHelpers.CreateCredito(total: 500, comprobante: 12345, socio: "S001")
        };

        var context = TestHelpers.CreateContext(creditos: creditos);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Where(a => a.Descripcion.Contains("sin código de autorización")).Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_WithDuplicateTransactions_GeneratesAlert()
    {
        // Arrange: 2 tarjetas del mismo banco, mismo monto, turnos cercanos
        var ahora = DateTime.UtcNow;
        var cierres = new List<CierreTurnoDto>
        {
            TestHelpers.CreateCierreTurno(turno: 100, fechaFin: ahora.AddMinutes(-5)),
            TestHelpers.CreateCierreTurno(turno: 101, fechaFin: ahora.AddMinutes(-3))
        };
        var tarjetas = new List<TarjetaTurnoDto>
        {
            TestHelpers.CreateTarjetaTurno(id: 1, turno: 100, banco: "01", valor: 75.00m),
            TestHelpers.CreateTarjetaTurno(id: 2, turno: 101, banco: "01", valor: 75.00m)
        };

        var context = TestHelpers.CreateContext(cierresTurno: cierres, tarjetasTurno: tarjetas);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Should().Contain(a => a.Descripcion.Contains("duplicadas"));
    }

    [Fact]
    public async Task DetectAsync_WithDifferentAmounts_NoDuplicateAlert()
    {
        // Arrange: mismos bancos pero montos distintos
        var ahora = DateTime.UtcNow;
        var cierres = new List<CierreTurnoDto>
        {
            TestHelpers.CreateCierreTurno(turno: 100, fechaFin: ahora.AddMinutes(-5)),
            TestHelpers.CreateCierreTurno(turno: 101, fechaFin: ahora.AddMinutes(-3))
        };
        var tarjetas = new List<TarjetaTurnoDto>
        {
            TestHelpers.CreateTarjetaTurno(id: 1, turno: 100, banco: "01", valor: 75.00m),
            TestHelpers.CreateTarjetaTurno(id: 2, turno: 101, banco: "01", valor: 100.00m)
        };

        var context = TestHelpers.CreateContext(cierresTurno: cierres, tarjetasTurno: tarjetas);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Where(a => a.Descripcion.Contains("duplicadas")).Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_WithNoData_ReturnsEmpty()
    {
        var context = TestHelpers.CreateContext();

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_ReversalAlert_HasCorrectMetadata()
    {
        var ahora = DateTime.UtcNow;
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(turno: 100, codigoPago: "TJ", fecha: ahora.AddMinutes(-90))
        };
        var cierres = new List<CierreTurnoDto>
        {
            TestHelpers.CreateCierreTurno(turno: 100, fechaFin: ahora.AddMinutes(-30))
        };
        var tarjetas = new List<TarjetaTurnoDto>
        {
            TestHelpers.CreateTarjetaTurno(id: 5, turno: 100, valor: -80)
        };

        var context = TestHelpers.CreateContext(
            facturas: facturas, cierresTurno: cierres, tarjetasTurno: tarjetas);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        var alert = result.FirstOrDefault(a => a.Descripcion.Contains("Reversión"));
        alert.Should().NotBeNull();
        alert!.Metadata.Should().ContainKey("DiferenciaMinutos");
        alert.Metadata.Should().ContainKey("MontoReversion");
        alert.TransaccionReferencia.Should().StartWith("TURN_TARJ-");
    }
}
