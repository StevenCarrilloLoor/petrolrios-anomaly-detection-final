using FluentAssertions;
using PetrolRios.StationMonitor.Models;
using PetrolRios.StationMonitor.Services;

namespace PetrolRios.StationMonitor.Tests;

public sealed class MonitorStateTests
{
    private static readonly UsuarioCentral Identity = new(
        2,
        "agent-est-001@petrolrios.com",
        "Agente EST-001",
        "Auditor",
        1,
        "EST-001",
        "Estación Centro");

    [Fact]
    public void RegistrarExito_SameSnapshot_DoesNotSpamActivity()
    {
        var state = new MonitorState();
        var problems = new[] { Problem(10) };

        state.RegistrarExito(Identity, problems);
        state.RegistrarExito(Identity, problems);
        state.RegistrarExito(Identity, problems);

        var snapshot = state.Snapshot(true, "EST-001");
        snapshot.Eventos.Should().ContainSingle();
        snapshot.Eventos[0].Nivel.Should().Be("OK");
    }

    [Fact]
    public void RegistrarExito_NewProblem_RegistersAlertEvent()
    {
        var state = new MonitorState();
        state.RegistrarExito(Identity, new[] { Problem(10) });

        state.RegistrarExito(Identity, new[] { Problem(11), Problem(10) });

        var snapshot = state.Snapshot(true, "EST-001");
        snapshot.Problemas.Select(p => p.Id).Should().Equal(11, 10);
        snapshot.Eventos[0].Nivel.Should().Be("ALERTA");
        snapshot.Eventos[0].Mensaje.Should().Contain("1 problema");
    }

    private static ProblemaOperativo Problem(int id) => new(
        id,
        "CashFraud",
        "Alto",
        "Operativa",
        "Nueva",
        $"Problema {id}",
        70,
        DateTime.UtcNow.AddSeconds(id),
        "EMP-001",
        $"REF-{id}",
        1,
        "Estación Centro",
        null);
}
