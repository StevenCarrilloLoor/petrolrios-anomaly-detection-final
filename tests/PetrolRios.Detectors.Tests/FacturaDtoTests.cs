using FluentAssertions;
using PetrolRios.Application.DTOs.Firebird;
using Xunit;

namespace PetrolRios.Detectors.Tests;

/// <summary>
/// FacturaDto.TotalNeto debe usar el total REAL (TotalSinIva + Iva) cuando TNI_DCTO llega en 0
/// (lo normal en la Contaplus real de San Pío), y respetar el TNI cuando viene poblado (datos demo).
/// Sin esto, el monto sale $0.00 en factura/consultas/evidencia, el scoring queda plano en la base
/// (multiplicador por monto inerte) y las reglas con umbral en dólares nunca disparan.
/// </summary>
public class FacturaDtoTests
{
    [Fact]
    public void TotalNeto_CuandoTniEsCero_UsaTotalSinIvaMasIva()
    {
        var f = new FacturaDto { TotalNeto = 0, TotalSinIva = 47.83, Iva = 7.17 };
        f.TotalNeto.Should().BeApproximately(55.00, 0.001);
    }

    [Fact]
    public void TotalNeto_CuandoTniViene_LoRespeta()
    {
        var f = new FacturaDto { TotalNeto = 56, TotalSinIva = 50, Iva = 6 };
        f.TotalNeto.Should().Be(56);
    }

    [Fact]
    public void TotalNeto_TodoEnCero_QuedaEnCero()
    {
        new FacturaDto().TotalNeto.Should().Be(0);
    }
}
