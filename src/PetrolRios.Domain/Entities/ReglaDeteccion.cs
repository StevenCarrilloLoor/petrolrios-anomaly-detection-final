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
