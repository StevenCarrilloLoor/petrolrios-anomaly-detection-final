using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors;

/// <summary>
/// Detector de violaciones de cumplimiento regulatorio:
/// 1. Venta excesiva a placa genérica ZZZ999949 (regulación ARCERNNR).
/// 2. Vehículo con múltiples tipos de combustible en el mismo día.
/// 3. Venta sin placa en monto mayor (Tabla 2 de la tesis: identificación obligatoria).
/// 4. Operación fuera de horario configurado — DESHABILITADA por defecto: las estaciones
///    de PetrolRíos operan 24/7. Se conserva como regla configurable para estaciones
///    que definan un horario restringido.
/// </summary>
public sealed class ComplianceViolationDetector : IAnomalyDetector
{
    private const string PlacaGenerica = "ZZZ999949";

    private readonly RiskScoringEngine _scoring;
    private readonly ILogger<ComplianceViolationDetector> _logger;

    public TipoDetector Type => TipoDetector.ComplianceViolation;

    public ComplianceViolationDetector(RiskScoringEngine scoring, ILogger<ComplianceViolationDetector> logger)
    {
        _scoring = scoring;
        _logger = logger;
    }

    public Task<IReadOnlyList<DetectedAnomaly>> DetectAsync(DetectionContext context, CancellationToken ct)
    {
        var anomalies = new List<DetectedAnomaly>();
        var reglas = context.Reglas.Where(r => r.TipoDetector == TipoDetector.ComplianceViolation).ToList();

        var galonesMaxPlacaGenerica = GetUmbral(reglas, "PlacaGenericaGalonesMaximo", 5.0);
        var multipleCombustibleHabilitado = GetUmbral(reglas, "MultipleCombustibleHabilitado", 1.0) >= 1.0;
        var fueraHorarioHabilitado = GetUmbral(reglas, "FueraHorarioHabilitado", 1.0) >= 1.0;
        var montoMinimoSinPlaca = GetUmbral(reglas, "VentaSinPlacaMontoMinimo", 200.0);
        var montoSinIdentificacion = GetUmbral(reglas, "VentaSinIdentificacionMontoMinimo", 50.0);
        var galonesSinPlaca = GetUmbral(reglas, "GalonesSinPlacaMaximo", 20.0);

        // Regla 1: Placa genérica con exceso de galones
        if (galonesMaxPlacaGenerica is not null)
            DetectPlacaGenericaExceso(context, galonesMaxPlacaGenerica.Value, anomalies);

        // Regla 2: Múltiples tipos de combustible por placa/día
        if (multipleCombustibleHabilitado)
            DetectMultipleCombustible(context, anomalies);

        // Regla 3: Venta sin placa en monto mayor
        if (montoMinimoSinPlaca is not null)
            DetectVentaSinPlacaMontoMayor(context, montoMinimoSinPlaca.Value, anomalies);

        // Regla 4: Operaciones fuera de horario (solo si la estación define horario restringido)
        if (fueraHorarioHabilitado)
            DetectFueraHorario(context, anomalies);

        // Regla 5: Venta sin cédula/RUC del cliente en monto material (exigido por el SRI)
        if (montoSinIdentificacion is not null)
            DetectVentaSinIdentificacion(context, montoSinIdentificacion.Value, anomalies);

        // Regla 6: Despacho de alto volumen sin placa (patrón de desvío de combustible)
        if (galonesSinPlaca is not null)
            DetectAltoVolumenSinPlaca(context, galonesSinPlaca.Value, anomalies);

        _logger.LogDebug("ComplianceViolationDetector: {Count} anomalías en estación {Est}",
            anomalies.Count, context.EstacionNombre);

        return Task.FromResult<IReadOnlyList<DetectedAnomaly>>(anomalies);
    }

