using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.InvoiceAnomaly;

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
                Fuente = factura,
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
