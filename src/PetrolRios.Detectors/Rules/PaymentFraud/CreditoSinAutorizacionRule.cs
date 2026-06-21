using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.PaymentFraud;

/// <summary>Crédito otorgado sin código de autorización (comprobante NUMMCOMP = 0).</summary>
public sealed class CreditoSinAutorizacionRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.PaymentFraud;
    public override string Parametro => "CreditoSinAutorizacionHabilitado";
    public override double UmbralPorDefecto => 1.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);

        // Créditos otorgados (CRED_CABE) sin comprobante asociado (NUMMCOMP = 0 → sin autorización)
        foreach (var credito in context.Creditos)
        {
            if (credito.NumeroComprobante > 0) continue;   // tiene comprobante → autorizado
            if (credito.TotalCredito <= 0) continue;

            var (score, nivel) = Scoring.Calculate(riesgoBase: 50, montoInvolucrado: credito.TotalCredito);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.PaymentFraud,
                Ambito = carril,
                Descripcion = $"Crédito de ${credito.TotalCredito:F2} otorgado sin código de autorización " +
                              $"(comprobante: {credito.NumeroComprobante}). Socio: {credito.CodigoSocio.Trim()}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                TransaccionReferencia = $"CRED_CABE-{credito.NumeroCabecera}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroCabecera"] = credito.NumeroCabecera,
                    ["CodigoSocio"] = credito.CodigoSocio.Trim(),
                    ["TotalCredito"] = credito.TotalCredito,
                    ["CodigoCredito"] = credito.CodigoCredito.Trim()
                }
            });
        }
        return anomalies;
    }
}
