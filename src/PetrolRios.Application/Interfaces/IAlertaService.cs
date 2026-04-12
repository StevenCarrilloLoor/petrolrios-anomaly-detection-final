using PetrolRios.Application.DTOs.Alertas;
using PetrolRios.Application.DTOs.Common;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Application.Interfaces;

public interface IAlertaService
{
    Task<PaginatedResponse<AlertaResponse>> GetFilteredAsync(
        TipoDetector? tipo, NivelRiesgo? nivel, EstadoAlerta? estado,
        int? estacionId, DateTime? desde, DateTime? hasta,
        int page, int pageSize, CancellationToken ct = default);

    Task<AlertaResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<AlertaResponse> CambiarEstadoAsync(int id, CambiarEstadoRequest request, CancellationToken ct = default);
    Task AsignarAsync(int alertaId, AsignarAlertaRequest request, CancellationToken ct = default);
}
