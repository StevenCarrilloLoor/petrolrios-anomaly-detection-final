using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.InvoiceAnomaly;

/// <summary>Anulaciones recurrentes (mismo punto de emisión en varios días): posible kiting.</summary>
public sealed class AnulacionRecurrenteRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "AnulacionRecurrenteDiasMinimo";
    public override double UmbralPorDefecto => 3.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        if (context.Anulaciones.Count == 0) return anomalies;

        var diasMinimo = (int)Umbral(regla);
        var carril = Carril(regla);

        var grupos = context.Anulaciones
            .GroupBy(a => new { Est = a.Establecimiento.Trim(), Pto = a.PuntoEmision.Trim() });

        foreach (var grupo in grupos)
        {
            var diasDistintos = grupo.Select(a => a.FechaAnulacion.Date).Distinct().Count();
            if (diasDistintos < diasMinimo) continue;

            var totalAnulaciones = grupo.Count();
            var (score, nivel) = Scoring.Calculate(riesgoBase: 60);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Ambito = carril,
                Descripcion = $"Anulaciones recurrentes en {grupo.Key.Est}-{grupo.Key.Pto}: " +
                              $"{totalAnulaciones} anulaciones en {diasDistintos} días distintos " +
                              $"(umbral: {diasMinimo}). Posible patrón de cancelar y reingresar (kiting).",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                TransaccionReferencia = $"ANUL-RECURR-{grupo.Key.Est}-{grupo.Key.Pto}",
                Metadata = new Dictionary<string, object>
                {
                    ["Establecimiento"] = grupo.Key.Est,
                    ["PuntoEmision"] = grupo.Key.Pto,
                    ["DiasDistintos"] = diasDistintos,
                    ["TotalAnulaciones"] = totalAnulaciones,
                    ["DiasMinimo"] = diasMinimo
                }
            });
        }
        return anomalies;
    }
}
