using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PetrolRios.Application.DTOs.Configuracion;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Infrastructure.Configuracion;

/// <summary>
/// Resuelve, prueba y persiste la conexión a PostgreSQL del sistema central de forma
/// flexible y editable sin tocar el código. Prioridad de la conexión activa:
/// variable de entorno (ConnectionStrings__PostgreSQL / PETROLRIOS_DB) ›
/// archivo config/connection.json › appsettings. La base puede vivir en cualquier
/// máquina o sistema operativo.
/// </summary>
public sealed class ConexionStore : IConexionStore
{
    private const string EnvPrincipal = "ConnectionStrings__PostgreSQL";
    private const string EnvAlias = "PETROLRIOS_DB";

    private readonly string _rutaArchivo;
    private readonly IConfiguration _configuration;

    public ConexionStore(string configDir, IConfiguration configuration)
    {
        _rutaArchivo = Path.Combine(configDir, "connection.json");
        _configuration = configuration;
    }

    public string? ResolverActiva()
    {
        var env = LeerEnv();
        if (!string.IsNullOrWhiteSpace(env)) return env.Trim();

        var archivo = LeerArchivo();
        if (!string.IsNullOrWhiteSpace(archivo)) return archivo;

        var app = _configuration.GetConnectionString("PostgreSQL");
        return string.IsNullOrWhiteSpace(app) ? null : app;
    }

    public ConexionActiva DescribirActiva()
    {
        var (fuente, editable) = DescribirFuente();
        var activa = ResolverActiva();
        if (string.IsNullOrWhiteSpace(activa))
            return new ConexionActiva("No configurada", true, "(sin configurar)", null, 5432, null, null, "Prefer");

        string? servidor = null, baseDatos = null, usuario = null, ssl = "Prefer";
        var puerto = 5432;
        try
        {
            var b = new NpgsqlConnectionStringBuilder(activa);
            servidor = b.Host;
            puerto = b.Port;
            baseDatos = b.Database;
            usuario = b.Username;
            ssl = b.SslMode.ToString();
        }
        catch
        {
            // Cadena no analizable por Npgsql: se muestra enmascarada igual y se conservan los nulos.
        }

        return new ConexionActiva(fuente, editable, Enmascarar(activa), servidor, puerto, baseDatos, usuario, ssl);
    }

    public string ConstruirCadena(string servidor, int puerto, string baseDatos, string usuario, string? password, string modoSsl)
    {
        var b = new NpgsqlConnectionStringBuilder
        {
            Host = servidor,
            Port = puerto <= 0 ? 5432 : puerto,
            Database = baseDatos,
            Username = usuario,
            Password = password,
            SslMode = MapearSsl(modoSsl),
            Timeout = 8,
            CommandTimeout = 30,
        };
        return b.ConnectionString;
    }

    public string CompletarPassword(string cadena)
    {
        try
        {
            var b = new NpgsqlConnectionStringBuilder(cadena);
            if (!string.IsNullOrEmpty(b.Password)) return cadena; // ya trae contraseña

            var activa = ResolverActiva();
            if (string.IsNullOrWhiteSpace(activa)) return cadena;
            var a = new NpgsqlConnectionStringBuilder(activa);

            var mismoDestino =
                string.Equals(b.Host, a.Host, StringComparison.OrdinalIgnoreCase)
                && b.Port == a.Port
                && string.Equals(b.Database, a.Database, StringComparison.OrdinalIgnoreCase)
                && string.Equals(b.Username, a.Username, StringComparison.OrdinalIgnoreCase);

            if (mismoDestino && !string.IsNullOrEmpty(a.Password))
            {
                b.Password = a.Password;
                return b.ConnectionString;
            }
            return cadena;
        }
        catch
        {
            return cadena;
        }
    }

    public string Enmascarar(string cadena)
    {
        try
        {
            var b = new NpgsqlConnectionStringBuilder(cadena);
            if (!string.IsNullOrEmpty(b.Password)) b.Password = "********";
            return b.ConnectionString;
        }
        catch
        {
            return "(cadena no analizable)";
        }
    }

    public async Task<(bool Ok, string? Mensaje, string? Version)> ProbarAsync(string cadena, CancellationToken ct)
    {
        try
        {
            var b = new NpgsqlConnectionStringBuilder(cadena);
            if (b.Timeout > 10) b.Timeout = 10;
            await using var con = new NpgsqlConnection(b.ConnectionString);
            await con.OpenAsync(ct);
            var version = con.PostgreSqlVersion?.ToString();
            return (true, "Conexión exitosa.", version);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public void Guardar(string cadena)
    {
        var dir = Path.GetDirectoryName(_rutaArchivo);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(
            new ArchivoConexion(cadena),
            new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_rutaArchivo, json);
        IntentarRestringirPermisos(_rutaArchivo);
    }

    private static string? LeerEnv()
    {
        var env = Environment.GetEnvironmentVariable(EnvPrincipal);
        if (string.IsNullOrWhiteSpace(env)) env = Environment.GetEnvironmentVariable(EnvAlias);
        return env;
    }

    private (string fuente, bool editable) DescribirFuente()
    {
        if (!string.IsNullOrWhiteSpace(LeerEnv()))
            return ("Variable de entorno", false); // la variable manda; el archivo no surte efecto hasta quitarla
        if (!string.IsNullOrWhiteSpace(LeerArchivo()))
            return ("Archivo (config/connection.json)", true);
        if (!string.IsNullOrWhiteSpace(_configuration.GetConnectionString("PostgreSQL")))
            return ("appsettings", true);
        return ("No configurada", true);
    }

    private string? LeerArchivo()
    {
        if (!File.Exists(_rutaArchivo)) return null;
        try
        {
            var a = JsonSerializer.Deserialize<ArchivoConexion>(File.ReadAllText(_rutaArchivo));
            return string.IsNullOrWhiteSpace(a?.PostgreSQL) ? null : a!.PostgreSQL;
        }
        catch
        {
            return null;
        }
    }

    private static SslMode MapearSsl(string? modo) => (modo ?? string.Empty).Trim().ToLowerInvariant() switch
    {
        "disable" or "desactivado" or "off" => SslMode.Disable,
        "allow" => SslMode.Allow,
        "require" or "requerido" => SslMode.Require,
        "verifyca" or "verify-ca" => SslMode.VerifyCA,
        "verifyfull" or "verify-full" => SslMode.VerifyFull,
        _ => SslMode.Prefer,
    };

    private static void IntentarRestringirPermisos(string ruta)
    {
        try
        {
            if (!OperatingSystem.IsWindows())
                File.SetUnixFileMode(ruta, UnixFileMode.UserRead | UnixFileMode.UserWrite); // chmod 600
        }
        catch
        {
            // Mejor esfuerzo: si el SO no lo permite, el archivo igual queda fuera de git.
        }
    }

    private sealed record ArchivoConexion(string PostgreSQL);
}
