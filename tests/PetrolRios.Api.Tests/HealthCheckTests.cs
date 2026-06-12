using System.Net;
using FluentAssertions;

namespace PetrolRios.Api.Tests;

/// <summary>
/// Test básico de disponibilidad del API.
/// Tests de autenticación y endpoints completos están en AuthIntegrationTests y ApiIntegrationTests.
/// </summary>
[Collection("Integracion")]
public sealed class HealthCheckTests
{
    private readonly HttpClient _client;

    public HealthCheckTests(PetrolRiosWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [DockerAvailableFact]
    public async Task RootEndpoint_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("operativa");
    }
}
