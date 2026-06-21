using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors;

/// <summary>
/// Detector de anomalías en facturas. Es un orquestador delgado (<see cref="RuleBasedDetector"/>):
/// solo declara su carril y ejecuta las reglas <see cref="IDetectionRule"/> de facturas que se le
/// inyectan. La lógica de cada regla vive en su propia clase (carpeta Rules/InvoiceAnomaly).
/// </summary>
public sealed class InvoiceAnomalyDetector : RuleBasedDetector
{
    public override TipoDetector Type => TipoDetector.InvoiceAnomaly;

    public InvoiceAnomalyDetector(IEnumerable<IDetectionRule> reglas, ILogger<InvoiceAnomalyDetector> logger)
        : base(reglas, logger)
    {
    }
}
