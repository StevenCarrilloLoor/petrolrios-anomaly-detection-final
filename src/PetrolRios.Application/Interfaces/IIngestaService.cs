using PetrolRios.Application.DTOs.Ingesta;

namespace PetrolRios.Application.Interfaces;

public interface IIngestaService
{
    Task<IngestaResponse> RecibirLoteAsync(IngestaRequest request, CancellationToken ct = default);
}
