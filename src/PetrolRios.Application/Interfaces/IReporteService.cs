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
}
