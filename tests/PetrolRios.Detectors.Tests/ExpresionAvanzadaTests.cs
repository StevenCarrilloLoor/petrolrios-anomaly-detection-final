using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PetrolRios.Application.ReglasPersonalizadas;
using PetrolRios.Application.ReglasPersonalizadas.Expresiones;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Tests;

/// <summary>
/// Tests del motor de expresiones avanzadas (lógica de programación en las reglas
/// personalizadas) y de su evaluación dentro del detector.
/// </summary>
public class ExpresionAvanzadaTests
{
    // ─── Evaluador de expresiones (unidad) ───

    private sealed class CtxFake(Dictionary<string, object> valores) : IContextoEvaluacion
    {
        public Valor ObtenerCampo(string nombre)
        {
            var v = valores[nombre];
            return v is string s ? Valor.DeTexto(s) : Valor.DeNumero(Convert.ToDouble(v));
        }
    }

    private static bool Eval(string expr, Dictionary<string, object> campos) =>
        EvaluadorExpresion.Compilar(expr).Evaluar(new CtxFake(campos));

    [Theory]
    [InlineData("TotalNeto > 400", true)]
    [InlineData("TotalNeto >= 500", true)]
    [InlineData("TotalNeto < 400", false)]
    [InlineData("TotalNeto == 500", true)]
    [InlineData("TotalNeto != 500", false)]
    public void Comparaciones_Numericas(string expr, bool esperado)
    {
        Eval(expr, new() { ["TotalNeto"] = 500.0 }).Should().Be(esperado);
    }

    [Fact]
    public void LogicaAndOr_ConParentesis()
    {
        var campos = new Dictionary<string, object> { ["TotalNeto"] = 500.0, ["CodigoPago"] = "EF" };
        Eval("TotalNeto > 400 && CodigoPago == 'EF'", campos).Should().BeTrue();
        Eval("TotalNeto > 400 && CodigoPago == 'TC'", campos).Should().BeFalse();
        Eval("TotalNeto > 1000 || CodigoPago == 'EF'", campos).Should().BeTrue();
        Eval("(TotalNeto > 1000 || CodigoPago == 'EF') && TotalNeto < 600", campos).Should().BeTrue();
    }

    [Fact]
    public void AritmeticaEntreCampos()
    {
        // Descuento como porcentaje del subtotal > 10%
        var campos = new Dictionary<string, object> { ["Descuento"] = 25.0, ["Subtotal"] = 100.0 };
        Eval("Descuento / Subtotal > 0.1", campos).Should().BeTrue();
        Eval("Descuento / Subtotal > 0.5", campos).Should().BeFalse();
    }

    [Fact]
    public void Funciones_TextoYNumero()
    {
        var campos = new Dictionary<string, object> { ["Placa"] = "", ["Descuento"] = -30.0 };
        Eval("vacio(Placa)", campos).Should().BeTrue();
        Eval("abs(Descuento) > 20", campos).Should().BeTrue();

        var campos2 = new Dictionary<string, object> { ["CodigoPago"] = "CREDITO" };
        Eval("contiene(CodigoPago, 'CRED')", campos2).Should().BeTrue();
        Eval("empieza(CodigoPago, 'CRED')", campos2).Should().BeTrue();
    }

    [Fact]
    public void Funciones_Matematicas()
    {
        var c = new Dictionary<string, object> { ["A"] = 10.0, ["B"] = 3.0 };
        Eval("min(A, B) == 3", c).Should().BeTrue();
        Eval("max(A, B) == 10", c).Should().BeTrue();
        Eval("modulo(A, B) == 1", c).Should().BeTrue();
        Eval("modulo(A, 0) == 0", c).Should().BeTrue(); // división por cero segura
        Eval("piso(2.9) == 2", c).Should().BeTrue();
        Eval("techo(2.1) == 3", c).Should().BeTrue();
        Eval("raiz(9) == 3", c).Should().BeTrue();
        Eval("potencia(2, 3) == 8", c).Should().BeTrue();
        Eval("redondear(2.6) == 3", c).Should().BeTrue();
        Eval("redondear(12.34, 1) == 12.3", c).Should().BeTrue();
    }

