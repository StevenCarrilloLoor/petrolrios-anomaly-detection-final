using FluentAssertions;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Domain.Tests;

/// <summary>
/// Pruebas de la entidad FuenteDatos (catálogo central de tablas extra de Firebird).
/// </summary>
public class FuenteDatosTests
{
    [Fact]
    public void Create_RecortaEspacios_YQuedaActiva()
    {
        var fuente = FuenteDatos.Create("  Tanques ", " TANQ_REPO ", " FECHA ", "  reportes de tanque ");

        fuente.Nombre.Should().Be("Tanques");
        fuente.Tabla.Should().Be("TANQ_REPO");
        fuente.ColumnaWatermark.Should().Be("FECHA");
        fuente.Descripcion.Should().Be("reportes de tanque");
        fuente.Activa.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_ColumnaWatermarkVacia_QuedaNull(string? columna)
    {
        var fuente = FuenteDatos.Create("Tanques", "TANQ_REPO", columna, "");

        fuente.ColumnaWatermark.Should().BeNull();
    }

    [Fact]
    public void Actualizar_CambiaCamposYMarcaUpdatedAt()
    {
        var fuente = FuenteDatos.Create("Tanques", "TANQ_REPO", null, "");

        fuente.Actualizar("Tanques v2", "TANQ_REPO2", "FECHA", "nueva desc", activa: false);

        fuente.Nombre.Should().Be("Tanques v2");
        fuente.Tabla.Should().Be("TANQ_REPO2");
        fuente.ColumnaWatermark.Should().Be("FECHA");
        fuente.Descripcion.Should().Be("nueva desc");
        fuente.Activa.Should().BeFalse();
        fuente.UpdatedAt.Should().NotBeNull();
    }
}
