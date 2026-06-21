using PetrolRios.Application.ReglasPersonalizadas;

namespace PetrolRios.Application.DTOs.ReglasPersonalizadas;

public sealed record ReglaPersonalizadaResponse
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string FuenteDatos { get; init; } = string.Empty;
    public IReadOnlyList<CondicionRegla> Condiciones { get; init; } = [];

    /// <summary>Combinador lógico de las condiciones: "Y" (todas) u "O" (cualquiera).</summary>
    public string CombinadorCondiciones { get; init; } = "Y";

    public AgregacionRegla? Agregacion { get; init; }

    /// <summary>Expresión del modo avanzado (null en modo básico).</summary>
    public string? ExpresionAvanzada { get; init; }

    public double RiesgoBase { get; init; }

    /// <summary>Carril de las alertas que genera la regla: "Operativa" o "Auditoria".</summary>
    public string Ambito { get; init; } = "Auditoria";

    public bool Activa { get; init; }
}

public sealed record GuardarReglaPersonalizadaRequest
{
    public required string Nombre { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public required string FuenteDatos { get; init; }
    public List<CondicionRegla> Condiciones { get; init; } = [];

    /// <summary>Combinador lógico de las condiciones: "Y" (todas, por defecto) u "O" (cualquiera).</summary>
    public string CombinadorCondiciones { get; init; } = "Y";

    public AgregacionRegla? Agregacion { get; init; }

    /// <summary>Si viene no vacía, la regla es de modo avanzado (expresión lógica).</summary>
    public string? ExpresionAvanzada { get; init; }

    public double RiesgoBase { get; init; } = 50;

    /// <summary>"Operativa" (problema de estación) o "Auditoria" (posible irregularidad). Por defecto Auditoría.</summary>
    public string Ambito { get; init; } = "Auditoria";

    public bool Activa { get; init; } = true;
}

/// <summary>Solicitud para validar/probar una expresión avanzada sin guardarla.</summary>
public sealed record ValidarExpresionRequest(string FuenteDatos, string Expresion);

public sealed record ValidarExpresionResponse(bool Valida, IReadOnlyList<string> Errores);

/// <summary>Catálogo para el builder: fuentes, campos, operadores y funciones disponibles.</summary>
public sealed record CatalogoReglasResponse
{
    public IReadOnlyList<FuenteCatalogo> Fuentes { get; init; } = [];
    public IReadOnlyList<string> OperadoresNumero { get; init; } = [];
    public IReadOnlyList<string> OperadoresTexto { get; init; } = [];
    public IReadOnlyList<string> Funciones { get; init; } = [];
}

public sealed record FuenteCatalogo(
    string Nombre,
    string Etiqueta,
    IReadOnlyList<CampoCatalogo> Campos);

public sealed record CampoCatalogo(string Nombre, string Etiqueta, string Tipo);

/// <summary>
/// Solicitud de backtest (vista previa): prueba una regla <b>borrador</b> contra los datos reales
/// de los últimos <see cref="Dias"/> días, sin guardarla, para ver cuántas alertas habría generado.
/// </summary>
public sealed record BacktestReglaRequest
{
    public required GuardarReglaPersonalizadaRequest Regla { get; init; }

    /// <summary>Ventana de evaluación en días (1–90). Por defecto 7.</summary>
    public int Dias { get; init; } = 7;
}

/// <summary>
/// Resultado del backtest: cuántas coincidencias habría generado la regla, su desglose por nivel
/// de riesgo y una muestra. Si la regla borrador es inválida, <see cref="Valida"/> es false y
/// <see cref="Errores"/> explica por qué (no se ejecuta nada).
/// </summary>
public sealed record BacktestReglaResponse
{
    public bool Valida { get; init; }
    public IReadOnlyList<string> Errores { get; init; } = [];

    public int VentanaDias { get; init; }

    /// <summary>Cantidad de registros de la fuente evaluados en la ventana.</summary>
    public int RegistrosEvaluados { get; init; }

    public int TotalCoincidencias { get; init; }

    // Desglose por nivel de riesgo de las coincidencias.
    public int Bajo { get; init; }
    public int Medio { get; init; }
    public int Alto { get; init; }
    public int Critico { get; init; }

    /// <summary>Muestra de coincidencias (las de mayor score) para la vista previa.</summary>
    public IReadOnlyList<BacktestCoincidencia> Muestra { get; init; } = [];
}

/// <summary>Una coincidencia de muestra del backtest.</summary>
public sealed record BacktestCoincidencia(
    string Nivel, double Score, string Descripcion, string? Empleado, string? Estacion);
