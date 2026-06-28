import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { estacionesService } from "@/services/estaciones.service";
import { consultasService, type LiquidacionFila } from "@/services/consultas.service";
import { Spinner } from "@/components/ui/Spinner";
import { EmptyState } from "@/components/ui/EmptyState";
import { Scale, Search, Printer, AlertTriangle, SearchX } from "lucide-react";

const inputClass =
  "rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40";
const dateInputClass = `${inputClass} w-full [color-scheme:dark]`;

function money(v: unknown): string {
  const n = typeof v === "number" ? v : Number(v);
  return Number.isFinite(n) ? `$${n.toFixed(2)}` : "—";
}
function texto(v: unknown): string {
  const s = v == null ? "" : String(v).trim();
  return s.length > 0 ? s : "—";
}
function fecha(v: unknown): string {
  if (!v) return "—";
  const d = new Date(String(v));
  return Number.isNaN(d.getTime()) ? String(v) : d.toLocaleString("es-EC");
}

interface Grupo {
  numeroLiquidacion: string;
  numeroTurno: string;
  fecha?: string;
  diferencia?: number;
  faltante?: number;
  sobrante?: number;
  facturas: LiquidacionFila[];
  total: number;
}

/**
 * Cuadre de liquidaciones (pedido de la auditora): dadas las liquidaciones de un período, qué facturas
 * componen cada una (LIQU.NUM_TURN ↔ DCTO.NUM_TURN). Lo que el sistema "no tenía en conjunto". En vivo,
 * solo lectura. Cada liquidación se muestra con sus facturas y el total facturado del turno.
 */
