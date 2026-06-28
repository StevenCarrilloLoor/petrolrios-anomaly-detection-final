using PetrolRios.Application.DTOs.Firebird;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.PaymentFraud;

/// <summary>
/// Despachos rápidos sucesivos al MISMO identificador: 2 o más despachos del mismo RUC/cliente (o de la
/// misma placa) separados por menos de N minutos. Es físicamente improbable repostar tan seguido; patrón
/// confirmado con datos REALES de SanPio (entrevista de auditoría): el despachador hace despachos en ráfaga
/// reutilizando un identificador para ahorrar tiempo o emitir facturas ficticias.
///
/// Es DINÁMICO en la llave (lo que pidió Steven): si <b>no cambian el RUC/cliente</b> y despachan a otras
/// placas, acumula por RUC (la evidencia avisa que las placas varían); si es la <b>misma placa</b> con
/// clientes/RUC distintos, acumula por placa; si coinciden ambos, es lo más fuerte (mismo RUC y misma placa).
///
/// Es ACUMULABLE/ESCALABLE (<see cref="DetectedAnomaly.EsAcumulable"/>): en vez de una alerta nueva por cada
/// ráfaga, el job acumula las del mismo caso (referencia <c>RAPIDOS-{RUC|PLA}-{valor}-{día}</c>) en UNA alerta
/// que crece y sube de nivel por cantidad (2–3 Medio, 4–5 Alto, 6+ Crítico) re-emergiendo arriba. Mínimo 2
/// (auditoría: 2 ya es raro). Excluye la placa genérica ZZZ999949 y las vacías. Complementa a "Placa
/// reutilizada en el día" (conteo por jornada, sin mirar el tiempo entre despachos).
/// </summary>
public sealed class DespachosRapidosRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    /// <summary>Mínimo de despachos consecutivos rápidos al mismo identificador para considerar el caso.</summary>
    private const int MinDespachosRapidos = 2;

    /// <summary>Placa genérica de "consumidor final" (ARCERNNR): aparece legítimamente muchas veces; la controla PlacaGenericaRule.</summary>
    private const string PlacaGenerica = "ZZZ999949";

    public override TipoDetector Detector => TipoDetector.PaymentFraud;
    public override string Parametro => "DespachosRapidosMinutosUmbral";
    public override double UmbralPorDefecto => 10.0;
    public override AmbitoAlerta AmbitoPorDefecto => AmbitoAlerta.Ambos;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var umbralMinutos = Umbral(regla);
        var carril = Carril(regla);
        var handled = new HashSet<double>(); // SEC_DCTO ya cubiertos por un caso de RUC (evita doble conteo)

        // 1) Por RUC/cédula (la identidad del cliente): "no quitan el cliente/RUC y despachan a otras placas".
        foreach (var grupo in context.Facturas
            .Where(f => !string.IsNullOrWhiteSpace(f.RucCliente))
            .GroupBy(f => new { Ruc = f.RucCliente.Trim(), Dia = f.FechaDocumento.Date }))
        {
            var rapidas = ExtraerRapidas(grupo.OrderBy(f => f.FechaDocumento).ToList(), umbralMinutos);
            if (rapidas.Count < MinDespachosRapidos) continue;
            foreach (var f in rapidas) handled.Add(f.SecuenciaDocumento);
            EmitirCaso(context, "RUC", grupo.Key.Ruc, grupo.Key.Dia, rapidas, umbralMinutos, carril, anomalies);
        }

        // 2) Por placa (misma placa con clientes/RUC distintos), entre las facturas NO cubiertas por un RUC.
        foreach (var grupo in context.Facturas
            .Where(f => !string.IsNullOrWhiteSpace(f.Placa)
                     && !f.Placa.Trim().Equals(PlacaGenerica, StringComparison.OrdinalIgnoreCase)
                     && !handled.Contains(f.SecuenciaDocumento))
            .GroupBy(f => new { Placa = f.Placa.Trim().ToUpperInvariant(), Dia = f.FechaDocumento.Date }))
        {
            var rapidas = ExtraerRapidas(grupo.OrderBy(f => f.FechaDocumento).ToList(), umbralMinutos);
            if (rapidas.Count < MinDespachosRapidos) continue;
            EmitirCaso(context, "PLA", grupo.Key.Placa, grupo.Key.Dia, rapidas, umbralMinutos, carril, anomalies);
        }

        return anomalies;
    }

    /// <summary>Facturas (ordenadas por fecha) que están en alguna racha de >= 2 con gaps &lt; umbral.</summary>
    private static List<FacturaDto> ExtraerRapidas(List<FacturaDto> ordenadas, double umbralMinutos)
    {
        var rapidas = new List<FacturaDto>();
        var racha = new List<FacturaDto> { ordenadas[0] };
        for (var i = 1; i <= ordenadas.Count; i++)
        {
            var continua = i < ordenadas.Count &&
                (ordenadas[i].FechaDocumento - ordenadas[i - 1].FechaDocumento).TotalMinutes < umbralMinutos;
            if (continua) { racha.Add(ordenadas[i]); continue; }
            if (racha.Count >= MinDespachosRapidos) rapidas.AddRange(racha);
            if (i < ordenadas.Count) racha = [ordenadas[i]];
        }
        return rapidas;
    }

    private void EmitirCaso(
        DetectionContext context, string clave, string valor, DateTime dia, List<FacturaDto> rapidas,
        double umbralMinutos, AmbitoAlerta carril, List<DetectedAnomaly> anomalies)
    {
        var montoTotal = rapidas.Sum(f => f.TotalNeto);
        var placas = rapidas.Select(f => f.Placa.Trim().ToUpperInvariant()).Where(p => p.Length > 0).Distinct().ToList();
        var rucs = rapidas.Select(f => f.RucCliente.Trim()).Where(r => r.Length > 0).Distinct().ToList();
        var clientes = rapidas.Select(f => f.CodigoCliente.Trim()).Where(c => c.Length > 0).Distinct().ToList();
        var vendedores = rapidas.Select(f => f.CodigoVendedor.Trim()).Where(v => v.Length > 0).Distinct().ToList();
        var documentos = rapidas.Select(f => f.NumeroDocumento.Trim()).Where(n => n.Length > 0).ToList();

        // El conteo NO va en la descripción (lo lleva EventosAcumulados, que crece y escala): la descripción
        // identifica el caso y AVISA qué identificador se mantiene constante (lo que pidió Steven).
        string descripcion;
        if (clave == "RUC")
        {
            var placaMsg = placas.Count == 1 ? $"misma placa {placas[0]}"
                         : placas.Count > 1 ? $"placas distintas ({string.Join(", ", placas)})"
                         : "sin placa";
            descripcion = $"Despachos rápidos al mismo RUC {valor} (gaps < {umbralMinutos:F0} min; {placaMsg}). " +
                          "No cambian el cliente/RUC entre despachos — patrón a revisar.";
        }
        else
        {
            var rucMsg = rucs.Count <= 1 ? "mismo RUC/cliente" : $"RUC/clientes distintos ({string.Join(", ", rucs)})";
            descripcion = $"Despachos rápidos a la misma placa {valor} (gaps < {umbralMinutos:F0} min; {rucMsg}). " +
                          "Posible reutilización de la placa por el despachador — patrón a revisar.";
        }

        var (score, nivel) = Alerta.EscalarPorConteo(rapidas.Count);

        anomalies.Add(new DetectedAnomaly
        {
            TipoDetector = TipoDetector.PaymentFraud,
            Ambito = carril,
            Descripcion = descripcion,
            Score = score,
            NivelRiesgo = nivel,
            EstacionId = context.EstacionId,
            EmpleadoCodigo = vendedores.Count == 1 ? vendedores[0] : null,
            TransaccionReferencia = $"RAPIDOS-{clave}-{valor}-{dia:yyyyMMdd}",
            EsAcumulable = true,
            EventosEnLote = rapidas.Count,
            Fuente = rapidas[0],
            Metadata = new Dictionary<string, object>
            {
                ["TipoCaso"] = clave == "RUC" ? "Mismo RUC/cliente" : "Misma placa",
                ["Identificador"] = valor,
                ["Dia"] = dia.ToString("yyyy-MM-dd"),
                ["DespachosRapidos"] = rapidas.Count,
                ["UmbralMinutos"] = umbralMinutos,
                ["MontoTotal"] = montoTotal,
                ["Placas"] = placas,
                ["Rucs"] = rucs,
                ["Clientes"] = clientes,
                ["Vendedores"] = vendedores,
                ["Documentos"] = documentos
            }
        });
    }
}
