using FluentAssertions;
using PetrolRios.Application.Programacion;
using Xunit;

namespace PetrolRios.Detectors.Tests;

/// <summary>
/// El cerebro de la "frecuencia/calendario por regla": cálculo de próxima ejecución (Intervalo simple +
/// Calendario anclado vía Cronos) y la decisión "¿le toca?". Cubre el caso fino del ingeniero: mensual
/// anclado (día 29 / último día) que NO se desfasa si el sistema se instala a mitad de mes.
/// </summary>
public class CalculadoraProgramacionTests
{
    private static DateTime Utc(int y, int mo, int d, int h = 0, int mi = 0) =>
        new(y, mo, d, h, mi, 0, DateTimeKind.Utc);

    // ─────────── Modo Intervalo ───────────

    [Theory]
    [InlineData(UnidadIntervalo.Segundos, 30)]
    [InlineData(UnidadIntervalo.Minutos, 5)]
    [InlineData(UnidadIntervalo.Horas, 2)]
    [InlineData(UnidadIntervalo.Dias, 3)]
    [InlineData(UnidadIntervalo.Semanas, 2)]
    [InlineData(UnidadIntervalo.Meses, 1)]
    public void Intervalo_suma_la_unidad_correcta(UnidadIntervalo unidad, int n)
    {
        var desde = Utc(2026, 6, 15, 10, 0);
        var prog = new ProgramacionEjecucion { Modo = ModoProgramacion.Intervalo, IntervaloN = n, IntervaloUnidad = unidad };

        var prox = CalculadoraProgramacion.CalcularProxima(prog, desde)!.Value;

        var esperado = unidad switch
        {
            UnidadIntervalo.Segundos => desde.AddSeconds(n),
            UnidadIntervalo.Minutos => desde.AddMinutes(n),
            UnidadIntervalo.Horas => desde.AddHours(n),
            UnidadIntervalo.Dias => desde.AddDays(n),
            UnidadIntervalo.Semanas => desde.AddDays(7 * n),
            UnidadIntervalo.Meses => desde.AddMonths(n),
            _ => desde,
        };
        prox.Should().Be(esperado);
    }

    // ─────────── ¿Le toca? ───────────

    [Fact]
    public void CadaCiclo_siempre_pendiente()
    {
        CalculadoraProgramacion.EstaPendiente(ProgramacionEjecucion.CadaCiclo, Utc(2026, 1, 1), Utc(2026, 1, 1))
            .Should().BeTrue();
    }

    [Fact]
    public void Pendiente_si_proxima_es_null_o_ya_paso_y_no_si_es_futura()
    {
        var prog = new ProgramacionEjecucion { Modo = ModoProgramacion.Intervalo };
        var ahora = Utc(2026, 6, 15, 12, 0);

        CalculadoraProgramacion.EstaPendiente(prog, null, ahora).Should().BeTrue();           // primera vez
        CalculadoraProgramacion.EstaPendiente(prog, ahora.AddHours(-1), ahora).Should().BeTrue();  // ya pasó
        CalculadoraProgramacion.EstaPendiente(prog, ahora.AddHours(1), ahora).Should().BeFalse(); // aún no
    }

    // ─────────── Modo Calendario (anclado, vía Cronos) ───────────

    [Fact]
    public void Mensual_dia_29_es_el_29_del_mes_que_corresponde()
    {
        var prog = new ProgramacionEjecucion { Modo = ModoProgramacion.Calendario, CalendarioTipo = TipoCalendario.Mensual, DiaMes = 29 };
        // El caso del ingeniero: el 29 de junio, no "30 días después".
        var prox = CalculadoraProgramacion.CalcularProxima(prog, Utc(2026, 6, 15))!.Value;
        prox.Should().Be(Utc(2026, 6, 29));
    }

