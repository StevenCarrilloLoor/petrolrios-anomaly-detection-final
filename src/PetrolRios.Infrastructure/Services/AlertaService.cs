using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.DTOs.Alertas;
using PetrolRios.Application.DTOs.Common;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

public sealed class AlertaService : IAlertaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PetrolRiosDbContext _dbContext;

    public AlertaService(IUnitOfWork unitOfWork, PetrolRiosDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task<PaginatedResponse<AlertaResponse>> GetFilteredAsync(
        TipoDetector? tipo, NivelRiesgo? nivel, EstadoAlerta? estado,
        int? estacionId, DateTime? desde, DateTime? hasta,
        int page, int pageSize, CancellationToken ct = default)
    {
        var alertas = await _unitOfWork.Alertas.GetFilteredAsync(
            tipo, nivel, estado, estacionId, desde, hasta, page, pageSize, ct);
        var count = await _unitOfWork.Alertas.GetFilteredCountAsync(
            tipo, nivel, estado, estacionId, desde, hasta, ct);

        // Cargar nombres de estación
        var estacionIds = alertas.Select(a => a.EstacionId).Distinct().ToList();
        var estaciones = await _dbContext.Estaciones
            .Where(e => estacionIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.Nombre, ct);

        return new PaginatedResponse<AlertaResponse>
        {
            Items = alertas.Select(a => MapToResponse(a, estaciones)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = count
        };
    }

    public async Task<AlertaResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var alerta = await _dbContext.Alertas
            .Include(a => a.Estacion)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (alerta is null) return null;

        var estaciones = new Dictionary<int, string> { [alerta.EstacionId] = alerta.Estacion.Nombre };
        return MapToResponse(alerta, estaciones);
    }

    public async Task<AlertaResponse> CambiarEstadoAsync(int id, CambiarEstadoRequest request, CancellationToken ct = default)
    {
        var alerta = await _dbContext.Alertas
            .Include(a => a.Estacion)
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new KeyNotFoundException($"Alerta {id} no encontrada.");

        if (!Enum.TryParse<EstadoAlerta>(request.Estado, true, out var nuevoEstado))
            throw new ArgumentException($"Estado '{request.Estado}' no es válido.");

        alerta.Estado = nuevoEstado;
        await _dbContext.SaveChangesAsync(ct);

        var estaciones = new Dictionary<int, string> { [alerta.EstacionId] = alerta.Estacion.Nombre };
        return MapToResponse(alerta, estaciones);
    }

    public async Task AsignarAsync(int alertaId, AsignarAlertaRequest request, CancellationToken ct = default)
    {
        var alerta = await _dbContext.Alertas.FindAsync(new object[] { alertaId }, ct)
            ?? throw new KeyNotFoundException($"Alerta {alertaId} no encontrada.");

        var auditor = await _dbContext.Usuarios.FindAsync(new object[] { request.AuditorId }, ct)
            ?? throw new KeyNotFoundException($"Auditor {request.AuditorId} no encontrado.");

        var asignacion = AsignacionAlerta.Create(alertaId, request.AuditorId, request.Comentario);
        await _dbContext.AsignacionesAlerta.AddAsync(asignacion, ct);

        alerta.Estado = EstadoAlerta.EnRevision;
        await _dbContext.SaveChangesAsync(ct);
    }

    private static AlertaResponse MapToResponse(Alerta a, Dictionary<int, string> estaciones) => new()
    {
        Id = a.Id,
        TipoDetector = a.TipoDetector.ToString(),
        NivelRiesgo = a.NivelRiesgo.ToString(),
        Estado = a.Estado.ToString(),
        Descripcion = a.Descripcion,
        Score = a.Score,
        FechaDeteccion = a.FechaDeteccion,
        EmpleadoCodigo = a.EmpleadoCodigo,
        TransaccionReferencia = a.TransaccionReferencia,
        EstacionId = a.EstacionId,
        EstacionNombre = estaciones.GetValueOrDefault(a.EstacionId, ""),
        MetadataJson = a.MetadataJson
    };
}
