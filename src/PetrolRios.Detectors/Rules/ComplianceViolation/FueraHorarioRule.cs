using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.ComplianceViolation;

/// <summary>
/// Operación fuera del horario configurado por la estación. DESHABILITADA por defecto (las estaciones
/// de PetrolRíos operan 24/7); se conserva configurable para estaciones con horario restringido.
/// </summary>
public sealed class FueraHorarioRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.ComplianceViolation;
    public override string Parametro => "FueraHorarioHabilitado";
    public override double UmbralPorDefecto => 1.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var carril = Carril(regla);

        foreach (var factura in context.Facturas)
        {
            var horaTransaccion = TimeOnly.FromDateTime(factura.FechaDocumento);

            bool fueraHorario;
            if (context.HoraApertura < context.HoraCierre)
            {
                // Horario normal (ej: 6:00 a 22:00)
                fueraHorario = horaTransaccion < context.HoraApertura
                            || horaTransaccion > context.HoraCierre;
            }
            else
            {
                // Horario nocturno (ej: 22:00 a 6:00) — menos común pero posible
                fueraHorario = horaTransaccion < context.HoraApertura
                            && horaTransaccion > context.HoraCierre;
            }

            if (!fueraHorario) continue;

            var (score, nivel) = Scoring.Calculate(riesgoBase: 50);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = carril,
                Descripcion = $"Transacción fuera de horario: {horaTransaccion:HH:mm} " +
                              $"(horario permitido: {context.HoraApertura:HH:mm}-{context.HoraCierre:HH:mm}). " +
                              $"Doc: {factura.NumeroDocumento}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Metadata = new Dictionary<string, object>
                {
                    ["HoraTransaccion"] = horaTransaccion.ToString("HH:mm"),
                    ["HoraApertura"] = context.HoraApertura.ToString("HH:mm"),
                    ["HoraCierre"] = context.HoraCierre.ToString("HH:mm"),
                    ["NumeroDocumento"] = factura.NumeroDocumento
                }
            });
        }
        return anomalies;
    }
}