    [Fact]
    public void Funcion_En_ListaDePertenencia()
    {
        var texto = new Dictionary<string, object> { ["CodigoPago"] = "EFE" };
        Eval("en(CodigoPago, 'EF', 'EFE', 'CONT')", texto).Should().BeTrue();
        Eval("en(CodigoPago, 'TC', 'CR')", texto).Should().BeFalse();

        var num = new Dictionary<string, object> { ["NumeroTurno"] = 3.0 };
        Eval("en(NumeroTurno, 1, 2, 3)", num).Should().BeTrue();
        Eval("en(NumeroTurno, 4, 5)", num).Should().BeFalse();
    }

    [Fact]
    public void Funcion_SiVacio_Coalescencia()
    {
        Eval("siVacio(Placa, 'SIN') == 'SIN'", new() { ["Placa"] = "" }).Should().BeTrue();
        Eval("siVacio(Placa, 'SIN') == 'ABC123'", new() { ["Placa"] = "ABC123" }).Should().BeTrue();
    }

    [Fact]
    public void Negacion()
    {
        var campos = new Dictionary<string, object> { ["CodigoPago"] = "TC" };
        Eval("!(CodigoPago == 'EF')", campos).Should().BeTrue();
        Eval("not (CodigoPago == 'TC')", campos).Should().BeFalse();
    }

    [Fact]
    public void ExpresionInvalida_LanzaExcepcion()
    {
        var act = () => EvaluadorExpresion.Compilar("TotalNeto > > 400");
        act.Should().Throw<ExpresionException>();
    }

    [Fact]
    public void ExpresionSinComparacion_FallaAlEvaluar()
    {
        var act = () => EvaluadorExpresion.Compilar("TotalNeto + 5")
            .Evaluar(new CtxFake(new() { ["TotalNeto"] = 1.0 }));
        act.Should().Throw<ExpresionException>();
    }

    [Fact]
    public void Validar_CampoInexistente_ReportaError()
    {
        var errores = EvaluadorExpresion.Validar("CampoQueNoExiste > 5", "Factura");
        errores.Should().ContainSingle().Which.Should().Contain("CampoQueNoExiste");
    }

    [Fact]
    public void Validar_ExpresionCorrecta_SinErrores()
    {
        var errores = EvaluadorExpresion.Validar(
            "TotalNeto > 400 && CodigoPago == 'EF'", "Factura");
        errores.Should().BeEmpty();
    }

    // ─── Detector en modo avanzado (integración) ───

    [Fact]
    public async Task CustomRuleDetector_ModoAvanzado_FiltraConExpresion()
    {
        var regla = ReglaPersonalizada.Create(
            "Efectivo grande con descuento", "x", "Factura",
            "[]", null, 70);
        regla.ExpresionAvanzada = "TotalNeto > 400 && CodigoPago == 'EF' && Descuento / Subtotal > 0.1";

        var sut = new CustomRuleDetector(new RiskScoringEngine(), NullLogger<CustomRuleDetector>.Instance);

        var context = TestHelpers.CreateContext(
            facturas:
            [
                // cumple: 500 EF, descuento 60/500=12%
                TestHelpers.CreateFactura(secuencia: 1, totalNeto: 500, codigoPago: "EF",
                    subtotal: 500, descuento: 60),
                // no cumple: es TC
                TestHelpers.CreateFactura(secuencia: 2, totalNeto: 500, codigoPago: "TC",
                    subtotal: 500, descuento: 60),
                // no cumple: descuento bajo
                TestHelpers.CreateFactura(secuencia: 3, totalNeto: 500, codigoPago: "EF",
                    subtotal: 500, descuento: 10)
            ],
            reglasPersonalizadas: [regla]);

        var result = await sut.DetectAsync(context, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].TipoDetector.Should().Be(TipoDetector.Personalizada);
        result[0].Metadata.Should().ContainKey("Expresion");
    }

    [Fact]
    public async Task CustomRuleDetector_ExpresionInvalida_NoTumbaElCiclo()
    {
        var regla = ReglaPersonalizada.Create("Rota", "x", "Factura", "[]", null, 50);
        regla.ExpresionAvanzada = "TotalNeto >>> 400";

        var sut = new CustomRuleDetector(new RiskScoringEngine(), NullLogger<CustomRuleDetector>.Instance);
        var context = TestHelpers.CreateContext(
            facturas: [TestHelpers.CreateFactura(totalNeto: 500)],
            reglasPersonalizadas: [regla]);

        var act = async () => await sut.DetectAsync(context, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
