using System.Text.Json;
using FluentAssertions;
using PetrolRios.Application.RealTime;

namespace PetrolRios.Api.Tests;

/// <summary>
/// El fan-out en tiempo real serializa el push a JSON (pg_notify) y lo deserializa en la otra
/// instancia. Esta prueba garantiza que el evento, los grupos y el payload viajan intactos.
/// </summary>
public sealed class AlertaPushTests
{
    [Fact]
    public void AlertaPush_RoundTrip_PreservaEventoGruposYPayload()
    {
        var original = new AlertaPush(
            "NuevaAlerta",
            new[] { "auditores", "supervisores", "administradores" },
            new AlertaNotificacionPayload(
                "abc123", 7, "CashFraud", "Critico", "Auditoria",
                "Faltante de caja", 88.5, new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc), 3));

        var json = JsonSerializer.Serialize(original);
        var back = JsonSerializer.Deserialize<AlertaPush>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        back.Should().NotBeNull();
        back!.Evento.Should().Be("NuevaAlerta");
        back.Grupos.Should().BeEquivalentTo("auditores", "supervisores", "administradores");
        back.Payload.NotificationId.Should().Be("abc123");
        back.Payload.Id.Should().Be(7);
        back.Payload.TipoDetector.Should().Be("CashFraud");
        back.Payload.NivelRiesgo.Should().Be("Critico");
        back.Payload.Ambito.Should().Be("Auditoria");
        back.Payload.Score.Should().Be(88.5);
        back.Payload.EstacionId.Should().Be(3);
    }
}
