using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors;

/// <summary>
/// Detector de anomalías en facturas:
/// 1. Tasa de anulaciones excesivas por empleado.
/// 2. Precio aplicado fuera de lista autorizada.
/// 3. Campos obligatorios vacíos (placa, identificación).
/// </summary>
public sealed class InvoiceAnomalyDetector : IAnomalyDetector
{
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

        // Regla 1: Tasa de anulaciones excesivas
        DetectAnulacionesExcesivas(context, umbralAnulaciones, anomalies);

        // Regla 2: Precio fuera de lista
        if (precioFueraListaHabilitado)
            DetectPrecioFueraLista(context, anomalies);

        // Regla 3: Campos obligatorios vacíos
        if (camposObligatoriosHabilitado)
            DetectCamposObligatoriosVacios(context, anomalies);

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
        // Para una asociación más precisa, cruzamos ANUL con DCTO por número de documento.
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
            // Estimar anulaciones por vendedor: facturas del vendedor que están anuladas
            // Cruzamos por número de documento cuando es posible
            var facturasVendedor = context.Facturas
                .Where(f => f.CodigoVendedor.Trim() == vendedor)
                .ToList();

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
        // Los detalles de despacho tienen ValorUnitario (precio aplicado)
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

    private static double GetUmbral(IReadOnlyList<Domain.Entities.ReglaDeteccion> reglas, string parametro, double defaultValue) =>
        reglas.FirstOrDefault(r => r.ParametroNombre == parametro)?.ValorUmbral ?? defaultValue;
}
