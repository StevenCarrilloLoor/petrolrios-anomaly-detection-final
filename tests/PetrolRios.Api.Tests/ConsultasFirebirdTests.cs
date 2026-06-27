using FluentAssertions;
using PetrolRios.Application.DTOs.Consultas;
using PetrolRios.Infrastructure.Services;

namespace PetrolRios.Api.Tests;

/// <summary>
/// Pruebas de la cola en memoria de consultas en vivo a Firebird: encolar → el agente toma la pendiente
/// (una sola vez) → responde → la interfaz lee el resultado. Sin BD.
/// </summary>
public sealed class ConsultasFirebirdTests
{
    private static SolicitudConsulta Solicitud(string est) =>
        new(est, "FV", null, null, "1790", 200);

    [Fact]
    public void Encolar_DejaLaConsultaPendiente()
    {
        var cola = new ConsultasFirebird();
        var id = cola.Encolar(Solicitud("EST-001"));

        var estado = cola.Obtener(id);
        estado.Should().NotBeNull();
        estado!.Estado.Should().Be("Pendiente");
        estado.ResultadoJson.Should().BeNull();
    }

    [Fact]
    public void TomarPendientes_DevuelveLaConsultaUnaSolaVez_YFiltraPorEstacion()
    {
        var cola = new ConsultasFirebird();
        var id = cola.Encolar(Solicitud("EST-001"));
        cola.Encolar(Solicitud("EST-002"));

        var deEst1 = cola.TomarPendientes("EST-001");
        deEst1.Should().ContainSingle();
        deEst1[0].Id.Should().Be(id);
        deEst1[0].TipoDocumento.Should().Be("FV");
        deEst1[0].Codigo.Should().Be("1790");

        // Segunda vez: ya tomada, no se reentrega.
        cola.TomarPendientes("EST-001").Should().BeEmpty();
    }

    [Fact]
    public void Responder_DejaElResultadoDisponibleParaLaInterfaz()
    {
        var cola = new ConsultasFirebird();
        var id = cola.Encolar(Solicitud("EST-001"));
        cola.TomarPendientes("EST-001");

        cola.Responder(id, ok: true, resultadoJson: "{\"documentos\":[],\"total\":0}", error: null);

        var estado = cola.Obtener(id);
        estado!.Estado.Should().Be("Listo");
        estado.ResultadoJson.Should().Contain("\"total\":0");
        estado.Error.Should().BeNull();
    }

    [Fact]
    public void Responder_ConError_MarcaError()
    {
        var cola = new ConsultasFirebird();
        var id = cola.Encolar(Solicitud("EST-001"));

        cola.Responder(id, ok: false, resultadoJson: null, error: "Firebird no responde");

        var estado = cola.Obtener(id);
        estado!.Estado.Should().Be("Error");
        estado.Error.Should().Be("Firebird no responde");
    }

    [Fact]
    public void Obtener_DeUnIdInexistente_EsNull()
    {
        new ConsultasFirebird().Obtener("no-existe").Should().BeNull();
    }
}
