using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.InvoiceAnomaly;

/// <summary>
/// Reutilización de placa en el día: una misma placa facturada más de N veces dentro del mismo día.
/// Patrón señalado por auditoría (caso real: <b>14 facturas en un día a la misma placa</b>): el
/// despachador asigna ventas de combustible a una placa "comodín" de un cliente frecuente para cuadrar
/// efectivo o emitir facturas ficticias, lo que expone a la estación a una denuncia ante el SRI.
///
/// Complementa a "Despachos rápidos sucesivos" (ventana de minutos) con una ventana <b>diaria</b>: por
/// eso esta regla se programa para correr una vez al día sobre la ventana del día (ver SeedData), de modo
/// que el conteo por placa abarque toda la jornada. El umbral es configurable; auditoría sugiere bajarlo
/// hasta 2 según la tolerancia a falsos positivos de cada operación.
/// </summary>
public sealed class PlacaReutilizadaRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    /// <summary>
    /// Placa genérica de "consumidor final" (ARCERNNR). Aparece legítimamente cientos de veces al día en
    /// ventas de contado sin vehículo identificado; su exceso lo controla <c>PlacaGenericaRule</c>, no esta.
    /// </summary>
    private const string PlacaGenerica = "ZZZ999949";

    public override TipoDetector Detector => TipoDetector.InvoiceAnomaly;
    public override string Parametro => "PlacaReutilizadaDiaUmbral";
    public override double UmbralPorDefecto => 5.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var umbral = Umbral(regla);
        var carril = Carril(regla);

        // Agrupar por (placa normalizada, día calendario). La ventana de datos la acota la programación
        // diaria de la regla; agrupar por día además mantiene el conteo "por jornada" aunque la ventana
        // configurada abarque varios días.
        var grupos = context.Facturas
            .Where(f => !string.IsNullOrWhiteSpace(f.Placa)
                     && !f.Placa.Trim().Equals(PlacaGenerica, StringComparison.OrdinalIgnoreCase))
            .GroupBy(f => new { Placa = f.Placa.Trim().ToUpperInvariant(), Dia = f.FechaDocumento.Date });

        foreach (var grupo in grupos)
        {
            if (grupo.Count() <= umbral) continue;

            var facturas = grupo.OrderBy(f => f.FechaDocumento).ToList();
            AgregarAlerta(context, grupo.Key.Placa, grupo.Key.Dia, facturas, umbral, carril, anomalies);
        }
        return anomalies;
    }

    private void AgregarAlerta(
        DetectionContext context, string placa, DateTime dia, List<FacturaDto> facturas,
        double umbral, AmbitoAlerta carril, List<DetectedAnomaly> anomalies)
    {
        var montoTotal = facturas.Sum(f => f.TotalNeto);
        var vendedores = facturas
            .Select(f => f.CodigoVendedor.Trim())
            .Where(v => v.Length > 0)
            .Distinct()
            .ToList();
        var clientes = facturas
            .Select(f => f.CodigoCliente.Trim())
            .Where(c => c.Length > 0)
            .Distinct()
            .ToList();
        var rucs = facturas
            .Select(f => f.RucCliente.Trim())
            .Where(r => r.Length > 0)
            .Distinct()
            .ToList();
        var documentos = facturas
            .Select(f => f.NumeroDocumento.Trim())
            .Where(n => n.Length > 0)
            .ToList();

        // Más riesgo si varios despachadores cargaron la misma placa el mismo día (colusión o comodín).
        var riesgoBase = vendedores.Count > 1 ? 65 : 55;
        var (score, nivel) = Scoring.Calculate(riesgoBase, montoInvolucrado: montoTotal);

        anomalies.Add(new DetectedAnomaly
        {
            TipoDetector = TipoDetector.InvoiceAnomaly,
            Ambito = carril,
            Descripcion = $"Placa {placa} facturada {facturas.Count} veces el {dia:dd/MM/yyyy} " +
                          $"(umbral: {umbral:F0}). Monto total: ${montoTotal:F2}. " +
                          $"Posible reutilización de placa (riesgo de denuncia SRI).",
            Score = score,
            NivelRiesgo = nivel,
            EstacionId = context.EstacionId,
            EmpleadoCodigo = vendedores.Count == 1 ? vendedores[0] : null,
            TransaccionReferencia = $"PLACA-{placa}-{dia:yyyyMMdd}",
            Metadata = new Dictionary<string, object>
            {
                ["Placa"] = placa,
                ["Dia"] = dia.ToString("yyyy-MM-dd"),
                ["CantidadFacturas"] = facturas.Count,
                ["Umbral"] = umbral,
                ["MontoTotal"] = montoTotal,
                ["NumerosFactura"] = documentos,
                ["Clientes"] = clientes,
                ["Rucs"] = rucs,
                ["Vendedores"] = vendedores
            }
        });
    }
}
