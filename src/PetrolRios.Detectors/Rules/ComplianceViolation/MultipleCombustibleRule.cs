using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.ComplianceViolation;

/// <summary>Vehículo (placa) con más de un tipo de combustible en el mismo día (diésel y extra).</summary>
public sealed class MultipleCombustibleRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.ComplianceViolation;
    public override string Parametro => "MultipleCombustibleHabilitado";
    public override double UmbralPorDefecto => 1.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);

        var ventasPorPlacaDia = context.Facturas
            .Where(f => !string.IsNullOrWhiteSpace(f.Placa))
            .GroupBy(f => new
            {
                Placa = f.Placa.Trim().ToUpperInvariant(),
                Dia = f.FechaDocumento.Date
            });

        foreach (var grupo in ventasPorPlacaDia)
        {
            var facturasDelGrupo = grupo.ToList();
            var productosDelDia = context.Detalles
                .Where(d => facturasDelGrupo.Any(f => f.CodigoManguera.Trim() == d.CodigoManguera.Trim()))
                .Select(d => d.CodigoProducto.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();

            // Si no hay detalles suficientes, intentar con las mangueras de las facturas
            if (productosDelDia.Count < 2 && facturasDelGrupo.Count > 1)
            {
                var mangueras = facturasDelGrupo
                    .Select(f => f.CodigoManguera.Trim())
                    .Distinct()
                    .ToList();
                if (mangueras.Count >= 2)
                    productosDelDia = mangueras;
            }

            if (productosDelDia.Count < 2) continue;

            var (score, nivel) = Scoring.Calculate(riesgoBase: 55);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = carril,
                Descripcion = $"Vehículo {grupo.Key.Placa} con múltiples combustibles " +
                              $"({string.Join(", ", productosDelDia)}) el {grupo.Key.Dia:yyyy-MM-dd}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                TransaccionReferencia = $"MULTI-{grupo.Key.Placa}-{grupo.Key.Dia:yyyyMMdd}",
                Metadata = new Dictionary<string, object>
                {
                    ["Placa"] = grupo.Key.Placa,
                    ["Fecha"] = grupo.Key.Dia,
                    ["Productos"] = productosDelDia,
                    ["CantidadTransacciones"] = facturasDelGrupo.Count
                }
            });
        }
        return anomalies;
    }
}
