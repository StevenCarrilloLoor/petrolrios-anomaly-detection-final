namespace PetrolRios.Application.Security;

/// <summary>Claims propios compartidos por la API, la infraestructura y los clientes.</summary>
public static class PetrolRiosClaimTypes
{
    /// <summary>
    /// Id de la estación asignada al usuario. Si existe, la cuenta queda limitada a los
    /// problemas operativos y operaciones técnicas de esa estación.
    /// </summary>
    public const string EstacionId = "estacion_id";
}
