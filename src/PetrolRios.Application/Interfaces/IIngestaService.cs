using PetrolRios.Application.DTOs.Ingesta;

namespace PetrolRios.Application.Interfaces;

public interface IIngestaService
{
    Task<IngestaResponse> RecibirLoteAsync(IngestaRequest request, CancellationToken ct = default);

    /// <summary>
    /// Heartbeat del agente: marca la estación como en línea aunque no haya datos
    /// nuevos. Si la estación no existe, se auto-registra.
    /// </summary>
    Task HeartbeatAsync(HeartbeatRequest request, CancellationToken ct = default);
}
