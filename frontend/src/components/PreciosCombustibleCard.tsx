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
};

function fmtFecha(iso: string | null): string {
  if (!iso) return "—";
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return "—";
  return d.toLocaleDateString("es-EC", { day: "2-digit", month: "short", year: "numeric" });
}

function Tile({ p }: { p: PrecioCombustible }) {
  const color = COLOR[p.producto] ?? "#64748b";
  return (
    <div className="rounded-xl border border-neutral-800 bg-neutral-900/50 p-4">
      <div className="flex items-center gap-2">
        <span className="h-2.5 w-2.5 rounded-full" style={{ background: color }} />
        <span className="text-sm font-medium text-neutral-200">{p.nombre}</span>
      </div>
      <div className="mt-2 flex items-baseline gap-1">
        <span className="text-2xl font-semibold text-white">${p.precioGalon.toFixed(2)}</span>
        <span className="text-xs text-neutral-500">/ galón</span>
      </div>
      {p.subsidio > 0 && (
        <div className="mt-1 text-xs text-neutral-400">
          Subsidio estatal: <span className="text-neutral-200">${p.subsidio.toFixed(2)}</span> / gal
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
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            <Skeleton className="h-24" />
            <Skeleton className="h-24" />
            <Skeleton className="h-24" />
          </div>
        ) : isError || precios.length === 0 ? (
          <div className="flex items-center gap-2 text-sm text-neutral-400">
            <Fuel className="h-4 w-4" />
            No hay precios de combustible configurados.
          </div>
        ) : (
          <>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
              {precios.map((p) => (
                <Tile key={p.producto} p={p} />
              ))}
            </div>
            <p className="mt-3 text-xs leading-relaxed text-neutral-500">{data?.nota}</p>
            {precios[0]?.fuente && (
              <p className="mt-1 text-xs text-neutral-600">Fuente: {precios[0].fuente}</p>
            )}
          </>
        )}
      </CardContent>
    </Card>
  );
}
