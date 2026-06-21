using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.InvoiceAnomaly;

/// <summary>Tasa de anulaciones excesiva (global y por vendedor) sobre las transacciones del período.</summary>
public sealed class TasaAnulacionesRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "AnulacionesPorcentajeUmbral";
    public override double UmbralPorDefecto => 3.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        if (context.Anulaciones.Count == 0 || context.Facturas.Count == 0) return anomalies;

        var umbralPorcentaje = Umbral(regla);
        var carril = Carril(regla);

        var facturasPorVendedor = context.Facturas
            .GroupBy(f => f.CodigoVendedor.Trim())
            .ToDictionary(g => g.Key, g => g.Count());

        var totalFacturas = context.Facturas.Count;
        var totalAnulaciones = context.Anulaciones.Count;
        var tasaGlobal = totalFacturas > 0 ? (double)totalAnulaciones / totalFacturas * 100 : 0;

        if (tasaGlobal > umbralPorcentaje)
        {
            var (score, nivel) = Scoring.Calculate(riesgoBase: 35, montoInvolucrado: 0);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Ambito = carril,
                Descripcion = $"Tasa de anulaciones excesiva: {tasaGlobal:F1}% " +
                              $"({totalAnulaciones}/{totalFacturas}) supera umbral de {umbralPorcentaje}%",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                Metadata = new Dictionary<string, object>
                {
                    ["TotalAnulaciones"] = totalAnulaciones,
                    ["TotalFacturas"] = totalFacturas,
                    ["TasaPorcentaje"] = tasaGlobal,
                    ["UmbralPorcentaje"] = umbralPorcentaje
                }
            });
        }

        foreach (var (vendedor, cantidadFacturas) in facturasPorVendedor)
        {
            if (cantidadFacturas < 5) continue;

            var anulacionesEstimadas = totalFacturas > 0
                ? (double)totalAnulaciones * cantidadFacturas / totalFacturas
                : 0;
            var tasaVendedor = cantidadFacturas > 0 ? anulacionesEstimadas / cantidadFacturas * 100 : 0;

            if (tasaVendedor > umbralPorcentaje * 2)
            {
                var reincidencias = context.AlertasPreviasPorEmpleado.GetValueOrDefault(vendedor, 0);
                var (score, nivel) = Scoring.Calculate(riesgoBase: 40, reincidenciasEmpleado: reincidencias);
                anomalies.Add(new DetectedAnomaly
                {
                    TipoDetector = TipoDetector.InvoiceAnomaly,
                    Ambito = carril,
                    Descripcion = $"Vendedor {vendedor} con tasa de anulaciones elevada: " +
                                  $"{tasaVendedor:F1}% ({cantidadFacturas} facturas)",
                    Score = score,
                    NivelRiesgo = nivel,
                    EstacionId = context.EstacionId,
                    EmpleadoCodigo = vendedor,
                    Metadata = new Dictionary<string, object>
                    {
                        ["Vendedor"] = vendedor,
                        ["FacturasVendedor"] = cantidadFacturas,
                        ["TasaEstimada"] = tasaVendedor
                    }
                });
            }
        }
        return anomalies;
    }
}
