namespace PetrolRios.Application.DTOs.Monitoreo;

/// <summary>Estado de conexión de una estación (agente) con el servidor central.</summary>
public sealed record ConexionEstacionResponse
{
    public int EstacionId { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Zona { get; init; } = string.Empty;

    /// <summary>En línea = el agente envió un heartbeat recientemente (aunque no haya datos nuevos).</summary>
    public bool Conectada { get; init; }

    /// <summary>"En línea", "Sin conexión" o "Nunca conectada".</summary>
    public string Estado { get; init; } = string.Empty;

    /// <summary>La estación está activa en el sistema (no fue dada de baja).</summary>
    public bool Activa { get; init; }

    /// <summary>Último heartbeat del agente (señal de vida).</summary>
    public DateTime? UltimoHeartbeat { get; init; }

    /// <summary>Minutos desde el último heartbeat.</summary>
    public double? MinutosDesdeUltimoHeartbeat { get; init; }

    /// <summary>Versión del agente reportada.</summary>
    public string? VersionAgente { get; init; }

    /// <summary>Fecha/hora de la última ingesta de DATOS recibida desde el agente.</summary>
    public DateTime? UltimaIngesta { get; init; }

    /// <summary>Minutos transcurridos desde la última ingesta.</summary>
    public double? MinutosDesdeUltimaIngesta { get; init; }

    /// <summary>Transacciones recibidas en las últimas 24 horas.</summary>
    public int TransaccionesUltimas24Horas { get; init; }

    /// <summary>Total de transacciones recibidas históricamente.</summary>
    public int TransaccionesTotales { get; init; }

    /// <summary>Transacciones en staging pendientes de análisis.</summary>
    public int PendientesAnalisis { get; init; }
}

/// <summary>Estado general del sistema para el panel de monitoreo.</summary>
public sealed record EstadoSistemaResponse
{
    // API
    public string VersionApi { get; init; } = string.Empty;
    public DateTime InicioApi { get; init; }
    public double UptimeSegundos { get; init; }
    public string Entorno { get; init; } = string.Empty;

    // Base de datos
    public bool BaseDatosConectada { get; init; }
    public double? LatenciaBaseDatosMs { get; init; }

    // SignalR
    public int ClientesSignalRConectados { get; init; }

    // Estaciones
    public int EstacionesConectadas { get; init; }
    public int EstacionesTotales { get; init; }

    // Hangfire / motor de detección
    public DateTime? UltimoCicloDeteccion { get; init; }
    public string? UltimoCicloEstado { get; init; }
    public int? UltimoCicloAlertas { get; init; }
    public double? UltimoCicloDuracionSegundos { get; init; }
    public double? MinutosDesdeUltimoCiclo { get; init; }
}
