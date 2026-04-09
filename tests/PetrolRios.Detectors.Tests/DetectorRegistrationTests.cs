using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PetrolRios.Detectors;

namespace PetrolRios.Detectors.Tests;

public class DetectorRegistrationTests
{
    [Fact]
    public void AddDetectors_NoDebeArrojarExcepcion()
    {
        var services = new ServiceCollection();

        var act = () => services.AddDetectors();

        act.Should().NotThrow();
    }
}
