using PetrolRios.Domain.Enums;

namespace PetrolRios.Domain.Entities;

public class Alerta : BaseEntity
{
    public TipoDetector TipoDetector { get; private set; }
    public NivelRiesgo NivelRiesgo { get; private set; }
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
        int? ejecucionJobId = null) =>
        new()
        {
            TipoDetector = tipoDetector,
            NivelRiesgo = nivelRiesgo,
            Descripcion = descripcion,
            Score = score,
            EstacionId = estacionId,
            EmpleadoCodigo = empleadoCodigo,
            TransaccionReferencia = transaccionReferencia,
            MetadataJson = metadataJson,
            EjecucionJobId = ejecucionJobId
        };
}
