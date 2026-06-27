import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { logsService } from "@/services/logs.service";
import { useRefrescoMs } from "@/contexts/RefrescoContext";
import { Spinner } from "@/components/ui/Spinner";
import { ChevronLeft, ChevronRight } from "lucide-react";

export function LogsPage() {
  const [page, setPage] = useState(1);
  const refrescoMs = useRefrescoMs();

  const { data, isLoading } = useQuery({
    queryKey: ["logs", page],
    queryFn: () => logsService.getAll(page, 50),
    // Tiempo real: la bitácora se refresca sola (tasa de refresco global configurable)
    refetchInterval: refrescoMs,
  });

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-foreground">
        Logs de Auditoría
      </h1>

      <div className="overflow-x-auto rounded-lg border border-border">
        <table className="w-full text-sm">
          <thead className="bg-muted">
            <tr>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                ID
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Acción
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Entidad
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Usuario
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                IP
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Fecha
              </th>
            </tr>
          </thead>
          <tbody>
            {data?.items.map((log) => (
              <tr key={log.id} className="border-t border-border">
                <td className="px-4 py-3 font-mono">{log.id}</td>
                <td className="px-4 py-3">{log.accion}</td>
                <td className="px-4 py-3">
                  {log.entidad}
                  {log.entidadId != null && (
                    <span className="text-muted-foreground">
                      {" "}
                      #{log.entidadId}
                    </span>
                  )}
                </td>
                <td className="px-4 py-3">
                  {log.usuarioEmail ?? `ID: ${log.usuarioId}`}
                </td>
                <td className="px-4 py-3 font-mono text-xs">
                  {log.direccionIp}
                </td>
                <td className="px-4 py-3 text-muted-foreground">
                  {new Date(log.createdAt).toLocaleString("es-EC")}
                </td>
              </tr>
            ))}
            {data?.items.length === 0 && (
              <tr>
                <td
                  colSpan={6}
                  className="px-4 py-8 text-center text-muted-foreground"
                >
                  No hay logs de auditoría.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            Página {data.page} de {data.totalPages} ({data.totalCount}{" "}
            registros)
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
    </div>
  );
}
