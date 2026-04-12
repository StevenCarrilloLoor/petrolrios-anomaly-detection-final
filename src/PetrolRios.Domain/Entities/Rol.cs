namespace PetrolRios.Domain.Entities;

public class Rol : BaseEntity
{
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }

    public ICollection<Usuario> Usuarios { get; private set; } = [];

    public static Rol Create(string nombre, string? descripcion = null) =>
        new() { Nombre = nombre, Descripcion = descripcion };
}
