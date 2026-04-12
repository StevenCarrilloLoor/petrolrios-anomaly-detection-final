namespace PetrolRios.Domain.Entities;

public class EstacionWatermark : BaseEntity
{
    public int EstacionId { get; private set; }
    public Estacion Estacion { get; private set; } = null!;
    public DateTime UltimaExtraccion { get; set; }

    public static EstacionWatermark Create(int estacionId, DateTime ultimaExtraccion) =>
        new() { EstacionId = estacionId, UltimaExtraccion = ultimaExtraccion };
}
