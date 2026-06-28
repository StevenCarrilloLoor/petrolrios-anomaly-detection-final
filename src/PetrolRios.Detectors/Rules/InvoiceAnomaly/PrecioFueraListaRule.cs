using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.InvoiceAnomaly;

/// <summary>
/// Precio fuera de lista. Para los combustibles REGULADOS (Extra/Ecopaís/Diésel) el precio es único
/// nacional por ley: se compara contra el PRECIO OFICIAL vigente a la fecha con tolerancia cero (± 1
/// centavo por el redondeo del POS). La Súper es libre mercado: se ignora. Si no hay precio oficial para
/// la fecha/producto (p. ej. datos históricos de otra banda), cae a una heurística: el mínimo del día.
/// </summary>
public sealed class PrecioFueraListaRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "PrecioFueraListaHabilitado";
    public override double UmbralPorDefecto => 1.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);
        // Tolerancia por el redondeo del POS: en datos reales el surtidor cobra ~$0,002 sobre el oficial
        // ($3,312 vs $3,31). Sin este epsilon, la "tolerancia cero" marcaría TODAS las ventas.
        const double Epsilon = 0.01;

        // Respaldo: mínimo del producto EN ESE DÍA (para fechas/productos sin precio oficial cargado).
        var preciosPorProductoDia = context.Detalles
            .Where(d => d.ValorUnitario > 0)
            .GroupBy(d => new { Prod = d.CodigoProducto.Trim(), Dia = d.FechaDespacho.Date })
            .ToDictionary(g => g.Key, g => g.Min(d => d.ValorUnitario));

        foreach (var detalle in context.Detalles)
        {
            if (detalle.ValorUnitario <= 0) continue;

            var producto = detalle.CodigoProducto.Trim();
            var fecha = detalle.FechaDespacho.Date;

            // 1) Precio OFICIAL regulado vigente a la fecha → tolerancia CERO (± epsilon).
            var oficial = context.PreciosOficiales.FirstOrDefault(o =>
                o.CodigoProducto == producto
                && fecha >= o.VigenteDesde.Date
                && (o.VigenteHasta == null || fecha <= o.VigenteHasta.Value.Date));
            if (oficial is not null)
            {
                if (!oficial.EsRegulado) continue; // Súper: libre mercado, excluida del detector

                var precioOficial = (double)oficial.Precio;
                var diferencia = detalle.ValorUnitario - precioOficial;
                if (Math.Abs(diferencia) > Epsilon)
                {
                    var (score, nivel) = Scoring.Calculate(
                        riesgoBase: 55, montoInvolucrado: Math.Abs(diferencia) * detalle.Cantidad);
                    var signo = diferencia > 0 ? "por encima" : "por debajo";
                    anomalies.Add(new DetectedAnomaly
                    {
                        TipoDetector = TipoDetector.InvoiceAnomaly,
                        Ambito = carril,
                        Descripcion = $"Precio fuera de lista (regulado): producto {producto} cobrado a " +
                                      $"${detalle.ValorUnitario:F3} ({signo} del oficial ${precioOficial:F2})",
                        Score = score,
                        NivelRiesgo = nivel,
                        EstacionId = context.EstacionId,
                        TransaccionReferencia = $"DESP-{detalle.NumeroDespacho}",
                        Fuente = detalle,
                        Metadata = new Dictionary<string, object>
                        {
                            ["Producto"] = producto,
                            ["PrecioAplicado"] = detalle.ValorUnitario,
                            ["PrecioOficial"] = precioOficial,
                            ["Diferencia"] = Math.Round(diferencia, 4),
                            ["Cantidad"] = detalle.Cantidad
                        }
                    });
                }
                continue; // producto con precio oficial → no usar la heurística de respaldo
            }

            // 2) Sin precio oficial → respaldo: mínimo del día (sobreprecio intra-jornada).
            if (!preciosPorProductoDia.TryGetValue(new { Prod = producto, Dia = fecha }, out var precioBase))
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
