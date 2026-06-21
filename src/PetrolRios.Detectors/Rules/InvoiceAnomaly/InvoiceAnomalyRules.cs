using PetrolRios.Application.Interfaces;
using PetrolRios.Detectors.Rules;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.InvoiceAnomaly;

// Cada regla del detector de facturas es ahora su propia clase (Strategy). Agregar una regla
// nueva = añadir una clase aquí (o en su archivo) y registrarla en DI; el detector no cambia.

/// <summary>Tasa de anulaciones excesiva (global y por vendedor) sobre las transacciones del período.</summary>
public sealed class TasaAnulacionesRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "AnulacionesPorcentajeUmbral";
    public override double UmbralPorDefecto => 3.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        if (context.Anulaciones.Count == 0 || context.Facturas.Count == 0) return anomalies;

        var umbralPorcentaje = Umbral(regla);
        var carril = Carril(regla);

        var facturasPorVendedor = context.Facturas
            .GroupBy(f => f.CodigoVendedor.Trim())
            .ToDictionary(g => g.Key, g => g.Count());

        var totalFacturas = context.Facturas.Count;
        var totalAnulaciones = context.Anulaciones.Count;
        var tasaGlobal = totalFacturas > 0 ? (double)totalAnulaciones / totalFacturas * 100 : 0;

        if (tasaGlobal > umbralPorcentaje)
        {
            var (score, nivel) = Scoring.Calculate(riesgoBase: 35, montoInvolucrado: 0);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Ambito = carril,
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

        foreach (var (vendedor, cantidadFacturas) in facturasPorVendedor)
        {
            if (cantidadFacturas < 5) continue;

            var anulacionesEstimadas = totalFacturas > 0
                ? (double)totalAnulaciones * cantidadFacturas / totalFacturas
                : 0;
            var tasaVendedor = cantidadFacturas > 0 ? anulacionesEstimadas / cantidadFacturas * 100 : 0;

            if (tasaVendedor > umbralPorcentaje * 2)
            {
                var reincidencias = context.AlertasPreviasPorEmpleado.GetValueOrDefault(vendedor, 0);
                var (score, nivel) = Scoring.Calculate(riesgoBase: 40, reincidenciasEmpleado: reincidencias);
                anomalies.Add(new DetectedAnomaly
                {
                    TipoDetector = TipoDetector.InvoiceAnomaly,
                    Ambito = carril,
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
        return anomalies;
    }
}

/// <summary>Precio aplicado por encima del autorizado (mínimo del período por producto).</summary>
public sealed class PrecioFueraListaRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "PrecioFueraListaHabilitado";
    public override double UmbralPorDefecto => 1.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);

        var preciosPorProducto = context.Detalles
            .Where(d => d.ValorUnitario > 0)
            .GroupBy(d => d.CodigoProducto.Trim())
            .ToDictionary(g => g.Key, g => g.Min(d => d.ValorUnitario));

        foreach (var detalle in context.Detalles)
        {
            if (detalle.ValorUnitario <= 0) continue;

            var producto = detalle.CodigoProducto.Trim();
            if (!preciosPorProducto.TryGetValue(producto, out var precioBase)) continue;

            if (detalle.ValorUnitario > precioBase * 1.01)
            {
                var diferencia = detalle.ValorUnitario - precioBase;
                var (score, nivel) = Scoring.Calculate(riesgoBase: 45, montoInvolucrado: diferencia * detalle.Cantidad);
                anomalies.Add(new DetectedAnomaly
                {
                    TipoDetector = TipoDetector.InvoiceAnomaly,
                    Ambito = carril,
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
        return anomalies;
    }
}

/// <summary>Campos obligatorios vacíos (placa, cédula/RUC). Por defecto carril Operativa.</summary>
public sealed class CamposObligatoriosRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "CamposObligatoriosHabilitado";
    public override double UmbralPorDefecto => 1.0;
    public override AmbitoAlerta AmbitoPorDefecto => AmbitoAlerta.Operativa;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);

        foreach (var factura in context.Facturas)
        {
            var camposFaltantes = new List<string>();
            if (string.IsNullOrWhiteSpace(factura.Placa)) camposFaltantes.Add("placa");
            if (string.IsNullOrWhiteSpace(factura.RucCliente)) camposFaltantes.Add("RUC/cédula");
            if (camposFaltantes.Count == 0) continue;

            var (score, nivel) = Scoring.Calculate(riesgoBase: 20);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Ambito = carril,
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
        return anomalies;
    }
}

/// <summary>Descuento que excede el porcentaje máximo de la política comercial.</summary>
public sealed class DescuentoExcesivoRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "DescuentoPorcentajeMaximo";
    public override double UmbralPorDefecto => 10.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var porcentajeMaximo = Umbral(regla);
        var carril = Carril(regla);

        foreach (var factura in context.Facturas)
        {
            if (factura.Subtotal <= 0 || factura.Descuento <= 0) continue;

            var porcentajeDescuento = factura.Descuento / factura.Subtotal * 100;
            if (porcentajeDescuento <= porcentajeMaximo) continue;

            var reincidencias = context.AlertasPreviasPorEmpleado.GetValueOrDefault(factura.CodigoVendedor.Trim(), 0);
            var (score, nivel) = Scoring.Calculate(riesgoBase: 45, montoInvolucrado: factura.Descuento, reincidenciasEmpleado: reincidencias);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Ambito = carril,
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
        return anomalies;
    }
}

/// <summary>Total que no cuadra con subtotal − descuento + IVA (manipulación documental).</summary>
public sealed class TotalInconsistenteRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    private const double ToleranciaTotal = 0.05;

    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "TotalInconsistenteHabilitado";
    public override double UmbralPorDefecto => 1.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);

        foreach (var factura in context.Facturas)
        {
            if (factura.Subtotal <= 0) continue;

            var totalEsperado = factura.Subtotal - factura.Descuento + factura.Iva;
            var diferencia = Math.Abs(factura.TotalNeto - totalEsperado);
            if (diferencia <= ToleranciaTotal) continue;

            var reincidencias = context.AlertasPreviasPorEmpleado.GetValueOrDefault(factura.CodigoVendedor.Trim(), 0);
            var (score, nivel) = Scoring.Calculate(riesgoBase: 55, montoInvolucrado: diferencia, reincidenciasEmpleado: reincidencias);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Ambito = carril,
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
        return anomalies;
    }
}

