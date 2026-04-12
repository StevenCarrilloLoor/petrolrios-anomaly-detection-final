using PetrolRios.Application.DTOs.Firebird;

namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Cliente de solo lectura contra una base de datos Firebird (CONTAC.FDB) de una estación.
/// Todas las consultas son SELECT — nunca se modifica la base de origen.
/// </summary>
public interface IFirebirdSourceClient
{
    Task<IReadOnlyList<FacturaDto>> GetFacturasDesdeAsync(DateTime watermark, CancellationToken ct = default);
    Task<IReadOnlyList<DetalleFacturaDto>> GetDetallesFacturaAsync(DateTime watermark, CancellationToken ct = default);
    Task<IReadOnlyList<CierreTurnoDto>> GetCierresTurnoAsync(DateTime watermark, CancellationToken ct = default);
    Task<IReadOnlyList<DepositoTurnoDto>> GetDepositosTurnoAsync(DateTime watermark, CancellationToken ct = default);
    Task<IReadOnlyList<AnulacionDto>> GetAnulacionesAsync(DateTime watermark, CancellationToken ct = default);
    Task<IReadOnlyList<CreditoDto>> GetCreditosAsync(DateTime watermark, CancellationToken ct = default);
    Task<IReadOnlyList<TarjetaTurnoDto>> GetTarjetasTurnoAsync(DateTime watermark, CancellationToken ct = default);
}