    private void DetectPlacaGenericaExceso(
        DetectionContext context, double galonesMaximo, List<DetectedAnomaly> anomalies)
    {
        // Buscar despachos con placa genérica y galones > umbral
        // La placa viene de DCTO.PLA_DCTO y los galones de DESP.CAN_DESP
        var facturasPlacaGenerica = context.Facturas
            .Where(f => f.Placa.Trim().Equals(PlacaGenerica, StringComparison.OrdinalIgnoreCase));

        foreach (var factura in facturasPlacaGenerica)
        {
            // Buscar detalles de despacho asociados al mismo turno/manguera
            var detallesAsociados = context.Detalles
                .Where(d => d.CodigoCliente.Trim() == factura.CodigoCliente.Trim()
                         || d.CodigoManguera.Trim() == factura.CodigoManguera.Trim())
                .ToList();

            var galones = detallesAsociados.Sum(d => d.Cantidad);
            if (galones <= 0) galones = factura.TotalNeto; // Fallback al monto si no hay detalles

            if (galones > galonesMaximo)
            {
                var (score, nivel) = _scoring.Calculate(
                    riesgoBase: 65,
                    montoInvolucrado: galones);

                anomalies.Add(new DetectedAnomaly
                {
                    TipoDetector = TipoDetector.ComplianceViolation,
                    Ambito = GetAmbito(context.Reglas, "PlacaGenericaGalonesMaximo", AmbitoAlerta.Auditoria),
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
        }
    }

    private void DetectMultipleCombustible(
        DetectionContext context, List<DetectedAnomaly> anomalies)
    {
        // Agrupar detalles de despacho por (placa del cliente, fecha del día) y verificar
        // si hay más de un tipo de producto (diésel Y gasolina extra)
        var ventasPorPlacaDia = context.Facturas
            .Where(f => !string.IsNullOrWhiteSpace(f.Placa))
            .GroupBy(f => new
            {
                Placa = f.Placa.Trim().ToUpperInvariant(),
                Dia = f.FechaDocumento.Date
            });

        foreach (var grupo in ventasPorPlacaDia)
        {
            // Obtener los despachos de estas facturas para ver los productos
            var facturasDelGrupo = grupo.ToList();
            var productosDelDia = context.Detalles
                .Where(d => facturasDelGrupo.Any(f =>
                    f.CodigoManguera.Trim() == d.CodigoManguera.Trim()))
                .Select(d => d.CodigoProducto.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();

            // Si no hay detalles suficientes, intentar con las facturas directamente
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

            var (score, nivel) = _scoring.Calculate(riesgoBase: 55);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = GetAmbito(context.Reglas, "MultipleCombustibleHabilitado", AmbitoAlerta.Auditoria),
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
    }

    /// <summary>
    /// Venta sin placa en monto mayor (Tabla 2 de la tesis: "Ventas sin placa en montos
    /// mayores"). Las ventas de combustible de montos altos sin identificación del vehículo
    /// impiden la trazabilidad exigida por la normativa de comercialización de combustibles.
    /// </summary>
    private void DetectVentaSinPlacaMontoMayor(
        DetectionContext context, double montoMinimo, List<DetectedAnomaly> anomalies)
    {
        var ventasSinPlaca = context.Facturas
            .Where(f => string.IsNullOrWhiteSpace(f.Placa) && f.TotalNeto > montoMinimo);

        foreach (var factura in ventasSinPlaca)
        {
            var reincidencias = context.AlertasPreviasPorEmpleado
                .GetValueOrDefault(factura.CodigoVendedor.Trim(), 0);

            var (score, nivel) = _scoring.Calculate(
                riesgoBase: 50,
                montoInvolucrado: factura.TotalNeto,
                reincidenciasEmpleado: reincidencias);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = GetAmbito(context.Reglas, "VentaSinPlacaMontoMinimo", AmbitoAlerta.Auditoria),
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
    }

    /// <summary>
    /// Venta sin identificación del cliente (cédula/RUC) en monto material. El SRI
    /// (Resolución NAC-DGERCGC13-00382) exige registrar la cédula/RUC del comprador en las
    /// facturas de combustibles líquidos; una venta significativa sin ese dato rompe la
    /// trazabilidad tributaria y puede encubrir fraccionamiento de ventas.
    /// </summary>
    private void DetectVentaSinIdentificacion(
        DetectionContext context, double montoMinimo, List<DetectedAnomaly> anomalies)
    {
        var ventas = context.Facturas
            .Where(f => string.IsNullOrWhiteSpace(f.RucCliente) && f.TotalNeto > montoMinimo);

        foreach (var factura in ventas)
        {
            var (score, nivel) = _scoring.Calculate(
                riesgoBase: 45,
                montoInvolucrado: factura.TotalNeto);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = GetAmbito(context.Reglas, "VentaSinIdentificacionMontoMinimo", AmbitoAlerta.Auditoria),
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
    }

    /// <summary>
    /// Despacho de alto volumen sin placa registrada. Cargar muchos galones sin identificar el
    /// vehículo es el patrón típico de desvío de combustible (llenado de tanques o canecas para
    /// reventa), que la ARCERNNR controla mediante cupos y trazabilidad por placa.
    /// </summary>
    private void DetectAltoVolumenSinPlaca(
        DetectionContext context, double galonesMaximo, List<DetectedAnomaly> anomalies)
    {
        foreach (var factura in context.Facturas.Where(f => string.IsNullOrWhiteSpace(f.Placa)))
        {
            var galones = context.Detalles
                .Where(d => d.CodigoManguera.Trim() == factura.CodigoManguera.Trim())
                .Sum(d => d.Cantidad);
            if (galones <= galonesMaximo) continue;

            var (score, nivel) = _scoring.Calculate(
                riesgoBase: 60,
                montoInvolucrado: galones);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = GetAmbito(context.Reglas, "GalonesSinPlacaMaximo", AmbitoAlerta.Auditoria),
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
    }

    private void DetectFueraHorario(
        DetectionContext context, List<DetectedAnomaly> anomalies)
    {
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

            var (score, nivel) = _scoring.Calculate(riesgoBase: 50);

            anomalies.Add(new DetectedAnomaly
            {
                TipoDetector = TipoDetector.ComplianceViolation,
                Ambito = GetAmbito(context.Reglas, "FueraHorarioHabilitado", AmbitoAlerta.Auditoria),
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
    }

    /// <summary>
    /// Obtiene el umbral de una regla. Devuelve null si la regla existe pero está desactivada.
    /// Si la regla no existe, usa el valor por defecto.
    /// </summary>
    private static double? GetUmbral(
        IReadOnlyList<Domain.Entities.ReglaDeteccion> reglas, string parametro, double defaultValue) =>
        reglas.FirstOrDefault(r => r.ParametroNombre == parametro) is { } regla
            ? (regla.Activa ? regla.ValorUmbral : null)
            : defaultValue;

    /// <summary>Carril configurado para la regla; si no existe en BD, usa el fallback del detector.</summary>
    private static AmbitoAlerta GetAmbito(
        IReadOnlyList<Domain.Entities.ReglaDeteccion> reglas, string parametro, AmbitoAlerta fallback) =>
        reglas.FirstOrDefault(r => r.ParametroNombre == parametro)?.Ambito ?? fallback;
}
