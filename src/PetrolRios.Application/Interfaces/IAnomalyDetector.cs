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

    /// <summary>
    /// Relaciones entre fuentes/tablas (p. ej. DetalleFactura→Factura por código de cliente).
    /// Las usa <c>CustomRuleDetector</c> para enriquecer la evidencia de una alerta con campos de una
    /// tabla relacionada (placa, vendedor, cliente, n° de factura), resolviendo el cruce en memoria.
    /// </summary>
    public IReadOnlyList<RelacionTabla> Relaciones { get; init; } = [];

    // Historial de alertas previas por empleado (para detección de reincidencia)
    public IReadOnlyDictionary<string, int> AlertasPreviasPorEmpleado { get; init; } =
        new Dictionary<string, int>();

    // Horario de operación de la estación
    public TimeOnly HoraApertura { get; init; } = new(6, 0);
    public TimeOnly HoraCierre { get; init; } = new(22, 0);

    /// <summary>
    /// Precios oficiales por código de producto, para que el detector de "precio fuera de lista" compare
    /// contra el precio regulado real (tolerancia cero) en vez de una heurística. Vacío = el detector cae
    /// a su heurística (mínimo del día). Lo arma el job desde la tabla de precios + el mapeo de producto.
    /// </summary>
    public IReadOnlyList<PrecioOficialContexto> PreciosOficiales { get; init; } = [];
}

/// <summary>
/// Precio oficial de un producto vigente en una ventana, para el detector. Para los combustibles regulados
/// (<see cref="EsRegulado"/> = true: Extra/Ecopaís/Diésel) el precio es único nacional y cobrar distinto es
/// fuera de lista. La Súper (EsRegulado = false) es libre mercado: el detector la ignora.
/// </summary>
public sealed record PrecioOficialContexto(
    string CodigoProducto,
    decimal Precio,
    bool EsRegulado,
    DateTime VigenteDesde,
    DateTime? VigenteHasta);

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

    /// <summary>
    /// Si es true, esta anomalía representa un CASO acumulable (p. ej. despachos rápidos del mismo RUC/placa
    /// en el día). El job, en vez de crear una alerta nueva cada vez, busca una alerta ABIERTA con la misma
    /// <see cref="TransaccionReferencia"/> y le acumula <see cref="EventosEnLote"/> ocurrencias, escalando el
    /// nivel por el conteo total (<c>Alerta.EscalarPorConteo</c>) y subiéndola arriba (re-emerge como Nueva).
    /// Si no hay una abierta, la crea con ese conteo inicial.
    /// </summary>
    public bool EsAcumulable { get; init; }

    /// <summary>Cuántas ocurrencias del caso aporta este lote (para acumular en la alerta). Default 1.</summary>
    public int EventosEnLote { get; init; } = 1;

    /// <summary>
    /// Objeto de origen de la anomalía (p. ej. una <c>FacturaDto</c>, <c>CierreTurnoDto</c>, <c>CreditoDto</c>…).
    /// El enriquecedor central refleja de aquí los campos IDENTIFICABLES estándar (RUC, nº de documento,
    /// placa, vendedor, cliente, turno, fecha, monto, forma de pago) hacia <see cref="Metadata"/>, sin pisar
    /// las claves que ya puso la regla. Así toda alerta trae la misma información identificable, de forma
    /// automática y escalable: una regla nueva solo fija <c>Fuente</c> y hereda la evidencia enriquecida.
    /// </summary>
    public object? Fuente { get; init; }

    /// <summary>
    /// La regla que originó esta anomalía pidió aviso por correo cuando se dispara (además del aviso
    /// automático de las críticas). Lo marca el orquestador (built-in) o el detector (personalizadas)
    /// desde la configuración de la regla; el job envía el correo. Es settable para poder estamparlo
    /// después de construir la anomalía.
    /// </summary>
    public bool NotificarCorreo { get; set; }
}
