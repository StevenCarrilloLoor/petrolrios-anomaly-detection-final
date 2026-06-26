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
        <div className="space-y-2 pl-1">
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

            {value.calendarioTipo === "Semanal" && (
              <>
                <span className="text-xs text-muted-foreground">el</span>
                <select
                  value={value.diaSemana}
                  onChange={(e) => set({ diaSemana: parseInt(e.target.value) })}
                  className={inputClass}
                >
                  {DIAS_SEMANA.map((d, i) => (
                    <option key={i} value={i}>
                      {d}
                    </option>
                  ))}
                </select>
              </>
            )}

            {value.calendarioTipo === "Mensual" && !value.ultimoDiaDelMes && (
              <>
                <span className="text-xs text-muted-foreground">el día</span>
                <input
                  type="number"
                  min={1}
                  max={31}
                  value={value.diaMes}
                  onChange={(e) =>
                    set({ diaMes: Math.min(31, Math.max(1, parseInt(e.target.value) || 1)) })
                  }
                  className={`${inputClass} w-20`}
                />
              </>
            )}
          </div>

          {value.calendarioTipo === "Mensual" && (
            <label className="flex cursor-pointer items-center gap-2 text-xs text-muted-foreground">
              <input
                type="checkbox"
                checked={value.ultimoDiaDelMes}
                onChange={(e) => set({ ultimoDiaDelMes: e.target.checked })}
                className="h-3.5 w-3.5 accent-primary"
              />
              Usar el <span className="font-medium text-foreground">último día del mes</span> (sirve
              para fin de mes: 28/29/30/31 según corresponda)
            </label>
          )}

          <div className="flex flex-wrap items-center gap-2 text-sm">
            <span className="text-xs text-muted-foreground">a las</span>
            <input
              type="number"
              min={0}
              max={23}
              value={value.hora}
              onChange={(e) =>
                set({ hora: Math.min(23, Math.max(0, parseInt(e.target.value) || 0)) })
              }
              className={`${inputClass} w-16`}
            />
            <span className="text-xs text-muted-foreground">:</span>
            <input
              type="number"
              min={0}
              max={59}
              value={value.minuto}
              onChange={(e) =>
                set({ minuto: Math.min(59, Math.max(0, parseInt(e.target.value) || 0)) })
              }
              className={`${inputClass} w-16`}
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
