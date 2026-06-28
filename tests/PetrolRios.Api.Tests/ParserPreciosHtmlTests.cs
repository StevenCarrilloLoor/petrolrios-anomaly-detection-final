using FluentAssertions;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Services.Precios;

namespace PetrolRios.Api.Tests;

/// <summary>
/// El parser extrae los precios del HTML de una fuente pública. Se prueba con el formato REAL de
/// gasolinaecuador.com (verificado), además de descartar porcentajes, valores fuera de rango y basura.
/// </summary>
public sealed class ParserPreciosHtmlTests
{
    // Fragmento con el formato real del sitio: "Combustible↑ +X% $precio por galón".
    private const string HtmlReal =
        "<p>Extra↑ +4,61% $3,310 por galón. La gasolina más usada.</p>" +
        "<p>Ecopaís↑ +4,61% $3,310 por galón. Mezcla con etanol.</p>" +
        "<p>Súper Premium↑ +17,46% $5,650 por galón. Sin subsidio.</p>" +
        "<p>Diésel Premium↑ +4,74% $3,250 por galón. Transporte.</p>";

    [Fact]
    public void Parsear_FormatoReal_ExtraeLosCuatroPrecios()
    {
        var p = ParserPreciosHtml.Parsear(HtmlReal);

        p[TipoCombustible.Extra].Should().Be(3.310m);
        p[TipoCombustible.Ecopais].Should().Be(3.310m);
        p[TipoCombustible.Diesel].Should().Be(3.250m);
        p[TipoCombustible.Super].Should().Be(5.650m);
    }

    [Fact]
    public void Parsear_NoConfundeElPorcentajeConElPrecio()
    {
        // "+4,61%" no debe leerse como precio (no tiene '$'); solo el importe con $.
        var p = ParserPreciosHtml.Parsear("Extra↑ +4,61% $3,310 por galón");
        p.Should().ContainKey(TipoCombustible.Extra);
        p[TipoCombustible.Extra].Should().Be(3.310m);
    }

    [Fact]
    public void Parsear_FormatoMetaDescripcion_TambienFunciona()
    {
        var p = ParserPreciosHtml.Parsear("Extra $3,310 · Súper $5,650 · Diésel $3,250 por galón");
        p[TipoCombustible.Extra].Should().Be(3.310m);
        p[TipoCombustible.Super].Should().Be(5.650m);
        p[TipoCombustible.Diesel].Should().Be(3.250m);
    }

    [Fact]
    public void Parsear_DescartaValoresFueraDeRango()
    {
        // $99 para Extra está fuera del rango plausible → no se toma.
        var p = ParserPreciosHtml.Parsear("Extra $99,00 por galón");
        p.Should().NotContainKey(TipoCombustible.Extra);
    }

    [Fact]
    public void Parsear_VacioOSinPrecios_DevuelveVacio()
    {
        ParserPreciosHtml.Parsear(null).Should().BeEmpty();
        ParserPreciosHtml.Parsear("<html><body>sin precios aquí</body></html>").Should().BeEmpty();
    }
}
