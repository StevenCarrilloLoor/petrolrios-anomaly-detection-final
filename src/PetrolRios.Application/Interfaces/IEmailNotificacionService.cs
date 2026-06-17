namespace PetrolRios.Application.Interfaces;

/// <summary>
/// Envío de notificaciones por correo (SMTP) para alertas críticas, como se
/// describe en la tesis (PASO 5 - Notificación por Email). Es opcional y
/// configurable: si no está habilitado en la configuración, no hace nada.
/// </summary>
public interface IEmailNotificacionService
{
    /// <summary>true si el envío de correo está habilitado y configurado.</summary>
    bool Habilitado { get; }

    /// <summary>Envía un correo a los destinatarios indicados. Nunca lanza: registra y continúa.</summary>
    Task EnviarAsync(string asunto, string cuerpoHtml, IReadOnlyList<string> destinatarios, CancellationToken ct = default);
}
