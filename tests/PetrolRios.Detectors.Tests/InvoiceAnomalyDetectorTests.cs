using FluentAssertions;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Tests;

public class InvoiceAnomalyDetectorTests
{
    private readonly InvoiceAnomalyDetector _sut;

    public InvoiceAnomalyDetectorTests()
    {
        // El detector ahora orquesta reglas Strategy inyectadas (las mismas que registra la DI).
        _sut = TestHelpers.CrearInvoiceAnomalyDetector(new RiskScoringEngine());
    }

    [Fact]
    public void Type_ReturnsInvoiceAnomaly()
    {
        _sut.Type.Should().Be(TipoDetector.InvoiceAnomaly);
    }

    [Fact]
    public async Task DetectAsync_WithHighAnnulmentRate_GeneratesAlert()
    {
        // Arrange: 4 anulaciones sobre 10 facturas = 40% (umbral 5%)
        var facturas = Enumerable.Range(1, 10)
            .Select(i => TestHelpers.CreateFactura(secuencia: i))
            .ToList();
        var anulaciones = Enumerable.Range(1, 4)
            .Select(_ => TestHelpers.CreateAnulacion())
            .ToList();

        var context = TestHelpers.CreateContext(facturas: facturas, anulaciones: anulaciones);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Should().Contain(a => a.Descripcion.Contains("anulaciones excesiva"));
    }

    [Fact]
    public async Task DetectAsync_WithLowAnnulmentRate_NoAlert()
    {
        // Arrange: 1 anulación sobre 100 facturas = 1% (umbral 5%)
        var facturas = Enumerable.Range(1, 100)
            .Select(i => TestHelpers.CreateFactura(secuencia: i))
            .ToList();
        var anulaciones = new List<AnulacionDto> { TestHelpers.CreateAnulacion() };

        var context = TestHelpers.CreateContext(facturas: facturas, anulaciones: anulaciones);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Where(a => a.Descripcion.Contains("anulaciones excesiva")).Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_WithPriceAboveList_GeneratesAlert()
    {
        // Arrange: dos despachos del mismo producto, uno con precio mayor
        var detalles = new List<DetalleFacturaDto>
        {
            TestHelpers.CreateDetalle(numero: 1, valorUnitario: 2.50, producto: "01"),
            TestHelpers.CreateDetalle(numero: 2, valorUnitario: 2.50, producto: "01"),
            TestHelpers.CreateDetalle(numero: 3, valorUnitario: 3.00, producto: "01"), // Sobreprecio
        };

        var context = TestHelpers.CreateContext(detalles: detalles);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Should().Contain(a => a.Descripcion.Contains("Precio fuera de lista"));
    }

    [Fact]
    public async Task DetectAsync_WithCorrectPrices_NoPriceAlert()
    {
        // Arrange: todos al mismo precio
        var detalles = new List<DetalleFacturaDto>
        {
            TestHelpers.CreateDetalle(numero: 1, valorUnitario: 2.50, producto: "01"),
            TestHelpers.CreateDetalle(numero: 2, valorUnitario: 2.50, producto: "01"),
            TestHelpers.CreateDetalle(numero: 3, valorUnitario: 2.50, producto: "01"),
        };

        var context = TestHelpers.CreateContext(detalles: detalles);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Where(a => a.Descripcion.Contains("Precio fuera de lista")).Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_WithMissingPlate_GeneratesAlert()
    {
        // Arrange: factura sin placa
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(placa: "", ruc: "1234567890")
        };
        var context = TestHelpers.CreateContext(facturas: facturas);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Should().Contain(a => a.Descripcion.Contains("Campos obligatorios vacíos")
            && a.Descripcion.Contains("placa"));
    }

    [Fact]
    public async Task DetectAsync_WithMissingRuc_GeneratesAlert()
    {
        // Arrange: factura sin RUC
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(placa: "ABC1234", ruc: "")
        };
        var context = TestHelpers.CreateContext(facturas: facturas);

        // Act
        var result = await _sut.DetectAsync(context, CancellationToken.None);

        // Assert
        result.Should().Contain(a => a.Descripcion.Contains("RUC/cédula"));
    }

