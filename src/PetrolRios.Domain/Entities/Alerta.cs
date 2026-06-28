using PetrolRios.Domain.Enums;

namespace PetrolRios.Domain.Entities;

public class Alerta : BaseEntity
{
    public TipoDetector TipoDetector { get; private set; }
    public NivelRiesgo NivelRiesgo { get; private set; }

    /// <summary>Carril de la alerta: operativa (estación) o de auditoría (fraude grave).</summary>
    public AmbitoAlerta Ambito { get; private set; } = AmbitoAlerta.Auditoria;

    public EstadoAlerta Estado { get; set; } = EstadoAlerta.Nueva;
    public string Descripcion { get; private set; } = string.Empty;
    public double Score { get; private set; }
    public DateTime FechaDeteccion { get; private set; } = DateTime.UtcNow;
    public string? EmpleadoCodigo { get; private set; }
    public string? TransaccionReferencia { get; private set; }
    public string? MetadataJson { get; private set; }

    public int EstacionId { get; private set; }
    public Estacion Estacion { get; private set; } = null!;

    /// <summary>Fecha en que la alerta fue resuelta (confirmada, falso positivo o cerrada).</summary>
    public DateTime? FechaResolucion { get; set; }

    /// <summary>
    /// Cuántos eventos/ocurrencias del MISMO caso se han acumulado en esta alerta. Para alertas
    /// acumulables (p. ej. despachos rápidos del mismo RUC/placa): empieza en el conteo inicial y crece
    /// cada vez que el patrón reincide, escalando el nivel (ver <see cref="EscalarPorConteo"/>).
    /// </summary>
    public int EventosAcumulados { get; private set; } = 1;

    /// <summary>
    /// Última vez que la alerta se creó o se actualizó (acumuló). La bandeja ordena por esta fecha para
    /// que las alertas que crecen "suban arriba" como si fueran nuevas.
    /// </summary>
    public DateTime FechaActualizacion { get; private set; } = DateTime.UtcNow;

    public int? EjecucionJobId { get; private set; }
    public EjecucionJob? EjecucionJob { get; private set; }

    public ICollection<AsignacionAlerta> Asignaciones { get; private set; } = [];
    public ICollection<ComentarioAlerta> Comentarios { get; private set; } = [];

    public static Alerta Create(
        TipoDetector tipoDetector,
        NivelRiesgo nivelRiesgo,
        string descripcion,
        double score,
        int estacionId,
        string? empleadoCodigo = null,
        string? transaccionReferencia = null,
        string? metadataJson = null,
        int? ejecucionJobId = null,
        AmbitoAlerta ambito = AmbitoAlerta.Auditoria,
        int eventosAcumulados = 1)
    {
        var ahora = DateTime.UtcNow;
        return new()
        {
            TipoDetector = tipoDetector,
            NivelRiesgo = nivelRiesgo,
            Ambito = ambito,
            Descripcion = descripcion,
            Score = score,
            EstacionId = estacionId,
            EmpleadoCodigo = empleadoCodigo,
            TransaccionReferencia = transaccionReferencia,
            MetadataJson = metadataJson,
            EjecucionJobId = ejecucionJobId,
            EventosAcumulados = eventosAcumulados < 1 ? 1 : eventosAcumulados,
            FechaDeteccion = ahora,
            FechaActualizacion = ahora
        };
    }

    /// <summary>
    /// Acumula nuevas ocurrencias del MISMO caso en esta alerta (en vez de crear una alerta nueva):
    /// suma al conteo, re-escala el nivel/score por el conteo total, actualiza la descripción y la
    /// evidencia, sube la alerta arriba (FechaActualizacion) y la re-marca como Nueva para que el auditor
    /// la vuelva a revisar. No reabre alertas ya resueltas (esa decisión se respeta — el caller no debe
    /// llamar Acumular sobre resueltas).
    /// </summary>
    public void Acumular(int eventosNuevos, double score, NivelRiesgo nivel, string descripcion,
        string? metadataJson, DateTime cuando)
    {
        EventosAcumulados += eventosNuevos < 0 ? 0 : eventosNuevos;
        Score = score;
        NivelRiesgo = nivel;
        Descripcion = descripcion;
        MetadataJson = metadataJson;
        FechaActualizacion = cuando;
        Estado = EstadoAlerta.Nueva;   // re-emerge para revisión
        FechaResolucion = null;
    }

    /// <summary>
    /// Escala el nivel y el score de una alerta acumulable según cuántos eventos del mismo caso se han
    /// acumulado. Por defecto (auditoría sugiere): 2–3 = Medio, 4–5 = Alto, 6+ = Crítico. El score crece
    /// dentro de cada banda para que, a igual nivel, más reincidencia ordene más arriba por riesgo.
    /// </summary>
    public static (double Score, NivelRiesgo Nivel) EscalarPorConteo(int conteo)
    {
        if (conteo >= 6) return (Math.Min(78 + (conteo - 6) * 3, 99), NivelRiesgo.Critico);
        if (conteo >= 4) return (53 + (conteo - 4) * 8, NivelRiesgo.Alto);
        if (conteo >= 2) return (30 + (conteo - 2) * 8, NivelRiesgo.Medio);
        return (20, NivelRiesgo.Bajo);
    }
}
