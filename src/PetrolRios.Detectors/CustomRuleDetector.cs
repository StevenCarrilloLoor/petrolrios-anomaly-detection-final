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
        var configCondiciones = CatalogoReglasPersonalizadas.LeerCondiciones(regla.CondicionesJson);
        var condiciones = configCondiciones.Condiciones;
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
            // Modo básico: condiciones combinadas con el combinador elegido (Y = todas, O = cualquiera).
            // Sin condiciones (p. ej. solo agregación) pasan todos los registros.
            filtrados = condiciones.Count == 0
                ? registros
                : registros
                    .Where(r => configCondiciones.Combinador == "O"
                        ? condiciones.Any(c => EvaluarCondicion(regla.FuenteDatos, r, c))
                        : condiciones.All(c => EvaluarCondicion(regla.FuenteDatos, r, c)))
                    .ToList();
        }

        if (filtrados.Count == 0) return;

        if (agregacion is null)
        {
            // Modo por registro: una alerta por cada registro que cumple las condiciones
            foreach (var registro in filtrados)
                anomalies.Add(CrearAlertaPorRegistro(context, regla, condiciones, configCondiciones.Combinador, registro));
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
    private static AmbitoAlerta AmbitoDe(ReglaPersonalizada regla)
    {
        var a = regla.Ambito?.Trim();
        if (string.Equals(a, "Operativa", StringComparison.OrdinalIgnoreCase)) return AmbitoAlerta.Operativa;
        if (string.Equals(a, "Ambos", StringComparison.OrdinalIgnoreCase)) return AmbitoAlerta.Ambos;
        return AmbitoAlerta.Auditoria;
    }

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
        List<CondicionRegla> condiciones, string combinador, object registro)
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

        // Frase en lenguaje natural para el auditor (etiquetas del catálogo + operadores en palabras),
        // independiente del "detalle" técnico que queda guardado en la evidencia.
        var fraseLegible = esAvanzada
            ? $"{EtiquetaFuente(regla.FuenteDatos)} cumple la condición configurada"
            : FrasearCondiciones(regla.FuenteDatos, condiciones, combinador);

        var metadata = new Dictionary<string, object>
        {
            ["ReglaPersonalizada"] = regla.Nombre,
            ["Fuente"] = regla.FuenteDatos,
            [esAvanzada ? "Expresion" : "Condiciones"] = detalle
        };
        // La descripción que el usuario escribió en la regla, para que aparezca en la alerta (no solo en Reglas).
        if (!string.IsNullOrWhiteSpace(regla.Descripcion))
            metadata["Qué detecta"] = regla.Descripcion.Trim();
        AgregarValoresClave(regla.FuenteDatos, registro, condiciones, metadata);

        // Enriquecimiento: agrega a la evidencia los campos elegidos por el usuario, incluidos los de
        // tablas relacionadas (placa, vendedor, cliente, n° de factura), resueltos en memoria.
        AgregarCamposMostrar(context, regla, registro, metadata);

        return new DetectedAnomaly
        {
            TipoDetector = TipoDetector.Personalizada,
            Descripcion = DescripcionLegible(regla, fraseLegible, monto),
            Score = score,
            NivelRiesgo = nivel,
            Ambito = AmbitoDe(regla),
            EstacionId = context.EstacionId,
            EmpleadoCodigo = empleado,
            NotificarCorreo = regla.NotificarCorreo,
            TransaccionReferencia = $"REGLA-{regla.Id}",
            Metadata = metadata
        };
    }

    private DetectedAnomaly CrearAlertaAgregada(
        DetectionContext context, ReglaPersonalizada regla, AgregacionRegla agregacion,
        string grupo, double valorAgregado, int cantidadRegistros)
    {
        var (score, nivel) = _scoring.Calculate(regla.RiesgoBase, montoInvolucrado: valorAgregado);

        var funcionLegible = agregacion.Funcion == "Conteo"
            ? $"{valorAgregado:F0} registros"
            : $"{agregacion.Funcion.ToLowerInvariant()} de {EtiquetaCampo(regla.FuenteDatos, agregacion.Campo ?? "")} = {valorAgregado:F2}";
        var fraseLegible =
            $"{EtiquetaFuente(regla.FuenteDatos)} agrupado por {EtiquetaCampo(regla.FuenteDatos, agregacion.AgruparPor)} '{grupo}': " +
            $"{funcionLegible} ({OperadorEnPalabras(agregacion.Operador)} {agregacion.Umbral}; {cantidadRegistros} registros)";

        var esEmpleado = agregacion.AgruparPor == CatalogoReglasPersonalizadas.CampoEmpleado(regla.FuenteDatos);

        var metadata = new Dictionary<string, object>
        {
            ["ReglaPersonalizada"] = regla.Nombre,
            ["Fuente"] = regla.FuenteDatos,
            ["AgrupadoPor"] = $"{agregacion.AgruparPor} = {grupo}",
            ["ValorAgregado"] = Math.Round(valorAgregado, 2),
            ["Umbral"] = agregacion.Umbral,
            ["RegistrosEnGrupo"] = cantidadRegistros
        };
        if (!string.IsNullOrWhiteSpace(regla.Descripcion))
            metadata["Qué detecta"] = regla.Descripcion.Trim();

        return new DetectedAnomaly
        {
            TipoDetector = TipoDetector.Personalizada,
            Descripcion = DescripcionLegible(regla, fraseLegible, null),
            Score = score,
            NivelRiesgo = nivel,
            Ambito = AmbitoDe(regla),
            EstacionId = context.EstacionId,
            EmpleadoCodigo = esEmpleado && !string.IsNullOrWhiteSpace(grupo) ? grupo : null,
            NotificarCorreo = regla.NotificarCorreo,
            TransaccionReferencia = $"REGLA-{regla.Id}-{grupo}",
            Metadata = metadata
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

    /// <summary>
    /// Agrega a la evidencia los campos que el usuario eligió mostrar (<c>CamposMostrarJson</c>). Cada
    /// elemento es "Campo" (propio de la fuente) o "Fuente.Campo" (de una tabla relacionada): en ese
    /// caso busca la relación definida, cruza en memoria por su llave y trae el campo (placa, vendedor,
    /// cliente, n° de factura…). Tolerante a fallos: lo que no resuelve se omite y nunca lanza.
    /// </summary>
    private static void AgregarCamposMostrar(
        DetectionContext context, ReglaPersonalizada regla, object registro, Dictionary<string, object> metadata)
    {
        if (string.IsNullOrWhiteSpace(regla.CamposMostrarJson)) return;

        List<string>? campos;
        try { campos = JsonSerializer.Deserialize<List<string>>(regla.CamposMostrarJson, JsonOpts); }
        catch { return; }
        if (campos is null) return;

        foreach (var refCampo in campos.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().Take(12))
        {
            var partes = refCampo.Split('.', 2);

            // Campo propio de la fuente.
            if (partes.Length == 1)
            {
                var propio = CatalogoReglasPersonalizadas.GetValor(regla.FuenteDatos, partes[0].Trim(), registro);
                if (propio is not null) metadata[EtiquetaCampo(regla.FuenteDatos, partes[0].Trim())] = propio;
                continue;
            }

            // Campo de una tabla relacionada: "Fuente.Campo".
            var destino = partes[0].Trim();
            var campoDestino = partes[1].Trim();
            var rel = context.Relaciones.FirstOrDefault(r =>
                r.Activa
                && string.Equals(r.FuenteOrigen, regla.FuenteDatos, StringComparison.OrdinalIgnoreCase)
                && string.Equals(r.FuenteDestino, destino, StringComparison.OrdinalIgnoreCase));
            if (rel is null) continue;

            var llave = Convert.ToString(
                CatalogoReglasPersonalizadas.GetValor(regla.FuenteDatos, rel.CampoOrigen, registro),
                CultureInfo.InvariantCulture)?.Trim();
            if (string.IsNullOrEmpty(llave)) continue;

            var relacionado = ObtenerFuente(context, destino).FirstOrDefault(d =>
                string.Equals(
                    Convert.ToString(CatalogoReglasPersonalizadas.GetValor(destino, rel.CampoDestino, d),
                        CultureInfo.InvariantCulture)?.Trim(),
                    llave, StringComparison.OrdinalIgnoreCase));
            if (relacionado is null) continue;

            var valorRel = CatalogoReglasPersonalizadas.GetValor(destino, campoDestino, relacionado);
            if (valorRel is not null) metadata[$"{EtiquetaCampo(destino, campoDestino)} ({destino})"] = valorRel;
        }
    }

    /// <summary>Etiqueta legible de un campo (del catálogo) o el nombre crudo si no está catalogado.</summary>
    private static string EtiquetaCampo(string fuente, string campo) =>
        CatalogoReglasPersonalizadas.BuscarCampo(fuente, campo)?.Etiqueta ?? campo;

    // ---- Descripción en lenguaje natural (para que un auditor entienda la alerta sin leer código) ----

    /// <summary>
    /// Arma la descripción de la alerta liderada por lo que el usuario escribió en la regla
    /// (campo <c>Descripcion</c>), seguida de la condición en lenguaje natural y, si aplica, el monto.
    /// Antes la alerta mostraba la condición en código ("Cantidad &gt;= '400'") sin la descripción.
    /// </summary>
    private static string DescripcionLegible(ReglaPersonalizada regla, string fraseLegible, double? monto)
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(regla.Descripcion))
            partes.Add(regla.Descripcion.Trim().TrimEnd('.'));
        partes.Add(fraseLegible);
        if (monto is not null)
            partes.Add($"monto ${monto.Value.ToString("N2", CultureInfo.InvariantCulture)}");
        return $"[{regla.Nombre}] " + string.Join(" · ", partes);
    }

    /// <summary>Condiciones del modo básico en palabras: "Despacho: Galones mayor o igual a 400".</summary>
    private static string FrasearCondiciones(string fuente, List<CondicionRegla> condiciones, string combinador)
    {
        if (condiciones.Count == 0) return $"{EtiquetaFuente(fuente)} (sin condiciones)";
        var conector = string.Equals(combinador, "O", StringComparison.OrdinalIgnoreCase) ? " o " : " y ";
        return $"{EtiquetaFuente(fuente)}: " +
               string.Join(conector, condiciones.Select(c => FrasearCondicion(fuente, c)));
    }

    private static string FrasearCondicion(string fuente, CondicionRegla c)
    {
        var etiqueta = EtiquetaCampo(fuente, c.Campo);
        var op = OperadorEnPalabras(c.Operador);
        return c.Operador is "vacio" or "noVacio" ? $"{etiqueta} {op}" : $"{etiqueta} {op} {c.Valor}";
    }

    /// <summary>Operador (símbolo o palabra clave del builder) traducido a una frase legible en español.</summary>
    private static string OperadorEnPalabras(string op) => op switch
    {
        ">" => "mayor que",
        ">=" => "mayor o igual a",
        "<" => "menor que",
        "<=" => "menor o igual a",
        "=" => "igual a",
        "!=" => "distinto de",
        "contiene" => "contiene",
        "noContiene" => "no contiene",
        "vacio" => "está vacío",
        "noVacio" => "tiene valor",
        _ => op
    };

    /// <summary>Nombre legible de la fuente de datos (para la descripción de la alerta).</summary>
    private static string EtiquetaFuente(string fuente) => fuente switch
    {
        "Factura" => "Factura",
        "DetalleFactura" => "Despacho (detalle de factura)",
        "CierreTurno" => "Cierre de turno",
        "Credito" => "Crédito",
        "TarjetaTurno" => "Tarjeta de turno",
        _ => fuente
    };

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
