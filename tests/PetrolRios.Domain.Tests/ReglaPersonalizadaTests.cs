using FluentAssertions;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Domain.Tests;

/// <summary>Pruebas del ámbito (carril) de las reglas personalizadas.</summary>
public class ReglaPersonalizadaTests
{
    [Fact]
    public void Create_SinAmbito_EsAuditoriaPorDefecto()
    {
        var regla = ReglaPersonalizada.Create("R", "d", "Factura", "[]", null, 50);
        regla.Ambito.Should().Be("Auditoria");
    }

    [Theory]
    [InlineData("Operativa", "Operativa")]
    [InlineData("operativa", "Operativa")]
    [InlineData("  OPERATIVA ", "Operativa")]
    [InlineData("Auditoria", "Auditoria")]
    [InlineData("cualquier cosa", "Auditoria")]
    [InlineData("", "Auditoria")]
    [InlineData(null, "Auditoria")]
    public void NormalizarAmbito_SoloAceptaOperativaOAuditoria(string? entrada, string esperado)
    {
        ReglaPersonalizada.NormalizarAmbito(entrada).Should().Be(esperado);
    }
}
