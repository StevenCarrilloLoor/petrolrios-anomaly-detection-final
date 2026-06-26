using PetrolRios.Domain.Enums;

namespace PetrolRios.Domain.Entities;

public class ReglaDeteccion : BaseEntity
{
    public TipoDetector TipoDetector { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string ParametroNombre { get; private set; } = string.Empty;
    public double ValorUmbral { get; set; }
    public bool Activa { get; set; } = true;

    /// <summary>
    /// Carril de las alertas que genera esta regla: <see cref="AmbitoAlerta.Operativa"/>
    /// (problema de estación → avisa al administrador) o <see cref="AmbitoAlerta.Auditoria"/>
    /// (fraude → central). Editable desde el panel de Reglas.
    /// </summary>
    public AmbitoAlerta Ambito { get; set; } = AmbitoAlerta.Auditoria;

    /// <summary>
    /// Si es true, además de aparecer en el panel, esta regla envía un correo a supervisores y
    /// administradores cuando se dispara (no solo las críticas). Opt-in; por defecto false.
    /// </summary>
    public bool NotificarCorreo { get; set; }

    /// <summary>
    /// Programación de ejecución (JSON de <c>ProgramacionEjecucion</c>). Vacío = "cada ciclo" (igual que
    /// antes). Permite que la regla corra en su propia cadencia: intervalo (cada N seg/min/h/d/sem/mes) o
    /// calendario anclado (día del mes, "último día", día de la semana, hora).
    /// </summary>
    public string ProgramacionJson { get; set; } = "";

    /// <summary>Próxima ejecución programada (UTC). null = correr en el próximo ciclo / aún sin calcular.</summary>
    public DateTime? ProximaEjecucion { get; set; }

    /// <summary>Última vez que la regla corrió (UTC). Informativo / base del intervalo.</summary>
    public DateTime? UltimaEjecucion { get; set; }

    public static ReglaDeteccion Create(
        TipoDetector tipoDetector,
        string nombre,
        string descripcion,
        string parametroNombre,
        double valorUmbral,
        AmbitoAlerta ambito = AmbitoAlerta.Auditoria) =>
        new()
        {
            TipoDetector = tipoDetector,
            Nombre = nombre,
            Descripcion = descripcion,
            ParametroNombre = parametroNombre,
            ValorUmbral = valorUmbral,
            Ambito = ambito
        };
}
