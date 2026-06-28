using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PetrolRios.Application.DTOs.Combustible;
using PetrolRios.Application.DTOs.Configuracion;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Persistence;
using PetrolRios.Infrastructure.Services;

namespace PetrolRios.Api.Tests;

/// <summary>
/// El servicio de precios sirve los valores guardados, hace upsert por producto y degrada con elegancia
/// cuando no hay fuente externa configurada (no rompe; devuelve lo guardado).
/// </summary>
public sealed class PreciosCombustibleServiceTests : IDisposable
{
    private readonly PetrolRiosDbContext _db;
    private readonly PreciosCombustibleService _sut;

    /// <summary>Conector externo deshabilitado (sin fuente configurada).</summary>
    private sealed class ProveedorDeshabilitado : IProveedorPreciosExterno
    {
        public bool Habilitado => false;
        public Task<IReadOnlyList<PrecioCombustibleExterno>?> ObtenerAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<PrecioCombustibleExterno>?>(null);
    }

    public PreciosCombustibleServiceTests()
    {
        var options = new DbContextOptionsBuilder<PetrolRiosDbContext>()
            .UseInMemoryDatabase($"Precios_{Guid.NewGuid()}")
            .Options;
        _db = new PetrolRiosDbContext(options);
        var desde = new DateTime(2026, 6, 12);
        var hasta = new DateTime(2026, 7, 11);
        // Se siembran desordenados a propósito para verificar el orden de salida.
        _db.PreciosCombustible.Add(PrecioCombustible.Create(TipoCombustible.Diesel, 3.25m, 1.602m, desde, hasta, "seed"));
        _db.PreciosCombustible.Add(PrecioCombustible.Create(TipoCombustible.Extra, 3.31m, 1.021m, desde, hasta, "seed"));
        _db.SaveChanges();
        var parametros = new Mock<IParametrosOperacion>();
        parametros.Setup(p => p.Actual()).Returns(new OperacionConfig("Critico", "*/5 * * * *", 1, "Auto"));
        _sut = new PreciosCombustibleService(
            _db, new ProveedorDeshabilitado(), parametros.Object, NullLogger<PreciosCombustibleService>.Instance);
    }

    [Fact]
    public async Task ObtenerVigentes_DevuelveOrdenadoPorProducto_ConMetadatos()
    {
        var r = await _sut.ObtenerVigentesAsync();

        r.Precios.Should().HaveCount(2);
        r.Precios[0].Producto.Should().Be("Extra");   // Extra(1) antes que Diesel(3)
        r.Precios[0].Nombre.Should().Be("Gasolina Extra");
        r.Precios[0].PrecioGalon.Should().Be(3.31m);
        r.Moneda.Should().Be("USD");
        r.Nota.Should().Contain("Súper");             // la nota aclara por qué se excluye la Súper
    }

    [Fact]
    public async Task Actualizar_HaceUpsert_DeProductoExistenteYNuevo()
    {
        // Actualiza Extra (existe) y crea Ecopais (no existía) → quedan 3.
        await _sut.ActualizarAsync(new ActualizarPrecioCombustibleRequest(
            "Extra", 3.40m, 1.05m, new DateTime(2026, 7, 12), new DateTime(2026, 8, 11), "decreto"));
        var r = await _sut.ActualizarAsync(new ActualizarPrecioCombustibleRequest(
            "Ecopais", 3.41m, 1.70m, new DateTime(2026, 7, 12), new DateTime(2026, 8, 11), null));

        r.Precios.Should().HaveCount(3);
        r.Precios.Single(p => p.Producto == "Extra").PrecioGalon.Should().Be(3.40m);
        r.Precios.Single(p => p.Producto == "Ecopais").PrecioGalon.Should().Be(3.41m);
        (await _db.PreciosCombustible.CountAsync()).Should().Be(3);
    }

    [Fact]
    public async Task Actualizar_ProductoInvalido_Lanza()
    {
        var act = () => _sut.ActualizarAsync(new ActualizarPrecioCombustibleRequest(
            "Querosene", 3.00m, 0m, DateTime.UtcNow, null, null));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Actualizar_PrecioFueraDeRango_Lanza()
    {
        // $99/gal en Extra está fuera del rango plausible ($1.50–$6.00) → rechazo.
        var act = () => _sut.ActualizarAsync(new ActualizarPrecioCombustibleRequest(
            "Extra", 99.00m, 0m, DateTime.UtcNow, null, null));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Actualizar_EscribeBitacoraDeAuditoria()
    {
        await _sut.ActualizarAsync(new ActualizarPrecioCombustibleRequest(
            "Diesel", 3.40m, 1.60m, new DateTime(2026, 7, 12), new DateTime(2026, 8, 11), null));

        var log = await _db.PreciosCombustibleLog.SingleAsync();
        log.Producto.Should().Be(TipoCombustible.Diesel);
        log.Fuente.Should().Be("admin_manual");
        log.Resultado.Should().Be("actualizado");
        log.PrecioAnterior.Should().Be(3.25m);
        log.PrecioNuevo.Should().Be(3.40m);
    }

    [Fact]
    public async Task Refrescar_SinFuenteExterna_DevuelveLosGuardados_SinError()
    {
        var r = await _sut.RefrescarDesdeFuenteAsync();

        r.Precios.Should().HaveCount(2);
        r.Precios.Select(p => p.Producto).Should().Contain(["Extra", "Diesel"]);
    }

    [Fact]
    public async Task Historial_DevuelveLaBitacoraTrasUnaActualizacion()
    {
        await _sut.ActualizarAsync(new ActualizarPrecioCombustibleRequest(
            "Diesel", 3.40m, 1.60m, new DateTime(2026, 7, 12), new DateTime(2026, 8, 11), null));

        var hist = await _sut.ObtenerHistorialAsync(12);

        hist.Should().ContainSingle();
        hist[0].Producto.Should().Be("Diesel");
        hist[0].Resultado.Should().Be("actualizado");
    }

    [Fact]
    public async Task Salud_SinErroresNiDegradadas_EsOk()
    {
        var salud = await _sut.ObtenerSaludAsync();

        salud.Estado.Should().Be("OK");
        salud.FuentesDegradadas.Should().BeEmpty();
        salud.ModoSchedule.Should().BeOneOf("Normal", "Alerta", "Inactivo");
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }
}
