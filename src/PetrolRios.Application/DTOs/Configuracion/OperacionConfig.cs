namespace PetrolRios.Application.DTOs.Configuracion;

/// <summary>
/// Parámetros de operación editables sin recompilar (Ajustes → Operación del sistema, solo Admin):
/// el nivel mínimo de alerta que dispara aviso por correo y la frecuencia (cron) del job de detección.
/// </summary>
public sealed record OperacionConfig(string NivelMinimoCorreo, string CronExpression);
