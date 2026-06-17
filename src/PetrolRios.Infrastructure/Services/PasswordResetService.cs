using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace PetrolRios.Infrastructure.Services;

/// <summary>
/// Maneja los tokens de recuperación de contraseña (enviados por correo). Los
/// tokens viven en memoria, son de un solo uso y caducan pronto — no se persisten
/// para no requerir cambios de esquema.
/// </summary>
public sealed class PasswordResetService
{
    private sealed record Token(int UsuarioId, DateTime Expira);
    private readonly ConcurrentDictionary<string, Token> _tokens = new();
    private static readonly TimeSpan Vigencia = TimeSpan.FromHours(1);

    /// <summary>Crea un token de recuperación para el usuario y lo devuelve.</summary>
    public string Crear(int usuarioId)
    {
        Limpiar();
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        _tokens[token] = new Token(usuarioId, DateTime.UtcNow.Add(Vigencia));
        return token;
    }

    /// <summary>Devuelve el usuarioId si el token es válido y no expiró; si no, null.</summary>
    public int? Validar(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        if (!_tokens.TryGetValue(token, out var t)) return null;
        if (t.Expira < DateTime.UtcNow) { _tokens.TryRemove(token, out _); return null; }
        return t.UsuarioId;
    }

    /// <summary>Consume (elimina) el token tras usarlo.</summary>
    public void Consumir(string token) => _tokens.TryRemove(token, out _);

    private void Limpiar()
    {
        foreach (var kv in _tokens)
            if (kv.Value.Expira < DateTime.UtcNow) _tokens.TryRemove(kv.Key, out _);
    }
}
