using System.Collections.Concurrent;
using PetrolRios.StationAgent.Configuration;

namespace PetrolRios.StationAgent.Services;

/// <summary>
/// Estado observable del agente para el panel de control local.
/// Thread-safe: lo escriben el Worker/CycleRunner y lo lee el panel web.
/// </summary>
public sealed class AgentState
{
    private readonly ConcurrentQueue<EventoAgente> _eventos = new();
    private readonly object _fuentesLock = new();
    private IReadOnlyList<FuenteCentralEstadoPanel> _fuentesCentrales = [];
    private const int MaxEventos = 60;

    /// <summary>Si es false, el agente NO sincroniza automáticamente (modo manual).</summary>
    public volatile bool ModoAutomatico = true;

    public DateTime InicioAgente { get; } = DateTime.UtcNow;

    public DateTime? UltimoCiclo { get; set; }
    public string UltimoResultado { get; set; } = "Sin ciclos todavía";
    public bool UltimoCicloExitoso { get; set; } = true;
    public int UltimoLoteTransacciones { get; set; }
    public long TotalTransaccionesEnviadas { get; set; }
    public int CiclosEjecutados { get; set; }
    public DateTime? UltimaConexionServidor { get; set; }
    public DateTime? UltimaDesconexionServidor { get; set; }
    public double? UltimaLatenciaServidorMs { get; set; }

    // ─── Actualización remota (control de versiones) ───
    /// <summary>true si el feed de actualización ofrece una versión mayor a la instalada.</summary>
    public bool ActualizacionDisponible { get; set; }
    public string? VersionDisponible { get; set; }
    public string? NotasActualizacion { get; set; }
    public string? UrlActualizacion { get; set; }
    public string? Sha256Actualizacion { get; set; }
    public bool ActualizacionObligatoria { get; set; }
    /// <summary>true mientras se descarga/aplica una actualización (evita doble clic).</summary>
    public volatile bool AplicandoActualizacion;

    /// <summary>Indica si hay un ciclo corriendo (evita ejecuciones simultáneas).</summary>
    public readonly SemaphoreSlim CandadoCiclo = new(1, 1);

    public void RegistrarEvento(string nivel, string mensaje)
    {
        _eventos.Enqueue(new EventoAgente(DateTime.UtcNow, nivel, mensaje));
        while (_eventos.Count > MaxEventos && _eventos.TryDequeue(out _)) { }
    }

    public IReadOnlyList<EventoAgente> Eventos => _eventos.Reverse().ToList();

    public IReadOnlyList<FuenteCentralEstadoPanel> FuentesCentrales
    {
        get
        {
            lock (_fuentesLock)
                return _fuentesCentrales.ToList();
        }
    }

    public void ActualizarFuentesCentrales(
        IReadOnlyList<FuenteExtraccion> fuentes,
        IReadOnlyList<EstadoFuenteAgente>? estados = null)
    {
        var porId = (estados ?? [])
            .Where(e => e.FuenteDatosId > 0)
            .ToDictionary(e => e.FuenteDatosId);

        lock (_fuentesLock)
        {
            _fuentesCentrales = fuentes.Select(f =>
            {
                porId.TryGetValue(f.Id, out var estado);
                return new FuenteCentralEstadoPanel(
                    f.Id,
                    f.Nombre,
                    f.Tabla,
                    f.ColumnaWatermark,
                    f.Version,
                    estado?.Estado ?? "Recibida",
                    estado?.TablaExiste,
                    estado?.ColumnaWatermarkValida,
                    estado?.FilasLeidas ?? 0,
                    estado?.FilasEnviadas ?? 0,
                    estado?.UltimoError,
                    DateTime.UtcNow);
            }).ToList();
        }
    }
}

public sealed record EventoAgente(DateTime Fecha, string Nivel, string Mensaje);

public sealed record FuenteCentralEstadoPanel(
    int Id,
    string Nombre,
    string Tabla,
    string? ColumnaWatermark,
    DateTime Version,
    string Estado,
    bool? TablaExiste,
    bool? ColumnaWatermarkValida,
    int FilasLeidas,
    int FilasEnviadas,
    string? UltimoError,
    DateTime Actualizado);
