import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { alertasService } from "@/services/alertas.service";
import { dashboardService } from "@/services/dashboard.service";
import { Badge } from "@/components/ui/Badge";
import { Skeleton } from "@/components/ui/Skeleton";
import { EmptyState } from "@/components/ui/EmptyState";
import {
  TIPO_DETECTOR_OPTIONS,
  NIVEL_RIESGO_OPTIONS,
  ESTADO_ALERTA_OPTIONS,
  TIPO_DETECTOR_LABELS,
  NIVEL_RIESGO_LABELS,
  ESTADO_ALERTA_LABELS,
} from "@/types/alert";
import { ChevronLeft, ChevronRight, FilterX, SearchX } from "lucide-react";

const selectClass =
  "rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40";

export function AlertasPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [tipoDetector, setTipoDetector] = useState("");
  const [nivelRiesgo, setNivelRiesgo] = useState("");
  const [estado, setEstado] = useState("");
  const [estacionId, setEstacionId] = useState("");
  const [fechaDesde, setFechaDesde] = useState("");
  const [fechaHasta, setFechaHasta] = useState("");

  const { data: estaciones } = useQuery({
    queryKey: ["dashboard", "alertas-por-estacion"],
    queryFn: dashboardService.getAlertasPorEstacion,
    staleTime: 5 * 60_000,
  });

  const filters = {
    page,
    pageSize,
    tipoDetector: tipoDetector || undefined,
    nivelRiesgo: nivelRiesgo || undefined,
    estado: estado || undefined,
    estacionId: estacionId ? Number(estacionId) : undefined,
    fechaDesde: fechaDesde || undefined,
    fechaHasta: fechaHasta ? `${fechaHasta}T23:59:59` : undefined,
  };

  const { data, isLoading } = useQuery({
    queryKey: ["alertas", filters],
    queryFn: () => alertasService.getAll(filters),
    // Además del refresco por SignalR, sondeo de respaldo cada 15 s
    refetchInterval: 15_000,
  });

  const hayFiltros =
    tipoDetector || nivelRiesgo || estado || estacionId || fechaDesde || fechaHasta;

  function limpiarFiltros() {
    setTipoDetector("");
    setNivelRiesgo("");
    setEstado("");
    setEstacionId("");
    setFechaDesde("");
    setFechaHasta("");
    setPage(1);
  }

  function actualizar(setter: (v: string) => void) {
    return (value: string) => {
      setter(value);
      setPage(1);
    };
  }

  return (
    <div className="space-y-6">
      <div className="flex items-end justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Alertas</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {data ? `${data.totalCount} alertas encontradas` : "Cargando…"}
          </p>
        </div>
      </div>

      <div className="flex flex-wrap items-center gap-3 rounded-xl border border-border bg-background p-4">
        <select
          value={tipoDetector}
          onChange={(e) => actualizar(setTipoDetector)(e.target.value)}
          className={selectClass}
        >
          <option value="">Todos los tipos</option>
          {TIPO_DETECTOR_OPTIONS.map((t) => (
            <option key={t} value={t}>
              {TIPO_DETECTOR_LABELS[t]}
            </option>
          ))}
        </select>

        <select
          value={nivelRiesgo}
          onChange={(e) => actualizar(setNivelRiesgo)(e.target.value)}
          className={selectClass}
        >
          <option value="">Todos los niveles</option>
          {NIVEL_RIESGO_OPTIONS.map((n) => (
            <option key={n} value={n}>
              {NIVEL_RIESGO_LABELS[n]}
            </option>
          ))}
        </select>

        <select
          value={estado}
          onChange={(e) => actualizar(setEstado)(e.target.value)}
          className={selectClass}
        >
          <option value="">Todos los estados</option>
          {ESTADO_ALERTA_OPTIONS.map((opt) => (
            <option key={opt} value={opt}>
              {ESTADO_ALERTA_LABELS[opt]}
            </option>
          ))}
        </select>

        <select
          value={estacionId}
          onChange={(e) => actualizar(setEstacionId)(e.target.value)}
          className={selectClass}
        >
          <option value="">Todas las estaciones</option>
          {(estaciones ?? []).map((est) => (
            <option key={est.estacionId} value={est.estacionId}>
              {est.estacionNombre}
            </option>
          ))}
        </select>

        <label className="flex items-center gap-2 text-sm text-muted-foreground">
          Desde
          <input
            type="date"
            value={fechaDesde}
            onChange={(e) => actualizar(setFechaDesde)(e.target.value)}
            className={selectClass}
          />
        </label>

        <label className="flex items-center gap-2 text-sm text-muted-foreground">
          Hasta
          <input
            type="date"
            value={fechaHasta}
            onChange={(e) => actualizar(setFechaHasta)(e.target.value)}
            className={selectClass}
          />
        </label>

        {hayFiltros && (
          <button
            onClick={limpiarFiltros}
            className="flex items-center gap-1.5 rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-muted hover:text-foreground"
          >
            <FilterX size={15} /> Limpiar filtros
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
        <>
          <div className="overflow-x-auto rounded-xl border border-border bg-background">
            <table className="w-full text-sm">
              <thead className="bg-muted/60">
                <tr>
                  {["ID", "Tipo", "Nivel", "Estado", "Estación", "Empleado", "Score", "Fecha"].map(
                    (h) => (
                      <th
                        key={h}
                        className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-muted-foreground"
                      >
                        {h}
                      </th>
                    ),
                  )}
                </tr>
              </thead>
              <tbody>
                {data?.items.map((alerta) => (
                  <tr
                    key={alerta.id}
                    onClick={() => navigate(`/alertas/${alerta.id}`)}
                    className="cursor-pointer border-t border-border transition-colors hover:bg-muted/50"
                  >
                    <td className="px-4 py-3 font-mono text-muted-foreground">
                      #{alerta.id}
                    </td>
                    <td className="max-w-md px-4 py-3">
                      <p className="font-medium">
                        {TIPO_DETECTOR_LABELS[alerta.tipoDetector]}
                      </p>
                      <p
                        className="mt-0.5 truncate text-xs text-muted-foreground"
                        title={alerta.descripcion}
                      >
                        {alerta.descripcion}
                      </p>
                    </td>
                    <td className="px-4 py-3">
                      <Badge variant="risk" riskLevel={alerta.nivelRiesgo}>
                        {NIVEL_RIESGO_LABELS[alerta.nivelRiesgo]}
                      </Badge>
                    </td>
                    <td className="px-4 py-3">
                      <Badge variant="status" status={alerta.estado}>
                        {ESTADO_ALERTA_LABELS[alerta.estado]}
                      </Badge>
                    </td>
                    <td className="px-4 py-3">{alerta.estacionNombre}</td>
                    <td className="px-4 py-3 font-mono">
                      {alerta.empleadoCodigo ?? "—"}
                    </td>
                    <td className="px-4 py-3">
                      <ScoreBar score={alerta.score} />
                    </td>
                    <td className="px-4 py-3 whitespace-nowrap text-muted-foreground">
                      {new Date(alerta.fechaDeteccion).toLocaleString("es-EC", {
                        dateStyle: "short",
                        timeStyle: "short",
                      })}
                    </td>
                  </tr>
                ))}
                {data?.items.length === 0 && (
                  <tr>
                    <td colSpan={8}>
                      <EmptyState
                        icon={<SearchX size={40} />}
                        title="No se encontraron alertas"
                        description="Ajuste los filtros o espere al próximo ciclo de detección (cada 5–10 minutos)."
                      />
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {data && data.totalPages > 1 && (
            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">
                Mostrando {(data.page - 1) * data.pageSize + 1}–
                {Math.min(data.page * data.pageSize, data.totalCount)} de{" "}
                {data.totalCount}
              </p>
              <div className="flex gap-2">
                <button
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={!data.hasPreviousPage}
                  className="flex items-center gap-1 rounded-md border border-border px-3 py-1.5 text-sm transition-colors hover:bg-muted disabled:opacity-50"
                >
                  <ChevronLeft size={16} /> Anterior
                </button>
                <button
                  onClick={() => setPage((p) => p + 1)}
                  disabled={!data.hasNextPage}
                  className="flex items-center gap-1 rounded-md border border-border px-3 py-1.5 text-sm transition-colors hover:bg-muted disabled:opacity-50"
                >
                  Siguiente <ChevronRight size={16} />
                </button>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}

function ScoreBar({ score }: { score: number }) {
  const color =
    score > 75
      ? "bg-risk-critical"
      : score > 50
        ? "bg-risk-high"
        : score > 25
          ? "bg-risk-medium"
          : "bg-risk-low";

  return (
    <div className="flex items-center gap-2">
      <div className="h-1.5 w-16 overflow-hidden rounded-full bg-muted">
        <div
          className={`h-full rounded-full ${color}`}
          style={{ width: `${Math.min(score, 100)}%` }}
        />
      </div>
      <span className="font-mono text-xs font-semibold">{score.toFixed(1)}</span>
    </div>
  );
}
