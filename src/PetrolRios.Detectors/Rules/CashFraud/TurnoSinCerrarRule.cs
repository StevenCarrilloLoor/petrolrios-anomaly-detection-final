using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.CashFraud;

/// <summary>
/// Turno sin cerrar: un turno que sigue abierto (EST_TURN = '0') desde hace más horas que el umbral.
/// Es el descuido operativo que mencionó el ingeniero ("se olvidan de cerrar turno"). Va al carril
/// Operativa (estación), no al de auditoría.
/// </summary>
public sealed class TurnoSinCerrarRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.CashFraud;
    public override string Parametro => "TurnoSinCerrarHorasUmbral";
    public override double UmbralPorDefecto => 18.0;
    public override AmbitoAlerta AmbitoPorDefecto => AmbitoAlerta.Operativa;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var umbralHoras = Umbral(regla);
        var carril = Carril(regla);

        foreach (var turno in context.CierresTurno)
        {
            // Solo turnos abiertos con fecha de inicio válida
            if (turno.EstadoTurno.Trim() != "0") continue;
            if (turno.FechaInicio.Year < 2000) continue;

            var horasAbierto = (context.ToWatermark - turno.FechaInicio).TotalHours;
            if (horasAbierto < umbralHoras) continue;

            var (score, nivel) = Scoring.Calculate(riesgoBase: 30);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.CashFraud,
                Ambito = carril,
                Descripcion = $"Turno {turno.NumeroTurno} sin cerrar: abierto hace {horasAbierto:F0} h " +
                              $"(umbral: {umbralHoras:F0} h). Vendedor: {turno.CodigoVendedor.Trim()}. " +
                              $"Revisar y cerrar el turno.",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = turno.CodigoVendedor.Trim(),
                TransaccionReferencia = $"TURN-ABIERTO-{turno.NumeroTurno}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroTurno"] = turno.NumeroTurno,
                    ["FechaInicio"] = turno.FechaInicio,
                    ["HorasAbierto"] = Math.Round(horasAbierto, 1),
                    ["UmbralHoras"] = umbralHoras
                }
            });
        }
        return anomalies;
    }
}
