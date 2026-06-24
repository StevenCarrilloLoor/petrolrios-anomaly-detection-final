namespace PetrolRios.Domain.Entities;

/// <summary>
/// Catálogo de empleados/despachadores por estación, sincronizado por el agente desde Firebird
/// (VEND, con respaldo a EMPL). Permite resolver el código de vendedor (COD_VEND) a su nombre para
/// mostrarlo junto al código en las alertas y reportes, y así actuar de inmediato. El código sigue
/// siendo la llave inmutable que guarda la alerta; el nombre es solo la resolución legible.
/// </summary>
public class Empleado : BaseEntity
{
    public int EstacionId { get; private set; }
    public Estacion Estacion { get; private set; } = null!;

    /// <summary>Código del vendedor/despachador (COD_VEND), normalizado en mayúsculas y sin espacios.</summary>
    public string Codigo { get; private set; } = string.Empty;

    /// <summary>Nombre del empleado (VEND.NOM_VEND, con respaldo a EMPL.NOM_EMPL).</summary>
    public string Nombre { get; private set; } = string.Empty;

    public static Empleado Create(int estacionId, string codigo, string nombre) =>
        new()
        {
            EstacionId = estacionId,
            Codigo = Normalizar(codigo),
            Nombre = (nombre ?? string.Empty).Trim()
        };

    public void Actualizar(string nombre) => Nombre = (nombre ?? string.Empty).Trim();

    /// <summary>
    /// Normaliza el código para que el cruce con el COD_VEND de las alertas sea estable: en
    /// Firebird viene como Char(9)/Char(3) relleno con espacios, así que se hace TRIM + mayúsculas.
    /// </summary>
    public static string Normalizar(string? codigo) =>
        (codigo ?? string.Empty).Trim().ToUpperInvariant();
}
