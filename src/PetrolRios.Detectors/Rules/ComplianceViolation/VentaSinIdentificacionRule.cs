using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.ComplianceViolation;

/// <summary>
/// Venta sin identificación del cliente (cédula/RUC) en monto material. El SRI (Resolución
/// NAC-DGERCGC13-00382) exige registrar la cédula/RUC del comprador en las facturas de combustibles
/// líquidos; una venta significativa sin ese dato rompe la trazabilidad tributaria y puede encubrir
/// fraccionamiento de ventas.
/// </summary>
public sealed class VentaSinIdentificacionRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.ComplianceViolation;
    public override string Parametro => "VentaSinIdentificacionMontoMinimo";
    public override double UmbralPorDefecto => 50.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var montoMinimo = Umbral(regla);
        var carril = Carril(regla);

        var ventas = context.Facturas
            .Where(f => string.IsNullOrWhiteSpace(f.RucCliente) && f.TotalNeto > montoMinimo);

        foreach (var factura in ventas)
        {
            var (score, nivel) = Scoring.Calculate(riesgoBase: 45, montoInvolucrado: factura.TotalNeto);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = carril,
                Descripcion = $"Venta de ${factura.TotalNeto:F2} sin cédula/RUC del cliente " +
                              $"(el SRI lo exige; mínimo: ${montoMinimo:F2}). Doc: {factura.NumeroDocumento}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Fuente = factura,
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroDocumento"] = factura.NumeroDocumento,
                    ["Monto"] = factura.TotalNeto,
                    ["MontoMinimo"] = montoMinimo,
                    ["Placa"] = factura.Placa.Trim()
                }
            });
        }
        return anomalies;
    }
}
