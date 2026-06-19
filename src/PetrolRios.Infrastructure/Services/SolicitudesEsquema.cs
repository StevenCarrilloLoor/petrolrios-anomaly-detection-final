using System.Collections.Concurrent;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Infrastructure.Services;

/// <summary>
/// Implementación en memoria de <see cref="ISolicitudesEsquema"/>. Singleton: las solicitudes viven
/// mientras la API esté en marcha (es una señal efímera "pide el esquema en el próximo latido").
/// </summary>
public sealed class SolicitudesEsquema : ISolicitudesEsquema
{
    private readonly ConcurrentDictionary<string, byte> _pendientes = new(StringComparer.OrdinalIgnoreCase);

    public void Solicitar(string codigoEstacion)
    {
        if (!string.IsNullOrWhiteSpace(codigoEstacion))
            _pendientes[codigoEstacion.Trim()] = 1;
    }

    public bool TomarPendiente(string codigoEstacion)
    {
        if (string.IsNullOrWhiteSpace(codigoEstacion)) return false;
        return _pendientes.TryRemove(codigoEstacion.Trim(), out _);
    }
}
