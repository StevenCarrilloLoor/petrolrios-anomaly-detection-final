using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.InvoiceAnomaly;

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
                Fuente = factura,
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
