namespace PetrolRios.Application.DTOs.Reglas;

public sealed record ReglaDeteccionResponse
{
    public int Id { get; init; }
    public string TipoDetector { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string ParametroNombre { get; init; } = string.Empty;
    public double ValorUmbral { get; init; }
    public bool Activa { get; init; }
}

public sealed record CrearReglaRequest(
    string TipoDetector,
    string Nombre,
    string Descripcion,
    string ParametroNombre,
    double ValorUmbral);

public sealed record ActualizarReglaRequest(
    double? ValorUmbral,
    bool? Activa);
