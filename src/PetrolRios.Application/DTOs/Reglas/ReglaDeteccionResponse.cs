namespace PetrolRios.Application.DTOs.Reglas;

public sealed record ReglaDeteccionResponse
{
    public int Id { get; init; }
    public string TipoDetector { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string ParametroNombre { get; init; } = string.Empty;
    public double ValorUmbral { get; init; }

    /// <summary>
    /// Unidad del umbral para que el usuario sepa qué está editando: "horas", "minutos", "días",
    /// "%", "USD ($)", "galones", "veces" o "1 = activado / 0 = desactivado". Derivada del parámetro.
    /// </summary>
    public string Unidad { get; init; } = "";

    /// <summary>Explicación corta de qué representa el umbral (para el tooltip del editor).</summary>
    public string AyudaUmbral { get; init; } = "";

    public bool Activa { get; init; }

    /// <summary>
    /// Carril al que pertenece la regla: "Operativa" (problema de estación → administrador de
    /// la estación) o "Auditoria" (posible irregularidad → central). Es editable desde el panel y los
    /// detectores lo respetan al generar las alertas.
    /// </summary>
    public string Ambito { get; init; } = "Auditoria";

    /// <summary>Si true, esta regla envía correo a supervisores/administradores cuando se dispara.</summary>
    public bool NotificarCorreo { get; init; }
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
    string? Ambito = null,
    bool? NotificarCorreo = null);
