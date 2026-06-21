using PetrolRios.Application.Interfaces;
using PetrolRios.Detectors.Rules;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors.Rules.ComplianceViolation;

// Cada regla del detector de cumplimiento es ahora su propia clase (Strategy). Agregar una regla
// nueva = añadir una clase aquí (o en su archivo) y registrarla en DI; el detector no cambia.

/// <summary>Venta excesiva a la placa genérica ZZZ999949 por encima del cupo regulatorio (ARCERNNR).</summary>
public sealed class PlacaGenericaRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    private const string PlacaGenerica = "ZZZ999949";

    public override TipoDetector Detector => TipoDetector.ComplianceViolation;
    public override string Parametro => "PlacaGenericaGalonesMaximo";
    public override double UmbralPorDefecto => 5.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var galonesMaximo = Umbral(regla);
        var carril = Carril(regla);

        // La placa viene de DCTO.PLA_DCTO y los galones de DESP.CAN_DESP
        var facturasPlacaGenerica = context.Facturas
            .Where(f => f.Placa.Trim().Equals(PlacaGenerica, StringComparison.OrdinalIgnoreCase));

        foreach (var factura in facturasPlacaGenerica)
        {
            // Buscar detalles de despacho asociados al mismo cliente/manguera
            var detallesAsociados = context.Detalles
                .Where(d => d.CodigoCliente.Trim() == factura.CodigoCliente.Trim()
                         || d.CodigoManguera.Trim() == factura.CodigoManguera.Trim())
                .ToList();

            var galones = detallesAsociados.Sum(d => d.Cantidad);
            if (galones <= 0) galones = factura.TotalNeto; // Fallback al monto si no hay detalles
            if (galones <= galonesMaximo) continue;

            var (score, nivel) = Scoring.Calculate(riesgoBase: 65, montoInvolucrado: galones);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = carril,
                Descripcion = $"Placa genérica {PlacaGenerica} con {galones:F2} galones " +
                              $"(máximo regulatorio: {galonesMaximo} gal). Doc: {factura.NumeroDocumento}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Metadata = new Dictionary<string, object>
                {
                    ["Placa"] = PlacaGenerica,
                    ["Galones"] = galones,
                    ["GalonesMaximo"] = galonesMaximo,
                    ["NumeroDocumento"] = factura.NumeroDocumento
                }
            });
        }
        return anomalies;
    }
}

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

/// <summary>
/// Venta sin placa en monto mayor (Tabla 2 de la tesis: "Ventas sin placa en montos mayores"). Las
/// ventas de combustible de montos altos sin identificación del vehículo impiden la trazabilidad
/// exigida por la normativa de comercialización de combustibles.
/// </summary>
public sealed class VentaSinPlacaRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.ComplianceViolation;
    public override string Parametro => "VentaSinPlacaMontoMinimo";
    public override double UmbralPorDefecto => 200.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var montoMinimo = Umbral(regla);
        var carril = Carril(regla);

        var ventasSinPlaca = context.Facturas
            .Where(f => string.IsNullOrWhiteSpace(f.Placa) && f.TotalNeto > montoMinimo);

        foreach (var factura in ventasSinPlaca)
        {
            var reincidencias = context.AlertasPreviasPorEmpleado.GetValueOrDefault(factura.CodigoVendedor.Trim(), 0);
            var (score, nivel) = Scoring.Calculate(riesgoBase: 50, montoInvolucrado: factura.TotalNeto, reincidenciasEmpleado: reincidencias);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = carril,
                Descripcion = $"Venta de ${factura.TotalNeto:F2} sin placa registrada " +
                              $"(monto mínimo que exige placa: ${montoMinimo:F2}). " +
                              $"Doc: {factura.NumeroDocumento}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroDocumento"] = factura.NumeroDocumento,
                    ["Monto"] = factura.TotalNeto,
                    ["MontoMinimo"] = montoMinimo,
                    ["Cliente"] = factura.CodigoCliente.Trim()
                }
            });
        }
        return anomalies;
    }
}

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

/// <summary>
/// Venta sin identificación del cliente (cédula/RUC) en monto material. El SRI (Resolución
/// NAC-DGERCGC13-00382) exige registrar la cédula/RUC del comprador en las facturas de combustibles
/// líquidos; una venta significativa sin ese dato rompe la trazabilidad tributaria y puede encubrir
/// fraccionamiento de ventas.
/// </summary>
public sealed class VentaSinIdentificacionRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.ComplianceViolation;
    public override string Parametro => "VentaSinIdentificacionMontoMinimo";
    public override double UmbralPorDefecto => 50.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var montoMinimo = Umbral(regla);
        var carril = Carril(regla);

        var ventas = context.Facturas
            .Where(f => string.IsNullOrWhiteSpace(f.RucCliente) && f.TotalNeto > montoMinimo);

        foreach (var factura in ventas)
        {
            var (score, nivel) = Scoring.Calculate(riesgoBase: 45, montoInvolucrado: factura.TotalNeto);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = carril,
                Descripcion = $"Venta de ${factura.TotalNeto:F2} sin cédula/RUC del cliente " +
                              $"(el SRI lo exige; mínimo: ${montoMinimo:F2}). Doc: {factura.NumeroDocumento}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroDocumento"] = factura.NumeroDocumento,
                    ["Monto"] = factura.TotalNeto,
                    ["MontoMinimo"] = montoMinimo,
                    ["Placa"] = factura.Placa.Trim()
                }
            });
        }
        return anomalies;
    }
}

/// <summary>
/// Despacho de alto volumen sin placa registrada. Cargar muchos galones sin identificar el vehículo
/// es el patrón típico de desvío de combustible (llenado de tanques o canecas para reventa), que la
/// ARCERNNR controla mediante cupos y trazabilidad por placa.
/// </summary>
public sealed class AltoVolumenSinPlacaRule(RiskScoringEngine scoring) : DetectionRuleBase(scoring)
{
    public override TipoDetector Detector => TipoDetector.ComplianceViolation;
    public override string Parametro => "GalonesSinPlacaMaximo";
    public override double UmbralPorDefecto => 20.0;

    public override IEnumerable<DetectedAnomaly> Evaluar(DetectionContext context, ReglaDeteccion? regla)
    {
        var anomalies = new List<DetectedAnomaly>();
        var galonesMaximo = Umbral(regla);
        var carril = Carril(regla);

        foreach (var factura in context.Facturas.Where(f => string.IsNullOrWhiteSpace(f.Placa)))
        {
            var galones = context.Detalles
                .Where(d => d.CodigoManguera.Trim() == factura.CodigoManguera.Trim())
                .Sum(d => d.Cantidad);
            if (galones <= galonesMaximo) continue;

            var (score, nivel) = Scoring.Calculate(riesgoBase: 60, montoInvolucrado: galones);
            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = carril,
                Descripcion = $"Despacho de {galones:F2} galones sin placa registrada " +
                              $"(máximo sin placa: {galonesMaximo:F0} gal; posible desvío). Doc: {factura.NumeroDocumento}",
                Score = score,
                NivelRiesgo = nivel,
                EstacionId = context.EstacionId,
                EmpleadoCodigo = factura.CodigoVendedor.Trim(),
                TransaccionReferencia = $"DCTO-{factura.SecuenciaDocumento}",
                Metadata = new Dictionary<string, object>
                {
                    ["NumeroDocumento"] = factura.NumeroDocumento,
                    ["Galones"] = galones,
                    ["GalonesMaximo"] = galonesMaximo
                }
            });
        }
        return anomalies;
    }
}
