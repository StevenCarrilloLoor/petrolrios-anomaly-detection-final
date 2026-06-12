using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Api.Extensions;

/// <summary>
/// Registro de auditoría desde los controllers (CU-17).
/// Captura usuario autenticado e IP de origen automáticamente.
/// </summary>
public static class AuditoriaExtensions
{
    public static Task RegistrarAuditoriaAsync(
        this ControllerBase controller,
        ILogService logService,
        string accion,
        string entidad,
        int? entidadId = null,
        object? detalle = null,
        int? usuarioIdExplicito = null,
        CancellationToken ct = default)
    {
        int? usuarioId = usuarioIdExplicito;
        if (usuarioId is null &&
            int.TryParse(controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id))
        {
            usuarioId = id;
        }

        var ip = controller.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
        var detalleJson = detalle is null ? null : JsonSerializer.Serialize(detalle);

        return logService.RegistrarAsync(accion, entidad, entidadId, detalleJson, ip, usuarioId, ct);
    }
}
