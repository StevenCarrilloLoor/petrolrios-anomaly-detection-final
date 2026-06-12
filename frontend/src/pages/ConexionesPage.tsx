import { useQuery } from "@tanstack/react-query";
import { monitoreoService } from "@/services/monitoreo.service";
import { Card, CardContent, CardHeader } from "@/components/ui/Card";
import { Skeleton } from "@/components/ui/Skeleton";
import {
  Server,
  Database,
  Radio,
  Cpu,
  Wifi,
  WifiOff,
  CircleSlash,
  RefreshCw,
} from "lucide-react";
import type { ReactNode } from "react";

const REFRESCO_MS = 10_000;

export function ConexionesPage() {
  const {
    data: sistema,
    isLoading: loadingSistema,
    dataUpdatedAt,
  } = useQuery({
    queryKey: ["monitoreo", "sistema"],
    queryFn: monitoreoService.getEstadoSistema,
    refetchInterval: REFRESCO_MS,
  });

  const { data: conexiones, isLoading: loadingConexiones } = useQuery({
    queryKey: ["monitoreo", "conexiones"],
    queryFn: monitoreoService.getConexiones,
    refetchInterval: REFRESCO_MS,
  });

  return (
    <div className="space-y-6">
      <div className="flex items-end justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">
            Monitoreo de Conexiones
          </h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Estado en vivo de los agentes de estación, la API y sus servicios
          </p>
        </div>
        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          <RefreshCw size={13} className="animate-[spin_3s_linear_infinite]" />
          Actualización automática cada 10 s · última:{" "}
          {new Date(dataUpdatedAt).toLocaleTimeString("es-EC")}
        </div>
      </div>

      {/* Estado del sistema */}
      {loadingSistema ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-32" />
          ))}
        </div>
      ) : (
        sistema && (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <EstadoCard
              icon={<Server size={18} />}
              titulo="API Central"
              ok
              estado="En línea"
              detalles={[
                ["Versión", `v${sistema.versionApi}`],
                ["Entorno", sistema.entorno],
                ["Uptime", formatearUptime(sistema.uptimeSegundos)],
              ]}
            />
            <EstadoCard
              icon={<Database size={18} />}
              titulo="Base de Datos (PostgreSQL)"
              ok={sistema.baseDatosConectada}
              estado={sistema.baseDatosConectada ? "Conectada" : "Sin conexión"}
              detalles={[
                [
                  "Latencia",
                  sistema.latenciaBaseDatosMs !== null
                    ? `${sistema.latenciaBaseDatosMs} ms`
                    : "—",
                ],
              ]}
            />
            <EstadoCard
              icon={<Radio size={18} />}
              titulo="SignalR (tiempo real)"
              ok
              estado="Activo"
              detalles={[
                ["Clientes conectados", String(sistema.clientesSignalRConectados)],
                ["Hub", "/hubs/alerts"],
              ]}
            />
            <EstadoCard
              icon={<Cpu size={18} />}
              titulo="Motor de Detección"
              ok={sistema.ultimoCicloEstado !== "Fallido"}
              estado={
                sistema.ultimoCicloDeteccion
                  ? `Último ciclo: ${sistema.ultimoCicloEstado}`
                  : "Sin ciclos aún"
              }
              detalles={[
                [
                  "Hace",
                  sistema.minutosDesdeUltimoCiclo !== null
                    ? `${sistema.minutosDesdeUltimoCiclo} min`
                    : "—",
                ],
                [
                  "Alertas / duración",
                  sistema.ultimoCicloAlertas !== null
                    ? `${sistema.ultimoCicloAlertas} · ${sistema.ultimoCicloDuracionSegundos?.toFixed(2)} s`
                    : "—",
                ],
              ]}
            />
          </div>
        )
      )}

      {/* Agentes por estación */}
      <Card>
        <CardHeader
          title={`Agentes de estación ${
            sistema
              ? `— ${sistema.estacionesConectadas} de ${sistema.estacionesTotales} conectados`
              : ""
          }`}
          subtitle="Un agente se considera conectado si envió datos en los últimos 10 minutos"
        />
        <CardContent className="p-0">
          {loadingConexiones ? (
            <div className="space-y-2 p-6">
              {Array.from({ length: 5 }).map((_, i) => (
                <Skeleton key={i} className="h-12" />
              ))}
            </div>
          ) : (
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-muted-foreground">
                  <th className="px-6 py-3 font-semibold">Estación</th>
                  <th className="px-4 py-3 font-semibold">Estado</th>
                  <th className="px-4 py-3 font-semibold">Última ingesta</th>
                  <th className="px-4 py-3 text-right font-semibold">Últimas 24 h</th>
                  <th className="px-4 py-3 text-right font-semibold">Históricas</th>
                  <th className="px-6 py-3 text-right font-semibold">
                    Pendientes de análisis
                  </th>
                </tr>
              </thead>
              <tbody>
                {(conexiones ?? []).map((c) => (
                  <tr
                    key={c.estacionId}
                    className="border-b border-border last:border-0"
                  >
                    <td className="px-6 py-3">
                      <p className="font-medium text-foreground">{c.nombre}</p>
                      <p className="font-mono text-xs text-muted-foreground">
                        {c.codigo} · zona {c.zona}
                      </p>
                    </td>
                    <td className="px-4 py-3">
                      <EstadoConexion estado={c.estado} />
                    </td>
                    <td className="px-4 py-3">
                      {c.ultimaIngesta ? (
                        <>
                          <p className="text-foreground">
                            {new Date(c.ultimaIngesta).toLocaleString("es-EC", {
                              dateStyle: "short",
                              timeStyle: "medium",
                            })}
                          </p>
                          <p className="text-xs text-muted-foreground">
                            hace {formatearMinutos(c.minutosDesdeUltimaIngesta)}
                          </p>
                        </>
                      ) : (
                        <span className="text-muted-foreground">—</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-right font-semibold">
                      {c.transaccionesUltimas24Horas}
                    </td>
                    <td className="px-4 py-3 text-right text-muted-foreground">
                      {c.transaccionesTotales}
                    </td>
                    <td className="px-6 py-3 text-right">
                      {c.pendientesAnalisis > 0 ? (
                        <span className="rounded-full bg-risk-medium/15 px-2.5 py-0.5 text-xs font-semibold text-risk-medium">
                          {c.pendientesAnalisis}
                        </span>
                      ) : (
                        <span className="text-muted-foreground">0</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </CardContent>
      </Card>

      <p className="text-xs text-muted-foreground">
        Nota: en producción cada estación ejecuta su propio Station Agent contra el
        Firebird local de Contaplus. En el entorno de demostración solo la estación
        EST-001 tiene un agente real conectado; el resto aparecerá como "Nunca
        conectada" — ese es el comportamiento esperado.
      </p>
    </div>
  );
}

function EstadoCard({
  icon,
  titulo,
  ok,
  estado,
  detalles,
}: {
  icon: ReactNode;
  titulo: string;
  ok: boolean;
  estado: string;
  detalles: [string, string][];
}) {
  return (
    <div className="rounded-xl border border-border bg-background p-5 shadow-sm">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2.5">
          <div
            className={`rounded-lg p-2 ${ok ? "bg-risk-low/10 text-risk-low" : "bg-risk-critical/10 text-risk-critical"}`}
          >
            {icon}
          </div>
          <p className="text-sm font-semibold text-foreground">{titulo}</p>
        </div>
        <span className="relative flex h-2.5 w-2.5">
          {ok && (
            <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-risk-low opacity-60" />
          )}
          <span
            className={`relative inline-flex h-2.5 w-2.5 rounded-full ${ok ? "bg-risk-low" : "bg-risk-critical"}`}
          />
        </span>
      </div>
      <p
        className={`mt-3 text-sm font-bold ${ok ? "text-risk-low" : "text-risk-critical"}`}
      >
        {estado}
      </p>
      <div className="mt-2 space-y-1">
        {detalles.map(([k, v]) => (
          <div key={k} className="flex justify-between text-xs">
            <span className="text-muted-foreground">{k}</span>
            <span className="font-mono text-foreground">{v}</span>
          </div>
        ))}
      </div>
    </div>
  );
}

function EstadoConexion({ estado }: { estado: string }) {
  if (estado === "Conectada")
    return (
      <span className="inline-flex items-center gap-1.5 rounded-full bg-risk-low/15 px-2.5 py-1 text-xs font-semibold text-risk-low">
        <Wifi size={12} /> Conectada
      </span>
    );
  if (estado === "Sin conexión")
    return (
      <span className="inline-flex items-center gap-1.5 rounded-full bg-risk-critical/15 px-2.5 py-1 text-xs font-semibold text-risk-critical">
        <WifiOff size={12} /> Sin conexión
      </span>
    );
  return (
    <span className="inline-flex items-center gap-1.5 rounded-full bg-muted px-2.5 py-1 text-xs font-semibold text-muted-foreground">
      <CircleSlash size={12} /> Nunca conectada
    </span>
  );
}

function formatearUptime(segundos: number): string {
  if (segundos < 60) return `${Math.round(segundos)} s`;
  if (segundos < 3600) return `${Math.floor(segundos / 60)} min`;
  if (segundos < 86400)
    return `${Math.floor(segundos / 3600)} h ${Math.floor((segundos % 3600) / 60)} min`;
  return `${Math.floor(segundos / 86400)} d ${Math.floor((segundos % 86400) / 3600)} h`;
}

function formatearMinutos(minutos: number | null): string {
  if (minutos === null) return "—";
  if (minutos < 1) return "menos de 1 min";
  if (minutos < 60) return `${Math.round(minutos)} min`;
  if (minutos < 1440) return `${Math.floor(minutos / 60)} h ${Math.round(minutos % 60)} min`;
  return `${Math.floor(minutos / 1440)} días`;
}
