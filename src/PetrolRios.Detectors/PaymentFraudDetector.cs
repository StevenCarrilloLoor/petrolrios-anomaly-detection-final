using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors;

/// <summary>
/// Detector de anomalías de pago. Es un orquestador delgado (<see cref="RuleBasedDetector"/>): solo
/// declara su carril y ejecuta las reglas <see cref="IDetectionRule"/> de pagos que se le inyectan.
/// La lógica de cada regla vive en su propia clase (carpeta Rules/PaymentFraud): reversión de
/// tarjeta tardía, crédito sin autorización, crédito sin garante, transacciones duplicadas y
/// despachos rápidos sucesivos.
/// </summary>
public sealed class PaymentFraudDetector : RuleBasedDetector
{
    public override TipoDetector Type => TipoDetector.PaymentFraud;

    public PaymentFraudDetector(IEnumerable<IDetectionRule> reglas, ILogger<PaymentFraudDetector> logger)
        : base(reglas, logger)
    {
    }
}
