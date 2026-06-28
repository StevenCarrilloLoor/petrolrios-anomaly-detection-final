import { useQuery } from "@tanstack/react-query";
import { Fuel } from "lucide-react";
import { preciosService } from "@/services/precios.service";
import { Card, CardContent, CardHeader } from "@/components/ui/Card";
import { Skeleton } from "@/components/ui/Skeleton";
import { useRefrescoMs } from "@/contexts/RefrescoContext";
import type { PrecioCombustible } from "@/types/precios";

// Color por combustible (consistente con el resto del dashboard).
const COLOR: Record<string, string> = {
  Extra: "#22c55e",
  Ecopais: "#3b82f6",
  Diesel: "#f59e0b",
  Super: "#a855f7",
};

function fmtFecha(iso: string | null): string {
  if (!iso) return "—";
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return "—";
  return d.toLocaleDateString("es-EC", { day: "2-digit", month: "short", year: "numeric" });
}

function Tile({ p }: { p: PrecioCombustible }) {
  const color = COLOR[p.producto] ?? "#64748b";
  const apiEsVigente = p.origenVigente === "API";
  return (
    <div className="rounded-xl border border-neutral-800 bg-neutral-900/50 p-4">
      <div className="flex items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <span className="h-2.5 w-2.5 rounded-full" style={{ background: color }} />
          <span className="text-sm font-medium text-neutral-200">{p.nombre}</span>
        </div>
        <span
          className={`rounded-full px-2 py-0.5 text-[10px] font-semibold ${
            p.esRegulado ? "bg-emerald-500/10 text-emerald-300" : "bg-violet-500/10 text-violet-300"
          }`}
          title={p.esRegulado ? "Precio regulado (único nacional)" : "Libre mercado (referencial)"}
        >
          {p.esRegulado ? "Regulado" : "Referencial"}
        </span>
      </div>

      {/* Precio VIGENTE (el efectivo según la preferencia) */}
      <div className="mt-2 flex items-baseline gap-1">
        <span className="text-2xl font-semibold text-white">${p.precioVigente.toFixed(2)}</span>
        <span className="text-xs text-neutral-500">/ galón</span>
        <span
          className={`ml-1 rounded px-1.5 py-0.5 text-[10px] font-semibold ${
            apiEsVigente ? "bg-sky-500/15 text-sky-300" : "bg-neutral-500/15 text-neutral-300"
          }`}
          title="Fuente del precio efectivo (preferencia configurable en Ajustes)"
        >
          {apiEsVigente ? "vía API" : "vía sistema"}
        </span>
        {p.precioPendiente && (
          <span className="ml-1 rounded bg-amber-500/15 px-1.5 py-0.5 text-[10px] font-semibold text-amber-300">
            pendiente
          </span>
        )}
      </div>

      {/* Comparación: precio del sistema y de la API, cada uno con su fecha */}
      <div className="mt-2 space-y-0.5 text-xs">
        <div className={apiEsVigente ? "text-neutral-500" : "text-neutral-300"}>
          Sistema: <span className="font-medium">${p.precioGalon.toFixed(2)}</span>
          <span className="text-neutral-600"> · {fmtFecha(p.fechaSistema)}</span>
        </div>
        <div className={apiEsVigente ? "text-neutral-300" : "text-neutral-500"}>
          API:{" "}
          {p.precioApi != null ? (
            <>
              <span className="font-medium">${p.precioApi.toFixed(2)}</span>
              <span className="text-neutral-600">
                {" "}
                · {fmtFecha(p.apiActualizadoEn)}
                {p.fuenteApi ? ` · ${p.fuenteApi}` : ""}
              </span>
            </>
          ) : (
            <span className="text-neutral-600">sin dato aún</span>
          )}
        </div>
      </div>

      {p.subsidio > 0 && (
        <div className="mt-1 text-xs text-neutral-500">
          Subsidio: <span className="text-neutral-300">${p.subsidio.toFixed(2)}</span> / gal
        </div>
      )}
    </div>
  );
}

export function PreciosCombustibleCard() {
  const refrescoMs = useRefrescoMs();
  const { data, isLoading, isError } = useQuery({
    queryKey: ["precios-combustible"],
    queryFn: () => preciosService.getVigentes(),
    refetchInterval: refrescoMs,
  });

  const precios = data?.precios ?? [];
  const vigencia =
    precios.length > 0
      ? `Vigente ${fmtFecha(precios[0].vigenteDesde)} – ${fmtFecha(precios[0].vigenteHasta)}`
      : "Precios oficiales regulados";

  return (
    <Card>
      <CardHeader title="Precios de combustible (Ecuador)" subtitle={vigencia} />
      <CardContent>
        {isLoading ? (
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <Skeleton className="h-28" />
            <Skeleton className="h-28" />
            <Skeleton className="h-28" />
            <Skeleton className="h-28" />
          </div>
        ) : isError || precios.length === 0 ? (
          <div className="flex items-center gap-2 text-sm text-neutral-400">
            <Fuel className="h-4 w-4" />
            No hay precios de combustible configurados.
          </div>
        ) : (
          <>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
              {precios.map((p) => (
                <Tile key={p.producto} p={p} />
              ))}
            </div>
            {(data?.fuentesDegradadas?.length ?? 0) > 0 && (
              <p className="mt-3 text-xs text-amber-400">
                Fuentes temporalmente no disponibles: {data?.fuentesDegradadas.join(", ")}
              </p>
            )}
            <p className="mt-3 text-xs leading-relaxed text-neutral-500">{data?.nota}</p>
            {precios[0]?.fuente && (
              <p className="mt-1 text-xs text-neutral-600">Fuente del sistema: {precios[0].fuente}</p>
            )}
          </>
        )}
      </CardContent>
    </Card>
  );
}
