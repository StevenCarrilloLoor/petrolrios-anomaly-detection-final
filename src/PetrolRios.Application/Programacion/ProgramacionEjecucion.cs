using System.Text.Json;
using System.Text.Json.Serialization;

namespace PetrolRios.Application.Programacion;

/// <summary>Cómo se decide cuándo corre una regla.</summary>
public enum ModoProgramacion
{
    /// <summary>En cada ciclo del job (comportamiento clásico).</summary>
    CadaCiclo,
    /// <summary>Cada N unidades de tiempo desde la última ejecución (estilo "SimpleTrigger").</summary>
    Intervalo,
    /// <summary>En fechas de calendario ancladas (estilo "CronTrigger"): diario/semanal/mensual.</summary>
    Calendario,
}

/// <summary>Unidad del modo Intervalo.</summary>
public enum UnidadIntervalo { Segundos, Minutos, Horas, Dias, Semanas, Meses }

/// <summary>Tipo del modo Calendario.</summary>
public enum TipoCalendario { Diario, Semanal, Mensual }

/// <summary>
/// Programación de ejecución de UNA regla. Dos modos (validados contra cómo lo hacen los schedulers
/// serios, p. ej. Quartz: SimpleTrigger vs CronTrigger):
///  • <see cref="ModoProgramacion.Intervalo"/>: "cada N segundos/minutos/horas/días/semanas/meses"
///    desde la última corrida. Para "cada tanto".
///  • <see cref="ModoProgramacion.Calendario"/>: anclado al calendario (día del mes, último día del mes,
///    día de la semana, hora). El cálculo de la próxima fecha lo hace Cronos (ver
///    <see cref="CalculadoraProgramacion"/>), que maneja "L" (último día), bisiestos y zona horaria.
/// Es un VALUE OBJECT: se serializa a JSON y se guarda en la regla. El default es "cada ciclo".
/// </summary>
public sealed record ProgramacionEjecucion
{
    public ModoProgramacion Modo { get; init; } = ModoProgramacion.CadaCiclo;

    // ── Modo Intervalo ──
    /// <summary>Cantidad (≥ 1).</summary>
    public int IntervaloN { get; init; } = 1;
    public UnidadIntervalo IntervaloUnidad { get; init; } = UnidadIntervalo.Minutos;

    // ── Modo Calendario ──
    public TipoCalendario CalendarioTipo { get; init; } = TipoCalendario.Diario;
    /// <summary>Hora del día (0–23) para diario/semanal/mensual.</summary>
    public int Hora { get; init; }
    /// <summary>Minuto (0–59).</summary>
    public int Minuto { get; init; }
    /// <summary>Día de la semana (0 = domingo … 6 = sábado) para semanal.</summary>
    public int DiaSemana { get; init; } = 1;
    /// <summary>Día del mes (1–31) para mensual. Si el mes no tiene ese día, Cronos lo salta; para
    /// "fin de mes" usa <see cref="UltimoDiaDelMes"/>.</summary>
    public int DiaMes { get; init; } = 1;
    /// <summary>Mensual: usar el ÚLTIMO día del mes (cron "L"), sin importar cuántos días tenga.</summary>
    public bool UltimoDiaDelMes { get; init; }

    public static ProgramacionEjecucion CadaCiclo => new();

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    /// <summary>Serializa a JSON (enums como texto, para que reordenarlos no rompa datos guardados).</summary>
    public string Serializar() => JsonSerializer.Serialize(this, JsonOpts);

    /// <summary>Lee una programación desde JSON; si viene vacía o inválida, devuelve "cada ciclo".</summary>
    public static ProgramacionEjecucion Leer(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return CadaCiclo;
        try { return JsonSerializer.Deserialize<ProgramacionEjecucion>(json, JsonOpts) ?? CadaCiclo; }
        catch { return CadaCiclo; }
    }

    /// <summary>Texto legible en español de la cadencia (para UI y logs).</summary>
    public string Descripcion()
    {
        switch (Modo)
        {
            case ModoProgramacion.Intervalo:
                var n = Math.Max(1, IntervaloN);
                var u = IntervaloUnidad switch
                {
                    UnidadIntervalo.Segundos => n == 1 ? "segundo" : "segundos",
                    UnidadIntervalo.Minutos => n == 1 ? "minuto" : "minutos",
                    UnidadIntervalo.Horas => n == 1 ? "hora" : "horas",
                    UnidadIntervalo.Dias => n == 1 ? "día" : "días",
                    UnidadIntervalo.Semanas => n == 1 ? "semana" : "semanas",
                    UnidadIntervalo.Meses => n == 1 ? "mes" : "meses",
                    _ => "minutos",
                };
                return $"Cada {n} {u}";
            case ModoProgramacion.Calendario:
                var hhmm = $"{Hora:00}:{Minuto:00}";
                return CalendarioTipo switch
                {
                    TipoCalendario.Diario => $"Todos los días a las {hhmm}",
                    TipoCalendario.Semanal => $"Cada {DiaSemanaNombre(DiaSemana)} a las {hhmm}",
                    TipoCalendario.Mensual => UltimoDiaDelMes
                        ? $"El último día de cada mes a las {hhmm}"
                        : $"El día {Math.Clamp(DiaMes, 1, 31)} de cada mes a las {hhmm}",
                    _ => $"En un calendario ({hhmm})",
                };
            default:
                return "En cada ciclo del análisis";
        }
    }

    private static string DiaSemanaNombre(int d) => (((d % 7) + 7) % 7) switch
    {
        0 => "domingo", 1 => "lunes", 2 => "martes", 3 => "miércoles",
        4 => "jueves", 5 => "viernes", 6 => "sábado", _ => "día",
    };
}
