using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Tests;

public class ComplianceViolationDetectorTests
{
    private readonly ComplianceViolationDetector _sut;

    public ComplianceViolationDetectorTests()
    {
        _sut = new ComplianceViolationDetector(
            new RiskScoringEngine(),
            NullLogger<ComplianceViolationDetector>.Instance);
    }

    [Fact]
    public void Type_ReturnsComplianceViolation()
    {
        _sut.Type.Should().Be(TipoDetector.ComplianceViolation);
    }

    [Fact]
    public async Task DetectAsync_WithGenericPlateExcessGallons_GeneratesAlert()
    {
        // Arrange: placa genérica ZZZ999949 con 10 galones (umbral 5)
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(placa: "ZZZ999949", manguera: "01", totalNeto: 25)
        };
        var detalles = new List<DetalleFacturaDto>
        {
            TestHelpers.CreateDetalle(numero: 1, cantidad: 10, manguera: "01")
        };

        var context = TestHelpers.CreateContext(facturas: facturas, detalles: detalles);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Should().Contain(a =>
            a.Descripcion.Contains("ZZZ999949") && a.Descripcion.Contains("galones"));
    }

    [Fact]
    public async Task DetectAsync_WithGenericPlateUnderLimit_NoAlert()
    {
        // Arrange: placa genérica con 3 galones (umbral 5)
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(placa: "ZZZ999949", manguera: "01", totalNeto: 3)
        };
        var detalles = new List<DetalleFacturaDto>
        {
            TestHelpers.CreateDetalle(numero: 1, cantidad: 3, manguera: "01")
        };

        var context = TestHelpers.CreateContext(facturas: facturas, detalles: detalles);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Where(a => a.Descripcion.Contains("ZZZ999949")).Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_WithNormalPlate_NoGenericPlateAlert()
    {
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(placa: "ABC1234", totalNeto: 100)
        };
        var detalles = new List<DetalleFacturaDto>
        {
            TestHelpers.CreateDetalle(numero: 1, cantidad: 50)
        };

        var context = TestHelpers.CreateContext(facturas: facturas, detalles: detalles);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("genérica")).Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_WithMultipleFuelTypes_GeneratesAlert()
    {
        // Arrange: misma placa con dos tipos de producto en el mismo día
        var hoy = DateTime.UtcNow.Date.AddHours(10);
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(secuencia: 1, placa: "GYS1234", manguera: "01", fecha: hoy),
            TestHelpers.CreateFactura(secuencia: 2, placa: "GYS1234", manguera: "03", fecha: hoy.AddHours(2))
        };
        var detalles = new List<DetalleFacturaDto>
        {
            TestHelpers.CreateDetalle(numero: 1, producto: "01", manguera: "01", fecha: hoy),
            TestHelpers.CreateDetalle(numero: 2, producto: "03", manguera: "03", fecha: hoy.AddHours(2))
        };

        var context = TestHelpers.CreateContext(facturas: facturas, detalles: detalles);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Should().Contain(a => a.Descripcion.Contains("múltiples combustibles"));
    }

    [Fact]
    public async Task DetectAsync_WithSingleFuelType_NoMultiFuelAlert()
    {
        var hoy = DateTime.UtcNow.Date.AddHours(10);
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(secuencia: 1, placa: "GYS1234", manguera: "01", fecha: hoy),
            TestHelpers.CreateFactura(secuencia: 2, placa: "GYS1234", manguera: "01", fecha: hoy.AddHours(2))
        };
        var detalles = new List<DetalleFacturaDto>
        {
            TestHelpers.CreateDetalle(numero: 1, producto: "01", manguera: "01", fecha: hoy),
            TestHelpers.CreateDetalle(numero: 2, producto: "01", manguera: "01", fecha: hoy.AddHours(2))
        };

        var context = TestHelpers.CreateContext(facturas: facturas, detalles: detalles);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("múltiples combustibles")).Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_WithAfterHoursTransaction_GeneratesAlert()
    {
        // Arrange: transacción a las 23:00 (horario 6:00-22:00)
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(fecha: DateTime.UtcNow.Date.AddHours(23))
        };

        var context = TestHelpers.CreateContext(
            facturas: facturas,
            horaApertura: new TimeOnly(6, 0),
            horaCierre: new TimeOnly(22, 0));

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Should().Contain(a => a.Descripcion.Contains("fuera de horario"));
    }

    [Fact]
    public async Task DetectAsync_WithinBusinessHours_NoAfterHoursAlert()
    {
        // Arrange: transacción a las 14:00 (horario 6:00-22:00)
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(fecha: DateTime.UtcNow.Date.AddHours(14))
        };

        var context = TestHelpers.CreateContext(
            facturas: facturas,
            horaApertura: new TimeOnly(6, 0),
            horaCierre: new TimeOnly(22, 0));

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Where(a => a.Descripcion.Contains("fuera de horario")).Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_WithNoData_ReturnsEmpty()
    {
        var context = TestHelpers.CreateContext();

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_GenericPlateAlert_HasCorrectMetadata()
    {
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(placa: "ZZZ999949", manguera: "01", totalNeto: 50)
        };
        var detalles = new List<DetalleFacturaDto>
        {
            TestHelpers.CreateDetalle(numero: 1, cantidad: 15, manguera: "01")
        };

        var context = TestHelpers.CreateContext(facturas: facturas, detalles: detalles);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        var alert = result.FirstOrDefault(a => a.Descripcion.Contains("ZZZ999949"));
        alert.Should().NotBeNull();
        alert!.Metadata.Should().ContainKey("Placa");
        alert.Metadata.Should().ContainKey("Galones");
        alert.Metadata.Should().ContainKey("GalonesMaximo");
    }
}
