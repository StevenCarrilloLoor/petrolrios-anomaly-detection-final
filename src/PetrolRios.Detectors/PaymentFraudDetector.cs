using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors;

/// <summary>
/// Detector de fraude en pagos:
/// 1. Reversión de tarjeta tardía (> N minutos después de la venta).
/// 2. Crédito otorgado sin código de autorización (excede límite).
/// 3. Transacciones duplicadas (misma tarjeta, mismo monto, < N minutos).
/// </summary>
public sealed class PaymentFraudDetector : IAnomalyDetector
{
    private readonly RiskScoringEngine _scoring;
    private readonly ILogger<PaymentFraudDetector> _logger;

    public TipoDetector Type => TipoDetector.PaymentFraud;

    public PaymentFraudDetector(RiskScoringEngine scoring, ILogger<PaymentFraudDetector> logger)
    {
        _scoring = scoring;
        _logger = logger;
    }

    public Task<IReadOnlyList<DetectedAnomaly>> DetectAsync(DetectionContext context, CancellationToken ct)
    {
        var anomalies = new List<DetectedAnomaly>();
        var reglas = context.Reglas.Where(r => r.TipoDetector == TipoDetector.PaymentFraud).ToList();

        var umbralReversionMinutos = GetUmbral(reglas, "ReversionTarjetaMinutosUmbral", 30.0);
        var creditoSinAutHabilitado = GetUmbral(reglas, "CreditoSinAutorizacionHabilitado", 1.0) >= 1.0;
        var umbralDuplicadaMinutos = GetUmbral(reglas, "DuplicadaMinutosUmbral", 5.0);

        // Regla 1: Reversión tarjeta tardía
        DetectReversionTardia(context, umbralReversionMinutos, anomalies);

        // Regla 2: Crédito sin autorización
        if (creditoSinAutHabilitado)
            DetectCreditoSinAutorizacion(context, anomalies);

        // Regla 3: Transacciones duplicadas
        DetectTransaccionesDuplicadas(context, umbralDuplicadaMinutos, anomalies);

        _logger.LogDebug("PaymentFraudDetector: {Count} anomalías en estación {Est}",
            anomalies.Count, context.EstacionNombre);

        return Task.FromResult<IReadOnlyList<DetectedAnomaly>>(anomalies);
    }

    private void DetectReversionTardia(
        DetectionContext context, double umbralMinutos, List<DetectedAnomaly> anomalies)
    {
        // Las tarjetas de turno (TURN_TARJ) con valores negativos indican reversiones.
        // Comparamos el fin del turno con la fecha de las facturas asociadas.
        var turnosPorNumero = context.CierresTurno
            .ToDictionary(t => t.NumeroTurno, t => t);

        foreach (var tarjeta in context.TarjetasTurno.Where(t => t.Valor < 0))
        {
            if (!turnosPorNumero.TryGetValue(tarjeta.NumeroTurno, out var turno))
                continue;

            // Buscar la factura original asociada al turno con pago tarjeta
            var facturasDelTurno = context.Facturas
                .Where(f => f.NumeroTurno == tarjeta.NumeroTurno)
                .OrderBy(f => f.FechaDocumento)
                .ToList();

            if (facturasDelTurno.Count == 0) continue;

            var primeraFactura = facturasDelTurno[0];
            var diferenciaMinutos = (turno.FechaFin - primeraFactura.FechaDocumento).TotalMinutes;

            if (diferenciaMinutos > umbralMinutos)
            {
                var montoReversion = Math.Abs((double)tarjeta.Valor);
                var (score, nivel) = _scoring.Calculate(
                    riesgoBase: 55,
                    montoInvolucrado: montoReversion);

                anomalies.Add(new DetectedAnomaly
                {
                    TipoDetector = TipoDetector.PaymentFraud,
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
        }
    }

    private void DetectCreditoSinAutorizacion(
        DetectionContext context, List<DetectedAnomaly> anomalies)
    {
        // Créditos otorgados (CRED_CABE) donde no hay comprobante asociado (NUMMCOMP = 0)
        // y el monto excede un umbral básico
        foreach (var credito in context.Creditos)
        {
            var sinAutorizacion = credito.NumeroComprobante == 0;
            if (!sinAutorizacion) continue;

            if (credito.TotalCredito <= 0) continue;

            var (score, nivel) = _scoring.Calculate(
                riesgoBase: 50,
                montoInvolucrado: credito.TotalCredito);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.PaymentFraud,
                Descripcion = $"Crédito de ${credito.TotalCredito:F2} otorgado sin código de autorización " +
                              $"(comprobante: {credito.NumeroComprobante}). Socio: {credito.CodigoSocio.Trim()}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                TransaccionReferencia = $"CRED_CABE-{credito.NumeroCabecera}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroCabecera"] = credito.NumeroCabecera,
                    ["CodigoSocio"] = credito.CodigoSocio.Trim(),
                    ["TotalCredito"] = credito.TotalCredito,
                    ["CodigoCredito"] = credito.CodigoCredito.Trim()
                }
            });
        }
    }

    private void DetectTransaccionesDuplicadas(
        DetectionContext context, double umbralMinutos, List<DetectedAnomaly> anomalies)
    {
        // Detectar tarjetas del mismo banco, mismo monto, en el mismo turno, con diferencia < N minutos
        // Agrupamos por turno y banco
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
                    // Verificar si están en turnos cercanos
                    var turnoA = context.CierresTurno.FirstOrDefault(t => t.NumeroTurno == items[i].NumeroTurno);
                    var turnoB = context.CierresTurno.FirstOrDefault(t => t.NumeroTurno == items[j].NumeroTurno);

                    if (turnoA is null || turnoB is null) continue;

                    var diferenciaMinutos = Math.Abs((turnoA.FechaFin - turnoB.FechaFin).TotalMinutes);
                    if (diferenciaMinutos > umbralMinutos) continue;

                    var monto = (double)grupo.Key.Valor;
                    var (score, nivel) = _scoring.Calculate(
                        riesgoBase: 45,
                        montoInvolucrado: monto);

                    anomalies.Add(new DetectedAnomaly
                    {
                        TipoDetector = TipoDetector.PaymentFraud,
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
    }

    private static double GetUmbral(IReadOnlyList<Domain.Entities.ReglaDeteccion> reglas, string parametro, double defaultValue) =>
        reglas.FirstOrDefault(r => r.ParametroNombre == parametro)?.ValorUmbral ?? defaultValue;
}
