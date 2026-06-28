using FluentAssertions;
using PetrolRios.Infrastructure.Services.Precios;

namespace PetrolRios.Api.Tests;

/// <summary>Horario adaptativo: normal en días 1–10 a las 08:00; alerta del 11 14:00 al 12 14:00; resto inactivo.</summary>
public sealed class PlanificadorPreciosTests
{
    private static DateTime Ec(int dia, int hora) => new(2026, 6, dia, hora, 0, 0);

    [Theory]
    [InlineData(1, 8)]
    [InlineData(5, 8)]
    [InlineData(10, 8)]
    public void Modo_DiasNormalesALas8_EsNormal(int dia, int hora) =>
        PlanificadorPrecios.Modo(Ec(dia, hora)).Should().Be(ModoScrapePrecios.Normal);

    [Theory]
    [InlineData(5, 7)]   // antes de las 8
    [InlineData(5, 9)]   // después de las 8
    [InlineData(20, 8)]  // día > 10
    [InlineData(13, 8)]  // tras el cambio, fuera de ventana
    public void Modo_FueraDeHoraOVentana_EsInactivo(int dia, int hora) =>
        PlanificadorPrecios.Modo(Ec(dia, hora)).Should().Be(ModoScrapePrecios.Inactivo);

    [Theory]
    [InlineData(11, 14)]  // inicio de la ventana
    [InlineData(11, 23)]
    [InlineData(12, 0)]
    [InlineData(12, 14)]  // fin de la ventana (inclusive)
    public void Modo_VentanaDelCambio_EsAlerta(int dia, int hora) =>
        PlanificadorPrecios.Modo(Ec(dia, hora)).Should().Be(ModoScrapePrecios.Alerta);

    [Theory]
    [InlineData(11, 13)]  // antes de las 14:00 del 11
    [InlineData(12, 15)]  // después de las 14:00 del 12
    public void Modo_BordesDeLaVentana_SonInactivos(int dia, int hora) =>
        PlanificadorPrecios.Modo(Ec(dia, hora)).Should().Be(ModoScrapePrecios.Inactivo);
}
