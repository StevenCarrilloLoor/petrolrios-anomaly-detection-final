using PetrolRios.Application.DTOs.Common;
using PetrolRios.Application.DTOs.Logs;

namespace PetrolRios.Application.Interfaces;

public interface ILogService
{
    Task<PaginatedResponse<LogAuditoriaResponse>> GetLogsAsync(
        int page = 1, int pageSize = 50, CancellationToken ct = default);

    Task RegistrarAsync(string accion, string entidad, int? entidadId = null,
        string? detalleJson = null, string direccionIp = "", int? usuarioId = null,
        CancellationToken ct = default);
}
