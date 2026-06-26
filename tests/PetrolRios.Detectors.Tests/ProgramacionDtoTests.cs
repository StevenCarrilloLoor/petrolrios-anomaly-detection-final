using FluentAssertions;
using PetrolRios.Application.Programacion;

namespace PetrolRios.Detectors.Tests;

/// <summary>
/// Pruebas del DTO de programación de la API: que el ida-y-vuelta no pierda nada y que la validación
/// rechace lo que está fuera de las listas cerradas (modo/unidad/tipo) o de los rangos.
/// </summary>
public class ProgramacionDtoTests
{
    [Fact]
    public void DeJson_vacio_es_cada_ciclo()
    {
        var dto = ProgramacionDto.DeJson("");
        dto.Modo.Should().Be("CadaCiclo");
        dto.Descripcion.Should().Be("En cada ciclo del análisis");
    }

    [Fact]
    public void Intervalo_valido_va_y_vuelve_sin_perder_nada()
    {
        var original = new ProgramacionEjecucion
        {
            Modo = ModoProgramacion.Intervalo,
            IntervaloN = 3,
            IntervaloUnidad = UnidadIntervalo.Semanas
        };

        var dto = ProgramacionDto.De(original);
        dto.TryConvertir(out var prog, out var error).Should().BeTrue();
        error.Should().BeNull();
        prog.Should().Be(original);
    }

    [Fact]
    public void Calendario_mensual_dia_29_va_y_vuelve_sin_perder_nada()
    {
        var original = new ProgramacionEjecucion
        {
            Modo = ModoProgramacion.Calendario,
            CalendarioTipo = TipoCalendario.Mensual,
            DiaMes = 29,
            Hora = 8,
            Minuto = 30
        };

        var dto = ProgramacionDto.De(original);
        dto.TryConvertir(out var prog, out _).Should().BeTrue();
        prog.Should().Be(original);
    }

    [Theory]
    [InlineData("Xyz")]
    [InlineData("")]
    public void Modo_invalido_se_rechaza(string modo)
    {
        var dto = ProgramacionDto.CadaCiclo with { Modo = modo };
        dto.TryConvertir(out _, out var error).Should().BeFalse();
        error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Intervalo_cero_se_rechaza()
    {
        var dto = ProgramacionDto.CadaCiclo with { Modo = "Intervalo", IntervaloN = 0, IntervaloUnidad = "Minutos" };
        dto.TryConvertir(out _, out var error).Should().BeFalse();
        error.Should().Contain("al menos 1");
    }

    [Fact]
    public void Unidad_de_intervalo_invalida_se_rechaza()
    {
        var dto = ProgramacionDto.CadaCiclo with { Modo = "Intervalo", IntervaloN = 1, IntervaloUnidad = "Lustros" };
        dto.TryConvertir(out _, out var error).Should().BeFalse();
        error.Should().Contain("Unidad");
    }

    [Theory]
    [InlineData(25, 0)]
    [InlineData(0, 60)]
    public void Calendario_hora_o_minuto_fuera_de_rango_se_rechaza(int hora, int minuto)
    {
        var dto = ProgramacionDto.CadaCiclo with
        {
            Modo = "Calendario",
            CalendarioTipo = "Diario",
            Hora = hora,
            Minuto = minuto
        };
        dto.TryConvertir(out _, out var error).Should().BeFalse();
        error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Calendario_dia_del_mes_fuera_de_rango_se_rechaza()
    {
        var dto = ProgramacionDto.CadaCiclo with { Modo = "Calendario", CalendarioTipo = "Mensual", DiaMes = 32 };
        dto.TryConvertir(out _, out var error).Should().BeFalse();
        error.Should().Contain("día del mes");
    }

    [Fact]
    public void Calendario_ultimo_dia_del_mes_no_exige_dia_valido()
    {
        // Con "último día del mes" el DiaMes es irrelevante: no debe rechazarse aunque venga fuera de rango.
        var dto = ProgramacionDto.CadaCiclo with
        {
            Modo = "Calendario",
            CalendarioTipo = "Mensual",
            UltimoDiaDelMes = true,
            DiaMes = 99
        };
        dto.TryConvertir(out var prog, out _).Should().BeTrue();
        prog.UltimoDiaDelMes.Should().BeTrue();
    }

    [Fact]
    public void Modo_y_unidad_se_aceptan_sin_distinguir_mayusculas()
    {
        var dto = ProgramacionDto.CadaCiclo with { Modo = "intervalo", IntervaloN = 2, IntervaloUnidad = "horas" };
        dto.TryConvertir(out var prog, out _).Should().BeTrue();
        prog.Modo.Should().Be(ModoProgramacion.Intervalo);
        prog.IntervaloUnidad.Should().Be(UnidadIntervalo.Horas);
    }
}
