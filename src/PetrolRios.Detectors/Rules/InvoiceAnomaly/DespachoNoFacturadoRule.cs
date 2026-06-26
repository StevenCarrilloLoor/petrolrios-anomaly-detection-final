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
            // FAC_DESP en Contaplus es un CÓDIGO de estado de facturación, no un 0/1. En los datos
            // reales toma valores poblados (2, 4, 5, 7…) según el tipo/canal con que se liquidó el
            // despacho: cualquiera de ellos significa que YA se facturó. El único caso de "combustible
            // servido sin cobrar" es cuando la marca viene VACÍA o en "0" (despacho sin liquidar).
            // (Antes se marcaba como anomalía todo lo que no fuera "1", lo que disparaba en cada
            //  despacho porque "1" prácticamente no se usa.)
            var sinFacturar = marca.Length == 0 || marca == "0";
            if (!sinFacturar) continue;
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
                    ["IndicadorFacturado"] = marca.Length == 0 ? "(vacío)" : marca
                }
            });
        }
        return anomalies;
    }
}
