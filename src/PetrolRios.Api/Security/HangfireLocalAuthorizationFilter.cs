using Hangfire.Dashboard;

namespace PetrolRios.Api.Security;

/// <summary>
/// Restringe el dashboard de Hangfire a peticiones locales (misma máquina) y,
/// si la petición trae usuario autenticado, exige el rol Administrador. Cierra el
/// hueco de tener el panel de jobs abierto a cualquiera en la red.
/// </summary>
public sealed class HangfireLocalAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();

        // Si hay un usuario autenticado, debe ser Administrador.
        if (http.User?.Identity?.IsAuthenticated == true)
            return http.User.IsInRole("Administrador");

        // Sin usuario: solo se permite desde la propia máquina del servidor.
        var remoto = http.Connection.RemoteIpAddress;
        var local = http.Connection.LocalIpAddress;

        if (remoto is null) return false;
        if (System.Net.IPAddress.IsLoopback(remoto)) return true;
        // Misma IP que el servidor (acceso desde la consola del host)
        return remoto.Equals(local);
    }
}
