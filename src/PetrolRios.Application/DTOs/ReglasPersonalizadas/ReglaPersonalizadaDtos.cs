using PetrolRios.Application.ReglasPersonalizadas;

namespace PetrolRios.Application.DTOs.ReglasPersonalizadas;

public sealed record ReglaPersonalizadaResponse
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string FuenteDatos { get; init; } = string.Empty;
    public IReadOnlyList<CondicionRegla> Condiciones { get; init; } = [];
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
