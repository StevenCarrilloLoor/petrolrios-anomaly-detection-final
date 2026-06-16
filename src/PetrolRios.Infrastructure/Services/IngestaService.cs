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

        // Insertar cada transacción en staging
        foreach (var item in request.Transacciones)
        {
            // Las fechas de Firebird llegan con Kind=Unspecified; Npgsql exige UTC
            // para columnas 'timestamp with time zone'.
            var fechaOriginal = item.FechaOriginal.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(item.FechaOriginal, DateTimeKind.Utc)
                : item.FechaOriginal.ToUniversalTime();

            var staging = TransaccionStaging.Create(
                estacion.Id,
                item.TipoTransaccion,
                item.DataJson,
                fechaOriginal);
            await _dbContext.TransaccionesStaging.AddAsync(staging, ct);
        }

        await _dbContext.SaveChangesAsync(ct);

        // Actualizar watermark de la estación
        await _unitOfWork.Estaciones.UpsertWatermarkAsync(estacion.Id, DateTime.UtcNow, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Ingesta: {Count} transacciones recibidas de estación {Codigo}",
            request.Transacciones.Count, request.CodigoEstacion);

        return new IngestaResponse
        {
            TransaccionesRecibidas = request.Transacciones.Count,
            FechaRecepcion = DateTime.UtcNow
        };
    }
}
