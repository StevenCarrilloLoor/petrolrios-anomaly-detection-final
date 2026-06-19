using PetrolRios.StationMonitor.Models;

namespace PetrolRios.StationMonitor.Services;

public sealed record MonitorEvent(DateTime Fecha, string Nivel, string Mensaje);

public sealed record MonitorSnapshot(
    bool Configurado,
    bool Conectado,
    string CodigoEstacion,
    string? EstacionNombre,
    string? Usuario,
    DateTime? UltimaConsulta,
    DateTime? UltimaConsultaExitosa,
    string? UltimoError,
    IReadOnlyList<ProblemaOperativo> Problemas,
    IReadOnlyList<MonitorEvent> Eventos);

public sealed class MonitorState
{
    private readonly object _sync = new();
    private bool _conectado;
    private string? _estacionNombre;
    private string? _usuario;
    private DateTime? _ultimaConsulta;
    private DateTime? _ultimaConsultaExitosa;
    private string? _ultimoError;
    private IReadOnlyList<ProblemaOperativo> _problemas = [];
    private readonly List<MonitorEvent> _eventos = [];

    public void RegistrarExito(
        UsuarioCentral identidad,
        IReadOnlyList<ProblemaOperativo> problemas)
    {
        lock (_sync)
        {
            var estabaConectado = _conectado;
            var totalAnterior = _problemas.Count;
            var anteriores = _problemas.Select(p => p.Id).ToHashSet();
            var nuevos = problemas.Count(p => !anteriores.Contains(p.Id));
            _conectado = true;
            _estacionNombre = identidad.EstacionNombre;
            _usuario = identidad.Email;
            _ultimaConsulta = DateTime.UtcNow;
            _ultimaConsultaExitosa = DateTime.UtcNow;
            _ultimoError = null;
            _problemas = problemas
                .OrderByDescending(p => p.FechaDeteccion)
                .ToList();

            if (nuevos > 0 && anteriores.Count > 0)
                AgregarEvento("ALERTA", $"{nuevos} problema(s) operativo(s) nuevo(s).");
            else if (!estabaConectado)
                AgregarEvento("OK", $"Conectado al central: {_problemas.Count} problema(s) activo(s).");
            else if (totalAnterior != _problemas.Count)
                AgregarEvento(
                    "INFO",
                    $"El total cambió de {totalAnterior} a {_problemas.Count} problema(s) activo(s).");
        }
    }

    public void RegistrarError(string mensaje)
    {
        lock (_sync)
        {
            _conectado = false;
            _ultimaConsulta = DateTime.UtcNow;
            _ultimoError = mensaje;
            AgregarEvento("ERROR", mensaje);
        }
    }

    public void RegistrarEvento(string nivel, string mensaje)
    {
        lock (_sync)
            AgregarEvento(nivel, mensaje);
    }

    public MonitorSnapshot Snapshot(bool configurado, string codigoEstacion)
    {
        lock (_sync)
        {
            return new MonitorSnapshot(
                configurado,
                _conectado,
                codigoEstacion,
                _estacionNombre,
                _usuario,
                _ultimaConsulta,
                _ultimaConsultaExitosa,
                _ultimoError,
                _problemas.ToList(),
                _eventos.ToList());
        }
    }

    private void AgregarEvento(string nivel, string mensaje)
    {
        _eventos.Insert(0, new MonitorEvent(DateTime.UtcNow, nivel, mensaje));
        if (_eventos.Count > 80)
            _eventos.RemoveRange(80, _eventos.Count - 80);
    }
}
