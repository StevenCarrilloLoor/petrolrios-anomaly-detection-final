using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.InvoiceAnomaly;

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

            // Evidencia para el auditor: ANUL no trae vendedor, pero SÍ identifica QUÉ comprobantes se
            // anularon (establecimiento-puntoEmisión-secuencial) — con eso el auditor jala las facturas
            // anuladas en Consultas. Se listan los secuenciales (rango si Inicio≠Fin), el tipo, las fechas
            // y las autorizaciones, en lugar de solo un conteo abstracto.
            var comprobantes = grupo
                .OrderBy(a => a.FechaAnulacion)
                .Select(a =>
                {
                    var ini = a.SecuencialInicio.Trim();
                    var fin = a.SecuencialFin.Trim();
                    var sec = string.IsNullOrEmpty(fin) || fin == ini ? ini : $"{ini}…{fin}";
                    return $"{a.Establecimiento.Trim()}-{a.PuntoEmision.Trim()}-{sec}";
                })
                .Where(s => s.Length > 2)
                .Distinct()
                .ToList();
            var tipos = grupo.Select(a => a.TipoComprobante.Trim())
                .Where(t => t.Length > 0).Distinct().ToList();
            var autorizaciones = grupo.Select(a => a.Autorizacion.Trim())
                .Where(a => a.Length > 0).Distinct().Take(10).ToList();
            var desde = grupo.Min(a => a.FechaAnulacion);
            var hasta = grupo.Max(a => a.FechaAnulacion);
            var muestra = string.Join(", ", comprobantes.Take(5));

            var (score, nivel) = Scoring.Calculate(riesgoBase: 60);
            var metadata = new Dictionary<string, object>
            {
                ["Establecimiento"] = grupo.Key.Est,
                ["PuntoEmision"] = grupo.Key.Pto,
                ["DiasDistintos"] = diasDistintos,
                ["TotalAnulaciones"] = totalAnulaciones,
                ["DiasMinimo"] = diasMinimo,
                ["DesdeFecha"] = desde,
                ["HastaFecha"] = hasta,
                ["Comprobantes"] = comprobantes
            };
            if (tipos.Count > 0) metadata["TiposComprobante"] = tipos;
            if (autorizaciones.Count > 0) metadata["Autorizaciones"] = autorizaciones;

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.InvoiceAnomaly,
                Ambito = carril,
                Descripcion = $"Anulaciones recurrentes en {grupo.Key.Est}-{grupo.Key.Pto}: " +
                              $"{totalAnulaciones} anulaciones en {diasDistintos} días distintos " +
                              $"(umbral: {diasMinimo}) entre {desde:dd/MM/yyyy} y {hasta:dd/MM/yyyy}. " +
                              $"Comprobantes: {muestra}{(comprobantes.Count > 5 ? "…" : "")}. " +
                              $"Posible patrón de cancelar y reingresar (kiting).",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                TransaccionReferencia = $"ANUL-RECURR-{grupo.Key.Est}-{grupo.Key.Pto}",
                Metadata = metadata
            });
        }
        return anomalies;
    }
}
