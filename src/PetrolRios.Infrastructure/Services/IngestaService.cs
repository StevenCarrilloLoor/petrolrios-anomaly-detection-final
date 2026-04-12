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

    public async Task<IngestaResponse> RecibirLoteAsync(IngestaRequest request, CancellationToken ct = default)
    {
        // Validar que la estación existe y está activa
        var estaciones = await _unitOfWork.Estaciones.GetActivasAsync(ct);
        var estacion = estaciones.FirstOrDefault(e => e.Codigo == request.CodigoEstacion);

        if (estacion is null)
            throw new InvalidOperationException(
                $"Estación '{request.CodigoEstacion}' no encontrada o no está activa.");

        // Insertar cada transacción en staging
        foreach (var item in request.Transacciones)
        {
            var staging = TransaccionStaging.Create(
                estacion.Id,
                item.TipoTransaccion,
                item.DataJson,
                item.FechaOriginal);
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
