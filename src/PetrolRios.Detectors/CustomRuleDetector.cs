using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;
using PetrolRios.Application.ReglasPersonalizadas;
using PetrolRios.Application.ReglasPersonalizadas.Expresiones;
using PetrolRios.Domain.Entities;
using PetrolRios.Domain.Enums;

namespace PetrolRios.Detectors;

/// <summary>
/// Detector genérico que evalúa las reglas de negocio definidas por el usuario
/// (escalabilidad: nuevas reglas sin tocar código). Cada regla filtra los registros
/// de su fuente con condiciones AND y, opcionalmente, agrupa y compara un agregado
/// contra un umbral. Una regla mal definida se ignora y se registra, sin afectar
/// al resto del ciclo.
/// </summary>
public sealed class CustomRuleDetector : IAnomalyDetector
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly RiskScoringEngine _scoring;
    private readonly ILogger<CustomRuleDetector> _logger;

    public TipoDetector Type => TipoDetector.Personalizada;

    public CustomRuleDetector(RiskScoringEngine scoring, ILogger<CustomRuleDetector> logger)
    {
        _scoring = scoring;
        _logger = logger;
    }

    public Task<IReadOnlyList<DetectedAnomaly>> DetectAsync(DetectionContext context, CancellationToken ct)
    {
        var anomalies = new List<DetectedAnomaly>();

        foreach (var regla in context.ReglasPersonalizadas.Where(r => r.Activa))
        {
            try
            {
                EvaluarRegla(context, regla, anomalies);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Regla personalizada '{Nombre}' (#{Id}) inválida; se omite en este ciclo",
                    regla.Nombre, regla.Id);
            }
        }

        _logger.LogDebug("CustomRuleDetector: {Count} anomalías en estación {Est}",
            anomalies.Count, context.EstacionNombre);

        return Task.FromResult<IReadOnlyList<DetectedAnomaly>>(anomalies);
    }

    private void EvaluarRegla(DetectionContext context, ReglaPersonalizada regla, List<DetectedAnomaly> anomalies)
    {
        var condiciones = JsonSerializer.Deserialize<List<CondicionRegla>>(regla.CondicionesJson, JsonOpts) ?? [];
        var agregacion = string.IsNullOrWhiteSpace(regla.AgregacionJson)
            ? null
            : JsonSerializer.Deserialize<AgregacionRegla>(regla.AgregacionJson, JsonOpts);

        var registros = ObtenerFuente(context, regla.FuenteDatos);
        if (registros.Count == 0) return;

        List<object> filtrados;
        if (!string.IsNullOrWhiteSpace(regla.ExpresionAvanzada))
        {
            // Modo avanzado: filtra con la expresión lógica (compilada una vez por regla)
            var evaluador = EvaluadorExpresion.Compilar(regla.ExpresionAvanzada);
            filtrados = registros
                .Where(r => evaluador.Evaluar(new ContextoRegistro(regla.FuenteDatos, r)))
                .ToList();
        }
        else
        {
            // Modo básico: condiciones simples combinadas con AND
            filtrados = registros
                .Where(r => condiciones.All(c => EvaluarCondicion(regla.FuenteDatos, r, c)))
                .ToList();
        }

        if (filtrados.Count == 0) return;

        if (agregacion is null)
        {
            // Modo por registro: una alerta por cada registro que cumple las condiciones
            foreach (var registro in filtrados)
                anomalies.Add(CrearAlertaPorRegistro(context, regla, condiciones, registro));
        }
        else
        {
            // Modo agregado: agrupar y comparar el agregado contra el umbral
            var grupos = filtrados.GroupBy(r =>
                Convert.ToString(CatalogoReglasPersonalizadas.GetValor(regla.FuenteDatos, agregacion.AgruparPor, r),
                    CultureInfo.InvariantCulture) ?? "");

            foreach (var grupo in grupos)
            {
                var valorAgregado = CalcularAgregado(regla.FuenteDatos, agregacion, grupo.ToList());
                if (!CompararNumeros(valorAgregado, agregacion.Operador, agregacion.Umbral)) continue;

                anomalies.Add(CrearAlertaAgregada(context, regla, agregacion, grupo.Key, valorAgregado, grupo.Count()));
            }
        }
    }

    /// <summary>Carril de la alerta según el ámbito configurado en la regla (por defecto Auditoría).</summary>
    private static AmbitoAlerta AmbitoDe(ReglaPersonalizada regla) =>
        string.Equals(regla.Ambito?.Trim(), "Operativa", StringComparison.OrdinalIgnoreCase)
            ? AmbitoAlerta.Operativa
            : AmbitoAlerta.Auditoria;

    private static List<object> ObtenerFuente(DetectionContext context, string fuente) => fuente switch
    {
        "Factura" => context.Facturas.Cast<object>().ToList(),
        "CierreTurno" => context.CierresTurno.Cast<object>().ToList(),
        "DetalleFactura" => context.Detalles.Cast<object>().ToList(),
        "Credito" => context.Creditos.Cast<object>().ToList(),
        "TarjetaTurno" => context.TarjetasTurno.Cast<object>().ToList(),
        // Fuente configurable (tabla arbitraria enviada por el agente): registros genéricos.
        _ => context.FuentesGenericas.TryGetValue(fuente, out var filas)
            ? filas.Cast<object>().ToList()
            : []
    };

    /// <summary>true si el valor es numérico (para inferir el tipo en fuentes genéricas sin catálogo).</summary>
    private static bool EsNumerico(object? valor) =>
        valor is double or float or decimal or int or long or short or byte;

    private static bool EvaluarCondicion(string fuente, object registro, CondicionRegla condicion)
    {
        var valor = CatalogoReglasPersonalizadas.GetValor(fuente, condicion.Campo, registro);
        var campo = CatalogoReglasPersonalizadas.BuscarCampo(fuente, condicion.Campo);

        // En fuentes conocidas, un campo inexistente invalida la condición. En fuentes
        // genéricas (tablas configurables) no hay catálogo: se infiere el tipo del valor.
        var fuenteConocida = CatalogoReglasPersonalizadas.Fuentes.ContainsKey(fuente);
        if (campo is null && fuenteConocida) return false;

        var esNumero = campo?.Tipo == CatalogoReglasPersonalizadas.TipoNumero
                       || (campo is null && EsNumerico(valor));

        if (esNumero)
        {
            var numero = Convert.ToDouble(valor ?? 0, CultureInfo.InvariantCulture);
            if (!double.TryParse(condicion.Valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var referencia))
                return false;
            return CompararNumeros(numero, condicion.Operador, referencia);
        }

        var texto = Convert.ToString(valor, CultureInfo.InvariantCulture)?.Trim() ?? "";
        var esperado = condicion.Valor.Trim();
        return condicion.Operador switch
        {
            "=" => string.Equals(texto, esperado, StringComparison.OrdinalIgnoreCase),
            "!=" => !string.Equals(texto, esperado, StringComparison.OrdinalIgnoreCase),
            "contiene" => texto.Contains(esperado, StringComparison.OrdinalIgnoreCase),
            "noContiene" => !texto.Contains(esperado, StringComparison.OrdinalIgnoreCase),
            "vacio" => string.IsNullOrWhiteSpace(texto),
            "noVacio" => !string.IsNullOrWhiteSpace(texto),
            _ => false
        };
    }

    private static bool CompararNumeros(double valor, string operador, double referencia) => operador switch
    {
        ">" => valor > referencia,
        ">=" => valor >= referencia,
        "<" => valor < referencia,
        "<=" => valor <= referencia,
        "=" => Math.Abs(valor - referencia) < 0.0001,
        "!=" => Math.Abs(valor - referencia) >= 0.0001,
        _ => false
    };

    private static double CalcularAgregado(string fuente, AgregacionRegla agregacion, List<object> registros)
    {
        if (agregacion.Funcion == "Conteo") return registros.Count;

        var valores = registros
            .Select(r => Convert.ToDouble(
                CatalogoReglasPersonalizadas.GetValor(fuente, agregacion.Campo ?? "", r) ?? 0,
                CultureInfo.InvariantCulture))
            .ToList();

        return agregacion.Funcion switch
        {
            "Suma" => valores.Sum(),
            "Promedio" => valores.Count > 0 ? valores.Average() : 0,
            _ => 0
        };
    }

    private DetectedAnomaly CrearAlertaPorRegistro(
        DetectionContext context, ReglaPersonalizada regla,
        List<CondicionRegla> condiciones, object registro)
    {
        var empleado = GetTexto(regla.FuenteDatos, CatalogoReglasPersonalizadas.CampoEmpleado(regla.FuenteDatos), registro);
        var monto = GetNumero(regla.FuenteDatos, CatalogoReglasPersonalizadas.CampoMonto(regla.FuenteDatos), registro);
        var reincidencias = empleado is null
            ? 0
            : context.AlertasPreviasPorEmpleado.GetValueOrDefault(empleado, 0);

        var (score, nivel) = _scoring.Calculate(regla.RiesgoBase, monto ?? 0, reincidencias);

        var esAvanzada = !string.IsNullOrWhiteSpace(regla.ExpresionAvanzada);
        var detalle = esAvanzada
            ? regla.ExpresionAvanzada!
            : string.Join(" y ", condiciones.Select(c => $"{c.Campo} {c.Operador} {FormatearValor(c)}"));

        var metadata = new Dictionary<string, object>
        {
            ["ReglaPersonalizada"] = regla.Nombre,
            ["Fuente"] = regla.FuenteDatos,
            [esAvanzada ? "Expresion" : "Condiciones"] = detalle
        };
        AgregarValoresClave(regla.FuenteDatos, registro, condiciones, metadata);

        return new DetectedAnomaly
        {
            TipoDetector = TipoDetector.Personalizada,
            Descripcion = $"Regla '{regla.Nombre}': registro de {regla.FuenteDatos} cumple [{detalle}]" +
                          (monto is not null ? $". Monto: ${monto:F2}" : ""),
            Score = score,
            NivelRiesgo = nivel,
            Ambito = AmbitoDe(regla),
            EstacionId = context.EstacionId,
            EmpleadoCodigo = empleado,
            TransaccionReferencia = $"REGLA-{regla.Id}",
            Metadata = metadata
        };
    }

    private DetectedAnomaly CrearAlertaAgregada(
        DetectionContext context, ReglaPersonalizada regla, AgregacionRegla agregacion,
        string grupo, double valorAgregado, int cantidadRegistros)
    {
        var (score, nivel) = _scoring.Calculate(regla.RiesgoBase, montoInvolucrado: valorAgregado);

        var descripcionFuncion = agregacion.Funcion == "Conteo"
            ? $"conteo = {valorAgregado:F0}"
            : $"{agregacion.Funcion.ToLowerInvariant()} de {agregacion.Campo} = {valorAgregado:F2}";

        var esEmpleado = agregacion.AgruparPor == CatalogoReglasPersonalizadas.CampoEmpleado(regla.FuenteDatos);

        return new DetectedAnomaly
        {
            TipoDetector = TipoDetector.Personalizada,
            Descripcion = $"Regla '{regla.Nombre}': {agregacion.AgruparPor} '{grupo}' con {descripcionFuncion} " +
                          $"{agregacion.Operador} {agregacion.Umbral} ({cantidadRegistros} registros de {regla.FuenteDatos})",
            Score = score,
            NivelRiesgo = nivel,
            Ambito = AmbitoDe(regla),
            EstacionId = context.EstacionId,
            EmpleadoCodigo = esEmpleado && !string.IsNullOrWhiteSpace(grupo) ? grupo : null,
            TransaccionReferencia = $"REGLA-{regla.Id}-{grupo}",
            Metadata = new Dictionary<string, object>
            {
                ["ReglaPersonalizada"] = regla.Nombre,
                ["Fuente"] = regla.FuenteDatos,
                ["AgrupadoPor"] = $"{agregacion.AgruparPor} = {grupo}",
                ["ValorAgregado"] = Math.Round(valorAgregado, 2),
                ["Umbral"] = agregacion.Umbral,
                ["RegistrosEnGrupo"] = cantidadRegistros
            }
        };
    }

    private static void AgregarValoresClave(
        string fuente, object registro, List<CondicionRegla> condiciones, Dictionary<string, object> metadata)
    {
        foreach (var campo in condiciones.Select(c => c.Campo).Distinct().Take(6))
        {
            var valor = CatalogoReglasPersonalizadas.GetValor(fuente, campo, registro);
            if (valor is not null) metadata[campo] = valor;
        }
    }

    private static string? GetTexto(string fuente, string? campo, object registro) =>
        campo is null
            ? null
            : Convert.ToString(CatalogoReglasPersonalizadas.GetValor(fuente, campo, registro),
                CultureInfo.InvariantCulture)?.Trim() is { Length: > 0 } s ? s : null;

    private static double? GetNumero(string fuente, string? campo, object registro) =>
        campo is null
            ? null
            : Convert.ToDouble(CatalogoReglasPersonalizadas.GetValor(fuente, campo, registro) ?? 0,
                CultureInfo.InvariantCulture);

    private static string FormatearValor(CondicionRegla c) =>
        c.Operador is "vacio" or "noVacio" ? "" : $"'{c.Valor}'";

    /// <summary>
    /// Adapta un registro de la fuente a <see cref="IContextoEvaluacion"/> para que
    /// el evaluador de expresiones resuelva campos por nombre usando el catálogo.
    /// </summary>
    private sealed class ContextoRegistro(string fuente, object registro) : IContextoEvaluacion
    {
        public Valor ObtenerCampo(string nombre)
        {
            var valor = CatalogoReglasPersonalizadas.GetValor(fuente, nombre, registro);
            var campo = CatalogoReglasPersonalizadas.BuscarCampo(fuente, nombre);

            // En fuentes conocidas el campo debe existir en el catálogo; en fuentes genéricas
            // (tablas configurables) se infiere el tipo del valor.
            var fuenteConocida = CatalogoReglasPersonalizadas.Fuentes.ContainsKey(fuente);
            if (campo is null && fuenteConocida)
                throw new ExpresionException($"El campo '{nombre}' no existe en la fuente '{fuente}'.");

            var esNumero = campo?.Tipo == CatalogoReglasPersonalizadas.TipoNumero
                           || (campo is null && EsNumerico(valor));

            return esNumero
                ? Valor.DeNumero(Convert.ToDouble(valor ?? 0, CultureInfo.InvariantCulture))
                : Valor.DeTexto(Convert.ToString(valor, CultureInfo.InvariantCulture)?.Trim() ?? "");
        }
    }
}
