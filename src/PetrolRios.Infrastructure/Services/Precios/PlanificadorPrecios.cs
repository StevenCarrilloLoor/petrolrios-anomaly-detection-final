namespace PetrolRios.Infrastructure.Services.Precios;

/// <summary>Modo del schedule de precios en un instante dado (hora local de Ecuador, UTC-5).</summary>
public enum ModoScrapePrecios
{
    /// <summary>No corresponde scrapear (fuera de la hora normal y fuera de la ventana de alerta).</summary>
    Inactivo,

    /// <summary>Días 1–10: una verificación diaria dentro de la hora 08:00.</summary>
    Normal,

    /// <summary>Ventana crítica del cambio de precio: desde las 14:00 del día 11 hasta las 14:00 del día 12.</summary>
    Alerta
}

/// <summary>
/// Lógica pura del schedule adaptativo de precios (sin dependencias, testeable). El precio oficial cambia
/// el día 12 a las 00:00; el anuncio llega la tarde del 11. Por eso: en días normales (1–10) basta una
/// verificación diaria a las 08:00; en la ventana 11→12 se vigila cada hora. Todo en hora local de Ecuador.
/// </summary>
public static class PlanificadorPrecios
{
    /// <summary>Zona horaria de Ecuador (UTC-5, sin horario de verano). Idéntica en Windows y Linux.</summary>
    public static readonly TimeZoneInfo ZonaEcuador =
        TimeZoneInfo.CreateCustomTimeZone("PetrolRios-EC", TimeSpan.FromHours(-5), "Ecuador (UTC-5)", "Ecuador (UTC-5)");

    /// <summary>Determina el modo de scrape para una hora local de Ecuador.</summary>
    public static ModoScrapePrecios Modo(DateTime ahoraEcuador)
    {
        var dia = ahoraEcuador.Day;
        var hora = ahoraEcuador.Hour;

        // Ventana de alerta: 14:00 del día 11 → 14:00 del día 12 (inclusive).
        if ((dia == 11 && hora >= 14) || (dia == 12 && hora <= 14))
            return ModoScrapePrecios.Alerta;

        // Modo normal: días 1–10, en la hora 08:00.
        if (dia is >= 1 and <= 10 && hora == 8)
            return ModoScrapePrecios.Normal;

        return ModoScrapePrecios.Inactivo;
    }

    /// <summary>Convierte el instante UTC a hora local de Ecuador.</summary>
    public static DateTime AhoraEcuador(DateTime utcNow) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), ZonaEcuador);
}
