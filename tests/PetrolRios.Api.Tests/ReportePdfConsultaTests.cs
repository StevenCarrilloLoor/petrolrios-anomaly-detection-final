using System.Text;
using FluentAssertions;
using PetrolRios.Infrastructure.Services;

namespace PetrolRios.Api.Tests;

/// <summary>
/// PDF AUTOGENERADO de una consulta de documentos (no "imprimir"): el método es puro (no toca la BD), así
/// que se prueba directo. Verifica que produce un PDF real (cabecera "%PDF-") con columnas y filas, y que
/// tolera el caso sin filas.
/// </summary>
public sealed class ReportePdfConsultaTests
{
    private static ReporteService Sut() => new(null!, null!); // el método de PDF no usa el DbContext ni el directorio

    [Fact]
    public void GenerarPdfConsultaDocumentos_DevuelveUnPdfValido()
    {
        var columnas = new[] { "N.º documento", "Fecha", "Despachador", "Total" };
        var filas = new List<IReadOnlyList<string>>
        {
            new[] { "032102000016081", "2026-06-28", "DD0000010", "$12.50" },
            new[] { "032102000016082", "2026-06-28", "DD0000010", "$8.00" },
        };

        var pdf = Sut().GenerarPdfConsultaDocumentos("EST-015", "Búsqueda: JBA0412 + DD0000010", columnas, filas);

        pdf.Should().NotBeNullOrEmpty();
        Encoding.ASCII.GetString(pdf, 0, 5).Should().Be("%PDF-");
    }

    [Fact]
    public void GenerarPdfConsultaDocumentos_SinFilas_NoLanza()
    {
        var pdf = Sut().GenerarPdfConsultaDocumentos(null, null, new[] { "Columna" }, new List<IReadOnlyList<string>>());
        pdf.Should().NotBeNullOrEmpty();
    }
}
