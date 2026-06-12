using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetrolRios.Application.Interfaces;
using PetrolRios.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace PetrolRios.Api.Tests;

/// <summary>
/// Factory de integración que usa Testcontainers para PostgreSQL real.
/// Reemplaza las conexiones a Firebird con mocks (no tenemos bases Firebird en CI).
/// </summary>
public sealed class PetrolRiosWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("petrolrios_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Agregar configuración mínima para que Program.cs no falle
            // por falta de Jwt:SecretKey o ConnectionStrings
            var testConfig = new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSQL"] = _postgres.GetConnectionString(),
                ["ConnectionStrings:Hangfire"] = _postgres.GetConnectionString(),
                ["Jwt:SecretKey"] = "TestingSecretKey_AtLeast32Characters_Long_For_Tests!",
                ["Jwt:Issuer"] = "PetrolRios.Api.Tests",
                ["Jwt:Audience"] = "PetrolRios.Tests",
                ["Jwt:ExpirationMinutes"] = "60",
                ["Jwt:RefreshExpirationDays"] = "7",
                ["FirebirdStations:Stations:EST-001"] = "User=SYSDBA;Password=masterkey;Database=test.fdb;ReadOnly=true"
            };

            config.AddInMemoryCollection(testConfig);
        });

        builder.ConfigureServices(services =>
        {
            // Mock Firebird factory (no tenemos Firebird en CI)
            var mockFactory = new Mock<IFirebirdSourceClientFactory>();
            var mockClient = new Mock<IFirebirdSourceClient>();

            // Devolver listas vacías para todos los métodos de Firebird
            mockClient.Setup(c => c.GetFacturasDesdeAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            mockClient.Setup(c => c.GetDetallesFacturaAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            mockClient.Setup(c => c.GetCierresTurnoAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            mockClient.Setup(c => c.GetDepositosTurnoAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            mockClient.Setup(c => c.GetAnulacionesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            mockClient.Setup(c => c.GetCreditosAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            mockClient.Setup(c => c.GetTarjetasTurnoAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            mockFactory.Setup(f => f.Create(It.IsAny<string>())).Returns(mockClient.Object);

            // Reemplazar registración de Firebird
            var firebirdDescriptors = services
                .Where(d => d.ServiceType == typeof(IFirebirdSourceClientFactory))
                .ToList();
            foreach (var d in firebirdDescriptors)
                services.Remove(d);
            services.AddSingleton(mockFactory.Object);
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Con minimal hosting, appsettings.{Environment}.json tiene prioridad sobre
        // ConfigureAppConfiguration del factory. Las variables de entorno se cargan
        // DESPUÉS de los json, así que son la vía fiable para inyectar el
        // connection string del Testcontainer (la paralelización está deshabilitada
        // en AssemblyInfo.cs para evitar carreras entre fixtures).
        Environment.SetEnvironmentVariable("ConnectionStrings__PostgreSQL", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__Hangfire", _postgres.GetConnectionString());
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
