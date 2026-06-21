using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.InvoiceAnomaly;

/// <summary>Descuento que excede el porcentaje máximo de la política comercial.</summary>
public sealed class DescuentoExcesivoRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "DescuentoPorcentajeMaximo";
    public override double UmbralPorDefecto => 10.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var porcentajeMaximo = Umbral(regla);
        var carril = Carril(regla);

        foreach (var factura in context.Facturas)
        {
            if (factura.Subtotal <= 0 || factura.Descuento <= 0) continue;

            var porcentajeDescuento = factura.Descuento / factura.Subtotal * 100;
            if (porcentajeDescuento <= porcentajeMaximo) continue;

            var reincidencias = context.AlertasPreviasPorEmpleado.GetValueOrDefault(factura.CodigoVendedor.Trim(), 0);
            var (score, nivel) = Scoring.Calculate(riesgoBase: 45, montoInvolucrado: factura.Descuento, reincidenciasEmpleado: reincidencias);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Ambito = carril,
                Descripcion = $"Descuento excesivo: {porcentajeDescuento:F1}% sobre subtotal " +
                              $"(máximo permitido: {porcentajeMaximo:F0}%). " +
                              $"Doc: {factura.NumeroDocumento}, descuento ${factura.Descuento:F2}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroDocumento"] = factura.NumeroDocumento,
                    ["PorcentajeDescuento"] = porcentajeDescuento,
                    ["PorcentajeMaximo"] = porcentajeMaximo,
                    ["MontoDescuento"] = factura.Descuento,
                    ["Subtotal"] = factura.Subtotal
                }
            });
        }
        return anomalies;
    }
}
