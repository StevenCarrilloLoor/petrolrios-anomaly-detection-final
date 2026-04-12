using PetrolRios.Domain.Enums;

namespace PetrolRios.Domain.Entities;

public class EjecucionJob : BaseEntity
{
    public EstadoJob Estado { get; set; } = EstadoJob.Pendiente;
    public DateTime FechaInicio { get; private set; } = DateTime.UtcNow;
    public DateTime? FechaFin { get; set; }
    public int AlertasGeneradas { get; set; }
    public int EstacionesProcesadas { get; set; }
    public int EstacionesConError { get; set; }
    public double DuracionSegundos { get; set; }
    public string? ErrorDetalle { get; set; }

    public ICollection<Alerta> Alertas { get; private set; } = [];

    public static EjecucionJob Create() => new();

    public void Completar(int alertas, int procesadas, int conError, double duracion)
    {
        Estado = conError == procesadas ? EstadoJob.Fallido : EstadoJob.Completado;
        FechaFin = DateTime.UtcNow;
        AlertasGeneradas = alertas;
        EstacionesProcesadas = procesadas;
        EstacionesConError = conError;
        DuracionSegundos = duracion;
    }

    public void Fallar(string error)
    {
        Estado = EstadoJob.Fallido;
        FechaFin = DateTime.UtcNow;
        ErrorDetalle = error;
    }
}
