using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors;

/// <summary>
/// Detector de anomalías en facturas:
/// 1. Tasa de anulaciones excesivas por empleado.
/// 2. Precio aplicado fuera de lista autorizada.
/// 3. Campos obligatorios vacíos (placa, identificación).
/// 4. Descuento que excede el porcentaje máximo de la política comercial (Tabla 3 de la tesis).
/// 5. Total de factura inconsistente con subtotal, descuento e IVA (interrogación de archivos).
/// </summary>
public sealed class InvoiceAnomalyDetector : IAnomalyDetector
{
    /// <summary>Tolerancia en dólares para validar la aritmética de la factura.</summary>
    private const double ToleranciaTotal = 0.05;

    private readonly RiskScoringEngine _scoring;
    private readonly ILogger<InvoiceAnomalyDetector> _logger;

    public TipoDetector Type => TipoDetector.InvoiceAnomaly;

    public InvoiceAnomalyDetector(RiskScoringEngine scoring, ILogger<InvoiceAnomalyDetector> logger)
    {
        _scoring = scoring;
        _logger = logger;
    }

    public Task<IReadOnlyList<DetectedAnomaly>> DetectAsync(DetectionContext context, CancellationToken ct)
    {
        var anomalies = new List<DetectedAnomaly>();
        var reglas = context.Reglas.Where(r => r.TipoDetector == TipoDetector.InvoiceAnomaly).ToList();

        var umbralAnulaciones = GetUmbral(reglas, "AnulacionesPorcentajeUmbral", 5.0);
        var precioFueraListaHabilitado = GetUmbral(reglas, "PrecioFueraListaHabilitado", 1.0) >= 1.0;
        var camposObligatoriosHabilitado = GetUmbral(reglas, "CamposObligatoriosHabilitado", 1.0) >= 1.0;
        var umbralDescuentoPorcentaje = GetUmbral(reglas, "DescuentoPorcentajeMaximo", 10.0);
        var totalInconsistenteHabilitado = GetUmbral(reglas, "TotalInconsistenteHabilitado", 1.0) >= 1.0;
        var toleranciaFuturaHoras = GetUmbral(reglas, "FechaFuturaToleranciaHoras", 24.0);
        var despachoNoFacturadoHabilitado = GetUmbral(reglas, "DespachoNoFacturadoHabilitado", 1.0) >= 1.0;
        var anulacionRecurrenteDias = GetUmbral(reglas, "AnulacionRecurrenteDiasMinimo", 3.0);

        // Regla 1: Tasa de anulaciones excesivas
        if (umbralAnulaciones is not null)
            DetectAnulacionesExcesivas(context, umbralAnulaciones.Value, anomalies);

        // Regla 2: Precio fuera de lista
        if (precioFueraListaHabilitado)
            DetectPrecioFueraLista(context, anomalies);

        // Regla 3: Campos obligatorios vacíos
        if (camposObligatoriosHabilitado)
            DetectCamposObligatoriosVacios(context, anomalies);

        // Regla 4: Descuento excesivo fuera de política
        if (umbralDescuentoPorcentaje is not null)
            DetectDescuentoExcesivo(context, umbralDescuentoPorcentaje.Value, anomalies);

        // Regla 5: Total de factura inconsistente
        if (totalInconsistenteHabilitado)
            DetectTotalInconsistente(context, anomalies);

        // Regla 6: Fecha fuera de rango plausible (futuro/backdating)
        if (toleranciaFuturaHoras is not null)
            DetectFechaFueraDeRango(context, toleranciaFuturaHoras.Value, anomalies);

        // Regla 7: Despacho no facturado (combustible servido sin cobrar)
        if (despachoNoFacturadoHabilitado)
            DetectDespachoNoFacturado(context, anomalies);

        // Regla 8: Anulaciones recurrentes en varios días (posible kiting / cancelar-reingresar)
        if (anulacionRecurrenteDias is not null)
            DetectAnulacionRecurrente(context, (int)anulacionRecurrenteDias.Value, anomalies);

        _logger.LogDebug("InvoiceAnomalyDetector: {Count} anomalías en estación {Est}",
            anomalies.Count, context.EstacionNombre);

        return Task.FromResult<IReadOnlyList<DetectedAnomaly>>(anomalies);
    }

