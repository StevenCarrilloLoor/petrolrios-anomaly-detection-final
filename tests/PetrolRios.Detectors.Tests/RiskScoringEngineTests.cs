using FluentAssertions;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Tests;

public class RiskScoringEngineTests
{
    private readonly RiskScoringEngine _sut = new();

    [Theory]
    [InlineData(20, NivelRiesgo.Bajo)]
    [InlineData(40, NivelRiesgo.Medio)]
    [InlineData(60, NivelRiesgo.Alto)]
    [InlineData(85, NivelRiesgo.Critico)]
    public void Calculate_WithBaseRiskOnly_ReturnsCorrectLevel(double riesgoBase, NivelRiesgo expectedNivel)
    {
        var (score, nivel) = _sut.Calculate(riesgoBase);
        nivel.Should().Be(expectedNivel);
        score.Should().Be(riesgoBase);
    }

    [Fact]
    public void Calculate_WithHighAmount_IncreasesScore()
    {
        var (scoreBase, _) = _sut.Calculate(40);
        var (scoreHigh, _) = _sut.Calculate(40, montoInvolucrado: 2000);
        scoreHigh.Should().BeGreaterThan(scoreBase);
    }

    [Fact]
    public void Calculate_WithReincidencia_IncreasesScore()
    {
        var (scoreBase, _) = _sut.Calculate(40);
        var (scoreReinc, _) = _sut.Calculate(40, reincidenciasEmpleado: 5);
        scoreReinc.Should().BeGreaterThan(scoreBase);
    }

    [Fact]
    public void Calculate_ScoreNeverExceeds100()
    {
        var (score, nivel) = _sut.Calculate(100, montoInvolucrado: 5000, reincidenciasEmpleado: 10, alertasHistoricasEstacion: 50);
        score.Should().BeLessThanOrEqualTo(100);
        nivel.Should().Be(NivelRiesgo.Critico);
    }

    [Fact]
    public void Calculate_WithZeroBase_ReturnsZero()
    {
        var (score, nivel) = _sut.Calculate(0, montoInvolucrado: 1000);
        score.Should().Be(0);
        nivel.Should().Be(NivelRiesgo.Bajo);
    }
}
