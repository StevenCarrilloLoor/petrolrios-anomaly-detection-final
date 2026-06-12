namespace PetrolRios.StationAgent.Configuration;

/// <summary>
/// Configuración del agente de estación.
/// </summary>
public sealed class AgentOptions
{
    public const string SectionName = "Agent";

    /// <summary>Código de la estación (ej. "EST-001").</summary>
    public required string CodigoEstacion { get; set; }

    /// <summary>URL base del servidor central (ej. "https://api.petrolrios.com").</summary>
    public required string ServerUrl { get; set; }

    /// <summary>Intervalo entre ciclos de extracción en segundos (default: 300 = 5 min).</summary>
    public int IntervaloSegundos { get; set; } = 300;

    /// <summary>Connection string de la base Firebird local (CONTAC.FDB).</summary>
    public required string FirebirdConnectionString { get; set; }

    /// <summary>Credenciales para autenticarse contra el servidor central.</summary>
    public required string Email { get; set; }
    public required string Password { get; set; }

    /// <summary>Ruta local para store-and-forward cuando no hay conectividad.</summary>
    public string LocalStorePath { get; set; } = "pending";

    /// <summary>Puerto del panel de control local del agente (default: 5180).</summary>
    public int PanelPuerto { get; set; } = 5180;

    /// <summary>Si es false, el agente arranca en modo manual (no sincroniza solo).</summary>
    public bool InicioAutomatico { get; set; } = true;
}
