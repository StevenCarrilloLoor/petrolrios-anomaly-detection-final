using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Tests;

/// <summary>
/// Tests de las reglas agregadas tras la revisión del jurado:
/// venta a crédito sin cliente, efectivo corporativo atípico, descuento excesivo,
/// total inconsistente, despachos rápidos sucesivos, venta sin placa en monto mayor
/// y respeto al flag Activa de las reglas.
/// </summary>
public class NuevasReglasDetectorTests
{
    private readonly CashFraudDetector _cashFraud;
    private readonly InvoiceAnomalyDetector _invoiceAnomaly;
    private readonly PaymentFraudDetector _paymentFraud;
    private readonly ComplianceViolationDetector _compliance;

    public NuevasReglasDetectorTests()
    {
        var scoring = new RiskScoringEngine();
        _cashFraud = new CashFraudDetector(scoring, NullLogger<CashFraudDetector>.Instance);
        _invoiceAnomaly = new InvoiceAnomalyDetector(scoring, NullLogger<InvoiceAnomalyDetector>.Instance);
        _paymentFraud = new PaymentFraudDetector(scoring, NullLogger<PaymentFraudDetector>.Instance);
        _compliance = new ComplianceViolationDetector(scoring, NullLogger<ComplianceViolationDetector>.Instance);
    }

    // ─── Cash Fraud: venta a crédito sin cliente identificado ───

    [Fact]
    public async Task CashFraud_CreditoSinClienteIdentificado_GeneraAlerta()
    {
        var context = TestHelpers.CreateContext(
            facturas:
            [
                TestHelpers.CreateFactura(
                    codigoPago: "CR", codigoCliente: "", ruc: "", totalNeto: 80)
            ]);

        var result = await _cashFraud.DetectAsync(context, CancellationToken.None);

        result.Should().Contain(a => a.Descripcion.Contains("sin cliente identificado"));
    }

