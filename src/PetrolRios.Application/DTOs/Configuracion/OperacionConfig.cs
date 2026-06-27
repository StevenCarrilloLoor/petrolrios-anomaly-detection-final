namespace PetrolRios.Application.DTOs.Configuracion;

/// <summary>
/// Parámetros de operación editables sin recompilar (Ajustes → Operación del sistema, solo Admin):
/// el nivel mínimo de alerta que dispara aviso por correo, la frecuencia (cron) del job de detección
/// y la tasa de refresco (en segundos) con la que TODAS las pantallas vuelven a consultar al servidor.
/// </summary>
public sealed record OperacionConfig(string NivelMinimoCorreo, string CronExpression, int RefrescoSegundos = 1);
