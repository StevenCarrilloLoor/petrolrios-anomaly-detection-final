using FluentAssertions;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Domain.Tests;

/// <summary>Catálogo de empleados: normalización del código y recorte del nombre.</summary>
public class EmpleadoTests
{
    [Theory]
    [InlineData("  v001 ", "V001")]
    [InlineData("01", "01")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Normalizar_HaceTrimYMayusculas(string? entrada, string esperado)
    {
        Empleado.Normalizar(entrada).Should().Be(esperado);
    }

    [Fact]
    public void Create_NormalizaCodigoYRecortaNombre()
    {
        var e = Empleado.Create(1, "  v007 ", "  Juan Pérez  ");

        e.EstacionId.Should().Be(1);
        e.Codigo.Should().Be("V007");
        e.Nombre.Should().Be("Juan Pérez");
    }

    [Fact]
    public void Actualizar_CambiaYRecortaElNombre()
    {
        var e = Empleado.Create(1, "V1", "Viejo");

        e.Actualizar("  Nuevo Nombre ");

        e.Nombre.Should().Be("Nuevo Nombre");
    }
}
