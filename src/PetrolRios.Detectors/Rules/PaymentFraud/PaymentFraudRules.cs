using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Application.Interfaces;
using PetrolRios.Detectors.Rules;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.PaymentFraud;

// Cada regla del detector de pagos es ahora su propia clase (Strategy). Agregar una regla nueva =
// añadir una clase aquí (o en su archivo) y registrarla en DI; el detector no cambia.

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

/// <summary>Crédito otorgado sin código de autorización (comprobante NUMMCOMP = 0).</summary>
public sealed class CreditoSinAutorizacionRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.PaymentFraud;
    public override string Parametro => "CreditoSinAutorizacionHabilitado";
    public override double UmbralPorDefecto => 1.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);

        // Créditos otorgados (CRED_CABE) donde no hay comprobante asociado (NUMMCOMP = 0)
        foreach (var credito in context.Creditos)
        {
            if (credito.NumeroComprobante != 0) continue;
            if (credito.TotalCredito <= 0) continue;

            var (score, nivel) = Scoring.Calculate(riesgoBase: 50, montoInvolucrado: credito.TotalCredito);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.PaymentFraud,
                Ambito = carril,
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
        return anomalies;
    }
}

/// <summary>
/// Crédito otorgado sin garante (COD_GARA vacío). Un crédito sin respaldo de garante sugiere una
/// autorización indebida — el patrón que mencionó el ingeniero ("autorizan créditos a personas que
/// no están autorizadas"). Es incobrable y de alto riesgo.
/// </summary>
public sealed class CreditoSinGaranteRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.PaymentFraud;
    public override string Parametro => "CreditoSinGaranteHabilitado";
    public override double UmbralPorDefecto => 1.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);

        foreach (var credito in context.Creditos)
        {
            if (credito.TotalCredito <= 0) continue;
            if (!string.IsNullOrWhiteSpace(credito.CodigoGarante)) continue;

            var (score, nivel) = Scoring.Calculate(riesgoBase: 55, montoInvolucrado: credito.TotalCredito);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.PaymentFraud,
                Ambito = carril,
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
        return anomalies;
    }
}

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

/// <summary>
/// Despachos rápidos sucesivos: el mismo cliente con 3 o más transacciones consecutivas separadas
/// por menos de N minutos. En el caso documentado (enero 2026), el 33.9% de las transacciones del
/// cliente investigado ocurrieron con menos de 10 minutos entre despachos, un patrón físicamente
/// improbable para vehículos reales que sugiere facturación ficticia.
/// </summary>
public sealed class DespachosRapidosRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    /// <summary>Mínimo de despachos consecutivos rápidos para generar alerta.</summary>
    private const int MinDespachosRapidos = 3;

    public override TipoDetector Detector => TipoDetector.PaymentFraud;
    public override string Parametro => "DespachosRapidosMinutosUmbral";
    public override double UmbralPorDefecto => 10.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var umbralMinutos = Umbral(regla);
        var carril = Carril(regla);

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
                    AgregarAlerta(context, grupo.Key, racha, umbralMinutos, carril, anomalies);

                if (i < ordenadas.Count)
                    racha = [ordenadas[i]];
            }
        }
        return anomalies;
    }

    private void AgregarAlerta(
        DetectionContext context, string cliente, List<FacturaDto> racha,
        double umbralMinutos, AmbitoAlerta carril, List<DetectedAnomaly> anomalies)
    {
        var montoTotal = racha.Sum(f => f.TotalNeto);
        var vendedores = racha.Select(f => f.CodigoVendedor.Trim()).Distinct().ToList();
        var duracionMinutos = (racha[^1].FechaDocumento - racha[0].FechaDocumento).TotalMinutes;

        var (score, nivel) = Scoring.Calculate(riesgoBase: 50, montoInvolucrado: montoTotal);
        anomalies.Add(new DetectedAnomaly
        {
            TipoDetector = TipoDetector.PaymentFraud,
            Ambito = carril,
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
}
