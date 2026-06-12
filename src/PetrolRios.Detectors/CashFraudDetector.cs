using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors;

/// <summary>
/// Detector de fraude en efectivo:
/// 1. Diferencia entre efectivo reportado y calculado por el sistema supera umbral por turno.
/// 2. Mismo empleado con faltantes recurrentes (patrón gineteo).
/// 3. Venta a crédito sin cliente identificado (posible venta en efectivo registrada como crédito).
/// 4. Proporción atípica de pagos en efectivo sobre clientes corporativos
///    (patrón del caso documentado enero 2026: despachador con 79.5% contado = crítico).
/// </summary>
public sealed class CashFraudDetector : IAnomalyDetector
{
    private static readonly string[] CodigosPagoEfectivo = ["EF", "EFE"];
    private static readonly string[] CodigosPagoCredito = ["CR", "CRE", "CRD"];

    /// <summary>Mínimo de transacciones corporativas para evaluar proporción de efectivo.</summary>
    private const int MinTransaccionesCorporativas = 4;

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
        var maxFaltantes = GetUmbral(reglas, "FaltantesRecurrentesMaximo", 3.0);
        var diasFaltantes = GetUmbral(reglas, "FaltantesRecurrentesDias", 30.0) ?? 30.0;
        var creditoSinClienteHabilitado = GetUmbral(reglas, "CreditoSinClienteHabilitado", 1.0) >= 1.0;
        var umbralEfectivoCorporativo = GetUmbral(reglas, "EfectivoCorporativoPorcentajeUmbral", 30.0);

        // Regla 1: Diferencia efectivo vs sistema por turno
        if (umbralDiferencia is not null)
            DetectDiferenciaEfectivo(context, umbralDiferencia.Value, anomalies);

        // Regla 2: Patrón de faltantes recurrentes (gineteo)
        if (maxFaltantes is not null)
            DetectFaltantesRecurrentes(context, (int)maxFaltantes.Value, (int)diasFaltantes, anomalies);

        // Regla 3: Venta a crédito sin cliente identificado
        if (creditoSinClienteHabilitado)
            DetectCreditoSinClienteIdentificado(context, anomalies);

        // Regla 4: Proporción atípica de efectivo en clientes corporativos
        if (umbralEfectivoCorporativo is not null)
            DetectEfectivoCorporativoAtipico(context, umbralEfectivoCorporativo.Value, anomalies);

        _logger.LogDebug("CashFraudDetector: {Count} anomalías en estación {Est}",
            anomalies.Count, context.EstacionNombre);

        return Task.FromResult<IReadOnlyList<DetectedAnomaly>>(anomalies);
    }

    private void DetectDiferenciaEfectivo(
        DetectionContext context, double umbral, List<DetectedAnomaly> anomalies)
    {
        // Agrupar facturas por turno y sumar ventas en efectivo (COD_PAGO = "EF" o similar)
        var ventasPorTurno = context.Facturas
            .Where(f => EsPagoEfectivo(f.CodigoPago))
            .GroupBy(f => f.NumeroTurno)
            .ToDictionary(g => g.Key, g => g.Sum(f => f.TotalNeto));

        // Agrupar depósitos en efectivo por turno
        var depositosPorTurno = context.DepositosTurno
            .Where(d => EsPagoEfectivo(d.TipoDeposito))
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

    /// <summary>
    /// Venta a crédito sin cliente identificado: un crédito sin deudor identificable es
    /// incobrable, lo que sugiere que la venta fue en efectivo y el empleado retuvo el dinero
    /// (Tabla 2 de la tesis: "Ventas en efectivo registradas como crédito").
    /// </summary>
    private void DetectCreditoSinClienteIdentificado(
        DetectionContext context, List<DetectedAnomaly> anomalies)
    {
        var facturasCredito = context.Facturas
            .Where(f => EsPagoCredito(f.CodigoPago))
            .Where(f => string.IsNullOrWhiteSpace(f.CodigoCliente)
                     || string.IsNullOrWhiteSpace(f.RucCliente));

        foreach (var factura in facturasCredito)
        {
            var reincidencias = context.AlertasPreviasPorEmpleado
                .GetValueOrDefault(factura.CodigoVendedor.Trim(), 0);

            var (score, nivel) = _scoring.Calculate(
                riesgoBase: 55,
                montoInvolucrado: factura.TotalNeto,
                reincidenciasEmpleado: reincidencias);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.CashFraud,
                Descripcion = $"Venta a crédito de ${factura.TotalNeto:F2} sin cliente identificado " +
                              $"(posible venta en efectivo registrada como crédito). Doc: {factura.NumeroDocumento}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroDocumento"] = factura.NumeroDocumento,
                    ["MontoCredito"] = factura.TotalNeto,
                    ["CodigoPago"] = factura.CodigoPago.Trim(),
                    ["ClienteVacio"] = string.IsNullOrWhiteSpace(factura.CodigoCliente),
                    ["RucVacio"] = string.IsNullOrWhiteSpace(factura.RucCliente)
                }
            });
        }
    }

    /// <summary>
    /// Proporción atípica de efectivo sobre clientes corporativos. En el caso documentado
    /// (enero 2026), el cliente corporativo pagaba 88.4% con tarjeta; los despachadores con
    /// porcentajes de contado superiores al umbral resultaron los de mayor riesgo
    /// (79.5% contado = nivel crítico). El efectivo en cuentas corporativas es difícil de
    /// rastrear y facilita la apropiación indebida.
    /// </summary>
    private void DetectEfectivoCorporativoAtipico(
        DetectionContext context, double umbralPorcentaje, List<DetectedAnomaly> anomalies)
    {
        var corporativasPorVendedor = context.Facturas
            .Where(f => !string.IsNullOrWhiteSpace(f.CodigoCliente))
            .GroupBy(f => f.CodigoVendedor.Trim());

        foreach (var grupo in corporativasPorVendedor)
        {
            var transacciones = grupo.ToList();
            if (transacciones.Count < MinTransaccionesCorporativas) continue;

            var enEfectivo = transacciones.Where(f => EsPagoEfectivo(f.CodigoPago)).ToList();
            var porcentajeEfectivo = (double)enEfectivo.Count / transacciones.Count * 100;

            if (porcentajeEfectivo <= umbralPorcentaje) continue;

            var montoEfectivo = enEfectivo.Sum(f => f.TotalNeto);
            var reincidencias = context.AlertasPreviasPorEmpleado
                .GetValueOrDefault(grupo.Key, 0);

            var (score, nivel) = _scoring.Calculate(
                riesgoBase: 50,
                montoInvolucrado: montoEfectivo,
                reincidenciasEmpleado: reincidencias);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.CashFraud,
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
    }

    private static bool EsPagoEfectivo(string codigoPago) =>
        CodigosPagoEfectivo.Contains(codigoPago.Trim(), StringComparer.OrdinalIgnoreCase);

    private static bool EsPagoCredito(string codigoPago) =>
        CodigosPagoCredito.Contains(codigoPago.Trim(), StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Obtiene el umbral de una regla. Devuelve null si la regla existe pero está desactivada
    /// (la regla no debe ejecutarse). Si la regla no existe, usa el valor por defecto.
    /// </summary>
    private static double? GetUmbral(
        IReadOnlyList<Domain.Entities.ReglaDeteccion> reglas, string parametro, double defaultValue) =>
        reglas.FirstOrDefault(r => r.ParametroNombre == parametro) is { } regla
            ? (regla.Activa ? regla.ValorUmbral : null)
            : defaultValue;
}
