using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.PaymentFraud;

/// <summary>
/// Despachos rápidos sucesivos: el mismo cliente con 3 o más transacciones consecutivas separadas
/// por menos de N minutos. En el caso documentado (enero 2026), el 33.9% de las transacciones del
/// cliente investigado ocurrieron con menos de 10 minutos entre despachos, un patrón físicamente
/// improbable para vehículos reales que sugiere facturación ficticia.
/// </summary>
public sealed class DespachosRapidosRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    /// <summary>Mínimo de despachos consecutivos rápidos para generar alerta.</summary>
    private const int MinDespachosRapidos = 3;

    public override TipoDetector Detector => TipoDetector.PaymentFraud;
    public override string Parametro => "DespachosRapidosMinutosUmbral";
    public override double UmbralPorDefecto => 10.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var umbralMinutos = Umbral(regla);
        var carril = Carril(regla);

        var facturasPorCliente = context.Facturas
            .Where(f => !string.IsNullOrWhiteSpace(f.CodigoCliente))
            .GroupBy(f => f.CodigoCliente.Trim());

        foreach (var grupo in facturasPorCliente)
        {
            var ordenadas = grupo.OrderBy(f => f.FechaDocumento).ToList();
            if (ordenadas.Count < MinDespachosRapidos) continue;

            // Buscar rachas de transacciones consecutivas con gap < umbral
            var racha = new List<FacturaDto> { ordenadas[0] };

            for (var i = 1; i <= ordenadas.Count; i++)
            {
                var continuaRacha = i < ordenadas.Count &&
                    (ordenadas[i].FechaDocumento - ordenadas[i - 1].FechaDocumento).TotalMinutes < umbralMinutos;

                if (continuaRacha)
                {
                    racha.Add(ordenadas[i]);
                    continue;
                }

                if (racha.Count >= MinDespachosRapidos)
                    AgregarAlerta(context, grupo.Key, racha, umbralMinutos, carril, anomalies);

                if (i < ordenadas.Count)
                    racha = [ordenadas[i]];
            }
        }
        return anomalies;
    }

    private void AgregarAlerta(
        DetectionContext context, string cliente, List<FacturaDto> racha,
        double umbralMinutos, AmbitoAlerta carril, List<DetectedAnomaly> anomalies)
    {
        var montoTotal = racha.Sum(f => f.TotalNeto);
        var vendedores = racha.Select(f => f.CodigoVendedor.Trim()).Distinct().ToList();
        var duracionMinutos = (racha[^1].FechaDocumento - racha[0].FechaDocumento).TotalMinutes;

        var (score, nivel) = Scoring.Calculate(riesgoBase: 50, montoInvolucrado: montoTotal);
        anomalies.Add(new DetectedAnomaly
        {
            TipoDetector = TipoDetector.PaymentFraud,
            Ambito = carril,
            Descripcion = $"Despachos rápidos sucesivos: cliente {cliente} con {racha.Count} " +
                          $"transacciones en {duracionMinutos:F0} minutos (gaps < {umbralMinutos} min). " +
                          $"Monto total: ${montoTotal:F2}",
            Score = score,
            NivelRiesgo = nivel,
            EstacionId = context.EstacionId,
            EmpleadoCodigo = vendedores.Count == 1 ? vendedores[0] : null,
            TransaccionReferencia = $"RAPIDOS-{cliente}-{racha[0].SecuenciaDocumento}",
            // Primera factura de la racha: representante con nº de documento (enlace), RUC y placa.
            Fuente = racha[0],
            Metadata = new Dictionary<string, object>
            {
                ["Cliente"] = cliente,
                ["CantidadTransacciones"] = racha.Count,
                ["DuracionMinutos"] = duracionMinutos,
                ["UmbralMinutos"] = umbralMinutos,
                ["MontoTotal"] = montoTotal,
                ["Vendedores"] = vendedores,
                ["Documentos"] = racha.Select(f => f.NumeroDocumento).ToList()
            }
        });
    }
}