export function LiquidacionesPage() {
  const [estacion, setEstacion] = useState("");
  const [desde, setDesde] = useState("");
  const [hasta, setHasta] = useState("");

  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filas, setFilas] = useState<LiquidacionFila[] | null>(null);
  const [rango, setRango] = useState("");

  const { data: estaciones } = useQuery({
    queryKey: ["estaciones"],
    queryFn: estacionesService.getAll,
    staleTime: 5 * 60_000,
  });

  const buscar = async () => {
    if (!estacion) {
      setError("Elija una estación.");
      return;
    }
    setCargando(true);
    setError(null);
    setFilas(null);
    try {
      const r = await consultasService.consultarLiquidaciones(
        estacion,
        desde || null,
        hasta ? `${hasta}T23:59:59` : null,
        2000,
      );
      setFilas(r);
      setRango(desde || hasta ? `${desde || "inicio"} → ${hasta || "hoy"}` : "más recientes");
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : "No se pudo consultar las liquidaciones.");
    } finally {
      setCargando(false);
    }
  };

  // Agrupar las filas por liquidación (la consulta devuelve una fila por liquidación×factura).
  const grupos = useMemo<Grupo[]>(() => {
    const map = new Map<string, Grupo>();
    for (const r of filas ?? []) {
      const key = `${texto(r.NumeroLiquidacion)}-${texto(r.NumeroTurno)}`;
      let g = map.get(key);
      if (!g) {
        g = {
          numeroLiquidacion: texto(r.NumeroLiquidacion),
          numeroTurno: texto(r.NumeroTurno),
          fecha: r.FechaLiquidacion ? String(r.FechaLiquidacion) : undefined,
          diferencia: typeof r.Diferencia === "number" ? r.Diferencia : undefined,
          faltante: typeof r.Faltante === "number" ? r.Faltante : undefined,
          sobrante: typeof r.Sobrante === "number" ? r.Sobrante : undefined,
          facturas: [],
          total: 0,
        };
        map.set(key, g);
      }
      if (texto(r.NumeroDocumento) !== "—") {
        g.facturas.push(r);
        g.total += typeof r.TotalNeto === "number" ? r.TotalNeto : 0;
      }
    }
    return [...map.values()];
  }, [filas]);

  const totalGeneral = grupos.reduce((s, g) => s + g.total, 0);
  const totalFacturas = grupos.reduce((s, g) => s + g.facturas.length, 0);

  return (
    <div className="space-y-5 print:bg-white print:text-black">
      <div className="flex flex-wrap items-start justify-between gap-3 print:hidden">
        <div>
          <h1 className="flex items-center gap-2 text-2xl font-bold text-foreground">
            <Scale size={24} /> Cuadre de liquidaciones
          </h1>
          <p className="max-w-3xl text-sm text-muted-foreground">
            Dadas las liquidaciones de un período, qué facturas componen cada una (en vivo, solo lectura).
            Cruza la liquidación con las facturas de su turno para cuadrar el cierre.
          </p>
        </div>
        {filas && grupos.length > 0 && (
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
            <span className="text-xs font-medium text-muted-foreground">Desde</span>
            <input type="date" className={dateInputClass} value={desde} onChange={(e) => setDesde(e.target.value)} />
          </label>
          <label className="space-y-1">
            <span className="text-xs font-medium text-muted-foreground">Hasta</span>
            <input type="date" className={dateInputClass} value={hasta} onChange={(e) => setHasta(e.target.value)} />
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

      {filas && (
        <div className="hidden print:block">
          <h1 className="text-xl font-bold">PetrolRíos — Cuadre de liquidaciones ({estacion})</h1>
          <p className="text-xs">
            Período: {rango} · {grupos.length} liquidación(es) · {totalFacturas} factura(s) · total {money(totalGeneral)} ·
            generado {new Date().toLocaleString("es-EC")}
          </p>
          <hr className="my-2" />
        </div>
      )}

      {cargando && (
        <div className="flex flex-col items-center gap-2 py-12 text-muted-foreground">
          <Spinner size="lg" />
          <p className="text-sm">Consultando las liquidaciones en la estación en vivo…</p>
        </div>
      )}

      {error && !cargando && (
        <div className="flex items-start gap-2 rounded-lg border border-risk-high/40 bg-risk-high/10 p-3 text-sm text-risk-high print:hidden">
          <AlertTriangle size={18} className="mt-0.5 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      {filas && !cargando && grupos.length === 0 && (
        <div className="rounded-xl border border-border bg-card">
          <EmptyState
            icon={<SearchX size={40} />}
            title="Sin liquidaciones"
            description="No hay liquidaciones en ese período en la estación."
          />
        </div>
      )}

      {filas && !cargando && grupos.length > 0 && (
        <>
          <div className="flex flex-wrap items-center gap-2 text-sm text-muted-foreground print:hidden">
            <span>
              {grupos.length} liquidación(es) · {totalFacturas} factura(s) · total facturado{" "}
              <span className="font-semibold text-foreground">{money(totalGeneral)}</span>
            </span>
          </div>

          <div className="space-y-4">
            {grupos.map((g) => (
              <div key={`${g.numeroLiquidacion}-${g.numeroTurno}`} className="overflow-hidden rounded-xl border border-border bg-card print:border-black/30">
                <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border bg-muted/30 px-4 py-2.5 print:bg-white">
                  <div className="text-sm">
                    <span className="font-semibold text-foreground">Liquidación {g.numeroLiquidacion}</span>
                    <span className="text-muted-foreground"> · turno {g.numeroTurno} · {fecha(g.fecha)}</span>
                  </div>
                  <div className="flex flex-wrap items-center gap-3 text-xs">
                    {g.faltante ? <span className="text-risk-high">Faltante {money(g.faltante)}</span> : null}
                    {g.sobrante ? <span className="text-risk-medium">Sobrante {money(g.sobrante)}</span> : null}
                    <span className="text-muted-foreground">
                      {g.facturas.length} factura(s) · total{" "}
                      <span className="font-semibold text-foreground">{money(g.total)}</span>
                    </span>
                  </div>
                </div>
                {g.facturas.length === 0 ? (
                  <p className="px-4 py-3 text-sm text-muted-foreground">
                    Esta liquidación no tiene facturas FV asociadas a su turno.
                  </p>
                ) : (
                  <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-muted-foreground">
                          <th className="px-3 py-2">N.º factura</th>
                          <th className="px-3 py-2">Fecha</th>
                          <th className="px-3 py-2">Cliente</th>
                          <th className="px-3 py-2">RUC</th>
                          <th className="px-3 py-2">Placa</th>
                          <th className="px-3 py-2">Despachador</th>
                          <th className="px-3 py-2">Pago</th>
                          <th className="px-3 py-2 text-right">Total</th>
                        </tr>
                      </thead>
                      <tbody>
                        {g.facturas.map((f, i) => (
                          <tr key={`${f.NumeroDocumento}-${i}`} className="border-b border-border/60 odd:bg-muted/20">
                            <td className="px-3 py-1.5 font-mono">{texto(f.NumeroDocumento)}</td>
                            <td className="whitespace-nowrap px-3 py-1.5">{fecha(f.FechaDocumento)}</td>
                            <td className="px-3 py-1.5">{texto(f.Cliente)}</td>
                            <td className="px-3 py-1.5 font-mono">{texto(f.Ruc)}</td>
                            <td className="px-3 py-1.5 font-mono">{texto(f.Placa)}</td>
                            <td className="px-3 py-1.5 font-mono">{texto(f.Vendedor)}</td>
                            <td className="px-3 py-1.5">{texto(f.FormaPago)}</td>
                            <td className="px-3 py-1.5 text-right font-medium">{money(f.TotalNeto)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            ))}
          </div>
        </>
      )}
    </div>
  );
}
