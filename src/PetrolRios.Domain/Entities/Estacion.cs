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

    /// <summary>
    /// Correo del administrador/contacto de la estación. Si está definido, recibe el aviso de
    /// los problemas operativos (carril Operativa) de su estación. Null = sin aviso por correo.
    /// </summary>
    public string? CorreoContacto { get; set; }

    /// <summary>Último heartbeat recibido del Station Agent (aunque no haya datos nuevos).</summary>
    public DateTime? UltimoHeartbeat { get; set; }

    /// <summary>Versión del agente reportada en el último heartbeat.</summary>
    public string? VersionAgente { get; set; }

    public EstacionWatermark? Watermark { get; private set; }
    public ICollection<Alerta> Alertas { get; private set; } = [];

    public static Estacion Create(string nombre, string codigo, string direccion, string? zona = null) =>
        new() { Nombre = nombre, Codigo = codigo, Direccion = direccion, Zona = zona };

    /// <summary>Auto-registro: estación creada al recibir el primer contacto de un agente.</summary>
    public static Estacion CreateDesdeAgente(string codigo) =>
        new()
        {
            Nombre = $"Estación {codigo}",
            Codigo = codigo,
            Direccion = "(pendiente de completar)",
            Zona = null
        };

    public void Actualizar(string nombre, string? direccion, string? zona)
    {
        Nombre = nombre;
        if (direccion is not null) Direccion = direccion;
        Zona = zona;
    }
}
