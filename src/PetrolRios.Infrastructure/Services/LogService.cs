using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.DTOs.Common;
using PetrolRios.Application.DTOs.Logs;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

public sealed class LogService : ILogService
{
    private readonly PetrolRiosDbContext _dbContext;

    public LogService(PetrolRiosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResponse<LogAuditoriaResponse>> GetLogsAsync(
        int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        var query = _dbContext.LogsAuditoria
            .Include(l => l.Usuario)
            .OrderByDescending(l => l.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LogAuditoriaResponse
            {
                Id = l.Id,
                Accion = l.Accion,
                Entidad = l.Entidad,
                EntidadId = l.EntidadId,
                DetalleJson = l.DetalleJson,
                DireccionIp = l.DireccionIp,
                UsuarioId = l.UsuarioId,
                UsuarioEmail = l.Usuario != null ? l.Usuario.Email : null,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync(ct);

        return new PaginatedResponse<LogAuditoriaResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task RegistrarAsync(string accion, string entidad, int? entidadId = null,
        string? detalleJson = null, string direccionIp = "", int? usuarioId = null,
        CancellationToken ct = default)
    {
        var log = LogAuditoria.Create(accion, entidad, entidadId, detalleJson, direccionIp, usuarioId);
        await _dbContext.LogsAuditoria.AddAsync(log, ct);
        await _dbContext.SaveChangesAsync(ct);
    }
}
