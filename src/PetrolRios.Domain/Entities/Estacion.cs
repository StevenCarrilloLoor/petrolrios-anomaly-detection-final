namespace PetrolRios.Domain.Entities;

public class Estacion : BaseEntity
{
    public string Nombre { get; private set; } = string.Empty;
    public string Codigo { get; private set; } = string.Empty;
    public string Direccion { get; private set; } = string.Empty;
    public string? Zona { get; private set; }
    public bool Activa { get; set; } = true;
    public TimeOnly HoraApertura { get; set; } = new(6, 0);
    public TimeOnly HoraCierre { get; set; } = new(22, 0);

    public EstacionWatermark? Watermark { get; private set; }
    public ICollection<Alerta> Alertas { get; private set; } = [];

    public static Estacion Create(string nombre, string codigo, string direccion, string? zona = null) =>
        new() { Nombre = nombre, Codigo = codigo, Direccion = direccion, Zona = zona };
}
