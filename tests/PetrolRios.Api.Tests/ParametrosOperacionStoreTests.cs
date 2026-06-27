using FluentAssertions;
using Microsoft.Extensions.Configuration;
using PetrolRios.Application.DTOs.Configuracion;
using PetrolRios.Infrastructure.Configuracion;

namespace PetrolRios.Api.Tests;

/// <summary>
/// Pruebas puras (sin BD) del store de parámetros de operación, enfocadas en la nueva tasa de
/// refresco: se acota a 1 s … 1 h y 0/negativo cae al valor por defecto (1 s).
/// </summary>
public sealed class ParametrosOperacionStoreTests
{
    private static IConfiguration ConfigVacia() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();

    private static string DirTemporal() =>
        Path.Combine(Path.GetTempPath(), "petrolrios-oper-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void SinArchivo_RefrescoEsUnoPorDefecto()
    {
        var store = new ParametrosOperacionStore(DirTemporal(), ConfigVacia());
        store.Actual().RefrescoSegundos.Should().Be(1);
    }

    [Theory]
    [InlineData(10, 10)]     // valor válido se conserva
    [InlineData(1, 1)]       // mínimo
    [InlineData(3600, 3600)] // máximo
    [InlineData(0, 1)]       // 0 → por defecto (1)
    [InlineData(-5, 1)]      // negativo → por defecto (1)
    [InlineData(99999, 3600)]// por encima del máximo → se acota a 3600
    public void Guardar_AcotaElRefrescoAlLeerlo(int guardado, int esperado)
    {
        var dir = DirTemporal();
        var store = new ParametrosOperacionStore(dir, ConfigVacia());
        try
        {
            store.Guardar(new OperacionConfig("Critico", "*/5 * * * *", guardado));
            store.Actual().RefrescoSegundos.Should().Be(esperado);
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Guardar_ConservaNivelYCronJuntoAlRefresco()
    {
        var dir = DirTemporal();
        var store = new ParametrosOperacionStore(dir, ConfigVacia());
        try
        {
            store.Guardar(new OperacionConfig("Alto", "0 * * * *", 5));
            var leido = store.Actual();
            leido.NivelMinimoCorreo.Should().Be("Alto");
            leido.CronExpression.Should().Be("0 * * * *");
            leido.RefrescoSegundos.Should().Be(5);
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }
}
