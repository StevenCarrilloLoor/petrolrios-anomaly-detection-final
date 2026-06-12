using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PetrolRios.Application.ReglasPersonalizadas;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Tests;

/// <summary>
/// Tests del motor de reglas personalizadas (escalabilidad: reglas creadas por el
/// usuario sin tocar código).
/// </summary>
public class CustomRuleDetectorTests
{
    private readonly CustomRuleDetector _sut;

    public CustomRuleDetectorTests()
    {
        _sut = new CustomRuleDetector(new RiskScoringEngine(), NullLogger<CustomRuleDetector>.Instance);
    }

    private static ReglaPersonalizada CrearRegla(
        string fuente, List<CondicionRegla> condiciones,
        AgregacionRegla? agregacion = null, double riesgoBase = 50, bool activa = true)
    {
        var regla = ReglaPersonalizada.Create(
            $"Regla de prueba {Guid.NewGuid():N}",
            "Descripción de prueba",
            fuente,
            JsonSerializer.Serialize(condiciones),
            agregacion is null ? null : JsonSerializer.Serialize(agregacion),
            riesgoBase);
        regla.Activa = activa;
        return regla;
    }

    [Fact]
    public void Type_ReturnsPersonalizada()
    {
        _sut.Type.Should().Be(TipoDetector.Personalizada);
    }

    [Fact]
    public async Task DetectAsync_CondicionNumericaCumplida_GeneraAlertaPorRegistro()
    {
        // Regla: facturas con TotalNeto > 300
        var regla = CrearRegla("Factura", [new CondicionRegla("TotalNeto", ">", "300")]);
        var context = TestHelpers.CreateContext(
            facturas:
            [
                TestHelpers.CreateFactura(secuencia: 1, totalNeto: 500),
                TestHelpers.CreateFactura(secuencia: 2, totalNeto: 100)
            ],
            reglasPersonalizadas: [regla]);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].TipoDetector.Should().Be(TipoDetector.Personalizada);
        result[0].Descripcion.Should().Contain(regla.Nombre);
    }

    [Fact]
    public async Task DetectAsync_CondicionesAndNoCumplidas_NoGeneraAlerta()
    {
        // Regla: TotalNeto > 300 Y pago a crédito — la factura grande es en efectivo
        var regla = CrearRegla("Factura",
        [
            new CondicionRegla("TotalNeto", ">", "300"),
            new CondicionRegla("CodigoPago", "=", "CR")
        ]);
        var context = TestHelpers.CreateContext(
            facturas: [TestHelpers.CreateFactura(totalNeto: 500, codigoPago: "EF")],
            reglasPersonalizadas: [regla]);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_AgregacionConteo_GeneraAlertaPorGrupo()
    {
        // Regla: más de 2 facturas en efectivo por vendedor
        var regla = CrearRegla("Factura",
            [new CondicionRegla("CodigoPago", "=", "EF")],
            new AgregacionRegla("CodigoVendedor", "Conteo", null, ">", 2));
        var context = TestHelpers.CreateContext(
            facturas:
            [
                TestHelpers.CreateFactura(secuencia: 1, vendedor: "V009"),
                TestHelpers.CreateFactura(secuencia: 2, vendedor: "V009"),
                TestHelpers.CreateFactura(secuencia: 3, vendedor: "V009"),
                TestHelpers.CreateFactura(secuencia: 4, vendedor: "V002")
            ],
            reglasPersonalizadas: [regla]);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].EmpleadoCodigo.Should().Be("V009");
        result[0].Metadata["ValorAgregado"].Should().Be(3.0);
    }

    [Fact]
    public async Task DetectAsync_AgregacionSuma_RespetaUmbral()
    {
        // Regla: suma de faltantes por vendedor > 100
        var regla = CrearRegla("CierreTurno",
            [new CondicionRegla("Faltante", ">", "0")],
            new AgregacionRegla("CodigoVendedor", "Suma", "Faltante", ">", 100));
        var context = TestHelpers.CreateContext(
            cierresTurno:
            [
                TestHelpers.CreateCierreTurno(turno: 1, vendedor: "V001", faltante: 80),
                TestHelpers.CreateCierreTurno(turno: 2, vendedor: "V001", faltante: 40),
                TestHelpers.CreateCierreTurno(turno: 3, vendedor: "V002", faltante: 30)
            ],
            reglasPersonalizadas: [regla]);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].EmpleadoCodigo.Should().Be("V001");
    }

    [Fact]
    public async Task DetectAsync_ReglaInactiva_NoGeneraAlertas()
    {
        var regla = CrearRegla("Factura",
            [new CondicionRegla("TotalNeto", ">", "0")], activa: false);
        var context = TestHelpers.CreateContext(
            facturas: [TestHelpers.CreateFactura(totalNeto: 500)],
            reglasPersonalizadas: [regla]);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAsync_ReglaConJsonInvalido_NoLanzaExcepcion()
    {
        var regla = ReglaPersonalizada.Create(
            "Regla corrupta", "x", "Factura", "esto no es json", null, 50);
        var context = TestHelpers.CreateContext(
            facturas: [TestHelpers.CreateFactura(totalNeto: 500)],
            reglasPersonalizadas: [regla]);

        var act = async () => await _sut.DetectAsync(context, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DetectAsync_OperadorTextoVacio_DetectaCamposVacios()
    {
        // Regla: facturas sin placa
        var regla = CrearRegla("Factura", [new CondicionRegla("Placa", "vacio", "")]);
        var context = TestHelpers.CreateContext(
            facturas:
            [
                TestHelpers.CreateFactura(secuencia: 1, placa: ""),
                TestHelpers.CreateFactura(secuencia: 2, placa: "ABC1234")
            ],
            reglasPersonalizadas: [regla]);

        var result = await _sut.DetectAsync(context, CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
