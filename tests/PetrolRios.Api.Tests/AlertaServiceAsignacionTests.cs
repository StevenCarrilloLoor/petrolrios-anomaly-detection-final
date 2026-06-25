using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PetrolRios.Application.DTOs.Alertas;
using PetrolRios.Application.Interfaces;
using PetrolRios.Application.RealTime;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Persistence;
using PetrolRios.Infrastructure.Services;

namespace PetrolRios.Api.Tests;

/// <summary>
/// Mejora de asignación de alertas: al asignar se registra QUIÉN asignó (AsignadoPor), la alerta
/// pasa a En Revisión, la respuesta trae los datos de asignación (a quién, por quién, cuándo) y se
/// avisa al asignado por correo y por SignalR ("AlertaAsignada"). Usa EF InMemory (sin Docker) y
/// fakes (Moq) para el correo y el broadcaster.
/// </summary>
public sealed class AlertaServiceAsignacionTests
{
    private static PetrolRiosDbContext NuevoContexto() =>
        new(new DbContextOptionsBuilder<PetrolRiosDbContext>()
            .UseInMemoryDatabase($"Asign_{Guid.NewGuid()}")
            .Options);

    private sealed record Semilla(int AlertaId, int AuditorId, int SupervisorId, string AuditorEmail, string AuditorNombre, string SupervisorNombre);

    private static async Task<Semilla> SembrarAsync(PetrolRiosDbContext db)
    {
        var rolAuditor = Rol.Create("Auditor");
        var rolSupervisor = Rol.Create("Supervisor");
        db.Roles.AddRange(rolAuditor, rolSupervisor);
        await db.SaveChangesAsync();

        var estacion = Estacion.Create("Estación Centro", "EST-001", "Av. Principal");
        db.Estaciones.Add(estacion);
        await db.SaveChangesAsync();

        var auditor = Usuario.Create("auditor@petrolrios.com", "María Fernanda Auditora", "hash", rolAuditor.Id);
        var supervisor = Usuario.Create("supervisor@petrolrios.com", "Carlos Supervisor", "hash", rolSupervisor.Id);
        db.Usuarios.AddRange(auditor, supervisor);
        await db.SaveChangesAsync();

        var alerta = Alerta.Create(
            TipoDetector.CashFraud, NivelRiesgo.Critico, "Faltante de caja por $95", 84, estacion.Id,
            empleadoCodigo: "004");
        db.Alertas.Add(alerta);
        await db.SaveChangesAsync();

        return new Semilla(alerta.Id, auditor.Id, supervisor.Id, auditor.Email, auditor.NombreCompleto, supervisor.NombreCompleto);
    }