    [Fact]
    public async Task CashFraud_CreditoConClienteIdentificado_NoGeneraAlerta()
    {
        var context = TestHelpers.CreateContext(
            facturas:
            [
                TestHelpers.CreateFactura(
                    codigoPago: "CR", codigoCliente: "C001", ruc: "1234567890", totalNeto: 80)
            ]);

        var result = await _cashFraud.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("sin cliente identificado")).Should().BeEmpty();
    }

    // ─── Cash Fraud: proporción atípica de efectivo corporativo ───

    [Fact]
    public async Task CashFraud_EfectivoCorporativoSobreUmbral_GeneraAlerta()
    {
        // 4 de 5 transacciones corporativas en efectivo = 80% (umbral 30%)
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(secuencia: 1, codigoPago: "EF", codigoCliente: "C001"),
            TestHelpers.CreateFactura(secuencia: 2, codigoPago: "EF", codigoCliente: "C001"),
            TestHelpers.CreateFactura(secuencia: 3, codigoPago: "EF", codigoCliente: "C001"),
            TestHelpers.CreateFactura(secuencia: 4, codigoPago: "EF", codigoCliente: "C001"),
            TestHelpers.CreateFactura(secuencia: 5, codigoPago: "TC", codigoCliente: "C001")
        };
        var context = TestHelpers.CreateContext(facturas: facturas);

        var result = await _cashFraud.DetectAsync(context, CancellationToken.None);

        result.Should().Contain(a => a.Descripcion.Contains("Proporción atípica de efectivo"));
    }

    [Fact]
    public async Task CashFraud_EfectivoCorporativoBajoUmbral_NoGeneraAlerta()
    {
        // 1 de 5 transacciones en efectivo = 20% (umbral 30%)
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(secuencia: 1, codigoPago: "EF", codigoCliente: "C001"),
            TestHelpers.CreateFactura(secuencia: 2, codigoPago: "TC", codigoCliente: "C001"),
            TestHelpers.CreateFactura(secuencia: 3, codigoPago: "TC", codigoCliente: "C001"),
            TestHelpers.CreateFactura(secuencia: 4, codigoPago: "TC", codigoCliente: "C001"),
            TestHelpers.CreateFactura(secuencia: 5, codigoPago: "TC", codigoCliente: "C001")
        };
        var context = TestHelpers.CreateContext(facturas: facturas);

        var result = await _cashFraud.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("Proporción atípica")).Should().BeEmpty();
    }

    // ─── Invoice Anomaly: descuento excesivo ───

    [Fact]
    public async Task InvoiceAnomaly_DescuentoExcesivo_GeneraAlerta()
    {
        // Descuento del 20% sobre subtotal de 100 (máximo permitido: 10%)
        var context = TestHelpers.CreateContext(
            facturas:
            [
                TestHelpers.CreateFactura(subtotal: 100, descuento: 20, totalNeto: 92, iva: 12)
            ]);

        var result = await _invoiceAnomaly.DetectAsync(context, CancellationToken.None);

        result.Should().Contain(a => a.Descripcion.Contains("Descuento excesivo"));
    }

    [Fact]
    public async Task InvoiceAnomaly_DescuentoDentroDePolitica_NoGeneraAlerta()
    {
        // Descuento del 5% (máximo permitido: 10%)
        var context = TestHelpers.CreateContext(
            facturas:
            [
                TestHelpers.CreateFactura(subtotal: 100, descuento: 5, totalNeto: 107, iva: 12)
            ]);

        var result = await _invoiceAnomaly.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("Descuento excesivo")).Should().BeEmpty();
    }

    // ─── Invoice Anomaly: total inconsistente ───

    [Fact]
    public async Task InvoiceAnomaly_TotalInconsistente_GeneraAlerta()
    {
        // Total esperado: 100 - 0 + 12 = 112, registrado: 150
        var context = TestHelpers.CreateContext(
            facturas:
            [
                TestHelpers.CreateFactura(subtotal: 100, descuento: 0, iva: 12, totalNeto: 150)
            ]);

        var result = await _invoiceAnomaly.DetectAsync(context, CancellationToken.None);

        result.Should().Contain(a => a.Descripcion.Contains("Total inconsistente"));
    }

    [Fact]
    public async Task InvoiceAnomaly_TotalConsistente_NoGeneraAlerta()
    {
        // Total esperado: 100 - 0 + 12 = 112, registrado: 112
        var context = TestHelpers.CreateContext(
            facturas:
            [
                TestHelpers.CreateFactura(subtotal: 100, descuento: 0, iva: 12, totalNeto: 112)
            ]);

        var result = await _invoiceAnomaly.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("Total inconsistente")).Should().BeEmpty();
    }

    // ─── Payment Fraud: despachos rápidos sucesivos ───

    [Fact]
    public async Task PaymentFraud_DespachosRapidosSucesivos_GeneraAlerta()
    {
        var inicio = DateTime.UtcNow.AddHours(-1);
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(secuencia: 1, codigoCliente: "C009", fecha: inicio),
            TestHelpers.CreateFactura(secuencia: 2, codigoCliente: "C009", fecha: inicio.AddMinutes(5)),
            TestHelpers.CreateFactura(secuencia: 3, codigoCliente: "C009", fecha: inicio.AddMinutes(9))
        };
        var context = TestHelpers.CreateContext(facturas: facturas);

        var result = await _paymentFraud.DetectAsync(context, CancellationToken.None);

        result.Should().Contain(a => a.Descripcion.Contains("Despachos rápidos sucesivos"));
    }

    [Fact]
    public async Task PaymentFraud_DespachosEspaciados_NoGeneraAlerta()
    {
        var inicio = DateTime.UtcNow.AddHours(-3);
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(secuencia: 1, codigoCliente: "C009", fecha: inicio),
            TestHelpers.CreateFactura(secuencia: 2, codigoCliente: "C009", fecha: inicio.AddMinutes(40)),
            TestHelpers.CreateFactura(secuencia: 3, codigoCliente: "C009", fecha: inicio.AddMinutes(90))
        };
        var context = TestHelpers.CreateContext(facturas: facturas);

        var result = await _paymentFraud.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("Despachos rápidos")).Should().BeEmpty();
    }

    [Fact]
    public async Task PaymentFraud_DosDespachosRapidos_NoGeneraAlerta()
    {
        // Solo 2 transacciones rápidas: por debajo del mínimo de 3
        var inicio = DateTime.UtcNow.AddHours(-1);
        var facturas = new List<FacturaDto>
        {
            TestHelpers.CreateFactura(secuencia: 1, codigoCliente: "C009", fecha: inicio),
            TestHelpers.CreateFactura(secuencia: 2, codigoCliente: "C009", fecha: inicio.AddMinutes(4))
        };
        var context = TestHelpers.CreateContext(facturas: facturas);

        var result = await _paymentFraud.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("Despachos rápidos")).Should().BeEmpty();
    }

    // ─── Compliance: venta sin placa en monto mayor ───

    [Fact]
    public async Task Compliance_VentaSinPlacaMontoMayor_GeneraAlerta()
    {
        var context = TestHelpers.CreateContext(
            facturas: [TestHelpers.CreateFactura(placa: "", totalNeto: 500)]);

        var result = await _compliance.DetectAsync(context, CancellationToken.None);

        result.Should().Contain(a => a.Descripcion.Contains("sin placa registrada"));
    }

    [Fact]
    public async Task Compliance_VentaSinPlacaMontoMenor_NoGeneraAlerta()
    {
        var context = TestHelpers.CreateContext(
            facturas: [TestHelpers.CreateFactura(placa: "", totalNeto: 50)]);

        var result = await _compliance.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("sin placa registrada")).Should().BeEmpty();
    }

    // ─── Respeto al flag Activa ───

    [Fact]
    public async Task Compliance_FueraHorarioConReglaInactiva_NoGeneraAlerta()
    {
        // Transacción a las 03:00 con horario 06:00–22:00, pero la regla está
        // desactivada (estaciones 24/7): no debe generar alerta.
        var madrugada = DateTime.UtcNow.Date.AddHours(3);
        var reglas = TestHelpers.DefaultReglas()
            .Where(r => r.ParametroNombre != "FueraHorarioHabilitado")
            .Append(TestHelpers.CreateReglaInactiva(
                TipoDetector.ComplianceViolation, "FueraHorarioHabilitado", 0.0))
            .ToList();

        var context = TestHelpers.CreateContext(
            facturas: [TestHelpers.CreateFactura(fecha: madrugada)],
            reglas: reglas,
            horaApertura: new TimeOnly(6, 0),
            horaCierre: new TimeOnly(22, 0));

        var result = await _compliance.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("fuera de horario")).Should().BeEmpty();
    }

    // ─── Robustez ante lotes duplicados (store-and-forward del agente) ───

    [Fact]
    public async Task PaymentFraud_CierresTurnoDuplicados_NoLanzaExcepcion()
    {
        // Caso real detectado en pruebas en vivo: el agente reenvió un lote
        // (store-and-forward) y llegaron dos cierres con el mismo NumeroTurno.
        var context = TestHelpers.CreateContext(
            cierresTurno:
            [
                TestHelpers.CreateCierreTurno(turno: 990001, vendedor: "V001"),
                TestHelpers.CreateCierreTurno(turno: 990001, vendedor: "V001")
            ],
            tarjetasTurno: [TestHelpers.CreateTarjetaTurno(turno: 990001, valor: -50)]);

        var act = async () => await _paymentFraud.DetectAsync(context, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CashFraud_ReglaCreditoSinClienteInactiva_NoGeneraAlerta()
    {
        var reglas = TestHelpers.DefaultReglas()
            .Where(r => r.ParametroNombre != "CreditoSinClienteHabilitado")
            .Append(TestHelpers.CreateReglaInactiva(
                TipoDetector.CashFraud, "CreditoSinClienteHabilitado", 1.0))
            .ToList();

        var context = TestHelpers.CreateContext(
            facturas:
            [
                TestHelpers.CreateFactura(codigoPago: "CR", codigoCliente: "", ruc: "")
            ],
            reglas: reglas);

        var result = await _cashFraud.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("sin cliente identificado")).Should().BeEmpty();
    }
}
