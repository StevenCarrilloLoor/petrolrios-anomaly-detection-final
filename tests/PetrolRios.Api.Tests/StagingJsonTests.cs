using FluentAssertions;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Jobs;
using Xunit;

namespace PetrolRios.Api.Tests;

/// <summary>
/// Pruebas del deserializador compartido del staging (lo usan el ciclo de detección y el backtest
/// de reglas). Son pruebas de lógica pura, sin base de datos.
/// </summary>
public class StagingJsonTests
{
    private sealed record Muestra(double X, string Y);

    private static TransaccionStaging Fila(string tipo, string json) =>
        TransaccionStaging.Create(1, tipo, json, DateTime.UtcNow);

    [Fact]
    public void DeserializarPorTipo_DevuelveSoloElTipoPedido()
    {
        var filas = new[]
        {
            Fila("A", "{\"X\":5,\"Y\":\"hola\"}"),
            Fila("B", "{\"X\":9,\"Y\":\"no\"}"),
        };

        var resultado = StagingJson.DeserializarPorTipo<Muestra>(filas, "A");

        resultado.Should().ContainSingle();
        resultado[0].X.Should().Be(5);
        resultado[0].Y.Should().Be("hola");
    }

    [Fact]
    public void DeserializarPorTipo_IgnoraFilasMalformadas()
    {
        var filas = new[] { Fila("A", "{ esto no es json") };

        StagingJson.DeserializarPorTipo<Muestra>(filas, "A").Should().BeEmpty();
    }

    [Fact]
    public void DeserializarDiccionario_ConvierteTiposBasicos()
    {
        var dict = StagingJson.DeserializarDiccionario("{\"num\":5,\"txt\":\"hola\",\"flag\":true}");

        dict.Should().NotBeNull();
        dict!["num"].Should().Be(5.0);
        dict["txt"].Should().Be("hola");
        dict["flag"].Should().Be(true);
    }

    [Fact]
    public void DeserializarDiccionario_DevuelveNullSiElJsonEsInvalido()
    {
        StagingJson.DeserializarDiccionario("no-es-json").Should().BeNull();
    }
}
