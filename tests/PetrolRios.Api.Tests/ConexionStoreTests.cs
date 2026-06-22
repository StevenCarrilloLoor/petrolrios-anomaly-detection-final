using FluentAssertions;
using Microsoft.Extensions.Configuration;
using PetrolRios.Infrastructure.Configuracion;

namespace PetrolRios.Api.Tests;

/// <summary>
/// Pruebas puras (sin PostgreSQL) del resolutor de conexión flexible: precedencia
/// archivo › appsettings, construcción de la cadena y enmascarado de la contraseña.
/// </summary>
public sealed class ConexionStoreTests
{
    private static IConfiguration ConfigConAppsettings(string? appCs) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSQL"] = appCs,
            })
            .Build();

    private static (string?, string?) LimpiarEnv()
    {
        var previo = (
            Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL"),
            Environment.GetEnvironmentVariable("PETROLRIOS_DB"));
        Environment.SetEnvironmentVariable("ConnectionStrings__PostgreSQL", null);
        Environment.SetEnvironmentVariable("PETROLRIOS_DB", null);
        return previo;
    }

    private static void RestaurarEnv((string?, string?) previo)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__PostgreSQL", previo.Item1);
        Environment.SetEnvironmentVariable("PETROLRIOS_DB", previo.Item2);
    }

    private static string DirTemporal() =>
        Path.Combine(Path.GetTempPath(), "petrolrios-conx-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void ResolverActiva_PrefiereArchivoSobreAppsettings()
    {
        var previo = LimpiarEnv();
        var dir = DirTemporal();
        try
        {
            Directory.CreateDirectory(dir);
            var store = new ConexionStore(dir, ConfigConAppsettings("Host=appsettings;Database=app;Username=app"));
            store.Guardar("Host=archivo;Database=archivo;Username=archivo");

            store.ResolverActiva().Should().Contain("archivo");
        }
        finally
        {
            RestaurarEnv(previo);
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ResolverActiva_SinArchivo_UsaAppsettings()
    {
        var previo = LimpiarEnv();
        var dir = DirTemporal();
        try
        {
            Directory.CreateDirectory(dir);
            var store = new ConexionStore(dir, ConfigConAppsettings("Host=appsettings;Database=app;Username=app"));

            store.ResolverActiva().Should().Be("Host=appsettings;Database=app;Username=app");
        }
        finally
        {
            RestaurarEnv(previo);
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void DescribirActiva_NoConfigurada_DevuelveEstadoVacio()
    {
        var previo = LimpiarEnv();
        var dir = DirTemporal();
        try
        {
            var store = new ConexionStore(dir, ConfigConAppsettings(null));
            var estado = store.DescribirActiva();

            estado.Fuente.Should().Be("No configurada");
            estado.EditableDesdeUi.Should().BeTrue();
        }
        finally
        {
            RestaurarEnv(previo);
        }
    }

    [Fact]
    public void ConstruirCadena_GeneraCadenaConHostPuertoYBase()
    {
        var store = new ConexionStore(DirTemporal(), ConfigConAppsettings(null));
        var cs = store.ConstruirCadena("10.0.0.5", 5432, "petrolrios", "pr", "secreto", "Require");

        cs.Should().Contain("10.0.0.5");
        cs.Should().Contain("petrolrios");
        cs.Should().Contain("Port=5432");
    }

    [Fact]
    public void Enmascarar_OcultaLaContrasena()
    {
        var store = new ConexionStore(DirTemporal(), ConfigConAppsettings(null));
        var cs = store.ConstruirCadena("h", 5432, "db", "u", "secretoSuperSecreto", "Prefer");

        var enmascarada = store.Enmascarar(cs);

        enmascarada.Should().NotContain("secretoSuperSecreto");
        enmascarada.Should().Contain("********");
    }

    [Fact]
    public void CompletarPassword_MismaBaseSinPassword_ReusaLaActiva()
    {
        var previo = LimpiarEnv();
        var dir = DirTemporal();
        try
        {
            Directory.CreateDirectory(dir);
            var store = new ConexionStore(dir, ConfigConAppsettings(null));
            store.Guardar("Host=db1;Port=5432;Database=petrolrios;Username=pr;Password=secreto123");

            // Mismo destino, sin contraseña: reusa la activa.
            store.CompletarPassword("Host=db1;Port=5432;Database=petrolrios;Username=pr")
                .Should().Contain("secreto123");

            // Otro destino: NO reusa.
            store.CompletarPassword("Host=otro;Port=5432;Database=petrolrios;Username=pr")
                .Should().NotContain("secreto123");
        }
        finally
        {
            RestaurarEnv(previo);
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }
}
