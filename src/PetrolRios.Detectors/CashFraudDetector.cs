using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors;

/// <summary>
/// Detector de anomalías de efectivo. Es un orquestador delgado (<see cref="RuleBasedDetector"/>):
/// solo declara su carril y ejecuta las reglas <see cref="IDetectionRule"/> de efectivo que se le
/// inyectan. La lógica de cada regla vive en su propia clase (carpeta Rules/CashFraud): diferencia
/// de efectivo por turno, faltantes recurrentes (gineteo), crédito sin cliente, proporción atípica
/// de efectivo corporativo y turno sin cerrar.
/// </summary>
public sealed class CashFraudDetector : RuleBasedDetector
{
    public override TipoDetector Type => TipoDetector.CashFraud;

    public CashFraudDetector(IEnumerable<IDetectionRule> reglas, ILogger<CashFraudDetector> logger)
        : base(reglas, logger)
    {
    }
}
