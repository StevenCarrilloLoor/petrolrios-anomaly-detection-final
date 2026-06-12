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
    public double RiesgoBase { get; init; }
    public bool Activa { get; init; }
}

public sealed record GuardarReglaPersonalizadaRequest
{
    public required string Nombre { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public required string FuenteDatos { get; init; }
    public List<CondicionRegla> Condiciones { get; init; } = [];
    public AgregacionRegla? Agregacion { get; init; }
    public double RiesgoBase { get; init; } = 50;
    public bool Activa { get; init; } = true;
}

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
