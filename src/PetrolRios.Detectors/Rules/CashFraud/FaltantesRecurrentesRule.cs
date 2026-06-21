using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.CashFraud;

/// <summary>Mismo empleado con faltantes recurrentes en la ventana de días (patrón de gineteo).</summary>
public sealed class FaltantesRecurrentesRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.CashFraud;
    public override string Parametro => "FaltantesRecurrentesMaximo";
    public override double UmbralPorDefecto => 3.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var maxFaltantes = (int)Umbral(regla);
        var diasFaltantes = (int)DiasVentana(context);
        var carril = Carril(regla);

        // Contar turnos con faltante > 0 por empleado en el período actual
        var faltantesPorEmpleado = context.CierresTurno
            .Where(t => t.Faltante > 0)
            .GroupBy(t => t.CodigoVendedor.Trim())
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var (empleado, turnos) in faltantesPorEmpleado)
        {
            // Sumar las alertas previas del histórico
            var alertasPrevias = context.AlertasPreviasPorEmpleado.GetValueOrDefault(empleado, 0);
            var totalFaltantes = turnos.Count + alertasPrevias;
            if (totalFaltantes < maxFaltantes) continue;

            var montoTotal = turnos.Sum(t => t.Faltante);
            var (score, nivel) = Scoring.Calculate(riesgoBase: 60, montoInvolucrado: montoTotal, reincidenciasEmpleado: alertasPrevias);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.CashFraud,
                Ambito = carril,
                Descripcion = $"Patrón de gineteo: empleado {empleado} con {totalFaltantes} faltantes " +
                              $"(máximo permitido: {maxFaltantes}) en los últimos {diasFaltantes} días",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = empleado,
                TransaccionReferencia = $"GINETEO-{empleado}",
                Metadata = new Dictionary<string, object>
                {
                    ["TotalFaltantes"] = totalFaltantes,
                    ["AlertasPrevias"] = alertasPrevias,
                    ["FaltantesActuales"] = turnos.Count,
                    ["MontoTotalFaltantes"] = montoTotal,
                    ["DiasEvaluados"] = diasFaltantes
                }
            });
        }
        return anomalies;
    }

    /// <summary>
    /// Ventana en días (parámetro secundario "FaltantesRecurrentesDias"). Usa su valor configurado
    /// si la regla auxiliar está activa; en caso contrario, 30 días por defecto.
    /// </summary>
    private static double DiasVentana(DetectionContext context)
    {
        var aux = context.Reglas.FirstOrDefault(r => r.ParametroNombre == "FaltantesRecurrentesDias");
        return aux is { Activa: true } ? aux.ValorUmbral : 30.0;
    }
}
