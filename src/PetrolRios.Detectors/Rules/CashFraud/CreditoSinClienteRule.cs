using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.CashFraud;

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
