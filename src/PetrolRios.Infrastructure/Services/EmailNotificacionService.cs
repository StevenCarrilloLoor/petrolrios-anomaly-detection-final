using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PetrolRios.Application.Interfaces;

namespace PetrolRios.Infrastructure.Services;

/// <summary>
/// Notificador SMTP de alertas críticas. Toda la configuración (servidor, puerto,
/// credenciales, remitente) viene de la sección <c>Notificaciones:Email</c> de la
/// configuración o de variables de entorno — nunca quemada en el código. Está
/// deshabilitado por defecto; se activa con <c>Notificaciones:Email:Habilitado=true</c>.
/// </summary>
public sealed class EmailNotificacionService : IEmailNotificacionService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailNotificacionService> _logger;

    public EmailNotificacionService(IConfiguration config, ILogger<EmailNotificacionService> logger)
    {
        _config = config;
        _logger = logger;
    }

    private IConfigurationSection Seccion => _config.GetSection("Notificaciones:Email");

    public bool Habilitado =>
        Seccion.GetValue("Habilitado", false)
        && !string.IsNullOrWhiteSpace(Seccion["Host"])
        && !string.IsNullOrWhiteSpace(Seccion["Remitente"]);

    public async Task EnviarAsync(
        string asunto, string cuerpoHtml, IReadOnlyList<string> destinatarios, CancellationToken ct = default)
    {
        if (!Habilitado)
        {
            _logger.LogDebug("Notificación por correo deshabilitada; no se envía.");
            return;
        }
        if (destinatarios is null || destinatarios.Count == 0)
        {
            _logger.LogDebug("Sin destinatarios para la notificación por correo.");
            return;
        }

        try
        {
            var host = Seccion["Host"]!;
            var puerto = Seccion.GetValue("Puerto", 587);
            var usuario = Seccion["Usuario"];
            var password = Seccion["Password"];
            var remitente = Seccion["Remitente"]!;
            var ssl = Seccion.GetValue("UsarSsl", true);

            using var mensaje = new MailMessage
            {
                From = new MailAddress(remitente, "PetrolRíos — Detección de Anomalías"),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };
            foreach (var d in destinatarios.Where(x => !string.IsNullOrWhiteSpace(x)))
                mensaje.To.Add(d.Trim());

            using var cliente = new SmtpClient(host, puerto)
            {
                EnableSsl = ssl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            // Solo autentica si hay credenciales configuradas (algunos relays internos no las piden)
            if (!string.IsNullOrWhiteSpace(usuario))
                cliente.Credentials = new NetworkCredential(usuario, password);

            await cliente.SendMailAsync(mensaje, ct);
            _logger.LogInformation("Notificación por correo enviada a {Total} destinatario(s)", mensaje.To.Count);
        }
        catch (Exception ex)
        {
            // Nunca tumbar el ciclo de detección por un fallo de correo
            _logger.LogWarning(ex, "No se pudo enviar la notificación por correo");
        }
    }
}
