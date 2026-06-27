using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.CashFraud;

/// <summary>Diferencia entre efectivo reportado y calculado por el sistema, superior al umbral por turno.</summary>
public sealed class DiferenciaEfectivoRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.CashFraud;
    public override string Parametro => "DiferenciaEfectivoUmbral";
    public override double UmbralPorDefecto => 50.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var umbral = Umbral(regla);
        var carril = Carril(regla);

        // Agrupar facturas por turno y sumar ventas en efectivo (COD_PAGO = "EF" o similar)
        var ventasPorTurno = context.Facturas
            .Where(f => PagoCodigos.EsEfectivo(f.CodigoPago))
            .GroupBy(f => f.NumeroTurno)
            .ToDictionary(g => g.Key, g => g.Sum(f => f.TotalNeto));

        // Agrupar depósitos en efectivo por turno
        var depositosPorTurno = context.DepositosTurno
            .Where(d => PagoCodigos.EsEfectivo(d.TipoDeposito))
            .GroupBy(d => d.NumeroTurno)
            .ToDictionary(g => g.Key, g => g.Sum(d => (double)d.Total));

        foreach (var turno in context.CierresTurno)
        {
            ventasPorTurno.TryGetValue(turno.NumeroTurno, out var ventasEfectivo);
            depositosPorTurno.TryGetValue(turno.NumeroTurno, out var depositosEfectivo);

            // Usar el faltante registrado en TURN si hay datos, o calcular la diferencia
            var diferencia = turno.Faltante > 0
                ? turno.Faltante
                : Math.Abs(ventasEfectivo - depositosEfectivo);

            if (diferencia <= umbral) continue;

            var reincidencias = context.AlertasPreviasPorEmpleado.GetValueOrDefault(turno.CodigoVendedor.Trim(), 0);
            var (score, nivel) = Scoring.Calculate(riesgoBase: 40, montoInvolucrado: diferencia, reincidenciasEmpleado: reincidencias);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.CashFraud,
                Ambito = carril,
                Descripcion = $"Diferencia de efectivo ${diferencia:F2} en turno {turno.NumeroTurno} " +
                              $"(umbral: ${umbral:F2}). Vendedor: {turno.CodigoVendedor.Trim()}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = turno.CodigoVendedor.Trim(),
                TransaccionReferencia = $"TURN-{turno.NumeroTurno}",
                Fuente = turno,
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroTurno"] = turno.NumeroTurno,
                    ["VentasEfectivo"] = ventasEfectivo,
                    ["DepositosEfectivo"] = depositosEfectivo,
                    ["Diferencia"] = diferencia,
                    ["Umbral"] = umbral
                }
            });
        }
        return anomalies;
    }
}
