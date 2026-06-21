using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.InvoiceAnomaly;

/// <summary>Despacho con galones servidos no marcado como facturado (combustible sin cobrar). Operativa.</summary>
public sealed class DespachoNoFacturadoRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "DespachoNoFacturadoHabilitado";
    public override double UmbralPorDefecto => 1.0;
    public override AmbitoAlerta AmbitoPorDefecto => AmbitoAlerta.Operativa;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);

        foreach (var despacho in context.Detalles)
        {
            var marca = despacho.Facturado.Trim();
            if (string.IsNullOrEmpty(marca)) continue;
            if (marca == "1") continue;
            if (despacho.Cantidad <= 0) continue;

            var (score, nivel) = Scoring.Calculate(riesgoBase: 35, montoInvolucrado: despacho.VolumenTotal);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Ambito = carril,
                Descripcion = $"Despacho {despacho.NumeroDespacho} NO facturado: " +
                              $"{despacho.Cantidad:F2} gal de {despacho.NombreProducto.Trim()} " +
                              $"por ${despacho.VolumenTotal:F2}. Revisar (combustible servido sin cobrar).",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                TransaccionReferencia = $"DESP-NOFACT-{despacho.NumeroDespacho}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroDespacho"] = despacho.NumeroDespacho,
                    ["Galones"] = despacho.Cantidad,
                    ["Monto"] = despacho.VolumenTotal,
                    ["Producto"] = despacho.NombreProducto.Trim(),
                    ["IndicadorFacturado"] = marca
                }
            });
        }
        return anomalies;
    }
}
