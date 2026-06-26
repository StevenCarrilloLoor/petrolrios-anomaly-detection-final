using Cronos;

namespace PetrolRios.Application.Programacion;

/// <summary>
/// Decide si una regla "le toca" correr y calcula su PRÓXIMA ejecución según su
/// <see cref="ProgramacionEjecucion"/>. El modo Calendario delega en <b>Cronos</b> el cálculo de la
/// siguiente fecha (anclada al calendario, con soporte de "L" = último día del mes, años bisiestos y
/// zona horaria), así no reinventamos esa matemática. El modo Intervalo es "desde + N".
/// </summary>
public static class CalculadoraProgramacion
{
    /// <summary>
    /// ¿Debe correr la regla en este instante? "Cada ciclo" siempre; lo demás corre cuando ya llegó (o
    /// pasó) su <paramref name="proximaUtc"/>, o si aún no se ha calculado (null = primera vez).
    /// </summary>
    public static bool EstaPendiente(ProgramacionEjecucion prog, DateTime? proximaUtc, DateTime ahoraUtc)
    {
        if (prog.Modo == ModoProgramacion.CadaCiclo) return true;
        return proximaUtc is null || ahoraUtc >= proximaUtc.Value;
    }

    /// <summary>
    /// Próxima ejecución estrictamente DESPUÉS de <paramref name="desdeUtc"/> (en UTC). Para "cada ciclo"
    /// no aplica (null). Para Calendario se evalúa en la zona <paramref name="zona"/> (por defecto UTC)
    /// para que "día 29 a las 00:00" se entienda en ese reloj.
    /// </summary>
    public static DateTime? CalcularProxima(ProgramacionEjecucion prog, DateTime desdeUtc, TimeZoneInfo? zona = null)
    {
        var desde = DateTime.SpecifyKind(desdeUtc, DateTimeKind.Utc);
        switch (prog.Modo)
        {
            case ModoProgramacion.CadaCiclo:
                return null;

            case ModoProgramacion.Intervalo:
                var n = Math.Max(1, prog.IntervaloN);
                return prog.IntervaloUnidad switch
                {
                    UnidadIntervalo.Segundos => desde.AddSeconds(n),
                    UnidadIntervalo.Minutos => desde.AddMinutes(n),
                    UnidadIntervalo.Horas => desde.AddHours(n),
                    UnidadIntervalo.Dias => desde.AddDays(n),
                    UnidadIntervalo.Semanas => desde.AddDays(7 * n),
                    UnidadIntervalo.Meses => desde.AddMonths(n),
                    _ => desde.AddMinutes(n),
                };

            case ModoProgramacion.Calendario:
                try
                {
                    var expr = CronExpression.Parse(ACron(prog));
                    var prox = expr.GetNextOccurrence(desde, zona ?? TimeZoneInfo.Utc);
                    // Cronos solo devuelve null en cron imposibles (p. ej. "día 30 de febrero"); los
                    // que generamos siempre son alcanzables. Fallback defensivo por si acaso.
                    return prox ?? desde.AddDays(1);
                }
                catch
                {
                    return desde.AddDays(1);
                }

            default:
                return null;
        }
    }

    /// <summary>
    /// Ventana (en días) que una regla programada debería ANALIZAR cuando le toca, para no perderse
    /// transacciones llegadas entre corridas. Deriva de la cadencia (mensual ≈ 31, semanal = 7, etc.).
    /// Se usa para construir el contexto histórico de las reglas lentas.
    /// </summary>
    public static int DiasVentanaSugerida(ProgramacionEjecucion prog) => prog.Modo switch
    {
        ModoProgramacion.CadaCiclo => 0,
        ModoProgramacion.Intervalo => prog.IntervaloUnidad switch
        {
            UnidadIntervalo.Segundos or UnidadIntervalo.Minutos or UnidadIntervalo.Horas => 1,
            UnidadIntervalo.Dias => Math.Max(1, prog.IntervaloN),
            UnidadIntervalo.Semanas => Math.Max(1, prog.IntervaloN) * 7,
            UnidadIntervalo.Meses => Math.Max(1, prog.IntervaloN) * 31,
            _ => 1,
        },
        ModoProgramacion.Calendario => prog.CalendarioTipo switch
        {
            TipoCalendario.Diario => 1,
            TipoCalendario.Semanal => 7,
            TipoCalendario.Mensual => 31,
            _ => 1,
        },
        _ => 0,
    };

    private static string ACron(ProgramacionEjecucion p)
    {
        var min = Math.Clamp(p.Minuto, 0, 59);
        var h = Math.Clamp(p.Hora, 0, 23);
        return p.CalendarioTipo switch
        {
            TipoCalendario.Diario => $"{min} {h} * * *",
            TipoCalendario.Semanal => $"{min} {h} * * {Math.Clamp(p.DiaSemana, 0, 6)}",
            TipoCalendario.Mensual => p.UltimoDiaDelMes
                ? $"{min} {h} L * *"
                : $"{min} {h} {Math.Clamp(p.DiaMes, 1, 31)} * *",
            _ => $"{min} {h} * * *",
        };
    }
}
