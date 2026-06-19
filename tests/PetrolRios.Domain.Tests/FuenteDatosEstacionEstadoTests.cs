using FluentAssertions;
using PetrolRios.Domain.Entities;

namespace PetrolRios.Domain.Tests;

public class FuenteDatosEstacionEstadoTests
{
    [Fact]
    public void Create_ConSincronizacionExitosa_RegistraFilasYUltimoExito()
    {
        var version = DateTime.UtcNow.AddMinutes(-1);

        var estado = FuenteDatosEstacionEstado.Create(
            4, 2, "Sincronizada", true, true, 12, 8, null, version);

        estado.Estado.Should().Be("Sincronizada");
        estado.FilasLeidas.Should().Be(12);
        estado.FilasEnviadas.Should().Be(8);
        estado.TotalFilasEnviadas.Should().Be(8);
        estado.UltimoError.Should().BeNull();
        estado.UltimoExito.Should().NotBeNull();
    }

    [Fact]
    public void Actualizar_AcumulaSoloFilasEnviadasYConservaErrorAccionable()
    {
        var version = DateTime.UtcNow;
        var estado = FuenteDatosEstacionEstado.Create(
            4, 2, "Sincronizada", true, true, 10, 4, null, version);

        estado.Actualizar(
            "WatermarkInvalido", true, false, 0, 0,
            "La columna FECHA no existe.", version);

        estado.TotalFilasEnviadas.Should().Be(4);
        estado.Estado.Should().Be("WatermarkInvalido");
        estado.ColumnaWatermarkValida.Should().BeFalse();
        estado.UltimoError.Should().Be("La columna FECHA no existe.");
    }

    [Fact]
    public void Create_ConEnvioPendiente_NoMarcaUltimoExito()
    {
        var estado = FuenteDatosEstacionEstado.Create(
            4, 2, "PendienteEnvio", true, true, 5, 0, null, DateTime.UtcNow);

        estado.UltimoExito.Should().BeNull();
    }
}
