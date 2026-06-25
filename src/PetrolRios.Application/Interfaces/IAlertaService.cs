using PetrolRios.Application.DTOs.Alertas;
using PetrolRios.Application.DTOs.Common;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Application.Interfaces;

public interface IAlertaService
{
    Task<PaginatedResponse<AlertaResponse>> GetFilteredAsync(
        TipoDetector? tipo, NivelRiesgo? nivel, EstadoAlerta? estado,
        int? estacionId, DateTime? desde, DateTime? hasta,
        int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Problemas operativos (carril Operativa) agrupados por estación y día, para la pestaña
    /// "Problemas de estación". <paramref name="dias"/> acota la ventana hacia atrás.
    /// </summary>
    Task<IReadOnlyList<ProblemaEstacionGrupo>> GetProblemasEstacionAsync(
        int? estacionId,
        int dias,
        bool soloActivos = false,
        CancellationToken ct = default);

    Task<AlertaResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<AlertaResponse> CambiarEstadoAsync(int id, CambiarEstadoRequest request, CancellationToken ct = default);
    Task AsignarAsync(int alertaId, AsignarAlertaRequest request, CancellationToken ct = default);

    // CU-07: comentarios de auditoría
    Task<IReadOnlyList<ComentarioResponse>> GetComentariosAsync(int alertaId, CancellationToken ct = default);
    Task<ComentarioResponse> AgregarComentarioAsync(
        int alertaId, int usuarioId, AgregarComentarioRequest request, CancellationToken ct = default);

    /// <summary>Marca una alerta como vista por un usuario concreto (leído/no leído POR USUARIO).</summary>
    Task MarcarVistaAsync(int alertaId, int usuarioId, CancellationToken ct = default);

    /// <summary>IDs de las alertas que el usuario ya vio (para marcar "nuevas para ti" en la lista).</summary>
    Task<IReadOnlyList<int>> GetVistasAsync(int usuarioId, CancellationToken ct = default);
}