    private void DetectAnulacionesExcesivas(
        DetectionContext context, double umbralPorcentaje, List<DetectedAnomaly> anomalies)
    {
        if (context.Anulaciones.Count == 0 || context.Facturas.Count == 0)
            return;

        // Contar facturas por vendedor (todas las transacciones del período)
        var facturasPorVendedor = context.Facturas
            .GroupBy(f => f.CodigoVendedor.Trim())
            .ToDictionary(g => g.Key, g => g.Count());

        // Las anulaciones no tienen COD_VEND directo; las asociamos por establecimiento/punto emisión.
        // Aproximación: contar total de anulaciones vs total de facturas por período.
        var totalFacturas = context.Facturas.Count;
        var totalAnulaciones = context.Anulaciones.Count;
        var tasaGlobal = totalFacturas > 0
            ? (double)totalAnulaciones / totalFacturas * 100
            : 0;

        if (tasaGlobal > umbralPorcentaje)
        {
            var (score, nivel) = _scoring.Calculate(
                riesgoBase: 35,
                montoInvolucrado: 0);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Descripcion = $"Tasa de anulaciones excesiva: {tasaGlobal:F1}% " +
                              $"({totalAnulaciones}/{totalFacturas}) supera umbral de {umbralPorcentaje}%",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                Metadata = new Dictionary<string, object>
                {
                    ["TotalAnulaciones"] = totalAnulaciones,
                    ["TotalFacturas"] = totalFacturas,
                    ["TasaPorcentaje"] = tasaGlobal,
                    ["UmbralPorcentaje"] = umbralPorcentaje
                }
            });
        }

        // También revisar por vendedor individual
        foreach (var (vendedor, cantidadFacturas) in facturasPorVendedor)
        {
            // Calcular la tasa individual si hay suficientes transacciones
            if (cantidadFacturas < 5) continue;

            // Sin cruce directo con ANUL, usamos la tasa proporcional
            var anulacionesEstimadas = totalFacturas > 0
                ? (double)totalAnulaciones * cantidadFacturas / totalFacturas
                : 0;
            var tasaVendedor = cantidadFacturas > 0
                ? anulacionesEstimadas / cantidadFacturas * 100
                : 0;

            // Solo generar alerta individual si la tasa del vendedor es significativamente alta
            if (tasaVendedor > umbralPorcentaje * 2)
            {
                var reincidencias = context.AlertasPreviasPorEmpleado
                    .GetValueOrDefault(vendedor, 0);
                var (score, nivel) = _scoring.Calculate(
                    riesgoBase: 40,
                    reincidenciasEmpleado: reincidencias);

                anomalies.Add(new DetectedAnomaly
                {
                    TipoDetector = TipoDetector.InvoiceAnomaly,
                    Descripcion = $"Vendedor {vendedor} con tasa de anulaciones elevada: " +
                                  $"{tasaVendedor:F1}% ({cantidadFacturas} facturas)",
                    Score = score,
                    NivelRiesgo = nivel,
                    EstacionId = context.EstacionId,
                    EmpleadoCodigo = vendedor,
                    Metadata = new Dictionary<string, object>
                    {
                        ["Vendedor"] = vendedor,
                        ["FacturasVendedor"] = cantidadFacturas,
                        ["TasaEstimada"] = tasaVendedor
                    }
                });
            }
        }
    }

    private void DetectPrecioFueraLista(
        DetectionContext context, List<DetectedAnomaly> anomalies)
    {
        // Comparar precio aplicado (VUN_DESP) contra precio autorizado en detalles
        // Agrupamos por producto y tomamos el precio mínimo como "autorizado" del período
        var preciosPorProducto = context.Detalles
            .Where(d => d.ValorUnitario > 0)
            .GroupBy(d => d.CodigoProducto.Trim())
            .ToDictionary(
                g => g.Key,
                g => g.Min(d => d.ValorUnitario));

        foreach (var detalle in context.Detalles)
        {
            if (detalle.ValorUnitario <= 0) continue;

            var producto = detalle.CodigoProducto.Trim();
            if (!preciosPorProducto.TryGetValue(producto, out var precioBase)) continue;

            // Si el precio aplicado es mayor que el precio base del producto (con tolerancia 1%)
            if (detalle.ValorUnitario > precioBase * 1.01)
            {
                var diferencia = detalle.ValorUnitario - precioBase;
                var (score, nivel) = _scoring.Calculate(
                    riesgoBase: 45,
                    montoInvolucrado: diferencia * detalle.Cantidad);

                anomalies.Add(new DetectedAnomaly
                {
                    TipoDetector = TipoDetector.InvoiceAnomaly,
                    Descripcion = $"Precio fuera de lista: producto {producto} cobrado a " +
                                  $"${detalle.ValorUnitario:F2} (autorizado: ${precioBase:F2})",
                    Score = score,
                    NivelRiesgo = nivel,
                    EstacionId = context.EstacionId,
                    TransaccionReferencia = $"DESP-{detalle.NumeroDespacho}",
                    Metadata = new Dictionary<string, object>
                    {
                        ["Producto"] = producto,
                        ["PrecioAplicado"] = detalle.ValorUnitario,
                        ["PrecioAutorizado"] = precioBase,
                        ["Diferencia"] = diferencia,
                        ["Cantidad"] = detalle.Cantidad
                    }
                });
            }
        }
    }

    private void DetectCamposObligatoriosVacios(
        DetectionContext context, List<DetectedAnomaly> anomalies)
    {
        foreach (var factura in context.Facturas)
        {
            var camposFaltantes = new List<string>();

            if (string.IsNullOrWhiteSpace(factura.Placa))
                camposFaltantes.Add("placa");
            if (string.IsNullOrWhiteSpace(factura.RucCliente))
                camposFaltantes.Add("RUC/cédula");

            if (camposFaltantes.Count == 0) continue;

            var (score, nivel) = _scoring.Calculate(riesgoBase: 20);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                // Error operativo (el cajero olvidó capturar placa/cédula): va al carril de la estación.
                Ambito = Domain.Enums.AmbitoAlerta.Operativa,
                Descripcion = $"Campos obligatorios vacíos en documento {factura.NumeroDocumento}: " +
                              string.Join(", ", camposFaltantes),
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroDocumento"] = factura.NumeroDocumento,
                    ["CamposFaltantes"] = camposFaltantes,
                    ["Vendedor"] = factura.CodigoVendedor.Trim()
                }
            });
        }
    }

    /// <summary>
    /// Descuento que excede el porcentaje máximo permitido por la política comercial
    /// (Tabla 3 de la tesis: "descuento_aplicado > descuento_máximo_permitido").
    /// </summary>
    private void DetectDescuentoExcesivo(
        DetectionContext context, double porcentajeMaximo, List<DetectedAnomaly> anomalies)
    {
        foreach (var factura in context.Facturas)
        {
            if (factura.Subtotal <= 0 || factura.Descuento <= 0) continue;

            var porcentajeDescuento = factura.Descuento / factura.Subtotal * 100;
            if (porcentajeDescuento <= porcentajeMaximo) continue;

            var reincidencias = context.AlertasPreviasPorEmpleado
                .GetValueOrDefault(factura.CodigoVendedor.Trim(), 0);

            var (score, nivel) = _scoring.Calculate(
                riesgoBase: 45,
                montoInvolucrado: factura.Descuento,
                reincidenciasEmpleado: reincidencias);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Descripcion = $"Descuento excesivo: {porcentajeDescuento:F1}% sobre subtotal " +
                              $"(máximo permitido: {porcentajeMaximo:F0}%). " +
                              $"Doc: {factura.NumeroDocumento}, descuento ${factura.Descuento:F2}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroDocumento"] = factura.NumeroDocumento,
                    ["PorcentajeDescuento"] = porcentajeDescuento,
                    ["PorcentajeMaximo"] = porcentajeMaximo,
                    ["MontoDescuento"] = factura.Descuento,
                    ["Subtotal"] = factura.Subtotal
                }
            });
        }
    }

    /// <summary>
    /// Total de factura inconsistente: el total registrado no corresponde a
    /// subtotal − descuento + IVA. Una factura cuya aritmética no cierra es un
    /// indicador clásico de manipulación documental en interrogación de archivos.
    /// </summary>
    private void DetectTotalInconsistente(
        DetectionContext context, List<DetectedAnomaly> anomalies)
    {
        foreach (var factura in context.Facturas)
        {
            if (factura.Subtotal <= 0) continue;

            var totalEsperado = factura.Subtotal - factura.Descuento + factura.Iva;
            var diferencia = Math.Abs(factura.TotalNeto - totalEsperado);

            if (diferencia <= ToleranciaTotal) continue;

            var reincidencias = context.AlertasPreviasPorEmpleado
                .GetValueOrDefault(factura.CodigoVendedor.Trim(), 0);

            var (score, nivel) = _scoring.Calculate(
                riesgoBase: 55,
                montoInvolucrado: diferencia,
                reincidenciasEmpleado: reincidencias);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Descripcion = $"Total inconsistente en documento {factura.NumeroDocumento}: " +
                              $"registrado ${factura.TotalNeto:F2}, esperado ${totalEsperado:F2} " +
                              $"(diferencia ${diferencia:F2})",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroDocumento"] = factura.NumeroDocumento,
                    ["TotalRegistrado"] = factura.TotalNeto,
                    ["TotalEsperado"] = totalEsperado,
                    ["Diferencia"] = diferencia,
                    ["Subtotal"] = factura.Subtotal,
                    ["Descuento"] = factura.Descuento,
                    ["Iva"] = factura.Iva
                }
            });
        }
    }

    /// <summary>
    /// Fecha fuera de rango plausible: una transacción fechada en el futuro (más allá de la
    /// tolerancia) respecto al momento de procesamiento es señal de manipulación de fecha
    /// (backdating/postdating) — el escenario donde alguien inserta un registro con fecha
    /// adelantada. Cubre facturas (DCTO) y créditos (CRED_CABE).
    /// </summary>
    private void DetectFechaFueraDeRango(
        DetectionContext context, double toleranciaHoras, List<DetectedAnomaly> anomalies)
    {
        var limiteFuturo = context.ToWatermark.AddHours(toleranciaHoras);

        foreach (var factura in context.Facturas)
        {
            if (factura.FechaDocumento <= limiteFuturo) continue;

            var horasAdelante = (factura.FechaDocumento - context.ToWatermark).TotalHours;
            var (score, nivel) = _scoring.Calculate(
                riesgoBase: 60,
                montoInvolucrado: factura.TotalNeto,
                reincidenciasEmpleado: context.AlertasPreviasPorEmpleado
                    .GetValueOrDefault(factura.CodigoVendedor.Trim(), 0));

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Descripcion = $"Documento {factura.NumeroDocumento} fechado en el futuro: " +
                              $"{factura.FechaDocumento:yyyy-MM-dd HH:mm} " +
                              $"({horasAdelante:F0} h adelante del procesamiento). Posible manipulación de fecha.",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroDocumento"] = factura.NumeroDocumento,
                    ["FechaDocumento"] = factura.FechaDocumento,
                    ["FechaProcesamiento"] = context.ToWatermark,
                    ["HorasAdelante"] = horasAdelante,
                    ["ToleranciaHoras"] = toleranciaHoras
                }
            });
        }

        foreach (var credito in context.Creditos)
        {
            if (credito.FechaCabecera <= limiteFuturo) continue;

            var horasAdelante = (credito.FechaCabecera - context.ToWatermark).TotalHours;
            var (score, nivel) = _scoring.Calculate(
                riesgoBase: 60,
                montoInvolucrado: credito.TotalCredito);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Descripcion = $"Crédito {credito.NumeroCabecera} fechado en el futuro: " +
                              $"{credito.FechaCabecera:yyyy-MM-dd HH:mm} " +
                              $"({horasAdelante:F0} h adelante). Posible manipulación de fecha.",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                TransaccionReferencia = $"CRED-{credito.NumeroCabecera}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroCredito"] = credito.NumeroCabecera,
                    ["FechaCabecera"] = credito.FechaCabecera,
                    ["FechaProcesamiento"] = context.ToWatermark,
                    ["HorasAdelante"] = horasAdelante,
                    ["ToleranciaHoras"] = toleranciaHoras
                }
            });
        }
    }

    /// <summary>
    /// Anulaciones recurrentes (posible kiting / "cancelar y reingresar"): un mismo punto de
    /// emisión con anulaciones en varios días distintos sugiere el patrón que mencionó el
    /// ingeniero —cancelan una operación y la vuelven a poner al día siguiente, repetidamente—
    /// para rodar la deuda o mover el período. Carril de auditoría (fraude).
    /// </summary>
    private void DetectAnulacionRecurrente(
        DetectionContext context, int diasMinimo, List<DetectedAnomaly> anomalies)
    {
        if (context.Anulaciones.Count == 0) return;

        var grupos = context.Anulaciones
            .GroupBy(a => new { Est = a.Establecimiento.Trim(), Pto = a.PuntoEmision.Trim() });

        foreach (var grupo in grupos)
        {
            var diasDistintos = grupo
                .Select(a => a.FechaAnulacion.Date)
                .Distinct()
                .Count();

            if (diasDistintos < diasMinimo) continue;

            var totalAnulaciones = grupo.Count();
            var (score, nivel) = _scoring.Calculate(riesgoBase: 60);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Descripcion = $"Anulaciones recurrentes en {grupo.Key.Est}-{grupo.Key.Pto}: " +
                              $"{totalAnulaciones} anulaciones en {diasDistintos} días distintos " +
                              $"(umbral: {diasMinimo}). Posible patrón de cancelar y reingresar (kiting).",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                TransaccionReferencia = $"ANUL-RECURR-{grupo.Key.Est}-{grupo.Key.Pto}",
                Metadata = new Dictionary<string, object>
                {
                    ["Establecimiento"] = grupo.Key.Est,
                    ["PuntoEmision"] = grupo.Key.Pto,
                    ["DiasDistintos"] = diasDistintos,
                    ["TotalAnulaciones"] = totalAnulaciones,
                    ["DiasMinimo"] = diasMinimo
                }
            });
        }
    }

    /// <summary>
    /// Despacho no facturado: combustible servido (CAN_DESP &gt; 0) cuyo indicador FAC_DESP no
    /// marca facturado. Es combustible que salió sin cobrarse — lo que el ingeniero describió
    /// como "no lo colgó bien / no cerró el despacho". Va al carril Operativa de la estación.
    /// </summary>
    private void DetectDespachoNoFacturado(
        DetectionContext context, List<DetectedAnomaly> anomalies)
    {
        foreach (var despacho in context.Detalles)
        {
            var marca = despacho.Facturado.Trim();
            // Si no hay dato del indicador, no se asume nada (evita falsos positivos).
            if (string.IsNullOrEmpty(marca)) continue;
            if (marca == "1") continue;            // facturado
            if (despacho.Cantidad <= 0) continue;  // sin despacho real

            var (score, nivel) = _scoring.Calculate(
                riesgoBase: 35,
                montoInvolucrado: despacho.VolumenTotal);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Ambito = Domain.Enums.AmbitoAlerta.Operativa,
                Descripcion = $"Despacho {despacho.NumeroDespacho} NO facturado: " +
                              $"{despacho.Cantidad:F2} gal de {despacho.NombreProducto.Trim()} " +
                              $"por ${despacho.VolumenTotal:F2}. Revisar (combustible servido sin cobrar).",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                TransaccionReferencia = $"DESP-NOFACT-{despacho.NumeroDespacho}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroDespacho"] = despacho.NumeroDespacho,
                    ["Galones"] = despacho.Cantidad,
                    ["Monto"] = despacho.VolumenTotal,
                    ["Producto"] = despacho.NombreProducto.Trim(),
                    ["IndicadorFacturado"] = marca
                }
            });
        }
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
}
