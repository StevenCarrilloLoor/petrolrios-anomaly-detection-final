using System;
using System.Collections.Generic;
using FluentAssertions;
using PetrolRios.Application.ReglasPersonalizadas;
using Xunit;

namespace PetrolRios.Detectors.Tests;

/// <summary>
/// Resolución tolerante de campos en fuentes CONFIGURABLES (tablas crudas de Firebird).
/// Una regla escrita con el nombre amigable ("TotalNeto") debe funcionar sobre una fila que trae
/// el nombre crudo ("TNI_DCTO"), igual que con el nombre exacto o con distinta capitalización.
/// (Causa del bug de San Pío: la regla apuntaba a "TotalNeto" pero la fila traía "TNI_DCTO".)
/// </summary>
public sealed class CatalogoGetValorTests
{
    private static Dictionary<string, object> FilaDcto() => new(StringComparer.Ordinal)
    {
        ["SEC_DCTO"] = 9900060,
        ["TNI_DCTO"] = 500.0,          // total con IVA = "TotalNeto" / "monto final"
        ["SUB_DCTO"] = 446.43,
        ["COD_PAGO"] = "001 ",          // San Pío usa códigos numéricos, con padding
        ["PLA_DCTO"] = "QAB9999             ",
        ["FEC_DCTO"] = "2026-06-25T17:11:12"
    };

    [Fact]
    public void GetValor_Configurable_MatchExacto_CrudoFirebird()
    {
        CatalogoReglasPersonalizadas.GetValor("Dcto", "TNI_DCTO", FilaDcto()).Should().Be(500.0);
    }

    [Fact]
    public void GetValor_Configurable_PuenteAmigableACrudo()
    {
        // El usuario escribió el nombre amigable; la fila trae el crudo → debe resolver igual.
        CatalogoReglasPersonalizadas.GetValor("Dcto", "TotalNeto", FilaDcto()).Should().Be(500.0);
        CatalogoReglasPersonalizadas.GetValor("Dcto", "Subtotal", FilaDcto()).Should().Be(446.43);
    }

    [Fact]
    public void GetValor_Configurable_InsensibleAMayusculasYEspacios()
    {
        CatalogoReglasPersonalizadas.GetValor("Dcto", "tni_dcto", FilaDcto()).Should().Be(500.0);
        CatalogoReglasPersonalizadas.GetValor("Dcto", " TNI_DCTO ", FilaDcto()).Should().Be(500.0);
    }

    [Fact]
    public void GetValor_Configurable_CampoInexistente_DevuelveNull()
    {
        CatalogoReglasPersonalizadas.GetValor("Dcto", "NoExisteEsteCampo", FilaDcto()).Should().BeNull();
    }

    [Fact]
    public void GetValor_BuiltIn_SiguefuncionandoConDtoTipado()
    {
        var f = new PetrolRios.Application.DTOs.Firebird.FacturaDto { TotalNeto = 500, CodigoPago = "EF" };
        CatalogoReglasPersonalizadas.GetValor("Factura", "TotalNeto", f).Should().Be(500.0);
    }
}
