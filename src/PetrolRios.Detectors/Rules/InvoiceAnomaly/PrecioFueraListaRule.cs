using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.InvoiceAnomaly;

/// <summary>Precio aplicado por encima del autorizado (mínimo del período por producto).</summary>
public sealed class PrecioFueraListaRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "PrecioFueraListaHabilitado";
    public override double UmbralPorDefecto => 1.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);

        var preciosPorProducto = context.Detalles
            .Where(d => d.ValorUnitario > 0)
            .GroupBy(d => d.CodigoProducto.Trim())
            .ToDictionary(g => g.Key, g => g.Min(d => d.ValorUnitario));

        foreach (var detalle in context.Detalles)
        {
            if (detalle.ValorUnitario <= 0) continue;

            var producto = detalle.CodigoProducto.Trim();
            if (!preciosPorProducto.TryGetValue(producto, out var precioBase)) continue;

            if (detalle.ValorUnitario > precioBase * 1.01)
            {
                var diferencia = detalle.ValorUnitario - precioBase;
                var (score, nivel) = Scoring.Calculate(riesgoBase: 45, montoInvolucrado: diferencia * detalle.Cantidad);
                anomalies.Add(new DetectedAnomaly
                {
                    TipoDetector = TipoDetector.InvoiceAnomaly,
                    Ambito = carril,
                    Descripcion = $"Precio fuera de lista: producto {producto} cobrado a " +
                                  $"${detalle.ValorUnitario:F2} (autorizado: ${precioBase:F2})",
                    Score = score,
                    NivelRiesgo = nivel,
                    EstacionId = context.EstacionId,
                    TransaccionReferencia = $"DESP-{detalle.NumeroDespacho}",
                    Fuente = detalle,
                    Metadata = new Dictionary<string, object>
                    {
                        ["Producto"] = producto,
                        ["PrecioAplicado"] = detalle.ValorUnitario,
                        ["PrecioAutorizado"] = precioBase,
                        ["Diferencia"] = diferencia,
                        ["Cantidad"] = detalle.Cantidad
                    }
                });
            }
        }
        return anomalies;
    }
}
