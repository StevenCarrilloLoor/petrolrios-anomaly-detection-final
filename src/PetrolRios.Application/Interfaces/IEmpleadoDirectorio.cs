namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Resuelve el código de empleado (COD_VEND) a su nombre para mostrarlo junto al código en alertas,
/// dashboard y reportes — y así poder actuar de inmediato. El catálogo lo sincroniza el agente desde
/// Firebird (tabla central <c>Empleado</c>, por estación).
/// </summary>
public interface IEmpleadoDirectorio
{
    /// <summary>
    /// Carga los nombres de las claves (estación, código) indicadas y devuelve un resolvedor en
    /// memoria. Si una clave no está en el catálogo, su nombre es <c>null</c> (se mostrará solo el código).
    /// </summary>
    Task<DirectorioEmpleados> CargarAsync(
        IEnumerable<(int EstacionId, string? Codigo)> claves, CancellationToken ct = default);
}

/// <summary>Resolvedor en memoria de (estación, código) → nombre de empleado.</summary>
public sealed class DirectorioEmpleados
{
    public static readonly DirectorioEmpleados Vacio =
        new(new Dictionary<(int, string), string>());

    private readonly IReadOnlyDictionary<(int EstacionId, string Codigo), string> _nombres;

    public DirectorioEmpleados(IReadOnlyDictionary<(int EstacionId, string Codigo), string> nombres) =>
        _nombres = nombres;

    /// <summary>Nombre del empleado, o <c>null</c> si no está en el catálogo (se muestra solo el código).</summary>
    public string? Nombre(int estacionId, string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return null;
        return _nombres.GetValueOrDefault((estacionId, codigo.Trim().ToUpperInvariant()));
    }
}
