using System.Reflection;

namespace PetrolRios.StationMonitor;

/// <summary>
/// Versión del monitor, tomada del ensamblado (fuente única en Directory.Build.props).
/// Se compara contra el manifiesto de actualización del central.
/// </summary>
public static class VersionMonitor
{
    /// <summary>Versión normalizada "Mayor.Menor.Parche" (sin metadatos de build).</summary>
    public static string Actual { get; } = Calcular();

    private static string Calcular()
    {
        var asm = Assembly.GetExecutingAssembly();
        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(info))
            return info.Split('+')[0];

        var v = asm.GetName().Version;
        return v is null ? "0.0.0" : $"{v.Major}.{v.Minor}.{v.Build}";
    }
}
