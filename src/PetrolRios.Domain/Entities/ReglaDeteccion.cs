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

    public static ReglaDeteccion Create(
        TipoDetector tipoDetector,
        string nombre,
        string descripcion,
        string parametroNombre,
        double valorUmbral) =>
        new()
        {
            TipoDetector = tipoDetector,
            Nombre = nombre,
            Descripcion = descripcion,
            ParametroNombre = parametroNombre,
            ValorUmbral = valorUmbral
        };
}
