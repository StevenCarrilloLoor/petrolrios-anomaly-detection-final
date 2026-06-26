namespace PetrolRios.StationAgent.Services;

/// <summary>
/// Arranque automático del agente al ENCENDER el equipo, SIN permisos de administrador.
/// En Windows escribe un pequeño lanzador <c>.vbs</c> en la carpeta de Inicio del usuario que
/// levanta el agente OCULTO (sin ventana de consola) cada vez que se inicia sesión. Con autologin
/// (lo normal en una PC de estación) eso equivale a "al encender". Es reversible con un clic.
///
/// La opción avanzada (servicio de Windows, arranca antes del login pero pide admin) sigue siendo el
/// <c>instalar_agente_servicio.bat</c> que viaja en el portable; este panel ofrece la vía sin fricción.
/// </summary>
public static class InicioAutomatico
{
    private const string NombreArchivo = "PetrolRiosAgente.vbs";

    /// <summary>Solo Windows expone el botón sin admin; en Linux/macOS se usa el instalador de servicio.</summary>
    public static bool Soportado => OperatingSystem.IsWindows();

    private static string RutaLauncher() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), NombreArchivo);

    private static string? RutaEjecutable()
    {
        var p = Environment.ProcessPath;
        return string.IsNullOrWhiteSpace(p) ? null : p;
    }

    /// <summary>Estado actual: si está soportado, si está habilitado y un mensaje legible.</summary>
    public static (bool Soportado, bool Habilitado, string Ejecutable, string Mensaje) Estado()
    {
        if (!Soportado)
            return (false, false, "",
                "El botón de arranque automático solo está disponible en Windows. En Linux/macOS use el instalador de servicio incluido (systemd/launchd).");

        var habilitado = File.Exists(RutaLauncher());
        return (true, habilitado, RutaEjecutable() ?? "",
            habilitado
                ? "Activado: el agente se levanta solo (oculto) al iniciar sesión en este equipo."
                : "Desactivado: el agente no arranca solo. Actívalo para que se levante al encender el equipo.");
    }

    /// <summary>Crea el lanzador en la carpeta de Inicio (arranque oculto al iniciar sesión).</summary>
    public static (bool Ok, bool Habilitado, string Mensaje) Habilitar()
    {
        if (!Soportado)
            return (false, false, "Disponible solo en Windows desde el panel.");

        var exe = RutaEjecutable();
        if (string.IsNullOrWhiteSpace(exe) || !File.Exists(exe))
            return (false, false, "No se pudo determinar el ejecutable del agente para programar el arranque.");

        try
        {
            var dir = Path.GetDirectoryName(exe) ?? "";
            // VBS: arranca el agente OCULTO (parámetro 0 = sin ventana, False = no esperar).
            var vbs =
                "' Generado por el panel del Agente PetrolRios. No editar.\r\n" +
                "' Levanta el agente oculto (sin ventana) al iniciar sesion en Windows.\r\n" +
                "Set sh = CreateObject(\"WScript.Shell\")\r\n" +
                $"sh.CurrentDirectory = \"{dir}\"\r\n" +
                $"sh.Run \"\"\"{exe}\"\"\", 0, False\r\n";
            File.WriteAllText(RutaLauncher(), vbs);
            return (true, true,
                "Listo. El agente arrancará solo (oculto) cada vez que se inicie sesión en este equipo.");
        }
        catch (Exception ex)
        {
            return (false, File.Exists(RutaLauncher()),
                "No se pudo activar el arranque automático: " + ex.Message);
        }
    }

    /// <summary>Elimina el lanzador de la carpeta de Inicio.</summary>
    public static (bool Ok, bool Habilitado, string Mensaje) Deshabilitar()
    {
        if (!Soportado)
            return (false, false, "Disponible solo en Windows desde el panel.");

        try
        {
            var ruta = RutaLauncher();
            if (File.Exists(ruta)) File.Delete(ruta);
            return (true, false, "Arranque automático desactivado. El agente ya no se levantará solo.");
        }
        catch (Exception ex)
        {
            return (false, File.Exists(RutaLauncher()),
                "No se pudo desactivar el arranque automático: " + ex.Message);
        }
    }
}
