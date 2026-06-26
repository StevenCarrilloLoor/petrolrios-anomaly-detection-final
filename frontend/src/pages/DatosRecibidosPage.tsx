import { useState, Fragment } from "react";
import { useQuery, keepPreviousData } from "@tanstack/react-query";
import { datosRecibidosService } from "@/services/datosRecibidos.service";
import { estacionesService } from "@/services/estaciones.service";
import { Skeleton } from "@/components/ui/Skeleton";
import { EmptyState } from "@/components/ui/EmptyState";
import {
  ChevronLeft,
  ChevronRight,
  FilterX,
  SearchX,
  Database,
  ChevronDown,
  ChevronUp,
} from "lucide-react";

function formatearJson(raw: string): string {
  try {
    return JSON.stringify(JSON.parse(raw), null, 2);
  } catch {
    return raw;
  }
}

export function DatosRecibidosPage() {
  const [tipo, setTipo] = useState("");
  const [estacionId, setEstacionId] = useState("");
  const [procesada, setProcesada] = useState("");
  const [q, setQ] = useState("");
  const [page, setPage] = useState(1);
  const [expandida, setExpandida] = useState<number | null>(null);
  const pageSize = 50;

  const { data: tipos } = useQuery({
    queryKey: ["datos-recibidos", "tipos"],
    queryFn: datosRecibidosService.getTipos,
  });
  const { data: estaciones } = useQuery({
    queryKey: ["estaciones"],
    queryFn: estacionesService.getAll,
  });

  const { data, isLoading, isFetching } = useQuery({
    queryKey: ["datos-recibidos", { tipo, estacionId, procesada, q, page }],
    queryFn: () =>
      datosRecibidosService.getAll({
        tipo: tipo || undefined,
        estacionId: estacionId ? Number(estacionId) : undefined,
        procesada: procesada === "" ? undefined : procesada === "si",
        q: q || undefined,
        page,
        pageSize,
      }),
    placeholderData: keepPreviousData,
  });

  const hayFiltros = tipo || estacionId || procesada || q;
  const limpiar = () => {
    setTipo("");
    setEstacionId("");
    setProcesada("");
    setQ("");
    setPage(1);
  };
  const cambiar = (set: (v: string) => void) => (v: string) => {
    set(v);
    setPage(1);
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="flex items-center gap-2 text-2xl font-bold text-foreground">
          <Database size={24} /> Datos recibidos
        </h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Todo lo crudo que envían los agentes (sea anomalía o no), lo más reciente primero. Filtra por
          tipo para confirmar que una tabla del selector realmente está llegando al central.
          {data ? ` ${data.totalCount.toLocaleString("es-EC")} registros.` : ""}
        </p>
      </div>

      {/* Filtros */}
      <div className="flex flex-wrap items-center gap-3 rounded-xl border border-border bg-background p-4">
        <select
          value={tipo}
          onChange={(e) => cambiar(setTipo)(e.target.value)}
          className="rounded-md border border-border bg-background px-3 py-2 text-sm"
        >
          <option value="">Todos los tipos</option>
          {(tipos ?? []).map((t) => (
            <option key={t} value={t}>
              {t}
            </option>
          ))}
        </select>

        <select
          value={estacionId}
          onChange={(e) => cambiar(setEstacionId)(e.target.value)}
          className="rounded-md border border-border bg-background px-3 py-2 text-sm"
        >
          <option value="">Todas las estaciones</option>
          {(estaciones ?? []).map((e) => (
            <option key={e.id} value={e.id}>
              {e.codigo} — {e.nombre}
            </option>
          ))}
        </select>

        <select
          value={procesada}
          onChange={(e) => cambiar(setProcesada)(e.target.value)}
          className="rounded-md border border-border bg-background px-3 py-2 text-sm"
        >
          <option value="">Procesada y sin procesar</option>
          <option value="si">Solo procesadas</option>
          <option value="no">Solo sin procesar</option>
        </select>

        <input
          value={q}
          onChange={(e) => cambiar(setQ)(e.target.value)}
          placeholder="Buscar tipo (ej. Tanques, Factura)…"
          className="min-w-[220px] flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
        />

        {hayFiltros && (
          <button
            onClick={limpiar}
            className="flex items-center gap-1 rounded-md px-3 py-2 text-sm text-muted-foreground hover:text-foreground"
          >
            <FilterX size={16} /> Limpiar
          </button>
        )}
      </div>

      {isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: 8 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </div>
      ) : (
        <div className={`overflow-x-auto rounded-xl border border-border bg-background ${isFetching ? "opacity-70" : ""}`}>
          <table className="w-full text-sm">
            <thead className="bg-muted/60">
              <tr>
                {["#", "Tipo", "Estación", "Fecha original", "Estado", "Datos crudos"].map((h) => (
                  <th
                    key={h}
                    className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground"
                  >
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {data?.items.map((d) => (
                <Fragment key={d.id}>
                  <tr
                    onClick={() => setExpandida(expandida === d.id ? null : d.id)}
                    className="cursor-pointer border-t border-border transition-colors hover:bg-muted/50"
                  >
                    <td className="px-4 py-3 font-mono text-muted-foreground">{d.id}</td>
                    <td className="px-4 py-3 font-medium">{d.tipoTransaccion}</td>
                    <td className="px-4 py-3">
                      <span className="font-medium">{d.estacionNombre || `Estación ${d.estacionId}`}</span>
                      <span className="ml-1 font-mono text-xs text-muted-foreground">{d.estacionCodigo}</span>
                    </td>
                    <td className="whitespace-nowrap px-4 py-3 text-muted-foreground">
                      {new Date(d.fechaOriginal).toLocaleString("es-EC", { dateStyle: "short", timeStyle: "short" })}
                    </td>
                    <td className="px-4 py-3">
                      <span
                        className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                          d.procesada
                            ? "bg-green-500/15 text-green-600"
                            : "bg-yellow-500/15 text-yellow-600"
                        }`}
                      >
                        {d.procesada ? "Procesada" : "Sin procesar"}
                      </span>
                    </td>
                    <td className="max-w-md px-4 py-3">
                      <span className="flex items-center gap-1.5 text-muted-foreground">
                        {expandida === d.id ? <ChevronUp size={14} /> : <ChevronDown size={14} />}
                        <span className="truncate font-mono text-xs" title={d.dataJson}>
                          {d.dataJson}
                        </span>
                      </span>
                    </td>
                  </tr>
                  {expandida === d.id && (
                    <tr className="border-t border-border bg-muted/30">
                      <td colSpan={6} className="px-4 py-3">
                        <pre className="max-h-80 overflow-auto whitespace-pre-wrap break-all rounded-lg bg-background p-3 font-mono text-xs">
                          {formatearJson(d.dataJson)}
                        </pre>
                      </td>
                    </tr>
                  )}
                </Fragment>
              ))}
              {data?.items.length === 0 && (
                <tr>
                  <td colSpan={6}>
                    <EmptyState
                      icon={<SearchX size={40} />}
                      title="Sin datos recibidos"
                      description="Ajusta los filtros, o espera a que un agente envíe transacciones de ese tipo."
                    />
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between text-sm text-muted-foreground">
          <span>
            Página {data.page} de {data.totalPages}
          </span>
          <div className="flex gap-2">
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={!data.hasPreviousPage}
              className="flex items-center gap-1 rounded-md border border-border px-3 py-1.5 disabled:opacity-40"
            >
              <ChevronLeft size={16} /> Anterior
            </button>
            <button
              onClick={() => setPage((p) => p + 1)}
              disabled={!data.hasNextPage}
              className="flex items-center gap-1 rounded-md border border-border px-3 py-1.5 disabled:opacity-40"
            >
              Siguiente <ChevronRight size={16} />
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
