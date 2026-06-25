using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.DTOs.Alertas;
using PetrolRios.Application.DTOs.Common;
using PetrolRios.Application.Interfaces;
using PetrolRios.Application.RealTime;
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
    private readonly IEmailNotificacionService _emailService;
    private readonly IAlertaBroadcaster _broadcaster;

    public AlertaService(
        IUnitOfWork unitOfWork,
        PetrolRiosDbContext dbContext,
        IEmpleadoDirectorio empleados,
        IEmailNotificacionService emailService,
        IAlertaBroadcaster broadcaster)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _empleados = empleados;
        _emailService = emailService;
        _broadcaster = broadcaster;
    }

    /// <summary>Datos de la última asignación de una alerta, para mostrarla en el detalle y la lista.</summary>
    private sealed record AsignacionInfo(
        int AuditorId, string AuditorNombre, string? AuditorRol, string? AsignadoPorNombre, DateTime Fecha);

    /// <summary>
    /// Carga, para un conjunto de alertas, su última asignación (a quién y por quién). Una sola
    /// consulta + agrupado en memoria; devuelve un diccionario alertaId → datos de asignación.
    /// </summary>
    private async Task<IReadOnlyDictionary<int, AsignacionInfo>> CargarAsignacionesAsync(
        IEnumerable<int> alertaIds, CancellationToken ct)
    {
        var ids = alertaIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<int, AsignacionInfo>();

        var asignaciones = await _dbContext.AsignacionesAlerta
            .AsNoTracking()
            .Where(a => ids.Contains(a.AlertaId))
            .Include(a => a.Usuario).ThenInclude(u => u.Rol)
            .Include(a => a.AsignadoPor)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        return asignaciones
            .GroupBy(a => a.AlertaId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var ultima = g.First(); // la más reciente por CreatedAt
                    return new AsignacionInfo(
                        ultima.UsuarioId,
                        ultima.Usuario.NombreCompleto,
                        ultima.Usuario.Rol?.Nombre,
                        ultima.AsignadoPor?.NombreCompleto,
                        ultima.CreatedAt);
                });
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

        // Asignaciones (a quién está asignada cada alerta) para mostrarlas en la lista.
        var asignaciones = await CargarAsignacionesAsync(alertas.Select(a => a.Id), ct);

        return new PaginatedResponse<AlertaResponse>
        {
            Items = alertas.Select(a => MapToResponse(a, estaciones, empleados, asignaciones)).ToList(),
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
            .Where(a => (a.Ambito == AmbitoAlerta.Operativa || a.Ambito == AmbitoAlerta.Ambos)
                        && a.FechaDeteccion >= desde);

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

        var asignaciones = await CargarAsignacionesAsync(alertas.Select(a => a.Id), ct);

        return alertas
            .GroupBy(a => new { a.EstacionId, Fecha = a.FechaDeteccion.Date })
            .Select(g => new ProblemaEstacionGrupo
            {
                EstacionId = g.Key.EstacionId,
                EstacionNombre = estaciones.GetValueOrDefault(g.Key.EstacionId, ""),
                Fecha = g.Key.Fecha,
                Total = g.Count(),
                Problemas = g.Select(a => MapToResponse(a, estaciones, empleados, asignaciones)).ToList()
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
        var asignaciones = await CargarAsignacionesAsync([alerta.Id], ct);
        return MapToResponse(alerta, estaciones, empleados, asignaciones);
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
        var asignaciones = await CargarAsignacionesAsync([alerta.Id], ct);
        return MapToResponse(alerta, estaciones, empleados, asignaciones);
    }

    public async Task<AlertaResponse> AsignarAsync(
        int alertaId, AsignarAlertaRequest request, int asignadoPorId, CancellationToken ct = default)
    {
        var alerta = await _dbContext.Alertas
            .Include(a => a.Estacion)
            .FirstOrDefaultAsync(a => a.Id == alertaId, ct)
            ?? throw new KeyNotFoundException($"Alerta {alertaId} no encontrada.");

        var auditor = await _dbContext.Usuarios.FindAsync(new object[] { request.AuditorId }, ct)
            ?? throw new KeyNotFoundException($"Auditor {request.AuditorId} no encontrado.");

        // Quién asigna (el supervisor/admin autenticado). Puede ser null si el claim no resolvió.
        var asignador = asignadoPorId > 0
            ? await _dbContext.Usuarios.FindAsync(new object[] { asignadoPorId }, ct)
            : null;

        var asignacion = AsignacionAlerta.Create(alertaId, request.AuditorId, asignador?.Id, request.Comentario);
        await _dbContext.AsignacionesAlerta.AddAsync(asignacion, ct);

        // Asignar pone la alerta En Revisión (un responsable la está atendiendo).
        alerta.Estado = EstadoAlerta.EnRevision;
        await _dbContext.SaveChangesAsync(ct);

        // Avisar al asignado por correo y en tiempo real. No debe romper la asignación si falla el envío.
        await NotificarAsignacionAsync(alerta, auditor, asignador, ct);

        var estaciones = new Dictionary<int, string> { [alerta.EstacionId] = alerta.Estacion.Nombre };
        var empleados = await _empleados.CargarAsync([(alerta.EstacionId, alerta.EmpleadoCodigo)], ct);
        var asignaciones = await CargarAsignacionesAsync([alerta.Id], ct);
        return MapToResponse(alerta, estaciones, empleados, asignaciones);
    }

    /// <summary>
    /// Notifica al usuario asignado: (1) un correo dirigido SOLO a él (si el correo está habilitado y
    /// tiene email real), y (2) un evento SignalR "AlertaAsignada" para que reciba un aviso en vivo y
    /// las bandejas se refresquen. Tolerante a fallos: registra la asignación aunque el aviso falle.
    /// </summary>
    private async Task NotificarAsignacionAsync(
        Alerta alerta, Usuario auditor, Usuario? asignador, CancellationToken ct)
    {
        if (_emailService.Habilitado
            && !string.IsNullOrWhiteSpace(auditor.Email)
            && !auditor.Email.StartsWith("agent-", StringComparison.OrdinalIgnoreCase))
        {
            var quien = asignador is null ? "Un supervisor" : asignador.NombreCompleto;
            var asunto = $"[PetrolRíos] Se te asignó la alerta #{alerta.Id} ({alerta.NivelRiesgo}) para revisión";
            var cuerpo =
                "<h2 style='color:#1d4ed8'>Tienes una alerta asignada</h2>" +
                $"<p>{quien} te asignó la alerta <b>#{alerta.Id}</b> para que la revises.</p>" +
                $"<p><b>Estación:</b> {alerta.Estacion.Nombre} ({alerta.Estacion.Codigo})</p>" +
                $"<p><b>Detector:</b> {alerta.TipoDetector}</p>" +
                $"<p><b>Nivel de riesgo:</b> {alerta.NivelRiesgo} — score {alerta.Score}/100</p>" +
                $"<p><b>Descripción:</b> {alerta.Descripcion}</p>" +
                $"<p><b>Fecha de detección:</b> {alerta.FechaDeteccion:yyyy-MM-dd HH:mm} UTC</p>" +
                "<hr><p style='color:#64748b;font-size:12px'>Abre el detalle en el panel de PetrolRíos para " +
                "clasificarla. Aviso automático; no respondas a este correo.</p>";

            await _emailService.EnviarAsync(asunto, cuerpo, new[] { auditor.Email }, ct);
        }

        var payload = new AlertaNotificacionPayload(
            NotificationId: Guid.NewGuid().ToString(),
            Id: alerta.Id,
            TipoDetector: alerta.TipoDetector.ToString(),
            NivelRiesgo: alerta.NivelRiesgo.ToString(),
            Ambito: alerta.Ambito.ToString(),
            Descripcion: alerta.Descripcion,
            Score: alerta.Score,
            FechaDeteccion: alerta.FechaDeteccion,
            EstacionId: alerta.EstacionId,
            AsignadoAId: auditor.Id,
            AsignadoANombre: auditor.NombreCompleto);

        // A las bandejas del central (no a estaciones): el asignado verá el aviso personalizado.
        await _broadcaster.PublicarAsync(
            new AlertaPush("AlertaAsignada", ["auditores", "supervisores", "administradores"], payload), ct);
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

    public async Task MarcarVistaAsync(int alertaId, int usuarioId, CancellationToken ct = default)
    {
        // Idempotente: si este usuario ya marcó la alerta como vista, no hace nada.
        var yaVista = await _dbContext.AlertasVistas
            .AnyAsync(v => v.AlertaId == alertaId && v.UsuarioId == usuarioId, ct);
        if (yaVista) return;

        await _dbContext.AlertasVistas.AddAsync(AlertaVista.Create(alertaId, usuarioId), ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<int>> GetVistasAsync(int usuarioId, CancellationToken ct = default)
    {
        return await _dbContext.AlertasVistas
            .Where(v => v.UsuarioId == usuarioId)
            .Select(v => v.AlertaId)
            .ToListAsync(ct);
    }

    private static AlertaResponse MapToResponse(
        Alerta a, Dictionary<int, string> estaciones, DirectorioEmpleados empleados,
        IReadOnlyDictionary<int, AsignacionInfo> asignaciones)
    {
        asignaciones.TryGetValue(a.Id, out var asig);
        return new()
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
            MetadataJson = a.MetadataJson,
            AsignadoAId = asig?.AuditorId,
            AsignadoANombre = asig?.AuditorNombre,
            AsignadoARol = asig?.AuditorRol,
            AsignadoPorNombre = asig?.AsignadoPorNombre,
            FechaAsignacion = asig?.Fecha
        };
    }
}
