using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using PetrolRios.Application.DTOs.Auth;

namespace PetrolRios.Api.Tests;

/// <summary>
/// Tests de integración para los endpoints de Dashboard y Alertas.
/// </summary>
[Collection("Integracion")]
public sealed class ApiIntegrationTests
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(PetrolRiosWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [DockerAvailableFact]
    public async Task Dashboard_Kpis_ReturnsOkWithData()
    {
        // Arrange
        var token = await LoginAndGetTokenAsync();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/dashboard/kpis");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrWhiteSpace();
    }

    [DockerAvailableFact]
    public async Task Dashboard_AlertasPorTipo_ReturnsOk()
    {
        // Arrange
        var token = await LoginAndGetTokenAsync();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/dashboard/alertas-por-tipo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [DockerAvailableFact]
    public async Task Dashboard_AlertasPorEstacion_ReturnsOk()
    {
        // Arrange
        var token = await LoginAndGetTokenAsync();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/dashboard/alertas-por-estacion");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [DockerAvailableFact]
    public async Task Alertas_GetAll_ReturnsOkPaginated()
    {
        // Arrange
        var token = await LoginAndGetTokenAsync();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/alertas?page=1&pageSize=10");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("items");
    }

    [DockerAvailableFact]
    public async Task Alertas_GetById_NonExistent_ReturnsNotFound()
    {
        // Arrange
        var token = await LoginAndGetTokenAsync();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/alertas/99999");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [DockerAvailableFact]
    public async Task Alertas_WithFilters_ReturnsOk()
    {
        // Arrange
        var token = await LoginAndGetTokenAsync();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get,
            "/api/v1/alertas?tipo=CashFraud&nivelRiesgo=Alto&page=1&pageSize=5");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [DockerAvailableFact]
    public async Task ReglasPersonalizadas_NombreDemasiadoLargo_DevuelveBadRequestNo500()
    {
        // Arrange: nombre de 200 caracteres (la columna admite 150). Regresión del bug
        // hallado en stress-test: antes provocaba un 500 al guardar; debe ser un 400 limpio.
        var token = await LoginAndGetTokenAsync();
        var regla = new
        {
            Nombre = new string('X', 200),
            Descripcion = "Regla de prueba de longitud",
            FuenteDatos = "Factura",
            Condiciones = new[] { new { Campo = "TotalNeto", Operador = ">", Valor = "1" } },
            CombinadorCondiciones = "Y",
            RiesgoBase = 50.0,
            Ambito = "Auditoria",
            Activa = true,
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/reglas-personalizadas")
        {
            Content = JsonContent.Create(regla),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("150");
    }

    [DockerAvailableFact]
    public async Task Operacion_CronInvalido_DevuelveBadRequestNo500()
    {
        // Arrange: un cron con valores fuera de rango (minuto 99) — antes podia tirar 500 al re-registrar
        // el job; ahora se valida y debe devolver un 400 limpio sin romper el job vigente.
        var token = await LoginAndGetTokenAsync();
        var malo = new { NivelMinimoCorreo = "Critico", CronExpression = "99 99 99 99 99" };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/v1/operacion")
        {
            Content = JsonContent.Create(malo),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    private async Task<string> LoginAndGetTokenAsync()
    {
        var loginRequest = new { Email = "admin@petrolrios.com", Password = "Admin123!" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }
}
