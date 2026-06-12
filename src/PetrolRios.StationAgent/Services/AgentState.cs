using System.Collections.Concurrent;

namespace PetrolRios.StationAgent.Services;

/// <summary>
/// Estado observable del agente para el panel de control local.
/// Thread-safe: lo escriben el Worker/CycleRunner y lo lee el panel web.
/// </summary>
public sealed class AgentState
{
    private readonly ConcurrentQueue<EventoAgente> _eventos = new();
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

    /// <summary>Indica si hay un ciclo corriendo (evita ejecuciones simultáneas).</summary>
    public readonly SemaphoreSlim CandadoCiclo = new(1, 1);

    public void RegistrarEvento(string nivel, string mensaje)
    {
        _eventos.Enqueue(new EventoAgente(DateTime.UtcNow, nivel, mensaje));
        while (_eventos.Count > MaxEventos && _eventos.TryDequeue(out _)) { }
    }

    public IReadOnlyList<EventoAgente> Eventos => _eventos.Reverse().ToList();
}

public sealed record EventoAgente(DateTime Fecha, string Nivel, string Mensaje);
