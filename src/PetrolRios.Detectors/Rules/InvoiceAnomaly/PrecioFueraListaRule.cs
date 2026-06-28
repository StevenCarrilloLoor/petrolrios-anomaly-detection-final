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

        // El precio "autorizado" es el mínimo del producto EN ESE DÍA (no del lote/mes entero). El precio del
        // combustible cambia legítimamente entre días; comparar contra el mínimo histórico marcaba TODAS las
        // ventas al precio nuevo como "fuera de lista" (miles de falsos positivos sobre un mes de datos reales).
        // Por (producto, día), solo se marca cobrar por encima del precio del día = sobreprecio real intra-jornada.
        var preciosPorProductoDia = context.Detalles
            .Where(d => d.ValorUnitario > 0)
            .GroupBy(d => new { Prod = d.CodigoProducto.Trim(), Dia = d.FechaDespacho.Date })
            .ToDictionary(g => g.Key, g => g.Min(d => d.ValorUnitario));

        foreach (var detalle in context.Detalles)
        {
            if (detalle.ValorUnitario <= 0) continue;

            var producto = detalle.CodigoProducto.Trim();
            if (!preciosPorProductoDia.TryGetValue(
                    new { Prod = producto, Dia = detalle.FechaDespacho.Date }, out var precioBase))
                continue;

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
