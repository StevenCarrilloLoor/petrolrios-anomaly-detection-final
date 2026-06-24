using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Persistence;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PetrolRios.Infrastructure.Services;

/// <summary>
/// Generación de reportes consolidados de alertas (CU-12).
/// PDF con QuestPDF (licencia Community) y Excel con ClosedXML.
/// </summary>
public sealed class ReporteService : IReporteService
{
    private const int MaxFilasReporte = 5000;

    private readonly PetrolRiosDbContext _dbContext;
    private readonly IEmpleadoDirectorio _empleados;

    static ReporteService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ReporteService(PetrolRiosDbContext dbContext, IEmpleadoDirectorio empleados)
    {
        _dbContext = dbContext;
        _empleados = empleados;
    }

    public async Task<byte[]> GenerarPdfAsync(
        TipoDetector? tipo, NivelRiesgo? nivel, EstadoAlerta? estado,
        int? estacionId, DateTime? desde, DateTime? hasta,
        CancellationToken ct = default)
    {
        var alertas = await GetAlertasAsync(tipo, nivel, estado, estacionId, desde, hasta, ct);
        var filtros = DescribirFiltros(tipo, nivel, estado, estacionId, desde, hasta,
            await NombreEstacionAsync(estacionId, ct));
        var empleados = await _empleados.CargarAsync(
            alertas.Select(a => (a.EstacionId, a.EmpleadoCodigo)), ct);

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(28);
                page.DefaultTextStyle(t => t.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("PetrolRíos S.A.").FontSize(16).Bold();
                            c.Item().Text("Reporte de Alertas — Sistema de Detección de Anomalías Transaccionales")
                                .FontSize(11);
                        });
                        row.ConstantItem(180).AlignRight().Column(c =>
                        {
                            c.Item().Text($"Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(8);
                            c.Item().Text($"Total de alertas: {alertas.Count}").FontSize(8).Bold();
                        });
                    });
                    col.Item().PaddingTop(4).Text($"Filtros: {filtros}").FontSize(8).Italic();
                    col.Item().PaddingTop(6).LineHorizontal(0.8f);
                });

                page.Content().PaddingTop(8).Column(col =>
                {
                    // Resumen por nivel de riesgo
                    col.Item().Row(row =>
                    {
                        foreach (var nivelRiesgo in Enum.GetValues<NivelRiesgo>())
                        {
                            var cantidad = alertas.Count(a => a.NivelRiesgo == nivelRiesgo);
                            row.RelativeItem().Border(0.5f).Padding(6).Column(c =>
                            {
                                c.Item().Text(nivelRiesgo.ToString()).FontSize(8);
                                c.Item().Text(cantidad.ToString()).FontSize(14).Bold();
                            });
                        }
                    });

                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(36);   // Id
                            columns.ConstantColumn(95);   // Fecha
                            columns.ConstantColumn(90);   // Tipo
                            columns.ConstantColumn(48);   // Nivel
                            columns.ConstantColumn(42);   // Score
                            columns.ConstantColumn(64);   // Estado
                            columns.ConstantColumn(95);   // Estación
                            columns.ConstantColumn(50);   // Empleado
                            columns.RelativeColumn();     // Descripción
                        });

                        table.Header(header =>
                        {
                            foreach (var titulo in new[]
                                { "Id", "Fecha", "Tipo", "Nivel", "Score", "Estado", "Estación", "Empleado", "Descripción" })
                            {
                                header.Cell().Background("#1e293b").Padding(4)
                                    .Text(titulo).FontColor("#ffffff").FontSize(8).Bold();
                            }
                        });

                        foreach (var alerta in alertas)
                        {
                            var fondo = alerta.NivelRiesgo switch
                            {
                                NivelRiesgo.Critico => "#fee2e2",
                                NivelRiesgo.Alto => "#ffedd5",
                                _ => "#ffffff"
                            };

                            table.Cell().Background(fondo).Padding(3).Text(alerta.Id.ToString()).FontSize(7);
                            table.Cell().Background(fondo).Padding(3)
                                .Text(alerta.FechaDeteccion.ToString("yyyy-MM-dd HH:mm")).FontSize(7);
                            table.Cell().Background(fondo).Padding(3).Text(alerta.TipoDetector.ToString()).FontSize(7);
                            table.Cell().Background(fondo).Padding(3).Text(alerta.NivelRiesgo.ToString()).FontSize(7);
                            table.Cell().Background(fondo).Padding(3).Text(alerta.Score.ToString("F1")).FontSize(7);
                            table.Cell().Background(fondo).Padding(3).Text(alerta.Estado.ToString()).FontSize(7);
                            table.Cell().Background(fondo).Padding(3).Text(alerta.Estacion.Nombre).FontSize(7);
                            table.Cell().Background(fondo).Padding(3).Text(EmpleadoMostrar(empleados, alerta)).FontSize(7);
                            table.Cell().Background(fondo).Padding(3).Text(alerta.Descripcion).FontSize(7);
                        }
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Página ").FontSize(8);
                    text.CurrentPageNumber().FontSize(8);
                    text.Span(" de ").FontSize(8);
                    text.TotalPages().FontSize(8);
                });
            });
        });

        return documento.GeneratePdf();
    }

    public async Task<byte[]> GenerarExcelAsync(
        TipoDetector? tipo, NivelRiesgo? nivel, EstadoAlerta? estado,
        int? estacionId, DateTime? desde, DateTime? hasta,
        CancellationToken ct = default)
    {
        var alertas = await GetAlertasAsync(tipo, nivel, estado, estacionId, desde, hasta, ct);
        var filtros = DescribirFiltros(tipo, nivel, estado, estacionId, desde, hasta,
            await NombreEstacionAsync(estacionId, ct));
        var empleados = await _empleados.CargarAsync(
            alertas.Select(a => (a.EstacionId, a.EmpleadoCodigo)), ct);

        using var workbook = new XLWorkbook();

        // Hoja 1: Alertas
        var hoja = workbook.Worksheets.Add("Alertas");
        var encabezados = new[]
            { "Id", "Fecha Detección", "Tipo Detector", "Nivel Riesgo", "Score", "Estado",
              "Estación", "Empleado", "Referencia", "Descripción", "Fecha Resolución" };

        for (var i = 0; i < encabezados.Length; i++)
        {
            var celda = hoja.Cell(1, i + 1);
            celda.Value = encabezados[i];
            celda.Style.Font.Bold = true;
            celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            celda.Style.Font.FontColor = XLColor.White;
        }

        var fila = 2;
        foreach (var alerta in alertas)
        {
            hoja.Cell(fila, 1).Value = alerta.Id;
            hoja.Cell(fila, 2).Value = alerta.FechaDeteccion;
            hoja.Cell(fila, 2).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
            hoja.Cell(fila, 3).Value = alerta.TipoDetector.ToString();
            hoja.Cell(fila, 4).Value = alerta.NivelRiesgo.ToString();
            hoja.Cell(fila, 5).Value = alerta.Score;
            hoja.Cell(fila, 6).Value = alerta.Estado.ToString();
            hoja.Cell(fila, 7).Value = alerta.Estacion.Nombre;
            hoja.Cell(fila, 8).Value = EmpleadoMostrar(empleados, alerta);
            hoja.Cell(fila, 9).Value = alerta.TransaccionReferencia ?? "";
            hoja.Cell(fila, 10).Value = alerta.Descripcion;
            if (alerta.FechaResolucion.HasValue)
            {
                hoja.Cell(fila, 11).Value = alerta.FechaResolucion.Value;
                hoja.Cell(fila, 11).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
            }

            if (alerta.NivelRiesgo == NivelRiesgo.Critico)
                hoja.Range(fila, 1, fila, encabezados.Length).Style.Fill.BackgroundColor =
                    XLColor.FromHtml("#fee2e2");
            else if (alerta.NivelRiesgo == NivelRiesgo.Alto)
                hoja.Range(fila, 1, fila, encabezados.Length).Style.Fill.BackgroundColor =
                    XLColor.FromHtml("#ffedd5");
            fila++;
        }

        hoja.Columns().AdjustToContents(1, Math.Min(fila, 200));
        hoja.Column(10).Width = 80;
        hoja.SheetView.FreezeRows(1);
        hoja.RangeUsed()?.SetAutoFilter();

        // Hoja 2: Resumen
        var resumen = workbook.Worksheets.Add("Resumen");
        resumen.Cell(1, 1).Value = "Reporte de Alertas — PetrolRíos S.A.";
        resumen.Cell(1, 1).Style.Font.Bold = true;
        resumen.Cell(1, 1).Style.Font.FontSize = 14;
        resumen.Cell(2, 1).Value = $"Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";
        resumen.Cell(3, 1).Value = $"Filtros: {filtros}";
        resumen.Cell(4, 1).Value = $"Total de alertas: {alertas.Count}";

        resumen.Cell(6, 1).Value = "Por nivel de riesgo";
        resumen.Cell(6, 1).Style.Font.Bold = true;
        var filaResumen = 7;
        foreach (var nivelRiesgo in Enum.GetValues<NivelRiesgo>())
        {
            resumen.Cell(filaResumen, 1).Value = nivelRiesgo.ToString();
            resumen.Cell(filaResumen, 2).Value = alertas.Count(a => a.NivelRiesgo == nivelRiesgo);
            filaResumen++;
        }

        resumen.Cell(filaResumen + 1, 1).Value = "Por tipo de detector";
        resumen.Cell(filaResumen + 1, 1).Style.Font.Bold = true;
        filaResumen += 2;
        foreach (var grupo in alertas.GroupBy(a => a.TipoDetector))
        {
            resumen.Cell(filaResumen, 1).Value = grupo.Key.ToString();
            resumen.Cell(filaResumen, 2).Value = grupo.Count();
            filaResumen++;
        }

        resumen.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private async Task<List<Alerta>> GetAlertasAsync(
        TipoDetector? tipo, NivelRiesgo? nivel, EstadoAlerta? estado,
        int? estacionId, DateTime? desde, DateTime? hasta, CancellationToken ct)
    {
        var query = _dbContext.Alertas
            .Include(a => a.Estacion)
            .AsNoTracking()
            .AsQueryable();

        if (tipo.HasValue) query = query.Where(a => a.TipoDetector == tipo.Value);
        if (nivel.HasValue) query = query.Where(a => a.NivelRiesgo == nivel.Value);
        if (estado.HasValue) query = query.Where(a => a.Estado == estado.Value);
        if (estacionId.HasValue) query = query.Where(a => a.EstacionId == estacionId.Value);
        if (desde.HasValue) query = query.Where(a => a.FechaDeteccion >= desde.Value);
        if (hasta.HasValue) query = query.Where(a => a.FechaDeteccion <= hasta.Value);

        return await query
            .OrderByDescending(a => a.FechaDeteccion)
            .Take(MaxFilasReporte)
            .ToListAsync(ct);
    }

    private async Task<string?> NombreEstacionAsync(int? estacionId, CancellationToken ct)
    {
        if (!estacionId.HasValue) return null;
        return await _dbContext.Estaciones
            .Where(e => e.Id == estacionId.Value)
            .Select(e => e.Nombre)
            .FirstOrDefaultAsync(ct);
    }

    private static string DescribirFiltros(
        TipoDetector? tipo, NivelRiesgo? nivel, EstadoAlerta? estado,
        int? estacionId, DateTime? desde, DateTime? hasta, string? estacionNombre)
    {
        var partes = new List<string>();
        if (tipo.HasValue) partes.Add($"Tipo: {tipo}");
        if (nivel.HasValue) partes.Add($"Nivel: {nivel}");
        if (estado.HasValue) partes.Add($"Estado: {estado}");
        if (estacionId.HasValue) partes.Add($"Estación: {estacionNombre ?? estacionId.ToString()}");
        if (desde.HasValue) partes.Add($"Desde: {desde:yyyy-MM-dd}");
        if (hasta.HasValue) partes.Add($"Hasta: {hasta:yyyy-MM-dd}");
        return partes.Count > 0 ? string.Join(" | ", partes) : "Sin filtros (todas las alertas)";
    }

    /// <summary>Texto del empleado para el reporte: "Nombre (código)", o solo el código si no hay nombre.</summary>
    private static string EmpleadoMostrar(DirectorioEmpleados empleados, Alerta a)
    {
        var codigo = a.EmpleadoCodigo;
        if (string.IsNullOrWhiteSpace(codigo)) return "—";
        var nombre = empleados.Nombre(a.EstacionId, codigo);
        return nombre is null ? codigo : $"{nombre} ({codigo})";
    }
}
