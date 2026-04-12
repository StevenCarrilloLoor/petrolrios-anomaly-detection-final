using System.Runtime.InteropServices;

namespace PetrolRios.Api.Tests;

/// <summary>
/// Fact que se salta automáticamente si Docker no está disponible.
/// Útil para tests de integración que usan Testcontainers.
/// </summary>
public sealed class DockerAvailableFactAttribute : FactAttribute
{
    private static readonly Lazy<bool> IsDockerAvailable = new(CheckDocker);

    public DockerAvailableFactAttribute()
    {
        if (!IsDockerAvailable.Value)
        {
            Skip = "Docker no está disponible. Inicie Docker Desktop para ejecutar los tests de integración.";
        }
    }

    private static bool CheckDocker()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "info",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(psi);
            if (process is null) return false;

            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
