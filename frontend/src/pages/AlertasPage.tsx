import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { alertasService } from "@/services/alertas.service";
import { Badge } from "@/components/ui/Badge";
import { Spinner } from "@/components/ui/Spinner";
import {
  TIPO_DETECTOR_OPTIONS,
  NIVEL_RIESGO_OPTIONS,
  ESTADO_ALERTA_OPTIONS,
  TIPO_DETECTOR_LABELS,
  NIVEL_RIESGO_LABELS,
  ESTADO_ALERTA_LABELS,
} from "@/types/alert";
import { ChevronLeft, ChevronRight } from "lucide-react";

export function AlertasPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [tipoDetector, setTipoDetector] = useState("");
  const [nivelRiesgo, setNivelRiesgo] = useState("");
  const [estado, setEstado] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["alertas", { page, pageSize, tipoDetector, nivelRiesgo, estado }],
    queryFn: () =>
      alertasService.getAll({
        page,
        pageSize,
        tipoDetector: tipoDetector || undefined,
        nivelRiesgo: nivelRiesgo || undefined,
        estado: estado || undefined,
      }),
  });

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-foreground">Alertas</h1>

      <div className="flex flex-wrap gap-3">
        <select
          value={tipoDetector}
          onChange={(e) => {
            setTipoDetector(e.target.value);
            setPage(1);
          }}
          className="rounded-md border border-border bg-background px-3 py-2 text-sm"
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
          onChange={(e) => {
            setNivelRiesgo(e.target.value);
            setPage(1);
          }}
          className="rounded-md border border-border bg-background px-3 py-2 text-sm"
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
          onChange={(e) => {
            setEstado(e.target.value);
            setPage(1);
          }}
          className="rounded-md border border-border bg-background px-3 py-2 text-sm"
        >
          <option value="">Todos los estados</option>
          {ESTADO_ALERTA_OPTIONS.map((opt) => (
            <option key={opt} value={opt}>
              {ESTADO_ALERTA_LABELS[opt]}
            </option>
          ))}
        </select>
      </div>

      {isLoading ? (
        <div className="flex h-64 items-center justify-center">
          <Spinner size="lg" />
        </div>
      ) : (
        <>
          <div className="overflow-x-auto rounded-lg border border-border">
            <table className="w-full text-sm">
              <thead className="bg-muted">
                <tr>
                  <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                    ID
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                    Tipo
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                    Nivel
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                    Estado
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                    Estación
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                    Score
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                    Fecha
                  </th>
                </tr>
              </thead>
              <tbody>
                {data?.items.map((alerta) => (
                  <tr
                    key={alerta.id}
                    onClick={() => navigate(`/alertas/${alerta.id}`)}
                    className="cursor-pointer border-t border-border hover:bg-muted/50"
                  >
                    <td className="px-4 py-3 font-mono">{alerta.id}</td>
                    <td className="px-4 py-3">
                      {TIPO_DETECTOR_LABELS[alerta.tipoDetector]}
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
                    <td className="px-4 py-3 font-mono font-bold">
                      {alerta.score.toFixed(1)}
                    </td>
                    <td className="px-4 py-3 text-muted-foreground">
                      {new Date(alerta.fechaDeteccion).toLocaleString("es-EC")}
                    </td>
                  </tr>
                ))}
                {data?.items.length === 0 && (
                  <tr>
                    <td
                      colSpan={7}
                      className="px-4 py-8 text-center text-muted-foreground"
                    >
                      No se encontraron alertas con los filtros seleccionados.
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
                  className="flex items-center gap-1 rounded-md border border-border px-3 py-1 text-sm disabled:opacity-50"
                >
                  <ChevronLeft size={16} /> Anterior
                </button>
                <button
                  onClick={() => setPage((p) => p + 1)}
                  disabled={!data.hasNextPage}
                  className="flex items-center gap-1 rounded-md border border-border px-3 py-1 text-sm disabled:opacity-50"
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
