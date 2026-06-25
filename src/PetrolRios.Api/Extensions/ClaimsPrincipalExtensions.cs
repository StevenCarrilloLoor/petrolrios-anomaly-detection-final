using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Application.Security;
using PetrolRios.Infrastructure.Persistence;

namespace PetrolRios.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetEstacionId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(PetrolRiosClaimTypes.EstacionId)?.Value;
        return int.TryParse(value, out var estacionId) && estacionId > 0
            ? estacionId
            : null;
    }

    /// <summary>Id del usuario autenticado (claim NameIdentifier), o 0 si no se pudo resolver.</summary>
    public static int GetUsuarioId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out var id) ? id : 0;
    }

    /// <summary>
    /// Las cuentas del central (sin estación) pueden operar globalmente. Una cuenta técnica
    /// asignada solo puede informar o consultar el código de su propia estación.
    /// </summary>
    public static async Task<bool> PuedeUsarEstacionAsync(
        this ClaimsPrincipal user,
        PetrolRiosDbContext dbContext,
        string? codigoEstacion,
        CancellationToken ct)
    {
        var estacionId = user.GetEstacionId();
        if (!estacionId.HasValue)
            return true;
        if (string.IsNullOrWhiteSpace(codigoEstacion))
            return false;

        var codigo = codigoEstacion.Trim().ToUpperInvariant();
        return await dbContext.Estaciones
            .AsNoTracking()
            .AnyAsync(e => e.Id == estacionId.Value && e.Codigo == codigo, ct);
    }
}
