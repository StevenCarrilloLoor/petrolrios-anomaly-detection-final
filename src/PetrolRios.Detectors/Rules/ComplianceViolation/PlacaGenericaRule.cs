using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.ComplianceViolation;

/// <summary>Venta excesiva a la placa genérica ZZZ999949 por encima del cupo regulatorio (ARCERNNR).</summary>
public sealed class PlacaGenericaRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    private const string PlacaGenerica = "ZZZ999949";

    public override TipoDetector Detector => TipoDetector.ComplianceViolation;
    public override string Parametro => "PlacaGenericaGalonesMaximo";
    public override double UmbralPorDefecto => 5.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var galonesMaximo = Umbral(regla);
        var carril = Carril(regla);

        // La placa viene de DCTO.PLA_DCTO y los galones de DESP.CAN_DESP. El despacho de cada factura se
        // localiza por el vínculo factura↔despacho de la base (CodigoCliente + cercanía temporal): así se
        // miden los galones de ESE despacho, no la suma de todo el día del cliente (que sobrecontaba).
        var despachosPorCliente = IndexarDespachosPorCliente(context.Detalles);
        var facturasPlacaGenerica = context.Facturas
            .Where(f => f.Placa.Trim().Equals(PlacaGenerica, StringComparison.OrdinalIgnoreCase));

        foreach (var factura in facturasPlacaGenerica)
        {
            var despacho = DespachoDeFactura(factura, despachosPorCliente);
            var galones = despacho?.Cantidad ?? 0;
            if (galones <= 0) galones = factura.TotalNeto; // Fallback al monto si no se ubica el despacho
            if (galones <= galonesMaximo) continue;

            var (score, nivel) = Scoring.Calculate(riesgoBase: 65, montoInvolucrado: galones);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = carril,
                Descripcion = $"Placa genérica {PlacaGenerica} con {galones:F2} galones " +
                              $"(máximo regulatorio: {galonesMaximo} gal). Doc: {factura.NumeroDocumento}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Fuente = factura,
                Metadata = new Dictionary<string, object>
                {
                    ["Placa"] = PlacaGenerica,
                    ["Galones"] = galones,
                    ["GalonesMaximo"] = galonesMaximo,
                    ["NumeroDocumento"] = factura.NumeroDocumento
                }
            });
        }
        return anomalies;
    }
}
