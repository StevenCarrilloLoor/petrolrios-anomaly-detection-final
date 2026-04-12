using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Firebird;
using PetrolRios.Infrastructure.Persistence;
using PetrolRios.Infrastructure.Services;

namespace PetrolRios.Api.Tests;

public class EtlOrchestratorTests : IDisposable
{
    private readonly PetrolRiosDbContext _dbContext;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEstacionRepository> _estacionRepoMock;
    private readonly Mock<IFirebirdSourceClientFactory> _clientFactoryMock;
    private readonly Mock<IFirebirdSourceClient> _firebirdClientMock;
    private readonly Mock<ILogger<EtlOrchestrator>> _loggerMock;
    private readonly EtlOrchestrator _sut;

    public EtlOrchestratorTests()
    {
        var options = new DbContextOptionsBuilder<PetrolRiosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PetrolRiosDbContext(options);

        _estacionRepoMock = new Mock<IEstacionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Estaciones).Returns(_estacionRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _clientFactoryMock = new Mock<IFirebirdSourceClientFactory>();
        _firebirdClientMock = new Mock<IFirebirdSourceClient>();
        _loggerMock = new Mock<ILogger<EtlOrchestrator>>();

        var firebirdOptions = Options.Create(new FirebirdOptions
        {
            Stations = new Dictionary<string, string>
            {
                ["EST-001"] = "User=SYSDBA;Password=masterkey;Database=test.fdb;ReadOnly=true",
                ["EST-002"] = "User=SYSDBA;Password=masterkey;Database=test2.fdb;ReadOnly=true"
            }
        });

        _sut = new EtlOrchestrator(
            _unitOfWorkMock.Object,
            _dbContext,
            _clientFactoryMock.Object,
            firebirdOptions,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithActiveStations_ExtractsAndPersistsData()
    {
        // Arrange
        var estaciones = CreateEstaciones("EST-001");
        _estacionRepoMock.Setup(r => r.GetActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(estaciones);
        _estacionRepoMock.Setup(r => r.GetWatermarkAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EstacionWatermark?)null);

        var facturas = new List<FacturaDto>
        {
            new() { SecuenciaDocumento = 1, FechaDocumento = DateTime.UtcNow, CodigoVendedor = "V001",
                     TipoDocumento = "FV", NumeroDocumento = "001-001-0001", TotalNeto = 100 }
        };

        SetupFirebirdClientWithData(facturas);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.EstacionesProcesadas.Should().Be(1);
        result.EstacionesConError.Should().Be(0);
        result.TransaccionesExtraidas.Should().Be(1);
        _dbContext.TransaccionesStaging.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStationFails_ContinuesWithOtherStations()
    {
        // Arrange
        var estaciones = CreateEstaciones("EST-001", "EST-002");
        _estacionRepoMock.Setup(r => r.GetActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(estaciones);
        _estacionRepoMock.Setup(r => r.GetWatermarkAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EstacionWatermark?)null);

        // Primera estación falla, segunda tiene datos
        var failClient = new Mock<IFirebirdSourceClient>();
        failClient.Setup(c => c.GetFacturasDesdeAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Firebird no disponible"));

        var successClient = new Mock<IFirebirdSourceClient>();
        SetupEmptyClient(successClient);
        successClient.Setup(c => c.GetFacturasDesdeAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FacturaDto>
            {
                new() { SecuenciaDocumento = 1, FechaDocumento = DateTime.UtcNow,
                         TipoDocumento = "FV", NumeroDocumento = "001", TotalNeto = 50 }
            });

        _clientFactoryMock.SetupSequence(f => f.Create(It.IsAny<string>()))
            .Returns(failClient.Object)
            .Returns(successClient.Object);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.EstacionesConError.Should().Be(1);
        result.EstacionesProcesadas.Should().Be(1);
        result.Errores.Should().ContainKey("EST-001");
    }

    [Fact]
    public async Task ExecuteAsync_WithNoActiveStations_ReturnsEmptyResult()
    {
        // Arrange
        _estacionRepoMock.Setup(r => r.GetActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Estacion>());

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.EstacionesProcesadas.Should().Be(0);
        result.TransaccionesExtraidas.Should().Be(0);
        result.EstacionesConError.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingConnectionString_RecordsError()
    {
        // Arrange: estación con código que NO está en FirebirdOptions
        var estaciones = CreateEstaciones("EST-999");
        _estacionRepoMock.Setup(r => r.GetActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(estaciones);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.EstacionesConError.Should().Be(1);
        result.Errores.Should().ContainKey("EST-999");
        result.Errores["EST-999"].Should().Contain("Connection string no configurada");
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesWatermarkAfterSuccessfulExtraction()
    {
        // Arrange
        var estaciones = CreateEstaciones("EST-001");
        _estacionRepoMock.Setup(r => r.GetActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(estaciones);
        _estacionRepoMock.Setup(r => r.GetWatermarkAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EstacionWatermark.Create(1, DateTime.UtcNow.AddHours(-2)));

        var facturas = new List<FacturaDto>
        {
            new() { SecuenciaDocumento = 1, FechaDocumento = DateTime.UtcNow.AddMinutes(-30),
                     TipoDocumento = "FV", NumeroDocumento = "001", TotalNeto = 100 },
            new() { SecuenciaDocumento = 2, FechaDocumento = DateTime.UtcNow.AddMinutes(-10),
                     TipoDocumento = "FV", NumeroDocumento = "002", TotalNeto = 200 }
        };

        SetupFirebirdClientWithData(facturas);

        // Act
        await _sut.ExecuteAsync();

        // Assert: verifica que se actualizó el watermark
        _estacionRepoMock.Verify(
            r => r.UpsertWatermarkAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyData_DoesNotUpdateWatermark()
    {
        // Arrange
        var estaciones = CreateEstaciones("EST-001");
        _estacionRepoMock.Setup(r => r.GetActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(estaciones);
        _estacionRepoMock.Setup(r => r.GetWatermarkAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EstacionWatermark.Create(1, DateTime.UtcNow.AddHours(-1)));

        SetupEmptyClient(_firebirdClientMock);
        _clientFactoryMock.Setup(f => f.Create(It.IsAny<string>()))
            .Returns(_firebirdClientMock.Object);

        // Act
        await _sut.ExecuteAsync();

        // Assert: no se actualiza watermark si no hay datos nuevos
        _estacionRepoMock.Verify(
            r => r.UpsertWatermarkAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static IReadOnlyList<Estacion> CreateEstaciones(params string[] codigos)
    {
        var estaciones = new List<Estacion>();
        for (var i = 0; i < codigos.Length; i++)
        {
            estaciones.Add(Estacion.Create($"Estacion {codigos[i]}", codigos[i], "Dirección test"));
        }
        return estaciones;
    }

    private void SetupFirebirdClientWithData(List<FacturaDto> facturas)
    {
        _firebirdClientMock.Setup(c => c.GetFacturasDesdeAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(facturas);
        SetupEmptyClient(_firebirdClientMock, skipFacturas: true);

        _clientFactoryMock.Setup(f => f.Create(It.IsAny<string>()))
            .Returns(_firebirdClientMock.Object);
    }

    private static void SetupEmptyClient(Mock<IFirebirdSourceClient> client, bool skipFacturas = false)
    {
        if (!skipFacturas)
            client.Setup(c => c.GetFacturasDesdeAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FacturaDto>());
        client.Setup(c => c.GetDetallesFacturaAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DetalleFacturaDto>());
        client.Setup(c => c.GetCierresTurnoAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CierreTurnoDto>());
        client.Setup(c => c.GetDepositosTurnoAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DepositoTurnoDto>());
        client.Setup(c => c.GetAnulacionesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AnulacionDto>());
        client.Setup(c => c.GetCreditosAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CreditoDto>());
        client.Setup(c => c.GetTarjetasTurnoAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TarjetaTurnoDto>());
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
