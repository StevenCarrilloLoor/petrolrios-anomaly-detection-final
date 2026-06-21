using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.ComplianceViolation;

/// <summary>
/// Despacho de alto volumen sin placa registrada. Cargar muchos galones sin identificar el vehículo
/// es el patrón típico de desvío de combustible (llenado de tanques o canecas para reventa), que la
/// ARCERNNR controla mediante cupos y trazabilidad por placa.
/// </summary>
public sealed class AltoVolumenSinPlacaRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.ComplianceViolation;
    public override string Parametro => "GalonesSinPlacaMaximo";
    public override double UmbralPorDefecto => 20.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var galonesMaximo = Umbral(regla);
        var carril = Carril(regla);

        foreach (var factura in context.Facturas.Where(f => string.IsNullOrWhiteSpace(f.Placa)))
        {
            var galones = context.Detalles
                .Where(d => d.CodigoManguera.Trim() == factura.CodigoManguera.Trim())
                .Sum(d => d.Cantidad);
            if (galones <= galonesMaximo) continue;

            var (score, nivel) = Scoring.Calculate(riesgoBase: 60, montoInvolucrado: galones);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = carril,
                Descripcion = $"Despacho de {galones:F2} galones sin placa registrada " +
                              $"(máximo sin placa: {galonesMaximo:F0} gal; posible desvío). Doc: {factura.NumeroDocumento}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroDocumento"] = factura.NumeroDocumento,
                    ["Galones"] = galones,
                    ["GalonesMaximo"] = galonesMaximo
                }
            });
        }
        return anomalies;
    }
}
