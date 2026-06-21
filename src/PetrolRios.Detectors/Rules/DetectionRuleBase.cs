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
}
