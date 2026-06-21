using Microsoft.Extensions.Logging;
using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors;

/// <summary>
/// Detector de fraude en pagos:
/// 1. Reversión de tarjeta tardía (> N minutos después de la venta).
/// 2. Crédito otorgado sin código de autorización (excede límite).
/// 3. Transacciones duplicadas (misma tarjeta, mismo monto, < N minutos).
/// 4. Despachos rápidos sucesivos al mismo cliente (patrón del caso documentado
///    enero 2026: 40 transacciones con menos de 10 minutos entre despachos).
/// </summary>
public sealed class PaymentFraudDetector : IAnomalyDetector
{
    /// <summary>Mínimo de despachos consecutivos rápidos para generar alerta.</summary>
    private const int MinDespachosRapidos = 3;

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
        var creditoSinGaranteHabilitado = GetUmbral(reglas, "CreditoSinGaranteHabilitado", 1.0) >= 1.0;
        var umbralDuplicadaMinutos = GetUmbral(reglas, "DuplicadaMinutosUmbral", 5.0);
        var umbralDespachosRapidosMinutos = GetUmbral(reglas, "DespachosRapidosMinutosUmbral", 10.0);

        // Regla 1: Reversión tarjeta tardía
        if (umbralReversionMinutos is not null)
            DetectReversionTardia(context, umbralReversionMinutos.Value, anomalies);

        // Regla 2: Crédito sin autorización
        if (creditoSinAutHabilitado)
            DetectCreditoSinAutorizacion(context, anomalies);

        // Regla 2b: Crédito sin garante (autorización indebida — patrón del ingeniero)
        if (creditoSinGaranteHabilitado)
            DetectCreditoSinGarante(context, anomalies);

        // Regla 3: Transacciones duplicadas
        if (umbralDuplicadaMinutos is not null)
            DetectTransaccionesDuplicadas(context, umbralDuplicadaMinutos.Value, anomalies);

        // Regla 4: Despachos rápidos sucesivos al mismo cliente
        if (umbralDespachosRapidosMinutos is not null)
            DetectDespachosRapidosSucesivos(context, umbralDespachosRapidosMinutos.Value, anomalies);

        _logger.LogDebug("PaymentFraudDetector: {Count} anomalías en estación {Est}",
            anomalies.Count, context.EstacionNombre);

