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
    private static readonly EstadoAlerta[] EstadosResueltos =
        [EstadoAlerta.Confirmada, EstadoAlerta.FalsoPositivo, EstadoAlerta.Cerrada];

    private readonly IUnitOfWork _unitOfWork;
    private readonly PetrolRiosDbContext _dbContext;
    private readonly IEmpleadoDirectorio _empleados;

    public AlertaService(IUnitOfWork unitOfWork, PetrolRiosDbContext dbContext, IEmpleadoDirectorio empleados)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _empleados = empleados;
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

        // Resolver el nombre del empleado (código → nombre) para mostrarlo junto al código.
        var empleados = await _empleados.CargarAsync(
            alertas.Select(a => (a.EstacionId, a.EmpleadoCodigo)), ct);

        return new PaginatedResponse<AlertaResponse>
        {
            Items = alertas.Select(a => MapToResponse(a, estaciones, empleados)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = count
        };
    }

    public async Task<IReadOnlyList<ProblemaEstacionGrupo>> GetProblemasEstacionAsync(
        int? estacionId,
        int dias,
        bool soloActivos = false,
        CancellationToken ct = default)
    {
        var desde = DateTime.UtcNow.Date.AddDays(-Math.Max(0, dias));

        var query = _dbContext.Alertas
            .Include(a => a.Estacion)
            .Where(a => a.Ambito == AmbitoAlerta.Operativa && a.FechaDeteccion >= desde);

        if (estacionId is not null)
            query = query.Where(a => a.EstacionId == estacionId);

        if (soloActivos)
            query = query.Where(a => !EstadosResueltos.Contains(a.Estado));

        var alertas = await query
            .OrderByDescending(a => a.FechaDeteccion)
            .ToListAsync(ct);

        var estaciones = alertas
            .GroupBy(a => a.EstacionId)
            .ToDictionary(g => g.Key, g => g.First().Estacion.Nombre);

        var empleados = await _empleados.CargarAsync(
            alertas.Select(a => (a.EstacionId, a.EmpleadoCodigo)), ct);

        return alertas
            .GroupBy(a => new { a.EstacionId, Fecha = a.FechaDeteccion.Date })
            .Select(g => new ProblemaEstacionGrupo
            {
                EstacionId = g.Key.EstacionId,
                EstacionNombre = estaciones.GetValueOrDefault(g.Key.EstacionId, ""),
                Fecha = g.Key.Fecha,
                Total = g.Count(),
                Problemas = g.Select(a => MapToResponse(a, estaciones, empleados)).ToList()
            })
            .OrderByDescending(x => x.Fecha)
            .ThenBy(x => x.EstacionNombre)
            .ToList();
    }

    public async Task<AlertaResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var alerta = await _dbContext.Alertas
            .Include(a => a.Estacion)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (alerta is null) return null;

        var estaciones = new Dictionary<int, string> { [alerta.EstacionId] = alerta.Estacion.Nombre };
        var empleados = await _empleados.CargarAsync([(alerta.EstacionId, alerta.EmpleadoCodigo)], ct);
        return MapToResponse(alerta, estaciones, empleados);
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

        // Registrar la fecha de resolución para métricas de tiempos (CU-13)
        alerta.FechaResolucion = EstadosResueltos.Contains(nuevoEstado)
            ? DateTime.UtcNow
            : null;

        await _dbContext.SaveChangesAsync(ct);

        var estaciones = new Dictionary<int, string> { [alerta.EstacionId] = alerta.Estacion.Nombre };
        var empleados = await _empleados.CargarAsync([(alerta.EstacionId, alerta.EmpleadoCodigo)], ct);
        return MapToResponse(alerta, estaciones, empleados);
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

    public async Task<IReadOnlyList<ComentarioResponse>> GetComentariosAsync(int alertaId, CancellationToken ct = default)
    {
        return await _dbContext.ComentariosAlerta
            .Where(c => c.AlertaId == alertaId)
            .Include(c => c.Usuario)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new ComentarioResponse
            {
                Id = c.Id,
                AlertaId = c.AlertaId,
                UsuarioId = c.UsuarioId,
                UsuarioNombre = c.Usuario.NombreCompleto,
                Texto = c.Texto,
                Fecha = c.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<ComentarioResponse> AgregarComentarioAsync(
        int alertaId, int usuarioId, AgregarComentarioRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Texto))
            throw new ArgumentException("El comentario no puede estar vacío.");

        _ = await _dbContext.Alertas.FindAsync(new object[] { alertaId }, ct)
            ?? throw new KeyNotFoundException($"Alerta {alertaId} no encontrada.");

        var usuario = await _dbContext.Usuarios.FindAsync(new object[] { usuarioId }, ct)
            ?? throw new KeyNotFoundException($"Usuario {usuarioId} no encontrado.");

        var comentario = ComentarioAlerta.Create(alertaId, usuarioId, request.Texto.Trim());
        await _dbContext.ComentariosAlerta.AddAsync(comentario, ct);
        await _dbContext.SaveChangesAsync(ct);

        return new ComentarioResponse
        {
            Id = comentario.Id,
            AlertaId = alertaId,
            UsuarioId = usuarioId,
            UsuarioNombre = usuario.NombreCompleto,
            Texto = comentario.Texto,
            Fecha = comentario.CreatedAt
        };
    }

    private static AlertaResponse MapToResponse(
        Alerta a, Dictionary<int, string> estaciones, DirectorioEmpleados empleados) => new()
    {
        Id = a.Id,
        TipoDetector = a.TipoDetector.ToString(),
        NivelRiesgo = a.NivelRiesgo.ToString(),
        Ambito = a.Ambito.ToString(),
        Estado = a.Estado.ToString(),
        Descripcion = a.Descripcion,
        Score = a.Score,
        FechaDeteccion = a.FechaDeteccion,
        EmpleadoCodigo = a.EmpleadoCodigo,
        EmpleadoNombre = empleados.Nombre(a.EstacionId, a.EmpleadoCodigo),
        TransaccionReferencia = a.TransaccionReferencia,
        EstacionId = a.EstacionId,
        EstacionNombre = estaciones.GetValueOrDefault(a.EstacionId, ""),
        MetadataJson = a.MetadataJson
    };
}
