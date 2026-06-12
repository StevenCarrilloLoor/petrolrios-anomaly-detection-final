using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PetrolRios.Application.DTOs.Auth;

namespace PetrolRios.Api.Tests;

/// <summary>
/// Tests de integración para autenticación y autorización JWT.
/// Usa PostgreSQL real vía Testcontainers.
/// </summary>
[Collection("Integracion")]
public sealed class AuthIntegrationTests
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(PetrolRiosWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [DockerAvailableFact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var request = new { Email = "admin@petrolrios.com", Password = "Admin123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.Usuario.Email.Should().Be("admin@petrolrios.com");
        body.Usuario.Rol.Should().Be("Administrador");
    }

    [DockerAvailableFact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var request = new { Email = "admin@petrolrios.com", Password = "WrongPassword!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [DockerAvailableFact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var request = new { Email = "noexiste@petrolrios.com", Password = "Admin123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [DockerAvailableFact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Act — intentar acceder al dashboard sin JWT
        var response = await _client.GetAsync("/api/v1/dashboard/kpis");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [DockerAvailableFact]
    public async Task ProtectedEndpoint_WithValidToken_ReturnsOk()
    {
        // Arrange — autenticarse primero
        var token = await LoginAndGetTokenAsync();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/dashboard/kpis");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [DockerAvailableFact]
    public async Task Refresh_WithValidToken_ReturnsNewToken()
    {
        // Arrange — autenticarse para obtener refresh token
        var loginRequest = new { Email = "admin@petrolrios.com", Password = "Admin123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Act — usar refresh token
        var refreshRequest = new { RefreshToken = loginBody!.RefreshToken };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrWhiteSpace();
    }

    private async Task<string> LoginAndGetTokenAsync()
    {
        var request = new { Email = "admin@petrolrios.com", Password = "Admin123!" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }
}