        return Task.FromResult<IReadOnlyList<DetectedAnomaly>>(anomalies);
    }

    private void DetectReversionTardia(
        DetectionContext context, double umbralMinutos, List<DetectedAnomaly> anomalies)
    {
        // Las tarjetas de turno (TURN_TARJ) con valores negativos indican reversiones.
        // Comparamos el fin del turno con la fecha de las facturas asociadas.
        // GroupBy tolera turnos duplicados (posibles por reenvíos store-and-forward del agente).
        var turnosPorNumero = context.CierresTurno
            .GroupBy(t => t.NumeroTurno)
            .ToDictionary(g => g.Key, g => g.First());

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
                    Ambito = GetAmbito(context.Reglas, "ReversionTarjetaMinutosUmbral", AmbitoAlerta.Auditoria),
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
                Ambito = GetAmbito(context.Reglas, "CreditoSinAutorizacionHabilitado", AmbitoAlerta.Auditoria),
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

    /// <summary>
    /// Crédito otorgado sin garante (COD_GARA vacío). Un crédito sin respaldo de garante sugiere
    /// una autorización indebida — el patrón que mencionó el ingeniero ("autorizan créditos a
    /// personas que no están autorizadas"). Es incobrable y de alto riesgo.
    /// </summary>
    private void DetectCreditoSinGarante(
        DetectionContext context, List<DetectedAnomaly> anomalies)
    {
        foreach (var credito in context.Creditos)
        {
            if (credito.TotalCredito <= 0) continue;
            if (!string.IsNullOrWhiteSpace(credito.CodigoGarante)) continue;

            var (score, nivel) = _scoring.Calculate(
                riesgoBase: 55,
                montoInvolucrado: credito.TotalCredito);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.PaymentFraud,
                Ambito = GetAmbito(context.Reglas, "CreditoSinGaranteHabilitado", AmbitoAlerta.Auditoria),
                Descripcion = $"Crédito de ${credito.TotalCredito:F2} otorgado SIN garante al socio " +
                              $"{credito.CodigoSocio.Trim()} (riesgo de autorización indebida). " +
                              $"Crédito {credito.NumeroCabecera}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                TransaccionReferencia = $"CRED-SINGAR-{credito.NumeroCabecera}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroCabecera"] = credito.NumeroCabecera,
                    ["CodigoSocio"] = credito.CodigoSocio.Trim(),
                    ["TotalCredito"] = credito.TotalCredito
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
                        Ambito = GetAmbito(context.Reglas, "DuplicadaMinutosUmbral", AmbitoAlerta.Auditoria),
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

    /// <summary>
    /// Despachos rápidos sucesivos: el mismo cliente con 3 o más transacciones consecutivas
    /// separadas por menos de N minutos. En el caso documentado (enero 2026), el 33.9% de las
    /// transacciones del cliente investigado ocurrieron con menos de 10 minutos entre despachos,
    /// un patrón físicamente improbable para vehículos reales que sugiere facturación ficticia.
    /// </summary>
    private void DetectDespachosRapidosSucesivos(
        DetectionContext context, double umbralMinutos, List<DetectedAnomaly> anomalies)
    {
        var facturasPorCliente = context.Facturas
            .Where(f => !string.IsNullOrWhiteSpace(f.CodigoCliente))
            .GroupBy(f => f.CodigoCliente.Trim());

        foreach (var grupo in facturasPorCliente)
        {
            var ordenadas = grupo.OrderBy(f => f.FechaDocumento).ToList();
            if (ordenadas.Count < MinDespachosRapidos) continue;

            // Buscar rachas de transacciones consecutivas con gap < umbral
            var racha = new List<FacturaDto> { ordenadas[0] };

            for (var i = 1; i <= ordenadas.Count; i++)
            {
                var continuaRacha = i < ordenadas.Count &&
                    (ordenadas[i].FechaDocumento - ordenadas[i - 1].FechaDocumento).TotalMinutes < umbralMinutos;

                if (continuaRacha)
                {
                    racha.Add(ordenadas[i]);
                    continue;
                }

                if (racha.Count >= MinDespachosRapidos)
                    AgregarAlertaDespachosRapidos(context, grupo.Key, racha, umbralMinutos, anomalies);

                if (i < ordenadas.Count)
                    racha = [ordenadas[i]];
            }
        }
    }

    private void AgregarAlertaDespachosRapidos(
        DetectionContext context, string cliente, List<FacturaDto> racha,
        double umbralMinutos, List<DetectedAnomaly> anomalies)
    {
        var montoTotal = racha.Sum(f => f.TotalNeto);
        var vendedores = racha.Select(f => f.CodigoVendedor.Trim()).Distinct().ToList();
        var duracionMinutos = (racha[^1].FechaDocumento - racha[0].FechaDocumento).TotalMinutes;

        var (score, nivel) = _scoring.Calculate(
            riesgoBase: 50,
            montoInvolucrado: montoTotal);

        anomalies.Add(new DetectedAnomaly
        {
            TipoDetector = TipoDetector.PaymentFraud,
            Ambito = GetAmbito(context.Reglas, "DespachosRapidosMinutosUmbral", AmbitoAlerta.Auditoria),
            Descripcion = $"Despachos rápidos sucesivos: cliente {cliente} con {racha.Count} " +
                          $"transacciones en {duracionMinutos:F0} minutos (gaps < {umbralMinutos} min). " +
                          $"Monto total: ${montoTotal:F2}",
            Score = score,
            NivelRiesgo = nivel,
            EstacionId = context.EstacionId,
            EmpleadoCodigo = vendedores.Count == 1 ? vendedores[0] : null,
            TransaccionReferencia = $"RAPIDOS-{cliente}-{racha[0].SecuenciaDocumento}",
            Metadata = new Dictionary<string, object>
            {
                ["Cliente"] = cliente,
                ["CantidadTransacciones"] = racha.Count,
                ["DuracionMinutos"] = duracionMinutos,
                ["UmbralMinutos"] = umbralMinutos,
                ["MontoTotal"] = montoTotal,
                ["Vendedores"] = vendedores,
                ["Documentos"] = racha.Select(f => f.NumeroDocumento).ToList()
            }
        });
    }

    /// <summary>
    /// Obtiene el umbral de una regla. Devuelve null si la regla existe pero está desactivada.
    /// Si la regla no existe, usa el valor por defecto.
    /// </summary>
    private static double? GetUmbral(
        IReadOnlyList<Domain.Entities.ReglaDeteccion> reglas, string parametro, double defaultValue) =>
        reglas.FirstOrDefault(r => r.ParametroNombre == parametro) is { } regla
            ? (regla.Activa ? regla.ValorUmbral : null)
            : defaultValue;

    /// <summary>Carril configurado para la regla; si no existe en BD, usa el fallback del detector.</summary>
    private static AmbitoAlerta GetAmbito(
        IReadOnlyList<Domain.Entities.ReglaDeteccion> reglas, string parametro, AmbitoAlerta fallback) =>
        reglas.FirstOrDefault(r => r.ParametroNombre == parametro)?.Ambito ?? fallback;
}
