using FluentAssertions;
using PetrolRios.Application.Fuentes;
using Xunit;

namespace PetrolRios.Detectors.Tests;

/// <summary>
/// El resolver que traduce el "tipo" del staging a (nombre natural, tabla técnica) para los logs
/// "Datos recibidos" — built-ins, variantes de agentes antiguos y fuentes configurables.
/// </summary>
public class CatalogoTiposTransaccionTests
{
    [Theory]
    [InlineData("Factura", "Factura", "DCTO")]
    [InlineData("DetalleFactura", "Detalle de factura", "DESP")]
    [InlineData("CierreTurno", "Cierre de turno", "TURN")]
    [InlineData("DepositoTurno", "Depósito de turno", "TURN_DEPO")]
    [InlineData("Anulacion", "Anulación", "ANUL")]
    [InlineData("Credito", "Crédito", "CRED_CABE")]
    [InlineData("TarjetaTurno", "Tarjeta de turno", "TURN_TARJ")]
    public void Resolver_builtins_devuelve_natural_y_tabla(string tipo, string natural, string tabla)
    {
        var (n, t) = CatalogoTiposTransaccion.Resolver(tipo);
        n.Should().Be(natural);
        t.Should().Be(tabla);
    }

    [Fact]
    public void Resolver_tolera_variante_plural_de_agentes_antiguos()
    {
        // "Anulaciones" (plural) lo enviaba un agente viejo; debe mapear a la misma tabla ANUL.
        CatalogoTiposTransaccion.Resolver("Anulaciones").Should().Be(("Anulación", "ANUL"));
    }

    [Fact]
    public void Resolver_dcto_del_selector_apunta_a_la_misma_tabla_que_factura()
    {
        CatalogoTiposTransaccion.Resolver("Dcto").Should().Be(("Documento", "DCTO"));
    }

    [Fact]
    public void Resolver_es_insensible_a_mayusculas_y_espacios()
    {
        CatalogoTiposTransaccion.Resolver("  factura ").Should().Be(("Factura", "DCTO"));
    }

    [Fact]
    public void Resolver_fuente_configurable_usa_la_tabla_del_catalogo()
    {
        // No es built-in: el tipo es el nombre de la fuente; la tabla viene del catálogo del selector.
        CatalogoTiposTransaccion.Resolver("Tanques", "TANQ_REPO").Should().Be(("Tanques", "TANQ_REPO"));
    }

    [Fact]
    public void Resolver_desconocida_sin_tabla_no_inventa_tabla()
    {
        CatalogoTiposTransaccion.Resolver("FuenteRara").Should().Be(("FuenteRara", ""));
    }

    [Theory]
    [InlineData("Factura", null, "Factura (DCTO)")]
    [InlineData("Tanques", "TANQ_REPO", "Tanques (TANQ_REPO)")]
    [InlineData("FuenteRara", null, "FuenteRara")]
    public void Etiqueta_formatea_natural_con_tabla_en_parentesis(string tipo, string? tabla, string esperado)
    {
        CatalogoTiposTransaccion.Etiqueta(tipo, tabla).Should().Be(esperado);
    }
}
