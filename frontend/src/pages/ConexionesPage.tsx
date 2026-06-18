import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { monitoreoService } from "@/services/monitoreo.service";
import { estacionesService } from "@/services/estaciones.service";
import { useAuth } from "@/contexts/AuthContext";
import { Card, CardContent, CardHeader } from "@/components/ui/Card";
import { Skeleton } from "@/components/ui/Skeleton";
import type { ConexionEstacionResponse } from "@/types/monitoreo";
import {
  Server,
  Database,
  Radio,
  Cpu,
  Wifi,
  WifiOff,
  CircleSlash,
  RefreshCw,
  Pencil,
  Trash2,
  Save,
  X,
  Users,
} from "lucide-react";
import type { ReactNode } from "react";

const REFRESCO_MS = 10_000;

/** Devuelve true si `disponible` es una versión mayor a `instalada` (semver simple). */
function hayVersionMasNueva(disponible?: string, instalada?: string | null): boolean {
  if (!disponible || !instalada) return false;
  const partes = (v: string) =>
    v.split("+")[0].split("-")[0].split(".").map((n) => parseInt(n, 10) || 0);
  const a = partes(disponible);
  const b = partes(instalada);
  for (let i = 0; i < 3; i++) {
    const x = a[i] ?? 0;
    const y = b[i] ?? 0;
    if (x !== y) return x > y;
  }
  return false;
}

