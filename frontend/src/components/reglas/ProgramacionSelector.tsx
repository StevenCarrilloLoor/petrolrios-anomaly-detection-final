import type { ReactNode } from "react";
import type {
  ProgramacionDto,
  TipoCalendario,
  UnidadIntervalo,
} from "@/types/regla";
import { Infinity as InfinityIcon, Repeat, CalendarDays } from "lucide-react";
import {
  UNIDADES,
  DIAS_SEMANA,
  describirProgramacion,
} from "@/lib/programacion";

const inputClass =
  "rounded-md border border-border bg-background px-2.5 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40";

/**
 * Selector reutilizable de la cadencia de una regla: "cada ciclo", "cada N seg/min/h/días/sem/meses"
 * (intervalo) o anclado a un calendario (diario / semanal / mensual día-D o último día). Controlado:
 * recibe el valor y emite el nuevo en cada cambio, recalculando la descripción legible.
 */
export function ProgramacionSelector({
  value,
  onChange,
}: {
  value: ProgramacionDto;
  onChange: (p: ProgramacionDto) => void;
}) {
  const set = (patch: Partial<ProgramacionDto>) => {
    const next = { ...value, ...patch };
    onChange({ ...next, descripcion: describirProgramacion(next) });
  };

  const modoBtn = (modo: ProgramacionDto["modo"], icon: ReactNode, titulo: string) => (
    <button
      type="button"
      onClick={() => set({ modo })}
      className={`flex flex-1 items-center justify-center gap-1.5 rounded-md px-3 py-2 text-sm font-medium transition-colors ${
        value.modo === modo
          ? "bg-primary text-primary-foreground"
          : "text-muted-foreground hover:bg-muted"
      }`}
    >
      {icon}
      {titulo}
    </button>
  );

  return (
    <div className="space-y-3">
      <div className="flex flex-col gap-2 rounded-lg border border-border bg-background p-1 sm:flex-row">
        {modoBtn("CadaCiclo", <InfinityIcon size={15} />, "En cada ciclo")}
        {modoBtn("Intervalo", <Repeat size={15} />, "Cada cierto tiempo")}
        {modoBtn("Calendario", <CalendarDays size={15} />, "En un calendario")}
      </div>

      {value.modo === "Intervalo" && (
        <div className="flex flex-wrap items-center gap-2 pl-1 text-sm">
          <span className="text-xs text-muted-foreground">Ejecutar cada</span>
          <input
            type="number"
            min={1}
            value={value.intervaloN}
            onChange={(e) => set({ intervaloN: Math.max(1, parseInt(e.target.value) || 1) })}
            className={`${inputClass} w-20`}
          />
          <select
            value={value.intervaloUnidad}
            onChange={(e) => set({ intervaloUnidad: e.target.value as UnidadIntervalo })}
            className={inputClass}
          >
            {UNIDADES.map((u) => (
              <option key={u.v} value={u.v}>
                {u.l}
              </option>
            ))}
          </select>
        </div>
      )}

      {value.modo === "Calendario" && (
        <div className="space-y-3 pl-1">
          <div className="flex flex-wrap items-center gap-2 text-sm">
            <span className="text-xs text-muted-foreground">Repetir</span>
            <select
              value={value.calendarioTipo}
              onChange={(e) => set({ calendarioTipo: e.target.value as TipoCalendario })}
              className={inputClass}
            >
              <option value="Diario">Todos los días</option>
              <option value="Semanal">Cada semana</option>
              <option value="Mensual">Cada mes</option>
            </select>
          </div>

          {/* Semanal: día de la semana como pastillas (más claro que un desplegable). */}
          {value.calendarioTipo === "Semanal" && (
            <div>
              <p className="mb-1 text-xs text-muted-foreground">El día de la semana</p>
              <div className="flex flex-wrap gap-1">
                {DIAS_SEMANA.map((d, i) => (
                  <button
                    key={i}
                    type="button"
                    onClick={() => set({ diaSemana: i })}
                    title={d}
                    className={`h-9 w-12 rounded-md border text-xs font-medium capitalize transition-colors ${
                      value.diaSemana === i
                        ? "border-primary bg-primary text-primary-foreground"
                        : "border-border text-muted-foreground hover:bg-muted"
                    }`}
                  >
                    {d.slice(0, 3)}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Mensual: calendario de días 1–31 (clic para elegir) + botón "último día del mes". */}
          {value.calendarioTipo === "Mensual" && (
            <div>
              <p className="mb-1 text-xs text-muted-foreground">El día del mes</p>
              <div className="grid grid-cols-7 gap-1">
                {Array.from({ length: 31 }, (_, i) => i + 1).map((d) => {
                  const sel = !value.ultimoDiaDelMes && value.diaMes === d;
                  return (
                    <button
                      key={d}
                      type="button"
                      onClick={() => set({ diaMes: d, ultimoDiaDelMes: false })}
                      className={`flex h-9 items-center justify-center rounded-md border text-sm transition-colors ${
                        sel
                          ? "border-primary bg-primary font-semibold text-primary-foreground"
                          : "border-border text-foreground hover:bg-muted"
                      }`}
                    >
                      {d}
                    </button>
                  );
                })}
              </div>
              <button
                type="button"
                onClick={() => set({ ultimoDiaDelMes: !value.ultimoDiaDelMes })}
                className={`mt-1.5 flex w-full items-center justify-center gap-1.5 rounded-md border px-3 py-2 text-xs font-medium transition-colors ${
                  value.ultimoDiaDelMes
                    ? "border-primary bg-primary/15 text-primary"
                    : "border-border text-muted-foreground hover:bg-muted"
                }`}
              >
                <CalendarDays size={13} /> Último día del mes (28/29/30/31 según corresponda)
              </button>
            </div>
          )}

          {/* Hora: selector nativo (reloj del navegador), más intuitivo que dos cajas de número. */}
          <div className="flex flex-wrap items-center gap-2 text-sm">
            <span className="text-xs text-muted-foreground">a las</span>
            <input
              type="time"
              value={`${String(value.hora).padStart(2, "0")}:${String(value.minuto).padStart(2, "0")}`}
              onChange={(e) => {
                const [h, m] = e.target.value.split(":");
                set({
                  hora: Math.min(23, Math.max(0, parseInt(h) || 0)),
                  minuto: Math.min(59, Math.max(0, parseInt(m) || 0)),
                });
              }}
              className={inputClass}
            />
            <span className="text-[11px] text-muted-foreground">(hora de la estación, UTC-5)</span>
          </div>
        </div>
      )}

      <p className="rounded-md bg-primary/10 px-3 py-1.5 text-xs font-medium text-primary">
        ▶ {describirProgramacion(value)}
      </p>
    </div>
  );
}
