using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules;

/// <summary>
/// Base cómoda para las reglas: inyecta el motor de scoring y resuelve el umbral y el carril
/// efectivos (los de la base si la regla existe, o los de por defecto si no).
/// </summary>
public abstract class DetectionRuleBase : IDetectionRule
{
    protected readonly RiskScoringEngine Scoring;

    protected DetectionRuleBase(RiskScoringEngine scoring) => Scoring = scoring;

    public abstract TipoDetector Detector { get; }
    public abstract string Parametro { get; }
    public abstract double UmbralPorDefecto { get; }
    public virtual AmbitoAlerta AmbitoPorDefecto => AmbitoAlerta.Auditoria;

    public abstract IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla);

    /// <summary>Umbral efectivo: el configurado en la base o, si no existe, el de por defecto.</summary>
    protected double Umbral(ReglaDeteccion? regla) => regla?.ValorUmbral ?? UmbralPorDefecto;

    /// <summary>Carril efectivo: el configurado en la base o, si no existe, el de por defecto.</summary>
    protected AmbitoAlerta Carril(ReglaDeteccion? regla) => regla?.Ambito ?? AmbitoPorDefecto;

    // ── Vínculo factura↔despacho (DCTO↔DESP) ────────────────────────────────────────────────────
    // El producto y los galones viven en el DETALLE (DESP); la placa, la identidad y el monto en la
    // FACTURA (DCTO). El vínculo real DESP.NUM_DESP ↔ DCTO.NDO_DCTO NO viaja en los DTO, así que se
    // reconstruye por CodigoCliente + cercanía temporal (FechaDespacho ≈ FechaDocumento, segundos de
    // diferencia). Unir por CodigoManguera es INCORRECTO: la factura trae la manguera "00" de cabecera
    // y el detalle la manguera real, así que nunca casan (bug que dejaba mudas a varias reglas).

    /// <summary>Tolerancia por defecto para emparejar una factura con su despacho por cercanía temporal.
    /// En datos reales se registran con ~1 s de diferencia; el margen amplio cubre desfase de reloj y,
    /// como siempre se elige el MÁS cercano, no cruza despachos de visitas distintas del mismo cliente.</summary>
    protected static readonly TimeSpan ToleranciaDespacho = TimeSpan.FromMinutes(15);

    /// <summary>Índice de despachos (detalles) por CodigoCliente, ordenados por hora, para emparejarlos
    /// con sus facturas. Se construye una vez por contexto y se reutiliza en cada factura.</summary>
    protected static Dictionary<string, List<DetalleFacturaDto>> IndexarDespachosPorCliente(
        IReadOnlyList<DetalleFacturaDto> detalles) =>
        detalles
            .Where(d => !string.IsNullOrWhiteSpace(d.CodigoCliente))
            .GroupBy(d => d.CodigoCliente.Trim())
            .ToDictionary(g => g.Key, g => g.OrderBy(d => d.FechaDespacho).ToList());

    /// <summary>Despacho (detalle) más cercano en el tiempo a una factura del mismo cliente, dentro de la
    /// tolerancia; null si ninguno está suficientemente cerca. Reconstruye el vínculo factura↔despacho.</summary>
    protected static DetalleFacturaDto? DespachoDeFactura(
        FacturaDto factura,
        IReadOnlyDictionary<string, List<DetalleFacturaDto>> despachosPorCliente,
        TimeSpan? tolerancia = null)
    {
        if (string.IsNullOrWhiteSpace(factura.CodigoCliente)) return null;
        if (!despachosPorCliente.TryGetValue(factura.CodigoCliente.Trim(), out var candidatos)) return null;

        var margen = tolerancia ?? ToleranciaDespacho;
        DetalleFacturaDto? mejor = null;
        foreach (var d in candidatos)
        {
            var delta = (d.FechaDespacho - factura.FechaDocumento).Duration();
            if (delta <= margen)
            {
                margen = delta;
                mejor = d;
            }
        }
        return mejor;
    }
}
