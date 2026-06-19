using FluentAssertions;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Domain.Tests;

/// <summary>Pruebas del catálogo de esquemas (tablas/columnas reportadas por los agentes).</summary>
public class EsquemaTablaTests
{
    [Fact]
    public void Create_NormalizaNombreAMayusculas_YGuardaColumnas()
    {
        var json = """[{"nombre":"NUM_TURN","tipo":"INTEGER","longitud":4,"nullable":true}]""";

        var e = EsquemaTabla.Create("  turn ", json, "EST-001");

        e.Tabla.Should().Be("TURN");
        e.ColumnasJson.Should().Be(json);
        e.EstacionCodigo.Should().Be("EST-001");
    }
}
