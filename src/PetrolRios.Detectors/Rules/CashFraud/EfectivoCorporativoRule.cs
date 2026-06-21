using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.CashFraud;

/// <summary>
/// Proporción atípica de efectivo sobre clientes corporativos. En el caso documentado (enero 2026),
/// el cliente corporativo pagaba 88.4% con tarjeta; los despachadores con porcentajes de contado
/// superiores al umbral resultaron los de mayor riesgo (79.5% contado = nivel crítico). El efectivo
/// en cuentas corporativas es difícil de rastrear y facilita la apropiación indebida.
/// </summary>
public sealed class EfectivoCorporativoRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    /// <summary>Mínimo de transacciones corporativas para evaluar proporción de efectivo.</summary>
    private const int MinTransaccionesCorporativas = 4;

    public override TipoDetector Detector => TipoDetector.CashFraud;
    public override string Parametro => "EfectivoCorporativoPorcentajeUmbral";
    public override double UmbralPorDefecto => 30.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var umbralPorcentaje = Umbral(regla);
        var carril = Carril(regla);

        var corporativasPorVendedor = context.Facturas
            .Where(f => !string.IsNullOrWhiteSpace(f.CodigoCliente))
            .GroupBy(f => f.CodigoVendedor.Trim());

        foreach (var grupo in corporativasPorVendedor)
        {
            var transacciones = grupo.ToList();
            if (transacciones.Count < MinTransaccionesCorporativas) continue;

            var enEfectivo = transacciones.Where(f => PagoCodigos.EsEfectivo(f.CodigoPago)).ToList();
            var porcentajeEfectivo = (double)enEfectivo.Count / transacciones.Count * 100;
            if (porcentajeEfectivo <= umbralPorcentaje) continue;

            var montoEfectivo = enEfectivo.Sum(f => f.TotalNeto);
            var reincidencias = context.AlertasPreviasPorEmpleado.GetValueOrDefault(grupo.Key, 0);
            var (score, nivel) = Scoring.Calculate(riesgoBase: 50, montoInvolucrado: montoEfectivo, reincidenciasEmpleado: reincidencias);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.CashFraud,
                Ambito = carril,
                Descripcion = $"Proporción atípica de efectivo: vendedor {grupo.Key} con " +
                              $"{porcentajeEfectivo:F1}% de ventas corporativas en efectivo " +
                              $"({enEfectivo.Count}/{transacciones.Count}, umbral: {umbralPorcentaje:F0}%). " +
                              $"Monto en efectivo: ${montoEfectivo:F2}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = grupo.Key,
                TransaccionReferencia = $"EFCORP-{grupo.Key}",
                Metadata = new Dictionary<string, object>
                {
                    ["PorcentajeEfectivo"] = porcentajeEfectivo,
                    ["UmbralPorcentaje"] = umbralPorcentaje,
                    ["TransaccionesEfectivo"] = enEfectivo.Count,
                    ["TransaccionesTotales"] = transacciones.Count,
                    ["MontoEfectivo"] = montoEfectivo
                }
            });
        }
        return anomalies;
    }
}
