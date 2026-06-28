using PetrolRios.Domain.Enums;

namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Generación de reportes consolidados de alertas en PDF y Excel (CU-12).
/// </summary>
public interface IReporteService
{
    Task<byte[]> GenerarPdfAsync(
        TipoDetector? tipo, NivelRiesgo? nivel, EstadoAlerta? estado,
        int? estacionId, DateTime? desde, DateTime? hasta,
        CancellationToken ct = default);

    Task<byte[]> GenerarExcelAsync(
        TipoDetector? tipo, NivelRiesgo? nivel, EstadoAlerta? estado,
        int? estacionId, DateTime? desde, DateTime? hasta,
        CancellationToken ct = default);

    /// <summary>
    /// PDF autogenerado (no "imprimir") de una consulta de documentos en vivo. Recibe las columnas y filas
    /// ya resueltas por el frontend (los datos vienen del agente, no de la BD central), con el mismo formato
    /// del reporte de alertas. <paramref name="busqueda"/> describe los criterios aplicados (tags).
    /// </summary>
    byte[] GenerarPdfConsultaDocumentos(
        string? estacion, string? busqueda,
        IReadOnlyList<string> columnas, IReadOnlyList<IReadOnlyList<string>> filas);
}
