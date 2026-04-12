namespace PetrolRios.Infrastructure.Firebird;

/// <summary>
/// Configuración de connection strings Firebird por código de estación.
/// Mapeado desde la sección "FirebirdStations" de appsettings.
/// </summary>
public sealed class FirebirdOptions
{
    public const string SectionName = "FirebirdStations";

    /// <summary>
    /// Diccionario: clave = código de estación (ej. "EST-001"), valor = connection string Firebird.
    /// </summary>
    public Dictionary<string, string> Stations { get; set; } = [];
}
