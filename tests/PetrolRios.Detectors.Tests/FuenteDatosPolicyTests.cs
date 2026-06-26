using FluentAssertions;
using PetrolRios.Application.Fuentes;
using Xunit;

namespace PetrolRios.Detectors.Tests;

/// <summary>
/// Guard de duplicación: una tabla que el agente ya extrae como fuente "built-in" (DCTO→Factura,
/// ANUL→Anulacion, …) no debe poder registrarse otra vez en el selector. (Causa del bug de San Pío:
/// se agregó DCTO al selector y se duplicó con la built-in Factura.)
/// </summary>
public sealed class FuenteDatosPolicyTests
{
    [Theory]
    [InlineData("DCTO", "Factura")]
    [InlineData("dcto", "Factura")]
    [InlineData(" DCTO ", "Factura")]
    [InlineData("DESP", "DetalleFactura")]
    [InlineData("ANUL", "Anulacion")]
    [InlineData("CRED_CABE", "Credito")]
    [InlineData("TURN_TARJ", "TarjetaTurno")]
    public void TablaBuiltIn_SeReconoceYDaSuFuente(string tabla, string fuente)
    {
        FuenteDatosPolicy.TablaCubiertaPorBuiltIn(tabla).Should().BeTrue();
        FuenteDatosPolicy.FuenteBuiltInDe(tabla).Should().Be(fuente);
    }

    [Theory]
    [InlineData("TANQ_REPO")]   // reporte de tanque: NO es built-in → se permite
    [InlineData("PLACA")]
    [InlineData("")]
    [InlineData(null)]
    public void TablaNoBuiltIn_SePermite(string? tabla)
    {
        FuenteDatosPolicy.TablaCubiertaPorBuiltIn(tabla).Should().BeFalse();
        FuenteDatosPolicy.FuenteBuiltInDe(tabla).Should().BeNull();
    }
}
