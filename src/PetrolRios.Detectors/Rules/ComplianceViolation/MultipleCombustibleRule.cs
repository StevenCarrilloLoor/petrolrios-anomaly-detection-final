using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.ComplianceViolation;

/// <summary>Vehículo (placa) con más de un tipo de combustible en el mismo día (diésel y extra).</summary>
public sealed class MultipleCombustibleRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    /// <summary>Placa genérica "consumidor final" (ARCERNNR): no es un vehículo real, agrupa miles de
    /// ventas de contado de todo tipo de combustible. "Múltiples combustibles" no significa nada para ella.</summary>
    private const string PlacaGenerica = "ZZZ999949";

    public override TipoDetector Detector => TipoDetector.ComplianceViolation;
    public override string Parametro => "MultipleCombustibleHabilitado";
    public override double UmbralPorDefecto => 1.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);

        // El producto vive en el DETALLE (DESP) y la placa en la FACTURA (DCTO): se cruzan por el vínculo
        // factura↔despacho reconstruido en la base (CodigoCliente + cercanía temporal). La unión anterior
        // por CodigoManguera era incorrecta y la regla no disparaba nunca (ver DetectionRuleBase).
        var despachosPorCliente = IndexarDespachosPorCliente(context.Detalles);

        // (placa, día) → conjunto de productos despachados a esa placa ese día.
        var productosPorPlacaDia = new Dictionary<(string Placa, DateTime Dia), HashSet<string>>();

        foreach (var f in context.Facturas)
        {
            if (string.IsNullOrWhiteSpace(f.Placa)) continue;
            var placa = f.Placa.Trim().ToUpperInvariant();
            if (placa.Equals(PlacaGenerica, StringComparison.OrdinalIgnoreCase)) continue;

            var despacho = DespachoDeFactura(f, despachosPorCliente);
            if (despacho is null || string.IsNullOrWhiteSpace(despacho.CodigoProducto)) continue;

            var clave = (placa, f.FechaDocumento.Date);
            if (!productosPorPlacaDia.TryGetValue(clave, out var set))
                productosPorPlacaDia[clave] = set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            set.Add(despacho.CodigoProducto.Trim().ToUpperInvariant());
        }

        foreach (var ((placa, dia), productos) in productosPorPlacaDia)
        {
            if (productos.Count < 2) continue;

            var listaProductos = productos.OrderBy(p => p).ToList();
            var (score, nivel) = Scoring.Calculate(riesgoBase: 55);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = carril,
                Descripcion = $"Vehículo {placa} con múltiples combustibles " +
                              $"({string.Join(", ", listaProductos)}) el {dia:yyyy-MM-dd}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                TransaccionReferencia = $"MULTI-{placa}-{dia:yyyyMMdd}",
                Metadata = new Dictionary<string, object>
                {
                    ["Placa"] = placa,
                    ["Fecha"] = dia,
                    ["Productos"] = listaProductos
                }
            });
        }
        return anomalies;
    }
}