    [Fact]
    public async Task DetectAsync_WithCompleteFields_NoMandatoryFieldAlert()
    {
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(placa: "ABC1234", ruc: "1234567890")
        };
        var context = TestHelpers.CreateContext(facturas: facturas);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("Campos obligatorios")).Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_WithNoData_ReturnsEmpty()
    {
        var context = TestHelpers.CreateContext();

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_WithFutureDatedInvoice_GeneratesAlert()
    {
        // Factura fechada 48h en el futuro (tolerancia por defecto 24h)
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(fecha: DateTime.UtcNow.AddHours(48))
        };
        var context = TestHelpers.CreateContext(facturas: facturas);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().Contain(a => a.Descripcion.Contains("fechado en el futuro"));
    }

    [Fact]
    public async Task DetectAsync_WithNormalDate_NoFutureDateAlert()
    {
        // Factura con fecha normal (reciente) no debe disparar la regla de fecha futura
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(fecha: DateTime.UtcNow.AddMinutes(-30))
        };
        var context = TestHelpers.CreateContext(facturas: facturas);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("fechado en el futuro")).Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_CamposObligatoriosVacios_EsAmbitoOperativa()
    {
        // Un campo faltante es un error operativo: debe marcarse como Operativa (carril estación)
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(placa: "", ruc: "")
        };
        var context = TestHelpers.CreateContext(facturas: facturas);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().Contain(a =>
            a.Descripcion.Contains("Campos obligatorios")
            && a.Ambito == PetrolRios.Domain.Enums.AmbitoAlerta.Operativa);
    }

    [Fact]
    public async Task DetectAsync_FutureDate_EsAmbitoAuditoria()
    {
        // El backdating es posible fraude: carril de auditoría (no se manda a la estación)
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(fecha: DateTime.UtcNow.AddHours(48))
        };
        var context = TestHelpers.CreateContext(facturas: facturas);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().Contain(a =>
            a.Descripcion.Contains("fechado en el futuro")
            && a.Ambito == PetrolRios.Domain.Enums.AmbitoAlerta.Auditoria);
    }

    [Fact]
    public async Task DetectAsync_DespachoNoFacturado_GeneraAlertaOperativa()
    {
        // Despacho con galones servidos pero no facturado (FAC_DESP != '1')
        var detalles = new List<DetalleFacturaDto>
        {
            TestHelpers.CreateDetalle(numero: 9, cantidad: 12, facturado: "0")
        };
        var context = TestHelpers.CreateContext(detalles: detalles);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().Contain(a =>
            a.Descripcion.Contains("NO facturado")
            && a.Ambito == PetrolRios.Domain.Enums.AmbitoAlerta.Operativa);
    }

    [Fact]
    public async Task DetectAsync_DespachoFacturado_NoGeneraAlerta()
    {
        var detalles = new List<DetalleFacturaDto>
        {
            TestHelpers.CreateDetalle(numero: 10, cantidad: 12, facturado: "1")
        };
        var context = TestHelpers.CreateContext(detalles: detalles);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("NO facturado")).Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_AnulacionesRecurrentes_GeneraAlertaKiting()
    {
        // Mismo punto de emisión con anulaciones en 3 días distintos → posible kiting
        var anulaciones = new List<AnulacionDto>
        {
            TestHelpers.CreateAnulacion(fecha: DateTime.UtcNow),
            TestHelpers.CreateAnulacion(fecha: DateTime.UtcNow.AddDays(-1)),
            TestHelpers.CreateAnulacion(fecha: DateTime.UtcNow.AddDays(-2))
        };
        var context = TestHelpers.CreateContext(anulaciones: anulaciones);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().Contain(a => a.Descripcion.Contains("Anulaciones recurrentes"));
    }

    [Fact]
    public async Task DetectAsync_AnulacionesMismoDia_NoGeneraKiting()
    {
        var anulaciones = new List<AnulacionDto>
        {
            TestHelpers.CreateAnulacion(fecha: DateTime.UtcNow),
            TestHelpers.CreateAnulacion(fecha: DateTime.UtcNow.AddHours(-1))
        };
        var context = TestHelpers.CreateContext(anulaciones: anulaciones);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("Anulaciones recurrentes")).Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_FutureDatedCredito_GeneraAlerta()
    {
        // El backdating también se detecta sobre créditos (CRED_CABE)
        var creditos = new List<CreditoDto>
        {
            TestHelpers.CreateCredito(fecha: DateTime.UtcNow.AddHours(72))
        };
        var context = TestHelpers.CreateContext(creditos: creditos);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().Contain(a => a.Descripcion.Contains("Crédito") && a.Descripcion.Contains("futuro"));
    }

    [Fact]
    public async Task DetectAsync_FutureDateRuleDisabled_NoAlert()
    {
        var reglas = TestHelpers.DefaultReglas()
            .Append(TestHelpers.CreateReglaInactiva(
                TipoDetector.InvoiceAnomaly, "FechaFuturaToleranciaHoras", 24.0))
            .ToList();
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(fecha: DateTime.UtcNow.AddHours(48))
        };
        var context = TestHelpers.CreateContext(facturas: facturas, reglas: reglas);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("fechado en el futuro")).Should().BeEmpty();
    }
}
