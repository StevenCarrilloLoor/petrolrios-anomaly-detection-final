using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Application.DTOs.Common;
using PetrolRios.Application.DTOs.Logs;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Api.Controllers.V1;

[ApiController]
[Route("api/v1/logs")]
[Authorize(Roles = "Administrador", Policy = "Central")]
public sealed class LogsController : ControllerBase
{
    private readonly ILogService _logService;

    public LogsController(ILogService logService)
    {
        _logService = logService;
    }

    /// <summary>
    /// Consultar logs de auditoría del sistema.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<LogAuditoriaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _logService.GetLogsAsync(page, pageSize, ct);
        return Ok(result);
    }
}