    [Fact]
    public void Mensual_NO_se_desfasa_si_se_instala_a_mitad_de_mes()
    {
        // Anclado al día 15: instalando el 20 de enero, la próxima es el 15 de FEBRERO (no el 19 = +30 días).
        var prog = new ProgramacionEjecucion { Modo = ModoProgramacion.Calendario, CalendarioTipo = TipoCalendario.Mensual, DiaMes = 15 };
        var prox = CalculadoraProgramacion.CalcularProxima(prog, Utc(2026, 1, 20))!.Value;
        prox.Day.Should().Be(15);
        prox.Should().Be(Utc(2026, 2, 15));
        // Y la siguiente, otra vez anclada al 15 (incremental).
        CalculadoraProgramacion.CalcularProxima(prog, prox)!.Value.Should().Be(Utc(2026, 3, 15));
    }

    [Fact]
    public void Mensual_ultimo_dia_usa_fin_de_mes_real_incluido_febrero()
    {
        var prog = new ProgramacionEjecucion { Modo = ModoProgramacion.Calendario, CalendarioTipo = TipoCalendario.Mensual, UltimoDiaDelMes = true };
        // 2026 no es bisiesto → febrero termina el 28.
        CalculadoraProgramacion.CalcularProxima(prog, Utc(2026, 2, 10))!.Value.Should().Be(Utc(2026, 2, 28));
        // El siguiente último día tras el 28 de feb es el 31 de marzo.
        CalculadoraProgramacion.CalcularProxima(prog, Utc(2026, 2, 28))!.Value.Should().Be(Utc(2026, 3, 31));
    }

    [Fact]
    public void Semanal_cae_en_el_dia_de_la_semana_pedido()
    {
        var prog = new ProgramacionEjecucion { Modo = ModoProgramacion.Calendario, CalendarioTipo = TipoCalendario.Semanal, DiaSemana = 1 }; // lunes
        var prox = CalculadoraProgramacion.CalcularProxima(prog, Utc(2026, 1, 1))!.Value; // 2026-01-01 es jueves
        prox.DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void Diario_a_la_hora_pedida()
    {
        var prog = new ProgramacionEjecucion { Modo = ModoProgramacion.Calendario, CalendarioTipo = TipoCalendario.Diario, Hora = 8 };
        CalculadoraProgramacion.CalcularProxima(prog, Utc(2026, 1, 1, 6, 0))!.Value.Should().Be(Utc(2026, 1, 1, 8, 0));
        CalculadoraProgramacion.CalcularProxima(prog, Utc(2026, 1, 1, 9, 0))!.Value.Should().Be(Utc(2026, 1, 2, 8, 0));
    }

    // ─────────── Serialización (value object → JSON → value object) ───────────

    [Fact]
    public void Serializa_y_relee_sin_perder_nada()
    {
        var prog = new ProgramacionEjecucion
        {
            Modo = ModoProgramacion.Calendario, CalendarioTipo = TipoCalendario.Mensual, DiaMes = 29, Hora = 6,
        };
        ProgramacionEjecucion.Leer(prog.Serializar()).Should().Be(prog);
        ProgramacionEjecucion.Leer(null).Should().Be(ProgramacionEjecucion.CadaCiclo);
        ProgramacionEjecucion.Leer("basura{").Should().Be(ProgramacionEjecucion.CadaCiclo);
    }

    [Fact]
    public void Descripcion_legible_en_espanol()
    {
        new ProgramacionEjecucion { Modo = ModoProgramacion.Intervalo, IntervaloN = 5, IntervaloUnidad = UnidadIntervalo.Minutos }
            .Descripcion().Should().Be("Cada 5 minutos");
        new ProgramacionEjecucion { Modo = ModoProgramacion.Calendario, CalendarioTipo = TipoCalendario.Mensual, DiaMes = 29 }
            .Descripcion().Should().Contain("día 29");
        new ProgramacionEjecucion { Modo = ModoProgramacion.Calendario, CalendarioTipo = TipoCalendario.Mensual, UltimoDiaDelMes = true }
            .Descripcion().Should().Contain("último día");
        ProgramacionEjecucion.CadaCiclo.Descripcion().Should().Contain("cada ciclo");
    }
}
