using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.ComplianceViolation;

/// <summary>
/// Venta sin placa en monto mayor (Tabla 2 de la tesis: "Ventas sin placa en montos mayores"). Las
/// ventas de combustible de montos altos sin identificación del vehículo impiden la trazabilidad
/// exigida por la normativa de comercialización de combustibles.
/// </summary>
public sealed class VentaSinPlacaRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.ComplianceViolation;
    public override string Parametro => "VentaSinPlacaMontoMinimo";
    public override double UmbralPorDefecto => 200.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var montoMinimo = Umbral(regla);
        var carril = Carril(regla);

        var ventasSinPlaca = context.Facturas
            .Where(f => string.IsNullOrWhiteSpace(f.Placa) && f.TotalNeto > montoMinimo);

        foreach (var factura in ventasSinPlaca)
        {
            var reincidencias = context.AlertasPreviasPorEmpleado.GetValueOrDefault(factura.CodigoVendedor.Trim(), 0);
            var (score, nivel) = Scoring.Calculate(riesgoBase: 50, montoInvolucrado: factura.TotalNeto, reincidenciasEmpleado: reincidencias);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = carril,
                Descripcion = $"Venta de ${factura.TotalNeto:F2} sin placa registrada " +
                              $"(monto mínimo que exige placa: ${montoMinimo:F2}). " +
                              $"Doc: {factura.NumeroDocumento}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroDocumento"] = factura.NumeroDocumento,
                    ["Monto"] = factura.TotalNeto,
                    ["MontoMinimo"] = montoMinimo,
                    ["Cliente"] = factura.CodigoCliente.Trim()
                }
            });
        }
        return anomalies;
    }
}
