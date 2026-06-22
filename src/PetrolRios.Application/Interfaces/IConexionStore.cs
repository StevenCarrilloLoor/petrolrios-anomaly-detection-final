using System.Threading;
using System.Threading.Tasks;
using PetrolRios.Application.DTOs.Configuracion;

namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Gestiona la conexión a PostgreSQL del sistema central de forma flexible y editable,
/// sin tocar el código. Prioridad de la conexión activa:
/// variable de entorno (ConnectionStrings__PostgreSQL / PETROLRIOS_DB) ›
/// archivo config/connection.json › appsettings. La base puede vivir en cualquier
/// máquina o sistema operativo; el sistema central solo necesita la cadena.
/// </summary>
public interface IConexionStore
{
    /// <summary>Cadena de conexión activa según prioridad, o null si no hay ninguna configurada.</summary>
    string? ResolverActiva();

    /// <summary>Describe la conexión activa (enmascarada) y su origen, para mostrarla en Ajustes.</summary>
    ConexionActiva DescribirActiva();

    /// <summary>Construye una cadena de conexión Npgsql a partir de campos simples.</summary>
    string ConstruirCadena(string servidor, int puerto, string baseDatos, string usuario, string? password, string modoSsl);

    /// <summary>Devuelve la cadena con la contraseña oculta, para mostrarla sin exponer el secreto.</summary>
    string Enmascarar(string cadena);

    /// <summary>Abre una conexión de prueba con timeout. No persiste nada.</summary>
    Task<(bool Ok, string? Mensaje, string? Version)> ProbarAsync(string cadena, CancellationToken ct);

    /// <summary>Persiste la cadena en config/connection.json (fuera de git, permisos restringidos).</summary>
    void Guardar(string cadena);
}
