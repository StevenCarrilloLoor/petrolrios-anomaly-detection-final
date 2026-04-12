using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Application.Interfaces;

public interface IAlertaRepository : IRepository<Alerta>
{
    Task<IReadOnlyList<Alerta>> GetByEstacionAsync(int estacionId, CancellationToken ct = default);

    Task<IReadOnlyList<Alerta>> GetFilteredAsync(
        TipoDetector? tipo = null,
        NivelRiesgo? nivel = null,
        EstadoAlerta? estado = null,
        int? estacionId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    Task<int> GetFilteredCountAsync(
        TipoDetector? tipo = null,
        NivelRiesgo? nivel = null,
        EstadoAlerta? estado = null,
        int? estacionId = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken ct = default);

    Task<int> CountByEmpleadoAndTipoAsync(
        string empleadoCodigo,
        TipoDetector tipo,
        DateTime desde,
        CancellationToken ct = default);
}
