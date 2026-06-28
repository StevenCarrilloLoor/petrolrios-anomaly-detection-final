using System.Collections.Concurrent;
using PetrolRios.Application.DTOs.Consultas;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Infrastructure.Services;

/// <summary>
/// Implementación en memoria de <see cref="IConsultasFirebird"/>. Singleton: las consultas viven mientras
/// la API esté en marcha (son efímeras: se piden, el agente las corre y la interfaz lee el resultado). Las
/// entradas se purgan a los pocos minutos para no acumular memoria.
/// </summary>
public sealed class ConsultasFirebird : IConsultasFirebird
{
    private sealed class Entrada
    {
        public required string CodigoEstacion { get; init; }
        public required SolicitudConsulta Solicitud { get; init; }
        public string Estado { get; set; } = "Pendiente";   // Pendiente | Listo | Error
        public string? ResultadoJson { get; set; }
        public string? Error { get; set; }
        public bool Tomada { get; set; }
        public DateTime CreadaUtc { get; } = DateTime.UtcNow;
    }

    private static readonly TimeSpan Expira = TimeSpan.FromMinutes(3);
    private readonly ConcurrentDictionary<string, Entrada> _entradas = new();

    public string Encolar(SolicitudConsulta solicitud)
    {
        Purgar();
        var id = Guid.NewGuid().ToString("N");
        _entradas[id] = new Entrada { CodigoEstacion = solicitud.CodigoEstacion.Trim(), Solicitud = solicitud };
        return id;
    }

    public IReadOnlyList<ConsultaPendiente> TomarPendientes(string codigoEstacion)
    {
        if (string.IsNullOrWhiteSpace(codigoEstacion)) return [];
        var est = codigoEstacion.Trim();
        var pendientes = new List<ConsultaPendiente>();
        foreach (var (id, e) in _entradas)
        {
            if (e.Tomada || e.Estado != "Pendiente") continue;
            if (!string.Equals(e.CodigoEstacion, est, StringComparison.OrdinalIgnoreCase)) continue;
            e.Tomada = true;
            var s = e.Solicitud;
            pendientes.Add(new ConsultaPendiente(id, s.TipoDocumento, s.FechaDesde, s.FechaHasta, s.Codigo, s.Limite, s.Tabla, s.Codigos));
        }
        return pendientes;
    }

    public void Responder(string id, bool ok, string? resultadoJson, string? error)
    {
        if (_entradas.TryGetValue(id, out var e))
        {
            e.Estado = ok ? "Listo" : "Error";
            e.ResultadoJson = resultadoJson;
            e.Error = error;
        }
    }

    public ConsultaEstado? Obtener(string id)
    {
        Purgar();
        return _entradas.TryGetValue(id, out var e)
            ? new ConsultaEstado(id, e.Estado, e.ResultadoJson, e.Error)
            : null;
    }

    private void Purgar()
    {
        var limite = DateTime.UtcNow - Expira;
        foreach (var (id, e) in _entradas)
            if (e.CreadaUtc < limite)
                _entradas.TryRemove(id, out _);
    }
}
