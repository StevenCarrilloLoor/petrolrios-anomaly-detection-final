using FluentAssertions;

namespace PetrolRios.Api.Tests;

public class HealthCheckTests
{
    [Fact]
    public void PlaceholderTest_Pass()
    {
        true.Should().BeTrue();
    }
}
