using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.PaymentFraud;

/// <summary>Transacciones duplicadas: misma tarjeta (banco) y monto en turnos a menos de N minutos.</summary>
public sealed class TransaccionesDuplicadasRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.PaymentFraud;
    public override string Parametro => "DuplicadaMinutosUmbral";
    public override double UmbralPorDefecto => 5.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var umbralMinutos = Umbral(regla);
        var carril = Carril(regla);

        // Tarjetas del mismo banco, mismo monto, en turnos con diferencia < N minutos
        var tarjetasAgrupadas = context.TarjetasTurno
            .Where(t => t.Valor > 0)
            .GroupBy(t => new { t.CodigoBanco, Valor = t.Valor })
            .Where(g => g.Count() > 1);

        foreach (var grupo in tarjetasAgrupadas)
        {
            var items = grupo.OrderBy(t => t.NumeroTarjetaTurno).ToList();

            for (var i = 0; i < items.Count - 1; i++)
            {
                for (var j = i + 1; j < items.Count; j++)
                {
                    var turnoA = context.CierresTurno.FirstOrDefault(t => t.NumeroTurno == items[i].NumeroTurno);
                    var turnoB = context.CierresTurno.FirstOrDefault(t => t.NumeroTurno == items[j].NumeroTurno);
                    if (turnoA is null || turnoB is null) continue;

                    var diferenciaMinutos = Math.Abs((turnoA.FechaFin - turnoB.FechaFin).TotalMinutes);
                    if (diferenciaMinutos > umbralMinutos) continue;

                    var monto = (double)grupo.Key.Valor;
                    var (score, nivel) = Scoring.Calculate(riesgoBase: 45, montoInvolucrado: monto);
                    anomalies.Add(new DetectedAnomaly
                    {
                        TipoDetector = TipoDetector.PaymentFraud,
                        Ambito = carril,
                        Descripcion = $"Transacciones duplicadas: tarjeta {grupo.Key.CodigoBanco.Trim()} " +
                                      $"con monto ${monto:F2}, diferencia {diferenciaMinutos:F0} min " +
                                      $"(umbral: {umbralMinutos} min)",
                        Score = score,
                        NivelRiesgo = nivel,
                        EstacionId = context.EstacionId,
                        TransaccionReferencia = $"DUP-{items[i].NumeroTarjetaTurno}-{items[j].NumeroTarjetaTurno}",
                        Metadata = new Dictionary<string, object>
                        {
                            ["CodigoBanco"] = grupo.Key.CodigoBanco.Trim(),
                            ["Monto"] = monto,
                            ["DiferenciaMinutos"] = diferenciaMinutos,
                            ["TarjetaA"] = items[i].NumeroTarjetaTurno,
                            ["TarjetaB"] = items[j].NumeroTarjetaTurno
                        }
                    });
                }
            }
        }
        return anomalies;
    }
}
