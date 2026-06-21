using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors;

/// <summary>
/// Detector de violaciones de cumplimiento regulatorio. Es un orquestador delgado
/// (<see cref="RuleBasedDetector"/>): solo declara su carril y ejecuta las reglas
/// <see cref="IDetectionRule"/> de cumplimiento que se le inyectan. La lógica de cada regla vive en
/// su propia clase (carpeta Rules/ComplianceViolation): placa genérica con exceso de galones,
/// múltiples combustibles por placa/día, venta sin placa en monto mayor, operación fuera de horario,
/// venta sin cédula/RUC y despacho de alto volumen sin placa.
/// </summary>
public sealed class ComplianceViolationDetector : RuleBasedDetector
{
    public override TipoDetector Type => TipoDetector.ComplianceViolation;

    public ComplianceViolationDetector(IEnumerable<IDetectionRule> reglas, ILogger<ComplianceViolationDetector> logger)
        : base(reglas, logger)
    {
    }
}
