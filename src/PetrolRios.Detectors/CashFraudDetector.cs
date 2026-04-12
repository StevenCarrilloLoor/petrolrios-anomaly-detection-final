using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors;

/// <summary>
/// Detector de fraude en efectivo:
/// 1. Diferencia entre efectivo reportado y calculado por el sistema supera umbral por turno.
/// 2. Mismo empleado con faltantes recurrentes (patrón gineteo).
/// </summary>
public sealed class CashFraudDetector : IAnomalyDetector
{
    private readonly RiskScoringEngine _scoring;
    private readonly ILogger<CashFraudDetector> _logger;

    public TipoDetector Type => TipoDetector.CashFraud;

    public CashFraudDetector(RiskScoringEngine scoring, ILogger<CashFraudDetector> logger)
    {
        _scoring = scoring;
        _logger = logger;
    }

    public Task<IReadOnlyList<DetectedAnomaly>> DetectAsync(DetectionContext context, CancellationToken ct)
    {
        var anomalies = new List<DetectedAnomaly>();
        var reglas = context.Reglas.Where(r => r.TipoDetector == TipoDetector.CashFraud).ToList();

        var umbralDiferencia = GetUmbral(reglas, "DiferenciaEfectivoUmbral", 50.0);
        var maxFaltantes = (int)GetUmbral(reglas, "FaltantesRecurrentesMaximo", 3.0);
        var diasFaltantes = (int)GetUmbral(reglas, "FaltantesRecurrentesDias", 30.0);

        // Regla 1: Diferencia efectivo vs sistema por turno
        DetectDiferenciaEfectivo(context, umbralDiferencia, anomalies);

        // Regla 2: Patrón de faltantes recurrentes (gineteo)
        DetectFaltantesRecurrentes(context, maxFaltantes, diasFaltantes, anomalies);

        _logger.LogDebug("CashFraudDetector: {Count} anomalías en estación {Est}",
            anomalies.Count, context.EstacionNombre);

        return Task.FromResult<IReadOnlyList<DetectedAnomaly>>(anomalies);
    }

    private void DetectDiferenciaEfectivo(
        DetectionContext context, double umbral, List<DetectedAnomaly> anomalies)
    {
        // Agrupar facturas por turno y sumar ventas en efectivo (COD_PAGO = "EF" o similar)
        var ventasPorTurno = context.Facturas
            .Where(f => f.CodigoPago.Trim().Equals("EF", StringComparison.OrdinalIgnoreCase)
                     || f.CodigoPago.Trim().Equals("EFE", StringComparison.OrdinalIgnoreCase))
            .GroupBy(f => f.NumeroTurno)
            .ToDictionary(g => g.Key, g => g.Sum(f => f.TotalNeto));

        // Agrupar depósitos en efectivo por turno
        var depositosPorTurno = context.DepositosTurno
            .Where(d => d.TipoDeposito.Trim().Equals("EF", StringComparison.OrdinalIgnoreCase)
                     || d.TipoDeposito.Trim().Equals("EFE", StringComparison.OrdinalIgnoreCase))
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

            if (diferencia > umbral)
            {
                var reincidencias = context.AlertasPreviasPorEmpleado
                    .GetValueOrDefault(turno.CodigoVendedor.Trim(), 0);

                var (score, nivel) = _scoring.Calculate(
                    riesgoBase: 40,
                    montoInvolucrado: diferencia,
                    reincidenciasEmpleado: reincidencias);

                anomalies.Add(new DetectedAnomaly
                {
                    TipoDetector = TipoDetector.CashFraud,
                    Descripcion = $"Diferencia de efectivo ${diferencia:F2} en turno {turno.NumeroTurno} " +
                                  $"(umbral: ${umbral:F2}). Vendedor: {turno.CodigoVendedor.Trim()}",
                    Score = score,
                    NivelRiesgo = nivel,
                    EstacionId = context.EstacionId,
                    EmpleadoCodigo = turno.CodigoVendedor.Trim(),
                    TransaccionReferencia = $"TURN-{turno.NumeroTurno}",
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
        }
    }

    private void DetectFaltantesRecurrentes(
        DetectionContext context, int maxFaltantes, int diasFaltantes, List<DetectedAnomaly> anomalies)
    {
        // Contar turnos con faltante > 0 por empleado en el período actual
        var faltantesPorEmpleado = context.CierresTurno
            .Where(t => t.Faltante > 0)
            .GroupBy(t => t.CodigoVendedor.Trim())
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var (empleado, turnos) in faltantesPorEmpleado)
        {
            // Sumar las alertas previas del histórico
            var alertasPrevias = context.AlertasPreviasPorEmpleado
                .GetValueOrDefault(empleado, 0);
            var totalFaltantes = turnos.Count + alertasPrevias;

            if (totalFaltantes >= maxFaltantes)
            {
                var montoTotal = turnos.Sum(t => t.Faltante);
                var (score, nivel) = _scoring.Calculate(
                    riesgoBase: 60,
                    montoInvolucrado: montoTotal,
                    reincidenciasEmpleado: alertasPrevias);

                anomalies.Add(new DetectedAnomaly
                {
                    TipoDetector = TipoDetector.CashFraud,
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
        }
    }

    private static double GetUmbral(IReadOnlyList<Domain.Entities.ReglaDeteccion> reglas, string parametro, double defaultValue) =>
        reglas.FirstOrDefault(r => r.ParametroNombre == parametro)?.ValorUmbral ?? defaultValue;
}
