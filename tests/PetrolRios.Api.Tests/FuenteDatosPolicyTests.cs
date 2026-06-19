using FluentAssertions;
using PetrolRios.Application.Fuentes;

namespace PetrolRios.Api.Tests;

public class FuenteDatosPolicyTests
{
    [Theory]
    [InlineData("DATE")]
    [InlineData("TIME")]
    [InlineData("TIMESTAMP")]
    [InlineData("timestamp with time zone")]
    public void EsTipoTemporal_AceptaTiposDeCursorValidos(string tipo)
    {
        FuenteDatosPolicy.EsTipoTemporal(tipo).Should().BeTrue();
    }

    [Theory]
    [InlineData("INTEGER")]
    [InlineData("VARCHAR(20)")]
    [InlineData("DECIMAL/NUMERIC")]
    [InlineData("")]
    [InlineData(null)]
    public void EsTipoTemporal_RechazaColumnasQueNoRepresentanFecha(string? tipo)
    {
        FuenteDatosPolicy.EsTipoTemporal(tipo).Should().BeFalse();
    }

    [Fact]
    public void NormalizarCursorFirebird_ConservaTicksYSuprimeZonaHoraria()
    {
        var utc = new DateTime(2026, 6, 19, 8, 45, 30, DateTimeKind.Utc);

        var cursor = FuenteDatosPolicy.NormalizarCursorFirebird(utc);

        cursor.Ticks.Should().Be(utc.Ticks);
        cursor.Kind.Should().Be(DateTimeKind.Unspecified);
    }
}
