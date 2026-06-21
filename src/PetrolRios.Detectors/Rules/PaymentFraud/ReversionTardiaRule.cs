using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.PaymentFraud;

/// <summary>Reversión de tarjeta tardía: más de N minutos después de la venta original.</summary>
public sealed class ReversionTardiaRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.PaymentFraud;
    public override string Parametro => "ReversionTarjetaMinutosUmbral";
    public override double UmbralPorDefecto => 30.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var umbralMinutos = Umbral(regla);
        var carril = Carril(regla);

        // Las tarjetas de turno (TURN_TARJ) con valores negativos indican reversiones.
        // Comparamos el fin del turno con la fecha de las facturas asociadas.
        // GroupBy tolera turnos duplicados (posibles por reenvíos store-and-forward del agente).
        var turnosPorNumero = context.CierresTurno
            .GroupBy(t => t.NumeroTurno)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var tarjeta in context.TarjetasTurno.Where(t => t.Valor < 0))
        {
            if (!turnosPorNumero.TryGetValue(tarjeta.NumeroTurno, out var turno)) continue;

            // Buscar la factura original asociada al turno con pago tarjeta
            var facturasDelTurno = context.Facturas
                .Where(f => f.NumeroTurno == tarjeta.NumeroTurno)
                .OrderBy(f => f.FechaDocumento)
                .ToList();
            if (facturasDelTurno.Count == 0) continue;

            var primeraFactura = facturasDelTurno[0];
            var diferenciaMinutos = (turno.FechaFin - primeraFactura.FechaDocumento).TotalMinutes;
            if (diferenciaMinutos <= umbralMinutos) continue;

            var montoReversion = Math.Abs((double)tarjeta.Valor);
            var (score, nivel) = Scoring.Calculate(riesgoBase: 55, montoInvolucrado: montoReversion);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.PaymentFraud,
                Ambito = carril,
                Descripcion = $"Reversión de tarjeta tardía: {diferenciaMinutos:F0} minutos después " +
                              $"de la venta (umbral: {umbralMinutos} min). Monto: ${montoReversion:F2}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = turno.CodigoVendedor.Trim(),
                TransaccionReferencia = $"TURN_TARJ-{tarjeta.NumeroTarjetaTurno}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroTurno"] = tarjeta.NumeroTurno,
                    ["DiferenciaMinutos"] = diferenciaMinutos,
                    ["UmbralMinutos"] = umbralMinutos,
                    ["MontoReversion"] = montoReversion,
                    ["CodigoBanco"] = tarjeta.CodigoBanco.Trim()
                }
            });
        }
        return anomalies;
    }
}
