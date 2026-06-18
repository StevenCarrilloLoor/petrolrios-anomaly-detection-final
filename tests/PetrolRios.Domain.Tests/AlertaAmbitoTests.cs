using FluentAssertions;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Domain.Tests;

/// <summary>Pruebas del carril (ámbito) de una alerta: Operativa vs Auditoría.</summary>
public class AlertaAmbitoTests
{
    [Fact]
    public void Create_SinAmbito_EsAuditoriaPorDefecto()
    {
        var alerta = Alerta.Create(
            TipoDetector.PaymentFraud, NivelRiesgo.Alto, "desc", 70, estacionId: 1);
        alerta.Ambito.Should().Be(AmbitoAlerta.Auditoria);
    }

    [Fact]
    public void Create_ConAmbitoOperativa_LoConserva()
    {
        var alerta = Alerta.Create(
            TipoDetector.InvoiceAnomaly, NivelRiesgo.Bajo, "campo faltante", 20,
            estacionId: 1, ambito: AmbitoAlerta.Operativa);
        alerta.Ambito.Should().Be(AmbitoAlerta.Operativa);
    }
}
