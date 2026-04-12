namespace PetrolRios.Application.DTOs.Alertas;

public sealed record AsignarAlertaRequest(int AuditorId, string? Comentario = null);
