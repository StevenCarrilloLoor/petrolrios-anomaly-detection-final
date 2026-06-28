namespace PetrolRios.Application.DTOs.Configuracion;

/// <summary>
/// Parámetros de operación editables sin recompilar (Ajustes → Operación del sistema, solo Admin):
/// el nivel mínimo de alerta que dispara aviso por correo, la frecuencia (cron) del job de detección,
/// la tasa de refresco (en segundos) con la que TODAS las pantallas vuelven a consultar al servidor, y
/// la preferencia de precio de combustible (qué fuente manda como precio efectivo).
/// </summary>
public sealed record OperacionConfig(
    string NivelMinimoCorreo,
    string CronExpression,
    int RefrescoSegundos = 1,
    /// <summary>"Auto" (más reciente válido, con la corrección manual del admin fijada por el período),
    /// "Api" (siempre el scrapeado) o "Sistema" (siempre el del sistema). Default "Auto".</summary>
    string PreferenciaPreciosCombustible = "Auto");
