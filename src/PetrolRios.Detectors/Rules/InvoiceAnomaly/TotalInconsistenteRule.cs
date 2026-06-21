using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.InvoiceAnomaly;

/// <summary>Total que no cuadra con subtotal − descuento + IVA (manipulación documental).</summary>
public sealed class TotalInconsistenteRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    private const double ToleranciaTotal = 0.05;

    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "TotalInconsistenteHabilitado";
    public override double UmbralPorDefecto => 1.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);

        foreach (var factura in context.Facturas)
        {
            if (factura.Subtotal <= 0) continue;

            var totalEsperado = factura.Subtotal - factura.Descuento + factura.Iva;
            var diferencia = Math.Abs(factura.TotalNeto - totalEsperado);
            if (diferencia <= ToleranciaTotal) continue;

            var reincidencias = context.AlertasPreviasPorEmpleado.GetValueOrDefault(factura.CodigoVendedor.Trim(), 0);
            var (score, nivel) = Scoring.Calculate(riesgoBase: 55, montoInvolucrado: diferencia, reincidenciasEmpleado: reincidencias);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Ambito = carril,
                Descripcion = $"Total inconsistente en documento {factura.NumeroDocumento}: " +
                              $"registrado ${factura.TotalNeto:F2}, esperado ${totalEsperado:F2} " +
                              $"(diferencia ${diferencia:F2})",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroDocumento"] = factura.NumeroDocumento,
                    ["TotalRegistrado"] = factura.TotalNeto,
                    ["TotalEsperado"] = totalEsperado,
                    ["Diferencia"] = diferencia,
                    ["Subtotal"] = factura.Subtotal,
                    ["Descuento"] = factura.Descuento,
                    ["Iva"] = factura.Iva
                }
            });
        }
        return anomalies;
    }
}
