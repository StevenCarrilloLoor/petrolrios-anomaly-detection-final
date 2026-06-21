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

    /// <summary>
    /// Carril al que pertenece la regla: "Operativa" (problema de estación → administrador de
    /// la estación) o "Auditoria" (posible irregularidad → central). Es editable desde el panel y los
    /// detectores lo respetan al generar las alertas.
    /// </summary>
    public string Ambito { get; init; } = "Auditoria";
}

public sealed record CrearReglaRequest(
    string TipoDetector,
    string Nombre,
    string Descripcion,
    string ParametroNombre,
    double ValorUmbral);

public sealed record ActualizarReglaRequest(
    double? ValorUmbral,
    bool? Activa,
    string? Ambito = null);
