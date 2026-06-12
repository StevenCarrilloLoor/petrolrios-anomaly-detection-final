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
        var estacion = await ObtenerORegistrarEstacionAsync(request.CodigoEstacion, ct);
        estacion.UltimoHeartbeat = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.VersionAgente))
            estacion.VersionAgente = request.VersionAgente;
        await _dbContext.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Busca la estación por código; si no existe la registra automáticamente
    /// (las estaciones se dan de alta cuando su agente se conecta por primera vez).
    /// </summary>
    private async Task<Domain.Entities.Estacion> ObtenerORegistrarEstacionAsync(
        string codigoEstacion, CancellationToken ct)
    {
        var codigo = codigoEstacion.Trim().ToUpperInvariant();
        var estacion = await _dbContext.Estaciones
            .FirstOrDefaultAsync(e => e.Codigo == codigo, ct);

        if (estacion is not null) return estacion;

        estacion = Domain.Entities.Estacion.CreateDesdeAgente(codigo);
        await _dbContext.Estaciones.AddAsync(estacion, ct);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Estación {Codigo} auto-registrada por primer contacto de su agente", codigo);
        return estacion;
    }

    public async Task<IngestaResponse> RecibirLoteAsync(IngestaRequest request, CancellationToken ct = default)
    {
        // Obtener (o auto-registrar) la estación y marcar el contacto
        var estacion = await ObtenerORegistrarEstacionAsync(request.CodigoEstacion, ct);
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
