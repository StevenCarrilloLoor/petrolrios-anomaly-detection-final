using FluentAssertions;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Domain.Tests;

public class NivelRiesgoTests
{
    [Theory]
    [InlineData(NivelRiesgo.Bajo, 1)]
    [InlineData(NivelRiesgo.Medio, 2)]
    [InlineData(NivelRiesgo.Alto, 3)]
    [InlineData(NivelRiesgo.Critico, 4)]
    public void NivelRiesgo_DebeContenerValoresEsperados(NivelRiesgo nivel, int expected)
    {
        ((int)nivel).Should().Be(expected);
    }
}