/// <summary>Documento o crédito fechado en el futuro más allá de la tolerancia (backdating).</summary>
public sealed class FechaFueraDeRangoRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "FechaFuturaToleranciaHoras";
    public override double UmbralPorDefecto => 24.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var toleranciaHoras = Umbral(regla);
        var carril = Carril(regla);
        var limiteFuturo = context.ToWatermark.AddHours(toleranciaHoras);

        foreach (var factura in context.Facturas)
        {
            if (factura.FechaDocumento <= limiteFuturo) continue;

            var horasAdelante = (factura.FechaDocumento - context.ToWatermark).TotalHours;
            var (score, nivel) = Scoring.Calculate(
                riesgoBase: 60,
                montoInvolucrado: factura.TotalNeto,
                reincidenciasEmpleado: context.AlertasPreviasPorEmpleado.GetValueOrDefault(factura.CodigoVendedor.Trim(), 0));
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Ambito = carril,
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
            var (score, nivel) = Scoring.Calculate(riesgoBase: 60, montoInvolucrado: credito.TotalCredito);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Ambito = carril,
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
        return anomalies;
    }
}

/// <summary>Anulaciones recurrentes (mismo punto de emisión en varios días): posible kiting.</summary>
public sealed class AnulacionRecurrenteRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "AnulacionRecurrenteDiasMinimo";
    public override double UmbralPorDefecto => 3.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        if (context.Anulaciones.Count == 0) return anomalies;

        var diasMinimo = (int)Umbral(regla);
        var carril = Carril(regla);

        var grupos = context.Anulaciones
            .GroupBy(a => new { Est = a.Establecimiento.Trim(), Pto = a.PuntoEmision.Trim() });

        foreach (var grupo in grupos)
        {
            var diasDistintos = grupo.Select(a => a.FechaAnulacion.Date).Distinct().Count();
            if (diasDistintos < diasMinimo) continue;

            var totalAnulaciones = grupo.Count();
            var (score, nivel) = Scoring.Calculate(riesgoBase: 60);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Ambito = carril,
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
        return anomalies;
    }
}

/// <summary>Despacho con galones servidos no marcado como facturado (combustible sin cobrar). Operativa.</summary>
public sealed class DespachoNoFacturadoRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "DespachoNoFacturadoHabilitado";
    public override double UmbralPorDefecto => 1.0;
    public override AmbitoAlerta AmbitoPorDefecto => AmbitoAlerta.Operativa;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);

        foreach (var despacho in context.Detalles)
        {
            var marca = despacho.Facturado.Trim();
            if (string.IsNullOrEmpty(marca)) continue;
            if (marca == "1") continue;
            if (despacho.Cantidad <= 0) continue;

            var (score, nivel) = Scoring.Calculate(riesgoBase: 35, montoInvolucrado: despacho.VolumenTotal);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Ambito = carril,
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
        return anomalies;
    }
}
