using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PetrolRios.Domain.Entities;
using PetrolRios.Infrastructure.Persistence;
using PetrolRios.Infrastructure.Services;

namespace PetrolRios.Api.Tests;

/// <summary>
/// Resolución (estación, código) → nombre del catálogo de empleados. Verifica el cruce con TRIM y
/// mayúsculas, el aislamiento por estación y el comportamiento sin match. Usa EF InMemory (sin Docker).
/// </summary>
public sealed class EmpleadoDirectorioTests
{
    private static PetrolRiosDbContext NuevoContexto() =>
        new(new DbContextOptionsBuilder<PetrolRiosDbContext>()
            .UseInMemoryDatabase($"EmpDir_{Guid.NewGuid()}")
            .Options);

    [Fact]
    public async Task CargarAsync_ResuelveNombrePorEstacionYCodigo_ConTrimYMayusculas()
    {
        await using var db = NuevoContexto();
        db.Empleados.Add(Empleado.Create(1, "V001", "Juan Pérez"));
        db.Empleados.Add(Empleado.Create(2, "V001", "Otra Estación")); // mismo código, otra estación
        await db.SaveChangesAsync();

        var sut = new EmpleadoDirectorio(db);
        var dir = await sut.CargarAsync(new (int, string?)[] { (1, "  v001 ") });

        dir.Nombre(1, "V001").Should().Be("Juan Pérez");
        dir.Nombre(1, " v001 ").Should().Be("Juan Pérez");          // trim + case en la resolución
        dir.Nombre(2, "V001").Should().BeNull("no se cargó la clave de la estación 2");
    }

    [Fact]
    public async Task CargarAsync_SinMatchOCodigoVacio_DevuelveNull()
    {
        await using var db = NuevoContexto();
        db.Empleados.Add(Empleado.Create(1, "V001", "Juan"));
        await db.SaveChangesAsync();

        var sut = new EmpleadoDirectorio(db);
        var dir = await sut.CargarAsync(new (int, string?)[] { (1, "V999"), (1, null) });

        dir.Nombre(1, "V999").Should().BeNull();
        dir.Nombre(1, null).Should().BeNull();
    }

    [Fact]
    public async Task CargarAsync_SinClaves_DevuelveDirectorioVacio()
    {
        await using var db = NuevoContexto();
        var sut = new EmpleadoDirectorio(db);

        var dir = await sut.CargarAsync(Array.Empty<(int, string?)>());

        dir.Nombre(1, "V001").Should().BeNull();
    }

    [Fact]
    public async Task CargarAsync_ResuelvePorNucleoNumerico_FormatoDD_ContraCatalogo3Digitos()
    {
        // Caso real SanPio: la factura trae el despachador como DCTO.COD_VEND="DD0000010" pero el
        // catálogo VEND lo guarda como "010" → ambos deben resolver al mismo nombre (núcleo numérico).
        await using var db = NuevoContexto();
        db.Empleados.Add(Empleado.Create(15, "010", "CARLA VALAREZO"));
        db.Empleados.Add(Empleado.Create(15, "009", "NESTOR INTRIAGO"));
        db.Empleados.Add(Empleado.Create(15, "11", "SAN JACINTO"));
        await db.SaveChangesAsync();

        var sut = new EmpleadoDirectorio(db);
        var dir = await sut.CargarAsync(new (int, string?)[]
        {
            (15, "DD0000010"), (15, "DD0000009"), (15, "DD0000011"), (15, "DD0000077")
        });

        dir.Nombre(15, "DD0000010").Should().Be("CARLA VALAREZO");     // DD0000010 → núcleo 10 → catálogo "010"
        dir.Nombre(15, "DD0000009").Should().Be("NESTOR INTRIAGO");     // DD0000009 → núcleo 9 → catálogo "009"
        dir.Nombre(15, "DD0000011").Should().Be("SAN JACINTO");         // DD0000011 → núcleo 11 → catálogo "11"
        dir.Nombre(15, "DD0000077").Should().BeNull("no hay despachador 77 en el catálogo");
    }
}