export function ConexionesPage() {
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const puedeGestionar = user?.rol === "Supervisor" || user?.rol === "Administrador";
  const puedeEliminar = user?.rol === "Administrador";

  const [editandoId, setEditandoId] = useState<number | null>(null);
  const [editNombre, setEditNombre] = useState("");
  const [editZona, setEditZona] = useState("");
  const [mensaje, setMensaje] = useState<string | null>(null);

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

  const { data: ultimaVersion } = useQuery({
    queryKey: ["agente", "version"],
    queryFn: estacionesService.getVersionAgente,
    refetchInterval: 60_000,
  });

  const { data: usuariosConectados } = useQuery({
    queryKey: ["monitoreo", "usuarios-conectados"],
    queryFn: monitoreoService.getUsuariosConectados,
    refetchInterval: REFRESCO_MS,
    enabled: puedeGestionar,
  });

  const invalidar = () => {
    void queryClient.invalidateQueries({ queryKey: ["monitoreo"] });
    void queryClient.invalidateQueries({ queryKey: ["dashboard"] });
  };

  const actualizarMutation = useMutation({
    mutationFn: ({ id }: { id: number }) =>
      estacionesService.update(id, { nombre: editNombre, zona: editZona || null }),
    onSuccess: () => {
      setEditandoId(null);
      invalidar();
    },
  });

  const eliminarMutation = useMutation({
    mutationFn: (id: number) => estacionesService.delete(id),
    onSuccess: (resultado) => {
      setMensaje(resultado.mensaje);
      invalidar();
      setTimeout(() => setMensaje(null), 6000);
    },
  });

  const enLinea = (conexiones ?? []).filter((c) => c.conectada).length;
  const registradas = (conexiones ?? []).length;

  function iniciarEdicion(conexion: ConexionEstacionResponse) {
    setEditandoId(conexion.estacionId);
    setEditNombre(conexion.nombre);
    setEditZona(conexion.zona ?? "");
  }

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

      {mensaje && (
        <div className="rounded-xl border border-primary/30 bg-primary/5 px-4 py-3 text-sm text-foreground">
          {mensaje}
        </div>
      )}

      {/* Usuarios conectados al central (en tiempo real, vía SignalR) */}
      {puedeGestionar && (
        <Card>
          <CardHeader
            title={
              <span className="flex items-center gap-2">
                <Users size={18} className="text-primary" />
                Usuarios conectados al central ({usuariosConectados?.length ?? 0})
              </span>
            }
            subtitle="Personas con una sesión activa en el sistema central en este momento."
          />
          <CardContent>
            {!usuariosConectados || usuariosConectados.length === 0 ? (
              <p className="text-sm text-muted-foreground">
                No hay otros usuarios conectados ahora mismo.
              </p>
            ) : (
              <ul className="divide-y divide-border">
                {usuariosConectados.map((u) => (
                  <li
                    key={u.usuarioId}
                    className="flex items-center justify-between gap-3 py-2"
                  >
                    <span className="flex items-center gap-2 text-sm text-foreground">
                      <span className="inline-flex h-7 w-7 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary">
                        {u.nombre.slice(0, 1).toUpperCase()}
                      </span>
                      {u.nombre}
                      {u.rol && (
                        <span className="rounded-full bg-muted px-2 py-0.5 text-xs text-muted-foreground">
                          {u.rol}
                        </span>
                      )}
                    </span>
                    <span className="text-xs text-muted-foreground">
                      desde {new Date(u.desde).toLocaleTimeString()}
                    </span>
                  </li>
                ))}
              </ul>
            )}
          </CardContent>
        </Card>
      )}

      {/* Agentes por estación */}
      <Card>
        <CardHeader
          title={`Agentes de estación — ${enLinea} en línea · ${registradas} registradas`}
          subtitle="Las estaciones se registran automáticamente cuando su agente se conecta por primera vez. Un agente está en línea si envió señal de vida en los últimos 3 minutos."
        />
        <CardContent className="p-0">
          {loadingConexiones ? (
            <div className="space-y-2 p-6">
              {Array.from({ length: 4 }).map((_, i) => (
                <Skeleton key={i} className="h-12" />
              ))}
            </div>
          ) : registradas === 0 ? (
            <p className="px-6 py-10 text-center text-sm text-muted-foreground">
              Aún no hay estaciones registradas. Cuando un Station Agent se conecte
              por primera vez, su estación aparecerá aquí automáticamente.
            </p>
          ) : (
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-muted-foreground">
                  <th className="px-6 py-3 font-semibold">Estación</th>
                  <th className="px-4 py-3 font-semibold">Estado</th>
                  <th className="px-4 py-3 font-semibold">Señal de vida</th>
                  <th className="px-4 py-3 font-semibold">Última ingesta de datos</th>
                  <th className="px-4 py-3 text-right font-semibold">Últimas 24 h</th>
                  <th className="px-4 py-3 text-right font-semibold">Pendientes</th>
                  {puedeGestionar && (
                    <th className="px-6 py-3 text-right font-semibold">Acciones</th>
                  )}
                </tr>
              </thead>
              <tbody>
                {(conexiones ?? []).map((c) => (
                  <tr
                    key={c.estacionId}
                    className={`border-b border-border last:border-0 ${!c.activa ? "opacity-50" : ""}`}
                  >
                    <td className="px-6 py-3">
                      {editandoId === c.estacionId ? (
                        <div className="flex flex-wrap items-center gap-2">
                          <input
                            value={editNombre}
                            onChange={(e) => setEditNombre(e.target.value)}
                            className="w-52 rounded-md border border-primary bg-background px-2 py-1.5 text-sm focus:outline-none"
                            placeholder="Nombre de la estación"
                            autoFocus
                          />
                          <input
                            value={editZona}
                            onChange={(e) => setEditZona(e.target.value)}
                            className="w-24 rounded-md border border-border bg-background px-2 py-1.5 text-sm focus:outline-none"
                            placeholder="Zona"
                          />
                          <button
                            onClick={() =>
                              actualizarMutation.mutate({ id: c.estacionId })
                            }
                            disabled={!editNombre.trim() || actualizarMutation.isPending}
                            className="rounded-md bg-primary p-1.5 text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
                            title="Guardar"
                          >
                            <Save size={14} />
                          </button>
                          <button
                            onClick={() => setEditandoId(null)}
                            className="rounded-md border border-border p-1.5 text-muted-foreground hover:bg-muted"
                            title="Cancelar"
                          >
                            <X size={14} />
                          </button>
                        </div>
                      ) : (
                        <>
                          <p className="font-medium text-foreground">
                            {c.nombre}
                            {!c.activa && (
                              <span className="ml-2 rounded-full bg-muted px-2 py-0.5 text-[10px] text-muted-foreground">
                                inactiva
                              </span>
                            )}
                          </p>
                          <p className="font-mono text-xs text-muted-foreground">
                            {c.codigo}
                            {c.zona ? ` · zona ${c.zona}` : ""}
                            {c.versionAgente ? ` · agente v${c.versionAgente}` : ""}
                            {hayVersionMasNueva(ultimaVersion?.version, c.versionAgente) && (
                              <span className="ml-2 rounded-full bg-blue-500/15 px-2 py-0.5 text-[10px] font-medium text-blue-400">
                                actualización v{ultimaVersion?.version} disponible
                              </span>
                            )}
                          </p>
                        </>
                      )}
                    </td>
                    <td className="px-4 py-3">
                      <EstadoConexion estado={c.estado} />
                    </td>
                    <td className="px-4 py-3">
                      {c.ultimoHeartbeat ? (
                        <>
                          <p className="text-foreground">
                            {new Date(c.ultimoHeartbeat).toLocaleTimeString("es-EC")}
                          </p>
                          <p className="text-xs text-muted-foreground">
                            hace {formatearMinutos(c.minutosDesdeUltimoHeartbeat)}
                          </p>
                        </>
                      ) : (
                        <span className="text-muted-foreground">—</span>
                      )}
                    </td>
                    <td className="px-4 py-3">
                      {c.ultimaIngesta ? (
                        <>
                          <p className="text-foreground">
                            {new Date(c.ultimaIngesta).toLocaleString("es-EC", {
                              dateStyle: "short",
                              timeStyle: "short",
                            })}
                          </p>
                          <p className="text-xs text-muted-foreground">
                            {c.transaccionesTotales} históricas
                          </p>
                        </>
                      ) : (
                        <span className="text-muted-foreground">sin datos aún</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-right font-semibold">
                      {c.transaccionesUltimas24Horas}
                    </td>
                    <td className="px-4 py-3 text-right">
                      {c.pendientesAnalisis > 0 ? (
                        <span className="rounded-full bg-risk-medium/15 px-2.5 py-0.5 text-xs font-semibold text-risk-medium">
                          {c.pendientesAnalisis}
                        </span>
                      ) : (
                        <span className="text-muted-foreground">0</span>
                      )}
                    </td>
                    {puedeGestionar && (
                      <td className="px-6 py-3">
                        <div className="flex justify-end gap-2">
                          <button
                            onClick={() => iniciarEdicion(c)}
                            className="rounded-md border border-border p-2 text-muted-foreground hover:border-primary hover:text-primary"
                            title="Editar nombre y zona"
                          >
                            <Pencil size={14} />
                          </button>
                          {puedeEliminar && (
                            <button
                              onClick={() => {
                                if (
                                  confirm(
                                    `¿Eliminar la estación ${c.codigo} (${c.nombre})?\n\nSi tiene historial de alertas se desactivará en lugar de eliminarse.`,
                                  )
                                )
                                  eliminarMutation.mutate(c.estacionId);
                              }}
                              className="rounded-md border border-border p-2 text-muted-foreground hover:border-risk-critical hover:text-risk-critical"
                              title="Eliminar estación"
                            >
                              <Trash2 size={14} />
                            </button>
                          )}
                        </div>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </CardContent>
      </Card>
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
  if (estado === "En línea")
    return (
      <span className="inline-flex items-center gap-1.5 rounded-full bg-risk-low/15 px-2.5 py-1 text-xs font-semibold text-risk-low">
        <Wifi size={12} /> En línea
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
  if (minutos < 1) return "segundos";
  if (minutos < 60) return `${Math.round(minutos)} min`;
  if (minutos < 1440) return `${Math.floor(minutos / 60)} h ${Math.round(minutos % 60)} min`;
  return `${Math.floor(minutos / 1440)} días`;
}
