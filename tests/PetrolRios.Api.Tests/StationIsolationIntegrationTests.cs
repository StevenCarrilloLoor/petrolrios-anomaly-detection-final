using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PetrolRios.Application.DTOs.Alertas;
using PetrolRios.Application.DTOs.Auth;
using PetrolRios.Application.Security;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Api.Tests;

[Collection("Integracion")]
public sealed class StationIsolationIntegrationTests
{
    private readonly PetrolRiosWebApplicationFactory _factory;

    public StationIsolationIntegrationTests(PetrolRiosWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [DockerAvailableFact]
    public async Task StationAccount_TokenAndEndpoints_AreScopedToAssignedStation()
    {
        using var client = _factory.CreateClient();
        var token = await LoginStationAsync(client);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Claims.Single(c => c.Type == PetrolRiosClaimTypes.EstacionId)
            .Value.Should().Be("1");

        var ownOperational = Alerta.Create(
            TipoDetector.CashFraud,
            NivelRiesgo.Alto,
            "EVIDENCIA TEST - turno sin cerrar EST-001",
            70,
            1,
            ambito: AmbitoAlerta.Operativa);
        var otherOperational = Alerta.Create(
            TipoDetector.InvoiceAnomaly,
            NivelRiesgo.Alto,
            "EVIDENCIA TEST - despacho abierto EST-002",
            68,
            2,
            ambito: AmbitoAlerta.Operativa);
        var ownAudit = Alerta.Create(
            TipoDetector.PaymentFraud,
            NivelRiesgo.Critico,
            "EVIDENCIA TEST - auditoría EST-001",
            90,
            1,
            ambito: AmbitoAlerta.Auditoria);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PetrolRiosDbContext>();
            db.Alertas.AddRange(ownOperational, otherOperational, ownAudit);
            await db.SaveChangesAsync();
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var dashboard = await client.GetAsync("/api/v1/dashboard/kpis");
        dashboard.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var allAlerts = await client.GetAsync("/api/v1/alertas");
        allAlerts.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var response = await client.GetAsync(
            "/api/v1/alertas/problemas-estacion?estacionId=2&dias=30&soloActivos=true");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var groups = await response.Content.ReadFromJsonAsync<List<ProblemaEstacionGrupo>>();
        groups.Should().NotBeNull();
        var problems = groups!.SelectMany(g => g.Problemas).ToList();
        problems.Should().Contain(p => p.Id == ownOperational.Id);
        problems.Should().NotContain(p => p.Id == otherOperational.Id);
        problems.Should().NotContain(p => p.Id == ownAudit.Id);

        (await client.GetAsync($"/api/v1/alertas/{ownOperational.Id}"))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync($"/api/v1/alertas/{otherOperational.Id}"))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.GetAsync($"/api/v1/alertas/{ownAudit.Id}"))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [DockerAvailableFact]
    public async Task StationAgent_CannotReportAnotherStation()
    {
        using var client = _factory.CreateClient();
        var token = await LoginStationAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync(
            "/api/v1/ingesta/heartbeat",
            new
            {
                CodigoEstacion = "EST-002",
                NombreEstacion = "Intento cruzado",
                VersionAgente = "test"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static async Task<string> LoginStationAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { Email = "agent-est-001@petrolrios.com", Password = "Agent123!" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        login.Should().NotBeNull();
        login!.Usuario.EstacionId.Should().Be(1);
        login.Usuario.EstacionCodigo.Should().Be("EST-001");
        return login.Token;
    }
}
