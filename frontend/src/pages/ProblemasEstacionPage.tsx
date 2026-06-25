import { useNavigate, useSearchParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { alertasService } from "@/services/alertas.service";
import type { ProblemaEstacionGrupo } from "@/types/alert";
import { Wrench, ChevronDown, ChevronRight, MapPin } from "lucide-react";
import { Spinner } from "@/components/ui/Spinner";

const DIAS_OPCIONES = [1, 7, 30];

export function ProblemasEstacionPage() {
  const navigate = useNavigate();
  // Estado de la vista en la URL: al abrir el detalle de un problema y volver, la estación sigue
  // expandida y con el mismo rango de días, sin perder el contexto de revisión.
  const [searchParams, setSearchParams] = useSearchParams();
  const dias = Number(searchParams.get("dias")) || 7;
  const abierto = searchParams.get("g");

  const setDias = (d: number) =>
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev);
        next.set("dias", String(d));
        next.delete("g");
        return next;
      },
      { replace: true },
    );

  const alternarAbierto = (clave: string) =>
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev);
        if (next.get("g") === clave) next.delete("g");
        else next.set("g", clave);
        return next;
      },
      { replace: true },
    );

  const { data, isLoading, isError } = useQuery({
    queryKey: ["problemas-estacion", dias],
    queryFn: () => alertasService.getProblemasEstacion(undefined, dias),
    refetchInterval: 30_000,
  });

  const grupos = data ?? [];
  const totalProblemas = grupos.reduce((acc, g) => acc + g.total, 0);

  const claveGrupo = (g: ProblemaEstacionGrupo) =>
    `${g.estacionId}-${g.fecha}`;

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="flex items-center gap-2 text-2xl font-bold text-foreground">
            <Wrench size={24} className="text-primary" />
            Problemas de estación
          </h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Errores operativos del día a día (turnos sin cerrar, despachos mal
            cerrados, campos faltantes) agrupados por estación. Se notifican en
            tiempo real al administrador de cada estación para corregirlos al
            instante.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <span className="text-sm text-muted-foreground">Últimos</span>
          <select
            value={dias}
            onChange={(e) => setDias(Number(e.target.value))}
            className="rounded-md border border-border bg-background px-2 py-1.5 text-sm"
          >
            {DIAS_OPCIONES.map((d) => (
              <option key={d} value={d}>
                {d === 1 ? "1 día" : `${d} días`}
              </option>
            ))}
          </select>
        </div>
      </div>

      {isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {isError && (
        <div className="rounded-lg border border-destructive/30 bg-destructive/5 p-4 text-sm text-destructive">
          No se pudieron cargar los problemas de estación.
        </div>
      )}

      {!isLoading && !isError && grupos.length === 0 && (
        <div className="rounded-lg border border-border bg-muted/30 p-10 text-center">
          <Wrench className="mx-auto mb-3 text-muted-foreground/50" size={40} />
          <p className="text-sm text-muted-foreground">
            Sin problemas operativos en el período. Todo en orden.
          </p>
        </div>
      )}

      {!isLoading && grupos.length > 0 && (
        <>
          <p className="text-sm text-muted-foreground">
            {totalProblemas} problema(s) en {grupos.length} grupo(s) estación/día.
          </p>
          <div className="space-y-2">
            {grupos.map((g) => {
              const clave = claveGrupo(g);
              const expandido = abierto === clave;
              return (
                <div
                  key={clave}
                  className="overflow-hidden rounded-lg border border-border bg-background"
                >
                  <button
                    type="button"
                    onClick={() => alternarAbierto(clave)}
                    className="flex w-full items-center justify-between gap-3 px-4 py-3 text-left transition-colors hover:bg-muted/50"
                  >
                    <span className="flex items-center gap-2 font-medium text-foreground">
                      {expandido ? (
                        <ChevronDown size={18} className="text-muted-foreground" />
                      ) : (
                        <ChevronRight size={18} className="text-muted-foreground" />
                      )}
                      <MapPin size={16} className="text-primary" />
                      {g.estacionNombre || `Estación ${g.estacionId}`}
                      <span className="text-xs font-normal text-muted-foreground">
                        · {formatearFechaLocal(g.fecha)}
                      </span>
                    </span>
                    <span className="inline-flex min-w-7 items-center justify-center rounded-full bg-amber-500/15 px-2 py-0.5 text-xs font-semibold text-amber-600">
                      {g.total}
                    </span>
                  </button>

                  {expandido && (
                    <ul className="divide-y divide-border border-t border-border">
                      {g.problemas.map((p) => (
                        <li key={p.id}>
                          <button
                            type="button"
                            onClick={() =>
                              navigate(`/alertas/${p.id}`, {
                                state: {
                                  volverA: `/problemas-estacion?${searchParams.toString()}`,
                                  volverLabel: "Volver a problemas de estación",
                                },
                              })
                            }
                            title="Ver el detalle de este problema"
                            className="group block w-full px-4 py-2.5 pl-11 text-left transition-colors hover:bg-muted/40"
                          >
                            <p className="text-sm text-foreground">{p.descripcion}</p>
                            <p className="mt-0.5 text-xs text-muted-foreground">
                              {new Date(p.fechaDeteccion).toLocaleString()}
                              {p.empleadoNombre
                                ? ` · 👤 ${p.empleadoNombre}${p.empleadoCodigo ? ` (${p.empleadoCodigo})` : ""}`
                                : p.empleadoCodigo
                                  ? ` · empleado ${p.empleadoCodigo}`
                                  : ""}
                              {p.transaccionReferencia ? ` · ${p.transaccionReferencia}` : ""}
                              <span className="ml-1.5 text-primary opacity-0 transition-opacity group-hover:opacity-100">
                                Ver detalle →
                              </span>
                            </p>
                          </button>
                        </li>
                      ))}
                    </ul>
                  )}
                </div>
              );
            })}
          </div>
        </>
      )}
    </div>
  );
}

function formatearFechaLocal(fechaIso: string): string {
  const [anio, mes, dia] = fechaIso.slice(0, 10).split("-").map(Number);
  if (!anio || !mes || !dia) return fechaIso;
  return new Date(anio, mes - 1, dia).toLocaleDateString();
}
