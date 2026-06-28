import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { estacionesService } from "@/services/estaciones.service";
import { esquemaService } from "@/services/esquema.service";
import { consultasService } from "@/services/consultas.service";
import { Spinner } from "@/components/ui/Spinner";
import { EmptyState } from "@/components/ui/EmptyState";
import { Table2, Search, SearchX, AlertTriangle, Printer } from "lucide-react";

const inputClass =
  "rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40";

/** Formatea un valor crudo de Firebird para mostrarlo en la celda. */
function celda(v: unknown): string {
  if (v === null || v === undefined) return "—";
  if (typeof v === "number") return Number.isInteger(v) ? String(v) : String(v);
  if (typeof v === "boolean") return v ? "Sí" : "No";
  const s = String(v).trim();
  if (s.length === 0) return "—";
  // Fechas ISO → legible
  if (/^\d{4}-\d{2}-\d{2}T/.test(s)) {
    const d = new Date(s);
    if (!Number.isNaN(d.getTime())) return d.toLocaleString("es-EC");
  }
  return s;
}

/**
 * Explorador GENÉRICO de cualquier tabla de la estación (solo lectura, en vivo). El agente valida que la
 * tabla exista y devuelve sus filas con TODAS las columnas; aquí se renderizan con columnas DINÁMICAS
 * (auto-estructuradas a partir de los datos). Es la prueba de que el agente envía info de forma dinámica:
 * sin cablear nada, se puede consultar una tabla "X" cualquiera y verla bien estructurada.
 */
