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
    public string FirebirdDatabase { get; set; } = @"C:\Programas\ContaGober1\Datosc\CONTAB.FDB";
    public string FirebirdUser { get; set; } = "SYSDBA";
    public string FirebirdPassword { get; set; } = "masterkey";
    public string FirebirdCharset { get; set; } = "NONE";
    public int FirebirdDialect { get; set; } = 3;

    /// <summary>WireCrypt: "Disabled" para Firebird 2.5 (Legacy_Auth), "Enabled" para FB 3+.</summary>
    public string FirebirdWireCrypt { get; set; } = "Disabled";

    // ─── Operación ───
    // Por defecto envía cada 1 s (casi tiempo real); editable desde el panel del agente.
    public int IntervaloSegundos { get; set; } = 1;
    public bool InicioAutomatico { get; set; } = true;
    public string LocalStorePath { get; set; } = "pending";
    public int PanelPuerto { get; set; } = 5180;

    // ─── Actualización remota (control de versiones) ───
    /// <summary>
    /// URL del manifiesto de actualización. Si queda vacía, se usa el servidor
    /// central: {ServerUrl}/api/v1/agente/version. Puede apuntarse a un JSON en
    /// GitHub (raw) o cualquier host estático cuando no haya servidor central.
    /// </summary>
    public string UpdateFeedUrl { get; set; } = "";

    /// <summary>URL de respaldo del manifiesto (p. ej. GitHub) si la primaria falla.</summary>
    public string UpdateFeedFallbackUrl { get; set; } = "";

    /// <summary>Nombre del servicio de Windows, para reiniciarlo tras actualizar.</summary>
    public string NombreServicioWindows { get; set; } = "PetrolRios Station Agent";

    // ─── Seguridad del panel local ───
    /// <summary>
    /// Si es false (por defecto), el panel del agente está abierto (solo localhost):
    /// permite configurar y conectar sin fricción en el primer despliegue. Cuando un
    /// administrador lo activa, el panel exige iniciar sesión (RBAC contra el central
    /// o respaldo local) para administrar el agente.
    /// </summary>
    public bool RequiereLoginPanel { get; set; } = false;

    /// <summary>Usuario de la contraseña local de respaldo (para acceso offline al panel).</summary>
    public string PanelLocalUsuario { get; set; } = "admin-local";

    /// <summary>Hash PBKDF2 de la contraseña local de respaldo. Vacío = sin respaldo local.</summary>
    public string PanelLocalPasswordHash { get; set; } = "";

    /// <summary>Resuelve la URL efectiva del feed (primaria o derivada del servidor central).</summary>
    public string ResolverUpdateFeedUrl() =>
        string.IsNullOrWhiteSpace(UpdateFeedUrl)
            ? $"{ServerUrl.TrimEnd('/')}/api/v1/agente/version"
            : UpdateFeedUrl.Trim();

    /// <summary>true cuando el usuario completó la configuración inicial desde la interfaz.</summary>
    public bool Configurado { get; set; }

    // ─── Fuentes de extracción configurables (multi-tabla) ───
    /// <summary>
    /// Tablas adicionales que el agente extrae y envía al central, configuradas desde el panel
    /// sin recompilar. Cada fuente se manda como una transacción de staging con su nombre como
    /// tipo, y queda disponible para el creador de reglas genérico.
    /// </summary>
    public List<FuenteExtraccion> FuentesExtraccion { get; set; } = [];

    /// <summary>Construye el connection string de Firebird (siempre solo lectura).</summary>
    public string ConstruirFirebirdConnectionString() =>
        $"User={FirebirdUser};Password={FirebirdPassword};Database={FirebirdDatabase};" +
        $"DataSource={FirebirdHost};Port={FirebirdPort};Dialect={FirebirdDialect};" +
        $"Charset={FirebirdCharset};WireCrypt={FirebirdWireCrypt};ReadOnly=true";

    public AgentSettings Clonar()
    {
        var copia = (AgentSettings)MemberwiseClone();
        // Copia profunda de la lista para que editar no mute la configuración activa.
        copia.FuentesExtraccion = FuentesExtraccion
            .Select(f => new FuenteExtraccion
            {
                Id = f.Id,
                Nombre = f.Nombre, Tabla = f.Tabla,
                ColumnaWatermark = f.ColumnaWatermark, Activa = f.Activa,
                Version = f.Version
            })
            .ToList();
        return copia;
    }
}

/// <summary>
/// Una fuente de extracción configurable: una tabla de Firebird que el agente lee y envía al
/// central, identificada por un nombre lógico. Si <see cref="ColumnaWatermark"/> está definida
/// (una columna de fecha), solo se extraen las filas posteriores a la marca de agua.
/// </summary>
public sealed class FuenteExtraccion
{
    /// <summary>Id del catálogo central. Cero para fuentes locales heredadas.</summary>
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Tabla { get; set; } = "";
    public string? ColumnaWatermark { get; set; }
    public bool Activa { get; set; } = true;
    public DateTime Version { get; set; }
}
