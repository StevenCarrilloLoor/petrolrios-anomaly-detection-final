using FluentAssertions;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Detectors;
using Xunit;

namespace PetrolRios.Detectors.Tests;

/// <summary>
/// Pruebas del enriquecedor central de evidencia: refleja los campos identificables de la Fuente
/// (RUC, nº de documento, placa, cliente, turno, fecha, monto, forma de pago) hacia la evidencia, sin
/// pisar lo que ya puso la regla ni copiar vacíos/cero.
/// </summary>
public class EvidenciaEnriquecidaTests
{
    private static FacturaDto Factura() => new()
    {
        RucCliente = "1790012345001",
        NumeroDocumento = "001-001-000123",
        Placa = "PCA1234",
        CodigoCliente = "CLI060",
        NumeroTurno = 7,
        FechaDocumento = new DateTime(2026, 6, 26, 12, 0, 0),
        TotalNeto = 45.50,
        CodigoPago = "001",
    };

    [Fact]
    public void Enriquece_LosCamposIdentificablesDeLaFactura()
    {
        var meta = new Dictionary<string, object>();

        EvidenciaEnriquecida.Enriquecer(meta, Factura());

        meta["Ruc"].Should().Be("1790012345001");
        meta["NumeroDocumento"].Should().Be("001-001-000123");
        meta["Placa"].Should().Be("PCA1234");
        meta["Cliente"].Should().Be("CLI060");
        meta["NumeroTurno"].Should().Be(7);
        meta["Monto"].Should().Be(45.50);
        meta["FormaPago"].Should().Be("001");
        meta["Fecha"].Should().Be("2026-06-26 12:00");
    }

    [Fact]
    public void NoPisa_LasClavesQueYaPusoLaRegla()
    {
        var meta = new Dictionary<string, object> { ["Monto"] = 999.0, ["Placa"] = "YA-PUESTA" };

        EvidenciaEnriquecida.Enriquecer(meta, Factura());

        meta["Monto"].Should().Be(999.0);       // la regla manda
        meta["Placa"].Should().Be("YA-PUESTA"); // la regla manda
        meta["Ruc"].Should().Be("1790012345001"); // lo nuevo sí entra
    }

    [Fact]
    public void Omite_CamposVaciosOCero()
    {
        var meta = new Dictionary<string, object>();

        EvidenciaEnriquecida.Enriquecer(meta, new FacturaDto
        {
            RucCliente = "   ",   // vacío → no entra
            Placa = "",           // vacío → no entra
            TotalNeto = 0,        // cero → no entra
            NumeroDocumento = "001-001-000999",
        });

        meta.Should().NotContainKey("Ruc");
        meta.Should().NotContainKey("Placa");
        meta.Should().NotContainKey("Monto");
        meta["NumeroDocumento"].Should().Be("001-001-000999");
    }

    [Fact]
    public void FuenteNula_NoHaceNada()
    {
        var meta = new Dictionary<string, object>();
        EvidenciaEnriquecida.Enriquecer(meta, null);
        meta.Should().BeEmpty();
    }
}
