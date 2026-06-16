namespace PetrolRios.StationAgent.Configuration;

/// <summary>
/// Configuración completa y editable del agente. Se persiste en disco
/// (config/agent-config.json) para que en cada estación se ajuste desde la
/// interfaz sin recompilar ni editar archivos a mano.
/// </summary>
public sealed class AgentSettings
{
    // ─── Identidad de la estación ───
    public string CodigoEstacion { get; set; } = "EST-001";

    /// <summary>Nombre legible de la estación; se refleja en el panel central de Conexiones.</summary>
    public string NombreEstacion { get; set; } = "";

    public string ZonaEstacion { get; set; } = "";

    // ─── Servidor central ───
    public string ServerUrl { get; set; } = "http://localhost:5170";
    public string Email { get; set; } = "agent-est-001@petrolrios.com";
    public string Password { get; set; } = "Agent123!";

    /// <summary>Timeout de las peticiones al servidor central, en segundos.</summary>
    public int ServerTimeoutSegundos { get; set; } = 30;

    // ─── Base Firebird local (campos estructurados, editables) ───
    public string FirebirdHost { get; set; } = "localhost";
    public int FirebirdPort { get; set; } = 3050;
    public string FirebirdDatabase { get; set; } = @"C:\CONTAC\CONTAC.FDB";
    public string FirebirdUser { get; set; } = "SYSDBA";
    public string FirebirdPassword { get; set; } = "masterkey";
    public string FirebirdCharset { get; set; } = "NONE";
    public int FirebirdDialect { get; set; } = 3;

    /// <summary>WireCrypt: "Disabled" para Firebird 2.5 (Legacy_Auth), "Enabled" para FB 3+.</summary>
    public string FirebirdWireCrypt { get; set; } = "Disabled";

    // ─── Operación ───
    public int IntervaloSegundos { get; set; } = 30;
    public bool InicioAutomatico { get; set; } = true;
    public string LocalStorePath { get; set; } = "pending";
    public int PanelPuerto { get; set; } = 5180;

    /// <summary>true cuando el usuario completó la configuración inicial desde la interfaz.</summary>
    public bool Configurado { get; set; }

    /// <summary>Construye el connection string de Firebird (siempre solo lectura).</summary>
    public string ConstruirFirebirdConnectionString() =>
        $"User={FirebirdUser};Password={FirebirdPassword};Database={FirebirdDatabase};" +
        $"DataSource={FirebirdHost};Port={FirebirdPort};Dialect={FirebirdDialect};" +
        $"Charset={FirebirdCharset};WireCrypt={FirebirdWireCrypt};ReadOnly=true";

    public AgentSettings Clonar() => (AgentSettings)MemberwiseClone();
}
