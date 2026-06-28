using FluentAssertions;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;
using Xunit;

namespace PetrolRios.Domain.Tests;

/// <summary>
/// Alertas acumulables/escalables (idea de Steven para despachos rápidos): una alerta del mismo caso
/// crece por cantidad y escala el nivel (2–3 Medio, 4–5 Alto, 6+ Crítico), re-emergiendo como Nueva.
/// </summary>
public class AlertaAcumulacionTests
{
    [Theory]
    [InlineData(1, NivelRiesgo.Bajo)]
    [InlineData(2, NivelRiesgo.Medio)]
    [InlineData(3, NivelRiesgo.Medio)]
    [InlineData(4, NivelRiesgo.Alto)]
    [InlineData(5, NivelRiesgo.Alto)]
    [InlineData(6, NivelRiesgo.Critico)]
    [InlineData(12, NivelRiesgo.Critico)]
    public void EscalarPorConteo_SubeNivelPorCantidad(int conteo, NivelRiesgo esperado)
    {
        Alerta.EscalarPorConteo(conteo).Nivel.Should().Be(esperado);
    }

    [Fact]
    public void Acumular_SumaConteo_EscalaYReemergeComoNueva()
    {
        var (score0, nivel0) = Alerta.EscalarPorConteo(2);
        var a = Alerta.Create(TipoDetector.PaymentFraud, nivel0, "caso", score0, estacionId: 1,
            transaccionReferencia: "RAPIDOS-RUC-X-20260628", ambito: AmbitoAlerta.Ambos, eventosAcumulados: 2);
        a.Estado = EstadoAlerta.EnRevision;
        a.FechaResolucion = DateTime.UtcNow;

        var (score1, nivel1) = Alerta.EscalarPorConteo(a.EventosAcumulados + 4); // total 6 → Crítico
        a.Acumular(4, score1, nivel1, "caso (actualizado)", "{}", DateTime.UtcNow.AddMinutes(1));

        a.EventosAcumulados.Should().Be(6);
        a.NivelRiesgo.Should().Be(NivelRiesgo.Critico);
        a.Estado.Should().Be(EstadoAlerta.Nueva);   // re-emerge para revisión
        a.FechaResolucion.Should().BeNull();
        a.Descripcion.Should().Be("caso (actualizado)");
    }
}
