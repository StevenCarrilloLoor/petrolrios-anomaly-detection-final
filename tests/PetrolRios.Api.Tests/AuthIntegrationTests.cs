using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PetrolRios.Application.DTOs.Auth;
using PetrolRios.Application.DTOs.Usuarios;

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

    [DockerAvailableFact]
    public async Task CreatedUser_IsAutoVerified_AndCanLoginImmediately()
    {
        // Arrange — el Administrador da de alta una cuenta nueva
        var adminToken = await LoginAndGetTokenAsync();
        var email = $"nuevo_{Guid.NewGuid():N}@petrolrios.com";
        const string password = "Prueba123!";

        var crear = new HttpRequestMessage(HttpMethod.Post, "/api/v1/usuarios")
        {
            Content = JsonContent.Create(new
            {
                Email = email,
                NombreCompleto = "Usuario de Prueba",
                Password = password,
                RolId = 1 // Auditor (primer rol sembrado)
            })
        };
        crear.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act — crear la cuenta y luego iniciar sesión de inmediato (sin verificar el correo)
        var crearResp = await _client.SendAsync(crear);
        crearResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var creado = await crearResp.Content.ReadFromJsonAsync<UsuarioResponse>();

        var loginResp = await _client.PostAsJsonAsync(
            "/api/v1/auth/login", new { Email = email, Password = password });

        // Assert — la cuenta queda verificada en el alta y puede entrar sin fricción
        creado.Should().NotBeNull();
        creado!.EmailVerificado.Should().BeTrue();
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();
        login!.Token.Should().NotBeNullOrWhiteSpace();
        login.Usuario.Email.Should().Be(email);
    }

    private async Task<string> LoginAndGetTokenAsync()
    {
        var request = new { Email = "admin@petrolrios.com", Password = "Admin123!" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }
}
