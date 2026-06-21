using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.PaymentFraud;

/// <summary>
/// Crédito otorgado sin garante (COD_GARA vacío). Un crédito sin respaldo de garante sugiere una
/// autorización indebida — el patrón que mencionó el ingeniero ("autorizan créditos a personas que
/// no están autorizadas"). Es incobrable y de alto riesgo.
/// </summary>
public sealed class CreditoSinGaranteRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.PaymentFraud;
    public override string Parametro => "CreditoSinGaranteHabilitado";
    public override double UmbralPorDefecto => 1.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);

        foreach (var credito in context.Creditos)
        {
            if (credito.TotalCredito <= 0) continue;
            if (!string.IsNullOrWhiteSpace(credito.CodigoGarante)) continue;

            var (score, nivel) = Scoring.Calculate(riesgoBase: 55, montoInvolucrado: credito.TotalCredito);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.PaymentFraud,
                Ambito = carril,
                Descripcion = $"Crédito de ${credito.TotalCredito:F2} otorgado SIN garante al socio " +
                              $"{credito.CodigoSocio.Trim()} (riesgo de autorización indebida). " +
                              $"Crédito {credito.NumeroCabecera}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                TransaccionReferencia = $"CRED-SINGAR-{credito.NumeroCabecera}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroCabecera"] = credito.NumeroCabecera,
                    ["CodigoSocio"] = credito.CodigoSocio.Trim(),
                    ["TotalCredito"] = credito.TotalCredito
                }
            });
        }
        return anomalies;
    }
}
