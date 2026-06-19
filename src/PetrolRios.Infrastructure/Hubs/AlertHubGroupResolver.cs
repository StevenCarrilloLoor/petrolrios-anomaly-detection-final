using System.Security.Claims;
using PetrolRios.Application.Security;

namespace PetrolRios.Infrastructure.Hubs;

/// <summary>Resuelve grupos SignalR únicamente desde claims firmados del JWT.</summary>
public static class AlertHubGroupResolver
{
    public static IReadOnlyList<string> Resolve(ClaimsPrincipal? user)
    {
        var estacionId = user?.FindFirst(PetrolRiosClaimTypes.EstacionId)?.Value;
        if (!string.IsNullOrWhiteSpace(estacionId))
            return [$"estacion-{estacionId}"];

        var rol = user?.FindFirst(ClaimTypes.Role)?.Value;
        var grupo = rol?.ToLowerInvariant() switch
        {
            "auditor" => "auditores",
            "supervisor" => "supervisores",
            "administrador" => "administradores",
            _ => null
        };

        return grupo is null ? [] : [grupo];
    }
}
