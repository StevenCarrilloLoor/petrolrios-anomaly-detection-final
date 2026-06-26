import type { ProgramacionDto, UnidadIntervalo } from "@/types/regla";

/** Programación por defecto: "en cada ciclo del análisis". */
export const PROGRAMACION_CADA_CICLO: ProgramacionDto = {
  modo: "CadaCiclo",
  intervaloN: 1,
  intervaloUnidad: "Minutos",
  calendarioTipo: "Diario",
  hora: 0,
  minuto: 0,
  diaSemana: 1,
  diaMes: 1,
  ultimoDiaDelMes: false,
  descripcion: "En cada ciclo del análisis",
};

export const UNIDADES: { v: UnidadIntervalo; l: string }[] = [
  { v: "Segundos", l: "segundos" },
  { v: "Minutos", l: "minutos" },
  { v: "Horas", l: "horas" },
  { v: "Dias", l: "días" },
  { v: "Semanas", l: "semanas" },
  { v: "Meses", l: "meses" },
];

export const DIAS_SEMANA = [
  "domingo",
  "lunes",
  "martes",
  "miércoles",
  "jueves",
  "viernes",
  "sábado",
];

function unidadSingularPlural(u: UnidadIntervalo, n: number): string {
  const par: Record<UnidadIntervalo, [string, string]> = {
    Segundos: ["segundo", "segundos"],
    Minutos: ["minuto", "minutos"],
    Horas: ["hora", "horas"],
    Dias: ["día", "días"],
    Semanas: ["semana", "semanas"],
    Meses: ["mes", "meses"],
  };
  return n === 1 ? par[u][0] : par[u][1];
}

/** Texto legible en español de una programación (espejo de ProgramacionEjecucion.Descripcion del backend). */
export function describirProgramacion(p: ProgramacionDto): string {
  if (p.modo === "Intervalo") {
    const n = Math.max(1, p.intervaloN);
    return `Cada ${n} ${unidadSingularPlural(p.intervaloUnidad, n)}`;
  }
  if (p.modo === "Calendario") {
    const hhmm = `${String(p.hora).padStart(2, "0")}:${String(p.minuto).padStart(2, "0")}`;
    if (p.calendarioTipo === "Diario") return `Todos los días a las ${hhmm}`;
    if (p.calendarioTipo === "Semanal")
      return `Cada ${DIAS_SEMANA[((p.diaSemana % 7) + 7) % 7]} a las ${hhmm}`;
    return p.ultimoDiaDelMes
      ? `El último día de cada mes a las ${hhmm}`
      : `El día ${Math.min(31, Math.max(1, p.diaMes))} de cada mes a las ${hhmm}`;
  }
  return "En cada ciclo del análisis";
}

/** Formatea una fecha ISO (UTC) a hora local legible; "—" si es null. */
export function formatProxima(iso: string | null): string {
  if (!iso) return "—";
  const d = new Date(iso);
  if (isNaN(d.getTime())) return "—";
  return d.toLocaleString("es-EC", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}
