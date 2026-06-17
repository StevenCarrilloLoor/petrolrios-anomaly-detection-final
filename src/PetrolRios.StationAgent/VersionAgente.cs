using System.Reflection;

namespace PetrolRios.StationAgent;

/// <summary>
/// Versión del agente, tomada del ensamblado (fuente única en Directory.Build.props).
/// Se reporta en el heartbeat y se compara contra el manifiesto de actualización.
/// </summary>
public static class VersionAgente
{
    /// <summary>Versión normalizada "Mayor.Menor.Parche" (sin metadatos de build).</summary>
    public static string Actual { get; } = Calcular();

    private static string Calcular()
    {
        var asm = Assembly.GetExecutingAssembly();
        // InformationalVersion respeta el <Version> de Directory.Build.props
        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(info))
        {
            // Quitar sufijos de build tipo "+abc123"
            var limpio = info.Split('+')[0];
            return limpio;
        }
        var v = asm.GetName().Version;
        return v is null ? "0.0.0" : $"{v.Major}.{v.Minor}.{v.Build}";
    }
}
