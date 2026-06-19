namespace PetrolRios.Application.Fuentes;

public static class FuenteDatosPolicy
{
    public static bool EsTipoTemporal(string? tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            return false;

        var normalizado = tipo.Trim().ToUpperInvariant();
        return normalizado.StartsWith("DATE", StringComparison.Ordinal)
               || normalizado.StartsWith("TIME", StringComparison.Ordinal)
               || normalizado.StartsWith("TIMESTAMP", StringComparison.Ordinal);
    }

    /// <summary>
    /// Firebird TIMESTAMP no almacena zona. Conserva exactamente los ticks del valor
    /// leído y elimina el Kind para reutilizarlo como parámetro del próximo SELECT.
    /// </summary>
    public static DateTime NormalizarCursorFirebird(DateTime fecha) =>
        DateTime.SpecifyKind(fecha, DateTimeKind.Unspecified);
}
