using FluentAssertions;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Tests;

/// <summary>
/// Tests de la regla "Placa reutilizada en el día" (InvoiceAnomaly): una misma placa facturada más de
/// N veces dentro del mismo día. Umbral por defecto 5 (configurable). Verifica el conteo por día, la
/// exclusión de la placa genérica, la separación por jornada, la configurabilidad del umbral y el
/// respeto al flag Activa.
/// </summary>
public class PlacaReutilizadaRuleTests
{
    private readonly InvoiceAnomalyDetector _invoiceAnomaly;

    public PlacaReutilizadaRuleTests() =>
        _invoiceAnomaly = TestHelpers.CrearInvoiceAnomalyDetector(new RiskScoringEngine());

    // Día fijo para evitar la frontera de medianoche al agrupar por FechaDocumento.Date.
    private static readonly DateTime Dia = new(2026, 3, 15, 8, 0, 0, DateTimeKind.Utc);

    private static List<FacturaDto> FacturasMismaPlaca(int cuantas, string placa, DateTime dia) =>
        Enumerable.Range(0, cuantas)
            .Select(i => TestHelpers.CreateFactura(
                secuencia: i + 1, placa: placa, totalNeto: 60, fecha: dia.AddMinutes(i * 10)))
            .ToList();

    [Fact]
    public async Task PlacaReutilizada_SobreUmbral_GeneraAlerta()
    {
        // 6 facturas a la misma placa el mismo día (umbral por defecto 5 → 6 > 5).
        var context = TestHelpers.CreateContext(facturas: FacturasMismaPlaca(6, "PXY1234", Dia));

        var result = await _invoiceAnomaly.DetectAsync(context, CancellationToken.None);

        result.Should().Contain(a => a.Descripcion.Contains("reutilización de placa")
                                  && a.Descripcion.Contains("PXY1234"));
    }

    [Fact]
    public async Task PlacaReutilizada_EnUmbral_NoGeneraAlerta()
    {
        // Exactamente 5 (no supera el umbral 5: la condición es "más de N").
        var context = TestHelpers.CreateContext(facturas: FacturasMismaPlaca(5, "PXY1234", Dia));

        var result = await _invoiceAnomaly.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("reutilización de placa")).Should().BeEmpty();
    }

    [Fact]
    public async Task PlacaReutilizada_PlacaGenerica_NoGeneraAlerta()
    {
        // La placa genérica (consumidor final) aparece legítimamente muchas veces; la excluye esta regla.
        var context = TestHelpers.CreateContext(facturas: FacturasMismaPlaca(10, "ZZZ999949", Dia));

        var result = await _invoiceAnomaly.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("reutilización de placa")).Should().BeEmpty();
    }

    [Fact]
    public async Task PlacaReutilizada_PlacasDistintas_NoGeneraAlerta()
    {
        // 6 facturas pero cada una con placa distinta: ninguna placa supera el umbral.
        var facturas = Enumerable.Range(0, 6)
            .Select(i => TestHelpers.CreateFactura(
                secuencia: i + 1, placa: $"AB{i:0000}", fecha: Dia.AddMinutes(i * 10)))
            .ToList();
        var context = TestHelpers.CreateContext(facturas: facturas);

        var result = await _invoiceAnomaly.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("reutilización de placa")).Should().BeEmpty();
    }

    [Fact]
    public async Task PlacaReutilizada_RepartidaEnDosDias_NoGeneraAlerta()
    {
        // 3 facturas el día 15 y 3 el día 16 a la misma placa: por jornada ninguna supera el umbral.
        var facturas = FacturasMismaPlaca(3, "PXY1234", Dia)
            .Concat(FacturasMismaPlaca(3, "PXY1234", Dia.AddDays(1)))
            .ToList();
        var context = TestHelpers.CreateContext(facturas: facturas);

        var result = await _invoiceAnomaly.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("reutilización de placa")).Should().BeEmpty();
    }

    [Fact]
    public async Task PlacaReutilizada_UmbralConfigurableA2_GeneraAlerta()
    {
        // La auditoría puede bajar el umbral a 2: con 3 facturas (3 > 2) ya debe alertar.
        var reglas = TestHelpers.DefaultReglas()
            .Append(ReglaDeteccion.Create(
                TipoDetector.InvoiceAnomaly, "Placa reutilizada", "D",
                "PlacaReutilizadaDiaUmbral", 2.0))
            .ToList();
        var context = TestHelpers.CreateContext(
            facturas: FacturasMismaPlaca(3, "PXY1234", Dia), reglas: reglas);

        var result = await _invoiceAnomaly.DetectAsync(context, CancellationToken.None);

        result.Should().Contain(a => a.Descripcion.Contains("reutilización de placa"));
    }

    [Fact]
    public async Task PlacaReutilizada_ReglaInactiva_NoGeneraAlerta()
    {
        // Con la regla desactivada, ni siquiera 6 repeticiones generan alerta.
        var reglas = TestHelpers.DefaultReglas()
            .Append(TestHelpers.CreateReglaInactiva(
                TipoDetector.InvoiceAnomaly, "PlacaReutilizadaDiaUmbral", 5.0))
            .ToList();
        var context = TestHelpers.CreateContext(
            facturas: FacturasMismaPlaca(6, "PXY1234", Dia), reglas: reglas);

        var result = await _invoiceAnomaly.DetectAsync(context, CancellationToken.None);

        result.Where(a => a.Descripcion.Contains("reutilización de placa")).Should().BeEmpty();
    }
}
