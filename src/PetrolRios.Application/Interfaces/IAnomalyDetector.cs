using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Contrato para cada detector de anomalías (Strategy Pattern).
/// </summary>
public interface IAnomalyDetector
{
    TipoDetector Type { get; }
    Task<IReadOnlyList<DetectedAnomaly>> DetectAsync(DetectionContext context, CancellationToken ct);
}

/// <summary>
/// Contexto que reciben los detectores con datos de staging y reglas vigentes.
/// </summary>
public sealed class DetectionContext
{
    public required int EstacionId { get; init; }
    public required string EstacionNombre { get; init; }
    public required DateTime FromWatermark { get; init; }
    public required DateTime ToWatermark { get; init; }

    // Datos extraídos de Firebird (staging)
    public IReadOnlyList<FacturaDto> Facturas { get; init; } = [];
    public IReadOnlyList<DetalleFacturaDto> Detalles { get; init; } = [];
    public IReadOnlyList<CierreTurnoDto> CierresTurno { get; init; } = [];
    public IReadOnlyList<DepositoTurnoDto> DepositosTurno { get; init; } = [];
    public IReadOnlyList<AnulacionDto> Anulaciones { get; init; } = [];
    public IReadOnlyList<CreditoDto> Creditos { get; init; } = [];
    public IReadOnlyList<TarjetaTurnoDto> TarjetasTurno { get; init; } = [];

    /// <summary>
    /// Datos de las fuentes de extracción configurables (tablas arbitrarias enviadas por el
    /// agente), por nombre de fuente. Cada registro es un diccionario campo→valor. Permite que
    /// las reglas personalizadas operen sobre cualquier tabla sin tocar el código.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<IDictionary<string, object>>> FuentesGenericas { get; init; }
        = new Dictionary<string, IReadOnlyList<IDictionary<string, object>>>();

    // Reglas de detección configuradas
    public IReadOnlyList<ReglaDeteccion> Reglas { get; init; } = [];

    // Reglas de negocio definidas por el usuario (evaluadas por CustomRuleDetector)
    public IReadOnlyList<ReglaPersonalizada> ReglasPersonalizadas { get; init; } = [];

    // Historial de alertas previas por empleado (para detección de reincidencia)
    public IReadOnlyDictionary<string, int> AlertasPreviasPorEmpleado { get; init; } =
        new Dictionary<string, int>();

    // Horario de operación de la estación
    public TimeOnly HoraApertura { get; init; } = new(6, 0);
    public TimeOnly HoraCierre { get; init; } = new(22, 0);
}

/// <summary>
/// Resultado de detección antes de persistirse como Alerta.
/// </summary>
public sealed class DetectedAnomaly
{
    public required TipoDetector TipoDetector { get; init; }
    public required string Descripcion { get; init; }
    public required double Score { get; init; }
    public required NivelRiesgo NivelRiesgo { get; init; }

    /// <summary>
    /// Carril de la anomalía. Por defecto <see cref="AmbitoAlerta.Auditoria"/> (fraude grave,
    /// solo central). Los detectores marcan <see cref="AmbitoAlerta.Operativa"/> en los
    /// problemas operativos de estación (errores honestos) para enrutarlos al administrador.
    /// </summary>
    public AmbitoAlerta Ambito { get; init; } = AmbitoAlerta.Auditoria;
    public required int EstacionId { get; init; }
    public string? EmpleadoCodigo { get; init; }
    public string? TransaccionReferencia { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = [];
}
