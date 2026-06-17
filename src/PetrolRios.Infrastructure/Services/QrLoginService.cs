using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace PetrolRios.Infrastructure.Services;

/// <summary>
/// Maneja el estado de los inicios de sesión por QR (estilo Steam): la pantalla
/// que quiere entrar genera un código, lo muestra como QR, y un usuario ya
/// autenticado lo aprueba desde otro dispositivo. El estado vive en memoria con
/// expiración corta (no se persiste; es de un solo uso).
/// </summary>
public sealed class QrLoginService
{
    public enum EstadoQr { NoExiste, Pendiente, Aprobado, Expirado }

    private sealed class Challenge
    {
        public int? UsuarioId { get; set; }
        public bool Aprobado { get; set; }
        public DateTime Expira { get; init; }
    }

    private readonly ConcurrentDictionary<string, Challenge> _challenges = new();
    private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(2);

    /// <summary>Crea un código de un solo uso y devuelve (codigo, segundos de vigencia).</summary>
    public (string Codigo, int ExpiraSegundos) Iniciar()
    {
        Limpiar();
        var codigo = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        _challenges[codigo] = new Challenge { Expira = DateTime.UtcNow.Add(Vigencia) };
        return (codigo, (int)Vigencia.TotalSeconds);
    }

    /// <summary>Un usuario autenticado aprueba un código pendiente.</summary>
    public bool Aprobar(string codigo, int usuarioId)
    {
        if (!_challenges.TryGetValue(codigo, out var ch)) return false;
        if (ch.Expira < DateTime.UtcNow) { _challenges.TryRemove(codigo, out _); return false; }
        ch.UsuarioId = usuarioId;
        ch.Aprobado = true;
        return true;
    }

    /// <summary>Consulta el estado del código (lo usa la pantalla que espera, en polling).</summary>
    public (EstadoQr Estado, int? UsuarioId) Consultar(string codigo)
    {
        if (!_challenges.TryGetValue(codigo, out var ch)) return (EstadoQr.NoExiste, null);
        if (ch.Expira < DateTime.UtcNow) { _challenges.TryRemove(codigo, out _); return (EstadoQr.Expirado, null); }
        return ch.Aprobado ? (EstadoQr.Aprobado, ch.UsuarioId) : (EstadoQr.Pendiente, null);
    }

    /// <summary>Consume (elimina) el código una vez emitido el token.</summary>
    public void Consumir(string codigo) => _challenges.TryRemove(codigo, out _);

    private void Limpiar()
    {
        foreach (var kv in _challenges)
            if (kv.Value.Expira < DateTime.UtcNow) _challenges.TryRemove(kv.Key, out _);
    }
}
