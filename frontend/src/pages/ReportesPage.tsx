import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { dashboardService } from "@/services/dashboard.service";
import { reportesService } from "@/services/reportes.service";
import type { ReporteFilters } from "@/services/reportes.service";
import {
  TIPO_DETECTOR_OPTIONS,
  NIVEL_RIESGO_OPTIONS,
  ESTADO_ALERTA_OPTIONS,
  TIPO_DETECTOR_LABELS,
  NIVEL_RIESGO_LABELS,
  ESTADO_ALERTA_LABELS,
} from "@/types/alert";
import { Card, CardContent, CardHeader } from "@/components/ui/Card";
import { FileSpreadsheet, FileText, Loader2 } from "lucide-react";

const selectClass =
  "w-full rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40";

export function ReportesPage() {
  const [tipo, setTipo] = useState("");
  const [nivelRiesgo, setNivelRiesgo] = useState("");
  const [estado, setEstado] = useState("");
  const [estacionId, setEstacionId] = useState("");
  const [fechaDesde, setFechaDesde] = useState("");
  const [fechaHasta, setFechaHasta] = useState("");
  const [descargando, setDescargando] = useState<"pdf" | "excel" | null>(null);
  const [error, setError] = useState<string | null>(null);

  const { data: estaciones } = useQuery({
    queryKey: ["dashboard", "alertas-por-estacion"],
    queryFn: dashboardService.getAlertasPorEstacion,
    staleTime: 5 * 60_000,
  });

  function buildFilters(): ReporteFilters {
    return {
      tipo: tipo || undefined,
      nivelRiesgo: nivelRiesgo || undefined,
      estado: estado || undefined,
      estacionId: estacionId ? Number(estacionId) : undefined,
      fechaDesde: fechaDesde || undefined,
      fechaHasta: fechaHasta ? `${fechaHasta}T23:59:59` : undefined,
    };
  }

  async function descargar(formato: "pdf" | "excel") {
    setError(null);
    setDescargando(formato);
    try {
      if (formato === "pdf") {
        await reportesService.descargarPdf(buildFilters());
      } else {
        await reportesService.descargarExcel(buildFilters());
      }
    } catch {
      setError("No se pudo generar el reporte. Intente nuevamente.");
    } finally {
      setDescargando(null);
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground">Reportes</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Genere reportes consolidados de alertas en PDF o Excel
        </p>
      </div>

      <Card>
        <CardHeader
          title="Filtros del reporte"
          subtitle="Todos los filtros son opcionales; sin filtros se incluyen todas las alertas (máx. 5000)"
        />
        <CardContent>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            <label className="space-y-1.5">
              <span className="text-xs font-medium text-muted-foreground">
                Tipo de detector
              </span>
              <select
                value={tipo}
                onChange={(e) => setTipo(e.target.value)}
                className={selectClass}
              >
                <option value="">Todos</option>
                {TIPO_DETECTOR_OPTIONS.map((t) => (
                  <option key={t} value={t}>
                    {TIPO_DETECTOR_LABELS[t]}
                  </option>
                ))}
              </select>
            </label>

            <label className="space-y-1.5">
              <span className="text-xs font-medium text-muted-foreground">
                Nivel de riesgo
              </span>
              <select
                value={nivelRiesgo}
                onChange={(e) => setNivelRiesgo(e.target.value)}
                className={selectClass}
              >
                <option value="">Todos</option>
                {NIVEL_RIESGO_OPTIONS.map((n) => (
                  <option key={n} value={n}>
                    {NIVEL_RIESGO_LABELS[n]}
                  </option>
                ))}
              </select>
            </label>

            <label className="space-y-1.5">
              <span className="text-xs font-medium text-muted-foreground">
                Estado
              </span>
              <select
                value={estado}
                onChange={(e) => setEstado(e.target.value)}
                className={selectClass}
              >
                <option value="">Todos</option>
                {ESTADO_ALERTA_OPTIONS.map((opt) => (
                  <option key={opt} value={opt}>
                    {ESTADO_ALERTA_LABELS[opt]}
                  </option>
                ))}
              </select>
            </label>

            <label className="space-y-1.5">
              <span className="text-xs font-medium text-muted-foreground">
                Estación
              </span>
              <select
                value={estacionId}
                onChange={(e) => setEstacionId(e.target.value)}
                className={selectClass}
              >
                <option value="">Todas</option>
                {(estaciones ?? []).map((est) => (
                  <option key={est.estacionId} value={est.estacionId}>
                    {est.estacionNombre}
                  </option>
                ))}
              </select>
            </label>

            <label className="space-y-1.5">
              <span className="text-xs font-medium text-muted-foreground">
                Fecha desde
              </span>
              <input
                type="date"
                value={fechaDesde}
                onChange={(e) => setFechaDesde(e.target.value)}
                className={selectClass}
              />
            </label>

            <label className="space-y-1.5">
              <span className="text-xs font-medium text-muted-foreground">
                Fecha hasta
              </span>
              <input
                type="date"
                value={fechaHasta}
                onChange={(e) => setFechaHasta(e.target.value)}
                className={selectClass}
              />
            </label>
          </div>

          {error && (
            <p className="mt-4 rounded-md bg-destructive/10 p-3 text-sm text-destructive">
              {error}
            </p>
          )}

          <div className="mt-6 flex flex-wrap gap-3">
            <button
              onClick={() => descargar("pdf")}
              disabled={descargando !== null}
              className="flex items-center gap-2 rounded-md bg-primary px-5 py-2.5 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-50"
            >
              {descargando === "pdf" ? (
                <Loader2 size={16} className="animate-spin" />
              ) : (
                <FileText size={16} />
              )}
              Descargar PDF
            </button>
            <button
              onClick={() => descargar("excel")}
              disabled={descargando !== null}
              className="flex items-center gap-2 rounded-md border border-border bg-background px-5 py-2.5 text-sm font-medium text-foreground transition-colors hover:bg-muted disabled:opacity-50"
            >
              {descargando === "excel" ? (
                <Loader2 size={16} className="animate-spin" />
              ) : (
                <FileSpreadsheet size={16} />
              )}
              Descargar Excel
            </button>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader title="Contenido del reporte" />
        <CardContent>
          <ul className="list-disc space-y-1.5 pl-5 text-sm text-muted-foreground">
            <li>
              Resumen ejecutivo con totales por nivel de riesgo y por tipo de detector.
            </li>
            <li>
              Detalle de cada alerta: fecha, detector, nivel, score, estado, estación,
              empleado y descripción.
            </li>
            <li>
              En Excel: hoja de datos con autofiltro y hoja de resumen; filas críticas y
              altas resaltadas.
            </li>
            <li>En PDF: formato apaisado con paginación, listo para impresión.</li>
          </ul>
        </CardContent>
      </Card>
    </div>
  );
}
