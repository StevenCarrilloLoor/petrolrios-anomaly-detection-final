using PetrolRios.Application.DTOs.Monitoreo;

namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Monitoreo de conexiones del aplicativo: agentes de estación, base de datos,
/// SignalR y motor de detección.
/// </summary>
public interface IMonitoreoService
{
    Task<IReadOnlyList<ConexionEstacionResponse>> GetConexionesEstacionesAsync(CancellationToken ct = default);
    Task<EstadoSistemaResponse> GetEstadoSistemaAsync(CancellationToken ct = default);
}
