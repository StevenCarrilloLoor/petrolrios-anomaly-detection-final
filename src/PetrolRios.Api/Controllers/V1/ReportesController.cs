using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Api.Controllers.V1;

/// <summary>
/// Reportes consolidados de alertas en PDF y Excel (CU-12).
/// Disponible para Supervisor y Administrador.
/// </summary>
[ApiController]
[Route("api/v1/reportes")]
[Authorize(Roles = "Supervisor,Administrador", Policy = "Central")]
public sealed class ReportesController : ControllerBase
{
    private readonly IReporteService _reporteService;

    public ReportesController(IReporteService reporteService)
    {
        _reporteService = reporteService;
    }

    /// <summary>
    /// Descargar reporte de alertas en PDF con los filtros indicados.
    /// </summary>
    [HttpGet("alertas/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DescargarPdf(
        [FromQuery] TipoDetector? tipo,
        [FromQuery] NivelRiesgo? nivelRiesgo,
        [FromQuery] EstadoAlerta? estado,
        [FromQuery] int? estacionId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        CancellationToken ct = default)
    {
        var pdf = await _reporteService.GenerarPdfAsync(
            tipo, nivelRiesgo, estado, estacionId, fechaDesde, fechaHasta, ct);

        var nombre = $"reporte-alertas-{DateTime.UtcNow:yyyyMMdd-HHmm}.pdf";
        return File(pdf, "application/pdf", nombre);
    }

    /// <summary>
    /// Descargar reporte de alertas en Excel con los filtros indicados.
    /// </summary>
    [HttpGet("alertas/excel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DescargarExcel(
        [FromQuery] TipoDetector? tipo,
        [FromQuery] NivelRiesgo? nivelRiesgo,
        [FromQuery] EstadoAlerta? estado,
        [FromQuery] int? estacionId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        CancellationToken ct = default)
    {
        var excel = await _reporteService.GenerarExcelAsync(
            tipo, nivelRiesgo, estado, estacionId, fechaDesde, fechaHasta, ct);

        var nombre = $"reporte-alertas-{DateTime.UtcNow:yyyyMMdd-HHmm}.xlsx";
        return File(excel,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            nombre);
    }
}
