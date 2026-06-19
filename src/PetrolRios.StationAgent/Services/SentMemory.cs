using System.Security.Cryptography;
using System.Text;
using PetrolRios.StationAgent.Configuration;

namespace PetrolRios.StationAgent.Services;

/// <summary>
/// Memoria de envío del agente: recuerda la huella de los registros ya enviados al
/// servidor central para no reenviarlos en cada ciclo.
///
/// Motivación: algunas extracciones incluyen registros "vivos" que reaparecen ciclo a
/// ciclo aunque su marca de agua sea anterior (p. ej. un turno aún sin cerrar,
/// <c>EST_TURN = '0'</c>). Sin esta memoria, ese mismo registro se reenviaría una y otra
/// vez ("1 transacción enviada" en bucle). El servidor central lo descarta por su índice
/// único de huella, así que no hay duplicados en la base, pero es tráfico y ruido inútiles.
///
/// La huella se calcula sobre el CONTENIDO del registro (tipo + datos), de modo que si el
/// registro cambia realmente (p. ej. el turno se cierra y cambia <c>EST_TURN</c>), su huella
/// será distinta y volverá a enviarse una sola vez.
/// </summary>
public sealed class SentMemory
{
    private const int CapacidadMaxima = 100_000;
    private readonly object _lock = new();
    private readonly string _archivo;
    private readonly ILogger<SentMemory> _logger;
    // Cola de inserción para poder recortar las huellas más antiguas (FIFO).
    private readonly LinkedList<string> _orden = new();
    private readonly HashSet<string> _huellas = new(StringComparer.Ordinal);

    public SentMemory(AgentConfigStore config, ILogger<SentMemory> logger)
    {
        _logger = logger;
        _archivo = Path.Combine(config.Actual.LocalStorePath, "enviados.huellas");
        Cargar();
    }

    /// <summary>Calcula la huella de contenido de un registro (igual contenido ⇒ igual huella).</summary>
    public static string CalcularHuella(TransaccionBatchItem item)
    {
        var bytes = Encoding.UTF8.GetBytes($"{item.TipoTransaccion}|{item.DataJson}");
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    /// <summary>Indica si un registro con esta huella ya fue enviado.</summary>
    public bool YaEnviado(TransaccionBatchItem item)
    {
        var huella = CalcularHuella(item);
        lock (_lock)
        {
            return _huellas.Contains(huella);
        }
    }

    /// <summary>Filtra una lista dejando solo los registros que aún no se han enviado.</summary>
    public List<TransaccionBatchItem> FiltrarNuevos(IReadOnlyList<TransaccionBatchItem> items)
    {
        lock (_lock)
        {
            var nuevos = new List<TransaccionBatchItem>(items.Count);
            foreach (var item in items)
            {
                if (!_huellas.Contains(CalcularHuella(item)))
                    nuevos.Add(item);
            }
            return nuevos;
        }
    }

    /// <summary>Marca un conjunto de registros como ya enviados y persiste la memoria.</summary>
    public void MarcarEnviados(IReadOnlyList<TransaccionBatchItem> items)
    {
        if (items.Count == 0) return;
        lock (_lock)
        {
            foreach (var item in items)
            {
                var huella = CalcularHuella(item);
                if (_huellas.Add(huella))
                    _orden.AddLast(huella);
            }
            // Recorte FIFO si superamos la capacidad máxima.
            while (_orden.Count > CapacidadMaxima)
            {
                var antigua = _orden.First!.Value;
                _orden.RemoveFirst();
                _huellas.Remove(antigua);
            }
            Guardar();
        }
    }

    private void Cargar()
    {
        try
        {
            if (!File.Exists(_archivo)) return;
            foreach (var linea in File.ReadAllLines(_archivo))
            {
                var huella = linea.Trim();
                if (huella.Length == 0) continue;
                if (_huellas.Add(huella))
                    _orden.AddLast(huella);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo cargar la memoria de envío; se empezará vacía");
        }
    }

    private void Guardar()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_archivo)!);
            File.WriteAllLines(_archivo, _orden);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo guardar la memoria de envío");
        }
    }
}
