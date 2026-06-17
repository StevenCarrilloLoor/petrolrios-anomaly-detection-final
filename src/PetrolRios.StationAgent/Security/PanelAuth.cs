using System.Collections.Concurrent;
using System.Security.Cryptography;
using PetrolRios.StationAgent.Configuration;
using PetrolRios.StationAgent.Services;

namespace PetrolRios.StationAgent.Security;

/// <summary>
/// Autenticación del panel local del agente con RBAC real. Para configurar el
/// agente hay que iniciar sesión como <b>Administrador</b> o <b>Supervisor</b>:
/// las credenciales se verifican contra el servidor central (el rango de la
/// empresa importa). Si el central no está disponible, se acepta una contraseña
/// local de respaldo (configurada por un admin) para no quedar bloqueado en campo.
/// Las sesiones son en memoria, con cookie HttpOnly y expiración.
/// </summary>
public sealed class PanelAuth
{
    public const string CookieNombre = "panel_sesion";
    private static readonly string[] RolesConfiguracion = { "Administrador", "Supervisor" };

    private readonly ServerClient _server;
    private readonly AgentConfigStore _config;
    private readonly ConcurrentDictionary<string, Sesion> _sesiones = new();

    public PanelAuth(ServerClient server, AgentConfigStore config)
    {
        _server = server;
        _config = config;
    }

    public sealed record Sesion(string Usuario, string Rol, bool EsLocal, DateTime Expira);

    /// <summary>true si todavía no hay ninguna forma de autenticación posible para arranque inicial.</summary>
    public bool RequiereBootstrap =>
        string.IsNullOrWhiteSpace(_config.Actual.PanelLocalPasswordHash) && !_config.Actual.Configurado;

    public async Task<(bool Ok, string? Token, string Rol, string Mensaje)> LoginAsync(
        string usuario, string password, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            return (false, null, "", "Ingrese usuario y contraseña.");

        // 1) Verificar contra el servidor central (rango real de la empresa)
        var (okCentral, rol, alcanzable, msg) = await _server.VerificarUsuarioAsync(usuario, password, ct);
        if (alcanzable)
        {
            if (!okCentral)
                return (false, null, "", "Credenciales inválidas.");
            if (!RolesConfiguracion.Contains(rol, StringComparer.OrdinalIgnoreCase))
                return (false, null, rol ?? "", $"Su rol ({rol}) no tiene permisos para administrar el agente.");
            return (true, CrearSesion(usuario, rol!, esLocal: false), rol!, "Sesión iniciada.");
        }

        // 2) Servidor no disponible: respaldo local (si un admin lo configuró)
        var local = _config.Actual;
        if (!string.IsNullOrWhiteSpace(local.PanelLocalPasswordHash)
            && usuario.Equals(local.PanelLocalUsuario, StringComparison.OrdinalIgnoreCase)
            && PasswordHasher.Verificar(password, local.PanelLocalPasswordHash))
        {
            return (true, CrearSesion(usuario, "Administrador (local)", esLocal: true), "Administrador (local)",
                "Sesión local (servidor central no disponible).");
        }

        return (false, null, "",
            string.IsNullOrWhiteSpace(local.PanelLocalPasswordHash)
                ? "No se pudo contactar al servidor central y no hay contraseña local configurada."
                : "Credenciales inválidas o servidor central no disponible.");
    }

    private string CrearSesion(string usuario, string rol, bool esLocal)
    {
        LimpiarExpiradas();
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        _sesiones[token] = new Sesion(usuario, rol, esLocal, DateTime.UtcNow.AddHours(8));
        return token;
    }

    public Sesion? Validar(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        if (!_sesiones.TryGetValue(token, out var s)) return null;
        if (s.Expira < DateTime.UtcNow) { _sesiones.TryRemove(token, out _); return null; }
        return s;
    }

    public void Cerrar(string? token)
    {
        if (!string.IsNullOrWhiteSpace(token)) _sesiones.TryRemove(token!, out _);
    }

    private void LimpiarExpiradas()
    {
        foreach (var kv in _sesiones)
            if (kv.Value.Expira < DateTime.UtcNow) _sesiones.TryRemove(kv.Key, out _);
    }
}
