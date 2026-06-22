using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PetrolRios.Application.RealTime;

/// <summary>
/// Payload de una notificación de alerta en tiempo real. Es exactamente el contrato que ve el
/// cliente SignalR (mismos nombres de campo que antes), para no romper el frontend.
/// </summary>
public sealed record AlertaNotificacionPayload(
    string NotificationId,
    int Id,
    string TipoDetector,
    string NivelRiesgo,
    string Ambito,
    string Descripcion,
    double Score,
    DateTime FechaDeteccion,
    int EstacionId);

/// <summary>Un push de alerta: el evento SignalR, los grupos destino y el payload.</summary>
public sealed record AlertaPush(
    string Evento,
    IReadOnlyList<string> Grupos,
    AlertaNotificacionPayload Payload);

/// <summary>
/// Publica un push de alerta para que TODAS las instancias del central lo entreguen a sus
/// clientes SignalR conectados. La implementación usa PostgreSQL LISTEN/NOTIFY sobre la base
/// compartida — sin infraestructura extra (respeta el stack de la tesis).
/// </summary>
public interface IAlertaBroadcaster
{
    Task PublicarAsync(AlertaPush push, CancellationToken ct = default);
}
