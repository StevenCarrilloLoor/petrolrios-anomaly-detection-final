using PetrolRios.Application.Interfaces;
using PetrolRios.Detectors.Rules;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.CashFraud;

// Cada regla del detector de efectivo es ahora su propia clase (Strategy). Agregar una regla
// nueva = añadir una clase aquí (o en su archivo) y registrarla en DI; el detector no cambia.

/// <summary>Códigos de pago de Contaplus compartidos por las reglas de efectivo/crédito.</summary>
internal static class PagoCodigos
{
    private static readonly string[] Efectivo = ["EF", "EFE"];
    private static readonly string[] Credito = ["CR", "CRE", "CRD"];

    public static bool EsEfectivo(string codigoPago) =>
        Efectivo.Contains(codigoPago.Trim(), StringComparer.OrdinalIgnoreCase);

    public static bool EsCredito(string codigoPago) =>
        Credito.Contains(codigoPago.Trim(), StringComparer.OrdinalIgnoreCase);
}

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

/// <summary>
/// Venta a crédito sin cliente identificado: un crédito sin deudor identificable es incobrable, lo
/// que sugiere que la venta fue en efectivo y el empleado retuvo el dinero (Tabla 2 de la tesis:
/// "Ventas en efectivo registradas como crédito").
/// </summary>
public sealed class CreditoSinClienteRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.CashFraud;
    public override string Parametro => "CreditoSinClienteHabilitado";
    public override double UmbralPorDefecto => 1.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);

        var facturasCredito = context.Facturas
            .Where(f => PagoCodigos.EsCredito(f.CodigoPago))
            .Where(f => string.IsNullOrWhiteSpace(f.CodigoCliente)
                     || string.IsNullOrWhiteSpace(f.RucCliente));

        foreach (var factura in facturasCredito)
        {
            var reincidencias = context.AlertasPreviasPorEmpleado.GetValueOrDefault(factura.CodigoVendedor.Trim(), 0);
            var (score, nivel) = Scoring.Calculate(riesgoBase: 55, montoInvolucrado: factura.TotalNeto, reincidenciasEmpleado: reincidencias);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.CashFraud,
                Ambito = carril,
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
        return anomalies;
    }
}

/// <summary>
/// Proporción atípica de efectivo sobre clientes corporativos. En el caso documentado (enero 2026),
/// el cliente corporativo pagaba 88.4% con tarjeta; los despachadores con porcentajes de contado
/// superiores al umbral resultaron los de mayor riesgo (79.5% contado = nivel crítico). El efectivo
/// en cuentas corporativas es difícil de rastrear y facilita la apropiación indebida.
/// </summary>
public sealed class EfectivoCorporativoRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    /// <summary>Mínimo de transacciones corporativas para evaluar proporción de efectivo.</summary>
    private const int MinTransaccionesCorporativas = 4;

    public override TipoDetector Detector => TipoDetector.CashFraud;
    public override string Parametro => "EfectivoCorporativoPorcentajeUmbral";
    public override double UmbralPorDefecto => 30.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var umbralPorcentaje = Umbral(regla);
        var carril = Carril(regla);

        var corporativasPorVendedor = context.Facturas
            .Where(f => !string.IsNullOrWhiteSpace(f.CodigoCliente))
            .GroupBy(f => f.CodigoVendedor.Trim());

        foreach (var grupo in corporativasPorVendedor)
        {
            var transacciones = grupo.ToList();
            if (transacciones.Count < MinTransaccionesCorporativas) continue;

            var enEfectivo = transacciones.Where(f => PagoCodigos.EsEfectivo(f.CodigoPago)).ToList();
            var porcentajeEfectivo = (double)enEfectivo.Count / transacciones.Count * 100;
            if (porcentajeEfectivo <= umbralPorcentaje) continue;

            var montoEfectivo = enEfectivo.Sum(f => f.TotalNeto);
            var reincidencias = context.AlertasPreviasPorEmpleado.GetValueOrDefault(grupo.Key, 0);
            var (score, nivel) = Scoring.Calculate(riesgoBase: 50, montoInvolucrado: montoEfectivo, reincidenciasEmpleado: reincidencias);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.CashFraud,
                Ambito = carril,
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
        return anomalies;
    }
}

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
