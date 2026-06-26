namespace PetrolRios.Application.Programacion;

/// <summary>
/// DTO de la programación de una regla para la API (entrada y salida). Usa <b>strings</b> para los
/// enums (modo, unidad, tipo de calendario) para ser robusto ante cualquier configuración de JSON y
/// poder validarlos contra listas cerradas. En salida incluye además <see cref="Descripcion"/> (texto
/// legible en español) para que la interfaz lo muestre sin recalcularlo.
/// </summary>
public sealed record ProgramacionDto(
    string Modo,
    int IntervaloN,
    string IntervaloUnidad,
    string CalendarioTipo,
    int Hora,
    int Minuto,
    int DiaSemana,
    int DiaMes,
    bool UltimoDiaDelMes,
    string Descripcion)
{
    /// <summary>La programación por defecto: "en cada ciclo".</summary>
    public static ProgramacionDto CadaCiclo { get; } = De(ProgramacionEjecucion.CadaCiclo);

    /// <summary>Proyecta una <see cref="ProgramacionEjecucion"/> a su DTO (con la descripción legible).</summary>
    public static ProgramacionDto De(ProgramacionEjecucion p) => new(
        p.Modo.ToString(),
        p.IntervaloN,
        p.IntervaloUnidad.ToString(),
        p.CalendarioTipo.ToString(),
        p.Hora,
        p.Minuto,
        p.DiaSemana,
        p.DiaMes,
        p.UltimoDiaDelMes,
        p.Descripcion());

    /// <summary>Lee la programación guardada (JSON) y la proyecta a DTO; vacío/ inválido = "cada ciclo".</summary>
    public static ProgramacionDto DeJson(string? json) => De(ProgramacionEjecucion.Leer(json));

    /// <summary>
    /// Convierte el DTO a <see cref="ProgramacionEjecucion"/> validando contra <b>listas cerradas</b>
    /// (modo/unidad/tipo) y rangos (N≥1, hora 0–23, minuto 0–59, día semana 0–6, día mes 1–31). Si algo
    /// no es válido, devuelve false y un <paramref name="error"/> en español (para responder 400 limpio).
    /// </summary>
    public bool TryConvertir(out ProgramacionEjecucion prog, out string? error)
    {
        prog = ProgramacionEjecucion.CadaCiclo;
        error = null;

        if (!Enum.TryParse<ModoProgramacion>(Modo, ignoreCase: true, out var modo))
        {
            error = $"Modo de programación no válido: '{Modo}'.";
            return false;
        }

        switch (modo)
        {
            case ModoProgramacion.CadaCiclo:
                prog = ProgramacionEjecucion.CadaCiclo;
                return true;

            case ModoProgramacion.Intervalo:
                if (IntervaloN < 1)
                {
                    error = "El intervalo debe ser de al menos 1.";
                    return false;
                }
                if (!Enum.TryParse<UnidadIntervalo>(IntervaloUnidad, ignoreCase: true, out var unidad))
                {
                    error = $"Unidad de intervalo no válida: '{IntervaloUnidad}'.";
                    return false;
                }
                prog = new ProgramacionEjecucion
                {
                    Modo = ModoProgramacion.Intervalo,
                    IntervaloN = IntervaloN,
                    IntervaloUnidad = unidad
                };
                return true;

            case ModoProgramacion.Calendario:
                if (!Enum.TryParse<TipoCalendario>(CalendarioTipo, ignoreCase: true, out var tipo))
                {
                    error = $"Tipo de calendario no válido: '{CalendarioTipo}'.";
                    return false;
                }
                if (Hora is < 0 or > 23) { error = "La hora debe estar entre 0 y 23."; return false; }
                if (Minuto is < 0 or > 59) { error = "El minuto debe estar entre 0 y 59."; return false; }
                if (tipo == TipoCalendario.Semanal && DiaSemana is < 0 or > 6)
                {
                    error = "El día de la semana debe estar entre 0 (domingo) y 6 (sábado).";
                    return false;
                }
                if (tipo == TipoCalendario.Mensual && !UltimoDiaDelMes && DiaMes is < 1 or > 31)
                {
                    error = "El día del mes debe estar entre 1 y 31 (o usa 'último día del mes').";
                    return false;
                }
                prog = new ProgramacionEjecucion
                {
                    Modo = ModoProgramacion.Calendario,
                    CalendarioTipo = tipo,
                    Hora = Hora,
                    Minuto = Minuto,
                    DiaSemana = DiaSemana,
                    DiaMes = DiaMes,
                    UltimoDiaDelMes = UltimoDiaDelMes
                };
                return true;

            default:
                error = "Modo de programación no soportado.";
                return false;
        }
    }
}
