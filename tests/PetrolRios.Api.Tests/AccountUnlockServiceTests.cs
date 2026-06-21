using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Persistence;
using PetrolRios.Infrastructure.Services;

namespace PetrolRios.Api.Tests;

/// <summary>
/// Pruebas del desbloqueo de cuenta por correo (autoservicio desde el login):
/// el servicio de tokens en memoria y su cableado en <see cref="AuthService"/>.
/// </summary>
public class AccountUnlockServiceTests
{
    // ─── Servicio de tokens en memoria (un solo uso, caducan) ───

    [Fact]
    public void Crear_DevuelveTokenQueValidaAlMismoUsuario()
    {
        var sut = new AccountUnlockService();

        var token = sut.Crear(usuarioId: 42);

        token.Should().NotBeNullOrWhiteSpace();
        sut.Validar(token).Should().Be(42);
    }

    [Fact]
    public void Validar_TokenInexistente_DevuelveNull()
    {
        var sut = new AccountUnlockService();
        sut.Validar("token-que-no-existe").Should().BeNull();
    }

    [Fact]
    public void Consumir_ImpideReusoDelToken()
    {
        var sut = new AccountUnlockService();
        var token = sut.Crear(7);

        sut.Consumir(token);

        sut.Validar(token).Should().BeNull("el token es de un solo uso");
    }

    [Fact]
    public void Crear_GeneraTokensDistintosPorSolicitud()
    {
        var sut = new AccountUnlockService();
        var t1 = sut.Crear(1);
        var t2 = sut.Crear(1);
        t1.Should().NotBe(t2);
    }

    // ─── Cableado en AuthService (EF InMemory + correo simulado) ───

    [Fact]
    public async Task DesbloquearCuenta_ConTokenValido_LevantaElBloqueo()
    {
        var (sut, db, unlock, _) = CrearAuthService();
        var u = await SembrarUsuarioBloqueadoAsync(db);
        u.EstaBloqueado().Should().BeTrue("se sembró una cuenta bloqueada");
        var token = unlock.Crear(u.Id);

        var ok = await sut.DesbloquearCuentaAsync(token);

        ok.Should().BeTrue();
        u.EstaBloqueado().Should().BeFalse("el desbloqueo levanta el bloqueo");
        unlock.Validar(token).Should().BeNull("el token se consume tras usarse");
    }

    [Fact]
    public async Task DesbloquearCuenta_ConTokenInvalido_DevuelveFalse()
    {
        var (sut, _, _, _) = CrearAuthService();
        (await sut.DesbloquearCuentaAsync("token-que-no-existe")).Should().BeFalse();
    }

    [Fact]
    public async Task SolicitarDesbloqueo_SoloEnviaCorreoSiLaCuentaEstaBloqueada()
    {
        var (sut, db, _, email) = CrearAuthService();

        // Cuenta sin bloqueo: no hay nada que desbloquear → no se envía correo.
        var normal = Usuario.Create("normal@petrolrios.com", "Usuario Normal", "hash", 1);
        db.Usuarios.Add(normal);
        await db.SaveChangesAsync();
        await sut.SolicitarDesbloqueoAsync("normal@petrolrios.com");
        email.Verify(e => e.EnviarAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Never);

        // Cuenta bloqueada: se envía exactamente un correo con el enlace de desbloqueo.
        await SembrarUsuarioBloqueadoAsync(db);
        await sut.SolicitarDesbloqueoAsync("bloqueado@petrolrios.com");
        email.Verify(e => e.EnviarAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─── Helpers ───

    private static (AuthService sut, PetrolRiosDbContext db, AccountUnlockService unlock, Mock<IEmailNotificacionService> email) CrearAuthService()
    {
        var options = new DbContextOptionsBuilder<PetrolRiosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var db = new PetrolRiosDbContext(options);

        var unlock = new AccountUnlockService();
        var email = new Mock<IEmailNotificacionService>();
        email.SetupGet(e => e.Habilitado).Returns(true);

        var sut = new AuthService(
            db,
            Mock.Of<IUnitOfWork>(),
            Mock.Of<IJwtService>(),
            Mock.Of<ITotpService>(),
            new QrLoginService(),
            new PasswordResetService(),
            unlock,
            email.Object,
            new ConfigurationBuilder().Build());

        return (sut, db, unlock, email);
    }

    private static async Task<Usuario> SembrarUsuarioBloqueadoAsync(PetrolRiosDbContext db)
    {
        var u = Usuario.Create("bloqueado@petrolrios.com", "Usuario Bloqueado", "hash", 1);
        u.RegistrarFalloLogin(maxIntentos: 1, minutosBloqueo: 15); // un fallo basta para bloquear
        db.Usuarios.Add(u);
        await db.SaveChangesAsync();
        return u;
    }
}