export function ExplorarTablaPage() {
  const [estacion, setEstacion] = useState("");
  const [tabla, setTabla] = useState("");
  const [limite, setLimite] = useState(100);

  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [rows, setRows] = useState<Record<string, unknown>[] | null>(null);
  const [tablaConsultada, setTablaConsultada] = useState("");

  const { data: estaciones } = useQuery({
    queryKey: ["estaciones"],
    queryFn: estacionesService.getAll,
    staleTime: 5 * 60_000,
  });

  // Sugerencias de tablas del esquema reportado por los agentes (datalist). Si el rol no puede verlas
  // (solo Supervisor/Admin) o no hay esquema, la lista queda vacía y el campo libre sigue funcionando.
  const { data: tablas } = useQuery({
    queryKey: ["esquema", "tablas", tabla],
    queryFn: () => esquemaService.buscarTablas(tabla.trim()),
    enabled: tabla.trim().length >= 2,
    retry: false,
    staleTime: 60_000,
  });

  const buscar = async () => {
    if (!estacion) {
      setError("Elija una estación.");
      return;
    }
    if (!tabla.trim()) {
      setError("Escriba el nombre de una tabla.");
      return;
    }
    setCargando(true);
    setError(null);
    setRows(null);
    try {
      const filas = await consultasService.consultarTabla(estacion, tabla, Math.min(Math.max(limite, 1), 1000));
      setRows(filas);
      setTablaConsultada(tabla.trim().toUpperCase());
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : "No se pudo consultar la tabla.");
    } finally {
      setCargando(false);
    }
  };

  // Columnas dinámicas: unión de las claves de todas las filas, en el orden de aparición.
  const columnas = useMemo(() => {
    if (!rows || rows.length === 0) return [];
    const orden: string[] = [];
    const vistos = new Set<string>();
    for (const r of rows) {
      for (const k of Object.keys(r)) {
        if (!vistos.has(k)) {
          vistos.add(k);
          orden.push(k);
        }
      }
    }
    return orden;
  }, [rows]);

  return (
    <div className="space-y-5 print:bg-white print:text-black">
      <div className="flex flex-wrap items-start justify-between gap-3 print:hidden">
        <div>
          <h1 className="flex items-center gap-2 text-2xl font-bold text-foreground">
            <Table2 size={24} /> Explorar una tabla
          </h1>
          <p className="max-w-3xl text-sm text-muted-foreground">
            Consulta EN VIVO cualquier tabla de la base de la estación (solo lectura). Las columnas se
            estructuran solas a partir de los datos. Útil para revisar una tabla "X" sin conocer su forma.
          </p>
        </div>
        {rows && rows.length > 0 && (
          <button
            onClick={() => window.print()}
            className="inline-flex shrink-0 items-center gap-2 rounded-md border border-border px-3 py-2 text-sm font-medium hover:bg-muted"
          >
            <Printer size={16} /> Imprimir
          </button>
        )}
      </div>

      <div className="rounded-xl border border-border bg-card p-4 print:hidden">
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <label className="space-y-1">
            <span className="text-xs font-medium text-muted-foreground">Estación</span>
            <select className={`${inputClass} w-full`} value={estacion} onChange={(e) => setEstacion(e.target.value)}>
              <option value="">Elija una estación…</option>
              {(estaciones ?? []).map((es) => (
                <option key={es.codigo} value={es.codigo}>
                  {es.nombre} ({es.codigo})
                </option>
              ))}
            </select>
          </label>
          <label className="space-y-1">
            <span className="text-xs font-medium text-muted-foreground">Tabla</span>
            <input
              className={`${inputClass} w-full font-mono`}
              value={tabla}
              onChange={(e) => setTabla(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && buscar()}
              placeholder="p. ej. TURN, LIQU, CRED_CABE…"
              list="tablas-sugeridas"
              autoCapitalize="characters"
            />
            <datalist id="tablas-sugeridas">
              {(tablas ?? []).map((t) => (
                <option key={t.tabla} value={t.tabla}>
                  {t.tabla} ({t.columnas} columnas)
                </option>
              ))}
            </datalist>
          </label>
          <label className="space-y-1">
            <span className="text-xs font-medium text-muted-foreground">Límite de filas</span>
            <input
              type="number"
              min={1}
              max={1000}
              className={`${inputClass} w-full`}
              value={limite}
              onChange={(e) => setLimite(Number(e.target.value) || 100)}
            />
          </label>
          <div className="flex items-end">
            <button
              onClick={buscar}
              disabled={cargando}
              className="inline-flex w-full items-center justify-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary/90 disabled:opacity-60"
            >
              <Search size={16} /> {cargando ? "Consultando…" : "Buscar"}
            </button>
          </div>
        </div>
      </div>

      {/* Encabezado solo-impresión */}
      {rows && (
        <div className="hidden print:block">
          <h1 className="text-xl font-bold">PetrolRíos — Tabla {tablaConsultada} ({estacion})</h1>
          <p className="text-xs">{rows.length} fila(s) · generado {new Date().toLocaleString("es-EC")}</p>
          <hr className="my-2" />
        </div>
      )}

      {cargando && (
        <div className="flex flex-col items-center gap-2 py-12 text-muted-foreground">
          <Spinner size="lg" />
          <p className="text-sm">Consultando la tabla en la estación en vivo…</p>
        </div>
      )}

      {error && !cargando && (
        <div className="flex items-start gap-2 rounded-lg border border-risk-high/40 bg-risk-high/10 p-3 text-sm text-risk-high print:hidden">
          <AlertTriangle size={18} className="mt-0.5 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      {rows && !cargando && (
        <div className="rounded-xl border border-border bg-card print:border-0">
          <div className="border-b border-border px-4 py-2.5 text-sm text-muted-foreground print:hidden">
            <span className="font-mono font-medium text-foreground">{tablaConsultada}</span> · {rows.length} fila(s) ·{" "}
            {columnas.length} columna(s)
          </div>
          {rows.length === 0 ? (
            <EmptyState
              icon={<SearchX size={40} />}
              title="Sin filas"
              description="La tabla existe pero no devolvió filas con ese límite."
            />
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-xs">
                <thead>
                  <tr className="border-b border-border text-left uppercase tracking-wide text-muted-foreground">
                    {columnas.map((c) => (
                      <th key={c} className="whitespace-nowrap px-3 py-2 font-medium">
                        {c}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {rows.map((r, i) => (
                    <tr key={i} className="border-b border-border/60 odd:bg-muted/20 hover:bg-muted/40">
                      {columnas.map((c) => (
                        <td key={c} className="max-w-[24rem] truncate whitespace-nowrap px-3 py-1.5 font-mono" title={celda(r[c])}>
                          {celda(r[c])}
                        </td>
                      ))}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