    private static (AlertaService Sut, Mock<IEmailNotificacionService> Email, Mock<IAlertaBroadcaster> Broadcaster)
        CrearServicio(PetrolRiosDbContext db, bool correoHabilitado = true)
    {
        var email = new Mock<IEmailNotificacionService>();
        email.SetupGet(e => e.Habilitado).Returns(correoHabilitado);
        email.Setup(e => e.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var broadcaster = new Mock<IAlertaBroadcaster>();
        broadcaster.Setup(b => b.PublicarAsync(It.IsAny<AlertaPush>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new AlertaService(
            Mock.Of<IUnitOfWork>(), db, new EmpleadoDirectorio(db), email.Object, broadcaster.Object);
        return (sut, email, broadcaster);
    }

    [Fact]
    public async Task AsignarAsync_RegistraAsignadoPor_PoneEnRevision_YDevuelveDatosDeAsignacion()
    {
        await using var db = NuevoContexto();
        var s = await SembrarAsync(db);
        var (sut, _, _) = CrearServicio(db);

        var resp = await sut.AsignarAsync(s.AlertaId, new AsignarAlertaRequest(s.AuditorId), s.SupervisorId);

        // La respuesta trae a quién, por quién y cuándo (lo que el detalle/lista deben mostrar).
        resp.Estado.Should().Be("EnRevision");
        resp.AsignadoAId.Should().Be(s.AuditorId);
        resp.AsignadoANombre.Should().Be(s.AuditorNombre);
        resp.AsignadoARol.Should().Be("Auditor");
        resp.AsignadoPorNombre.Should().Be(s.SupervisorNombre);
        resp.FechaAsignacion.Should().NotBeNull();

        // Persistencia: una asignación con auditor y quién la asignó; la alerta quedó En Revisión.
        var asignacion = await db.AsignacionesAlerta.SingleAsync();
        asignacion.UsuarioId.Should().Be(s.AuditorId);
        asignacion.AsignadoPorId.Should().Be(s.SupervisorId);
        (await db.Alertas.FindAsync(s.AlertaId))!.Estado.Should().Be(EstadoAlerta.EnRevision);
    }

    [Fact]
    public async Task AsignarAsync_EnviaCorreoAlAsignado_YEmiteEventoSignalR()
    {
        await using var db = NuevoContexto();
        var s = await SembrarAsync(db);
        var (sut, email, broadcaster) = CrearServicio(db);

        IReadOnlyList<string>? destinatarios = null;
        email.Setup(e => e.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, IReadOnlyList<string>, CancellationToken>((_, _, dest, _) => destinatarios = dest)
            .Returns(Task.CompletedTask);

        AlertaPush? push = null;
        broadcaster.Setup(b => b.PublicarAsync(It.IsAny<AlertaPush>(), It.IsAny<CancellationToken>()))
            .Callback<AlertaPush, CancellationToken>((p, _) => push = p)
            .Returns(Task.CompletedTask);

        await sut.AsignarAsync(s.AlertaId, new AsignarAlertaRequest(s.AuditorId), s.SupervisorId);

        // Correo dirigido SOLO al asignado.
        destinatarios.Should().NotBeNull();
        destinatarios!.Should().ContainSingle().Which.Should().Be(s.AuditorEmail);

        // Evento de tiempo real con el id del asignado para que el frontend lo personalice.
        push.Should().NotBeNull();
        push!.Evento.Should().Be("AlertaAsignada");
        push.Grupos.Should().Contain(new[] { "auditores", "supervisores", "administradores" });
        push.Payload.AsignadoAId.Should().Be(s.AuditorId);
        push.Payload.AsignadoANombre.Should().Be(s.AuditorNombre);
    }

    [Fact]
    public async Task AsignarAsync_SinCorreoHabilitado_NoEnviaPeroSiAsignaYEmite()
    {
        await using var db = NuevoContexto();
        var s = await SembrarAsync(db);
        var (sut, email, broadcaster) = CrearServicio(db, correoHabilitado: false);

        var resp = await sut.AsignarAsync(s.AlertaId, new AsignarAlertaRequest(s.AuditorId), s.SupervisorId);

        resp.AsignadoAId.Should().Be(s.AuditorId);
        email.Verify(e => e.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Never);
        broadcaster.Verify(b => b.PublicarAsync(It.IsAny<AlertaPush>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AsignarAsync_Reasignar_LaUltimaAsignacionEsLaQueSeMuestra()
    {
        await using var db = NuevoContexto();
        var s = await SembrarAsync(db);

        // Segundo auditor para reasignar.
        var rolAuditorId = (await db.Usuarios.FindAsync(s.AuditorId))!.RolId;
        var auditor2 = Usuario.Create("auditor2@petrolrios.com", "Pedro Segundo", "hash", rolAuditorId);
        db.Usuarios.Add(auditor2);
        await db.SaveChangesAsync();

        var (sut, _, _) = CrearServicio(db);

        await sut.AsignarAsync(s.AlertaId, new AsignarAlertaRequest(s.AuditorId), s.SupervisorId);
        var resp = await sut.AsignarAsync(s.AlertaId, new AsignarAlertaRequest(auditor2.Id), s.SupervisorId);

        // La alerta muestra al ÚLTIMO asignado; quedan las dos asignaciones en el historial.
        resp.AsignadoAId.Should().Be(auditor2.Id);
        resp.AsignadoANombre.Should().Be("Pedro Segundo");
        (await db.AsignacionesAlerta.CountAsync(a => a.AlertaId == s.AlertaId)).Should().Be(2);
    }
}
