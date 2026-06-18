using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetrolRios.Application.DTOs.Ingesta;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Infrastructure.Services;

/// <summary>
/// Recibe lotes de transacciones enviados por los agentes de estación,
/// valida que la estación exista y almacena en la tabla de staging.
/// </summary>
public sealed class IngestaService : IIngestaService
{
    private readonly PetrolRiosDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IngestaService> _logger;

    public IngestaService(
        PetrolRiosDbContext dbContext,
        IUnitOfWork unitOfWork,
        ILogger<IngestaService> logger)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HeartbeatAsync(HeartbeatRequest request, CancellationToken ct = default)
    {
        var estacion = await ObtenerORegistrarEstacionAsync(
            request.CodigoEstacion, request.NombreEstacion, request.ZonaEstacion, ct);
        estacion.UltimoHeartbeat = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.VersionAgente))
            estacion.VersionAgente = request.VersionAgente;
        await _dbContext.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Busca la estación por código; si no existe la registra automáticamente
    /// (las estaciones se dan de alta cuando su agente se conecta por primera vez).
    /// El nombre y la zona que reporta el agente se aplican: en el alta inicial, y
    /// después solo si la estación conserva el nombre auto-generado (no se pisa una
    /// edición manual hecha desde la interfaz central).
    /// </summary>
    private async Task<Domain.Entities.Estacion> ObtenerORegistrarEstacionAsync(
        string codigoEstacion, string? nombreEstacion, string? zonaEstacion, CancellationToken ct)
    {
        var codigo = codigoEstacion.Trim().ToUpperInvariant();
        var estacion = await _dbContext.Estaciones
            .FirstOrDefaultAsync(e => e.Codigo == codigo, ct);

        if (estacion is null)
        {
            estacion = Domain.Entities.Estacion.CreateDesdeAgente(codigo);
            if (!string.IsNullOrWhiteSpace(nombreEstacion))
                estacion.Actualizar(nombreEstacion.Trim(), null, zonaEstacion?.Trim());
            await _dbContext.Estaciones.AddAsync(estacion, ct);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Estación {Codigo} ('{Nombre}') auto-registrada por primer contacto de su agente",
                codigo, estacion.Nombre);
            return estacion;
        }

        // Estación existente: el agente puede actualizar el nombre si nadie lo cambió
        // manualmente (sigue con el nombre por defecto "Estación EST-XXX").
        if (!string.IsNullOrWhiteSpace(nombreEstacion)
            && estacion.Nombre.Equals($"Estación {codigo}", StringComparison.OrdinalIgnoreCase))
        {
            estacion.Actualizar(nombreEstacion.Trim(), null, zonaEstacion?.Trim());
        }
        return estacion;
    }

    public async Task<IngestaResponse> RecibirLoteAsync(IngestaRequest request, CancellationToken ct = default)
    {
        // Obtener (o auto-registrar) la estación y marcar el contacto
        var estacion = await ObtenerORegistrarEstacionAsync(
            request.CodigoEstacion, request.NombreEstacion, request.ZonaEstacion, ct);
        estacion.UltimoHeartbeat = DateTime.UtcNow;

        // Construir las entidades (calcula el hash de contenido de cada una)
        var entrantes = request.Transacciones.Select(item =>
        {
            // Las fechas de Firebird llegan con Kind=Unspecified; Npgsql exige UTC
            // para columnas 'timestamp with time zone'.
            var fechaOriginal = item.FechaOriginal.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(item.FechaOriginal, DateTimeKind.Utc)
                : item.FechaOriginal.ToUniversalTime();

            return TransaccionStaging.Create(
                estacion.Id, item.TipoTransaccion, item.DataJson, fechaOriginal);
        }).ToList();

        // Idempotencia: descartar lo que ya existe en BD (reenvío del agente) y los
        // duplicados dentro del mismo lote. Así un registro fechado al futuro —que el
        // agente vuelve a extraer en cada ciclo— no genera la misma alerta una y otra vez.
        var hashesEntrantes = entrantes.Select(e => e.HashContenido).ToList();
        var yaExisten = await _dbContext.TransaccionesStaging
            .Where(t => t.EstacionId == estacion.Id && hashesEntrantes.Contains(t.HashContenido))
            .Select(t => t.HashContenido)
            .ToListAsync(ct);

        var vistos = new HashSet<string>(yaExisten);
        var nuevos = new List<TransaccionStaging>();
        foreach (var staging in entrantes)
        {
            if (vistos.Add(staging.HashContenido))
                nuevos.Add(staging);
        }

        if (nuevos.Count > 0)
            await _dbContext.TransaccionesStaging.AddRangeAsync(nuevos, ct);

        await _dbContext.SaveChangesAsync(ct);

        // Actualizar watermark de la estación
        await _unitOfWork.Estaciones.UpsertWatermarkAsync(estacion.Id, DateTime.UtcNow, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var duplicadas = entrantes.Count - nuevos.Count;
        _logger.LogInformation(
            "Ingesta: {Recibidas} recibidas de estación {Codigo} — {Nuevas} nuevas, {Duplicadas} duplicadas descartadas",
            request.Transacciones.Count, request.CodigoEstacion, nuevos.Count, duplicadas);

        return new IngestaResponse
        {
            TransaccionesRecibidas = nuevos.Count,
            FechaRecepcion = DateTime.UtcNow
        };
    }
}
