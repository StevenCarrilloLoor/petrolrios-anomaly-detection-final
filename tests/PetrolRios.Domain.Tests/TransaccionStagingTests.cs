using FluentAssertions;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Domain.Tests;

/// <summary>Pruebas de la huella de contenido que da idempotencia a la ingesta.</summary>
public class TransaccionStagingTests
{
    [Fact]
    public void CalcularHash_MismaEntrada_ProduceMismaHuella()
    {
        var a = TransaccionStaging.CalcularHash(1, "Factura", "{\"x\":1}");
        var b = TransaccionStaging.CalcularHash(1, "Factura", "{\"x\":1}");
        a.Should().Be(b);
    }

    [Fact]
    public void CalcularHash_TieneLongitudSha256Hex()
    {
        TransaccionStaging.CalcularHash(1, "Factura", "{}").Should().HaveLength(64);
    }

    [Theory]
    [InlineData(2, "Factura", "{\"x\":1}")]   // estación distinta
    [InlineData(1, "Credito", "{\"x\":1}")]   // tipo distinto
    [InlineData(1, "Factura", "{\"x\":2}")]   // datos distintos
    public void CalcularHash_CambiaSiCambiaCualquierComponente(int estacionId, string tipo, string json)
    {
        var baseHash = TransaccionStaging.CalcularHash(1, "Factura", "{\"x\":1}");
        TransaccionStaging.CalcularHash(estacionId, tipo, json).Should().NotBe(baseHash);
    }

    [Fact]
    public void Create_AsignaHashDelContenido()
    {
        var staging = TransaccionStaging.Create(7, "Tanques", "{\"DIFERENCIA\":600}", DateTime.UtcNow);
        staging.HashContenido.Should().Be(
            TransaccionStaging.CalcularHash(7, "Tanques", "{\"DIFERENCIA\":600}"));
    }
}
