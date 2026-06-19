import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  fuentesDatosService,
  type FuenteDatosResponse,
} from "@/services/fuentesDatos.service";
import { useAuth } from "@/contexts/AuthContext";
import { esquemaService } from "@/services/esquema.service";
import { estacionesService } from "@/services/estaciones.service";
import { Card, CardContent, CardHeader } from "@/components/ui/Card";
import { Skeleton } from "@/components/ui/Skeleton";
import {
  Database,
  Plus,
  Trash2,
  Pencil,
  X,
  Save,
  Info,
  Search,
  DownloadCloud,
} from "lucide-react";

const inputClass =
  "rounded-md border border-border bg-background px-2.5 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40";

interface Formulario {
  nombre: string;
  tabla: string;
  columnaWatermark: string;
  descripcion: string;
  activa: boolean;
}

const formularioVacio: Formulario = {
  nombre: "",
  tabla: "",
  columnaWatermark: "",
  descripcion: "",
  activa: true,
};

export function FuentesDatosSection() {
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const esAdmin = user?.rol === "Administrador";

  const [editandoId, setEditandoId] = useState<number | "nueva" | null>(null);
  const [form, setForm] = useState<Formulario>(formularioVacio);
  const [error, setError] = useState<string | null>(null);
  const [estacionEsquema, setEstacionEsquema] = useState("");
  const [avisoEsquema, setAvisoEsquema] = useState<string | null>(null);

  const { data: fuentes, isLoading } = useQuery({
    queryKey: ["fuentes-datos"],
    queryFn: fuentesDatosService.getAll,
  });

  // Estaciones para elegir de cuál cargar el esquema (las conectadas tienen heartbeat reciente).
  const { data: estaciones } = useQuery({
    queryKey: ["estaciones"],
    queryFn: estacionesService.getAll,
    enabled: esAdmin,
  });

  const solicitarEsquemaMutation = useMutation({
    mutationFn: (codigo: string) => esquemaService.solicitar(codigo),
    onSuccess: () =>
      setAvisoEsquema(
        "Solicitud enviada. La estación reportará su esquema en unos segundos; vuelve a buscar la tabla.",
      ),
    onError: () => setAvisoEsquema("No se pudo solicitar el esquema."),
  });

  function estacionConectada(ultimoHeartbeat: string | null): boolean {
    if (!ultimoHeartbeat) return false;
    return Date.now() - new Date(ultimoHeartbeat).getTime() < 3 * 60 * 1000;
  }

  const invalidar = () =>
    void queryClient.invalidateQueries({ queryKey: ["fuentes-datos"] });

  const crearMutation = useMutation({
    mutationFn: fuentesDatosService.create,
    onSuccess: () => {
      invalidar();
      cerrar();
    },
    onError: (e: unknown) => setError(mensajeError(e)),
  });

  const actualizarMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Formulario }) =>
      fuentesDatosService.update(id, data),
    onSuccess: () => {
      invalidar();
      cerrar();
    },
    onError: (e: unknown) => setError(mensajeError(e)),
  });

  const eliminarMutation = useMutation({
    mutationFn: fuentesDatosService.remove,
    onSuccess: invalidar,
  });

  function cerrar() {
    setEditandoId(null);
    setForm(formularioVacio);
    setError(null);
  }

  function empezarNueva() {
    setForm(formularioVacio);
    setError(null);
    setEditandoId("nueva");
  }

  function empezarEdicion(f: FuenteDatosResponse) {
    setForm({
      nombre: f.nombre,
      tabla: f.tabla,
      columnaWatermark: f.columnaWatermark ?? "",
      descripcion: f.descripcion,
      activa: f.activa,
    });
    setError(null);
    setEditandoId(f.id);
  }

  function guardar() {
    setError(null);
    if (!form.nombre.trim() || !form.tabla.trim()) {
      setError("Nombre y tabla son obligatorios.");
      return;
    }
    if (editandoId === "nueva") {
      crearMutation.mutate({
        nombre: form.nombre,
        tabla: form.tabla,
        columnaWatermark: form.columnaWatermark || null,
        descripcion: form.descripcion || null,
      });
    } else if (typeof editandoId === "number") {
      actualizarMutation.mutate({ id: editandoId, data: form });
    }
  }

  const guardando = crearMutation.isPending || actualizarMutation.isPending;

  return (
    <Card>
      <CardHeader
        title="Fuentes de datos (tablas extra)"
        subtitle="Catálogo central de tablas adicionales de Firebird que todos los agentes extraen. Se registra una sola vez aquí; cada estación la recibe automáticamente."
        action={
          <div className="rounded-lg bg-primary/10 p-2 text-primary">
            <Database size={18} />
          </div>
        }
      />
      <CardContent className="space-y-4">
        <div className="flex items-start gap-3 rounded-lg border border-border bg-muted/40 p-3">
          <Info size={16} className="mt-0.5 shrink-0 text-muted-foreground" />
          <p className="text-xs text-muted-foreground">
            Estas tablas se suman a las estándar que el agente ya envía (turnos,
            despachos, facturas, anulaciones, créditos). Cada agente verifica que la
            tabla y la columna existan en <span className="font-medium">su</span> base
            antes de extraer, así que registrar una tabla que falte en alguna estación
            no causa errores: esa estación simplemente la omite. Para ver los campos de
            una tabla, búscala abajo o cárgala desde una estación conectada.
          </p>
        </div>

        {/* Cargar el esquema (tablas+columnas) desde una estación conectada. El central no toca
            Firebird; el agente de la estación elegida envía su esquema en su próximo latido. */}
        {esAdmin && (
          <div className="rounded-lg border border-border bg-background p-3">
            <p className="mb-2 flex items-center gap-2 text-xs font-medium text-foreground">
              <DownloadCloud size={14} className="text-primary" />
              Cargar el esquema de una estación (para poder buscar tablas y ver sus columnas)
            </p>
            <div className="flex flex-wrap items-center gap-2">
              <select
                className={`${inputClass} min-w-[220px]`}
                value={estacionEsquema}
                onChange={(e) => setEstacionEsquema(e.target.value)}
              >
                <option value="">Elige una estación…</option>
                {(estaciones ?? []).map((e) => (
                  <option key={e.id} value={e.codigo}>
                    {e.codigo} — {e.nombre}
                    {estacionConectada(e.ultimoHeartbeat) ? " (en línea)" : " (sin conexión)"}
                  </option>
                ))}
              </select>
              <button
                type="button"
                disabled={!estacionEsquema || solicitarEsquemaMutation.isPending}
                onClick={() => {
                  setAvisoEsquema(null);
                  solicitarEsquemaMutation.mutate(estacionEsquema);
                }}
                className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
              >
                <DownloadCloud size={14} /> Cargar esquema
              </button>
            </div>
            {avisoEsquema && (
              <p className="mt-2 text-xs text-muted-foreground">{avisoEsquema}</p>
            )}
          </div>
        )}

        {isLoading ? (
          <Skeleton className="h-24" />
        ) : (fuentes ?? []).length === 0 && editandoId !== "nueva" ? (
          <p className="text-sm text-muted-foreground">
            Aún no hay fuentes adicionales registradas.
          </p>
        ) : (
          <div className="divide-y divide-border rounded-lg border border-border">
            {(fuentes ?? []).map((f) =>
              editandoId === f.id ? (
                <FormularioFila
                  key={f.id}
                  form={form}
                  setForm={setForm}
                  onGuardar={guardar}
                  onCancelar={cerrar}
                  guardando={guardando}
                  error={error}
                />
              ) : (
                <div
                  key={f.id}
                  className={`flex flex-col gap-2 px-4 py-3 sm:flex-row sm:items-center sm:justify-between ${
                    !f.activa ? "opacity-60" : ""
                  }`}
                >
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <span className="font-medium text-foreground">{f.nombre}</span>
                      <span className="font-mono text-xs text-primary">{f.tabla}</span>
                      {!f.activa && (
                        <span className="rounded-full bg-muted px-2 py-0.5 text-[10px] font-semibold uppercase text-muted-foreground">
                          Inactiva
                        </span>
                      )}
                    </div>
                    <p className="mt-0.5 text-xs text-muted-foreground">
                      {f.descripcion || "Sin descripción"}
                      {f.columnaWatermark
                        ? ` · marca de agua: ${f.columnaWatermark}`
                        : " · tope de filas por ciclo"}
                    </p>
                  </div>
                  {esAdmin && (
                    <div className="flex shrink-0 items-center gap-2">
                      <button
                        onClick={() => empezarEdicion(f)}
                        className="rounded-md border border-border p-1.5 text-muted-foreground hover:border-primary hover:text-primary"
                        title="Editar"
                      >
                        <Pencil size={14} />
                      </button>
                      <button
                        onClick={() => {
                          if (
                            confirm(`¿Eliminar la fuente "${f.nombre}" (${f.tabla})?`)
                          )
                            eliminarMutation.mutate(f.id);
                        }}
                        className="rounded-md border border-border p-1.5 text-muted-foreground hover:border-risk-high hover:text-risk-high"
                        title="Eliminar"
                      >
                        <Trash2 size={14} />
                      </button>
                    </div>
                  )}
                </div>
              ),
            )}
            {editandoId === "nueva" && (
              <FormularioFila
                form={form}
                setForm={setForm}
                onGuardar={guardar}
                onCancelar={cerrar}
                guardando={guardando}
                error={error}
              />
            )}
          </div>
        )}

        {esAdmin && editandoId === null && (
          <button
            onClick={empezarNueva}
            className="inline-flex items-center gap-2 rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground hover:bg-primary/90"
          >
            <Plus size={16} /> Registrar tabla
          </button>
        )}
        {!esAdmin && (
          <p className="text-xs text-muted-foreground">
            Solo un Administrador puede registrar o editar fuentes.
          </p>
        )}
      </CardContent>
    </Card>
  );
}

function FormularioFila({
  form,
  setForm,
  onGuardar,
  onCancelar,
  guardando,
  error,
}: {
  form: Formulario;
  setForm: (f: Formulario) => void;
  onGuardar: () => void;
  onCancelar: () => void;
  guardando: boolean;
  error: string | null;
}) {
  // Buscador de tablas del esquema reportado por los agentes (para quien no conoce la base).
  const tabla = form.tabla.trim();
  const { data: sugerencias } = useQuery({
    queryKey: ["esquema-tablas", tabla],
    queryFn: () => esquemaService.buscarTablas(tabla),
    enabled: tabla.length >= 1,
    staleTime: 30_000,
  });
  const coincideExacta = (sugerencias ?? []).some(
    (s) => s.tabla.toUpperCase() === tabla.toUpperCase(),
  );
  const { data: detalle } = useQuery({
    queryKey: ["esquema-tabla", tabla],
    queryFn: () => esquemaService.getTabla(tabla),
    enabled: coincideExacta,
    retry: false,
    staleTime: 30_000,
  });

  return (
    <div className="space-y-3 bg-muted/30 px-4 py-4">
      <div className="grid gap-3 sm:grid-cols-2">
        <label className="flex flex-col gap-1 text-xs text-muted-foreground">
          Nombre lógico
          <input
            className={inputClass}
            placeholder="Ej: Tanques"
            value={form.nombre}
            onChange={(e) => setForm({ ...form, nombre: e.target.value })}
          />
        </label>
        <div className="flex flex-col gap-1 text-xs text-muted-foreground">
          Tabla de Firebird (busca por nombre)
          <div className="relative">
            <Search
              size={13}
              className="pointer-events-none absolute left-2 top-1/2 -translate-y-1/2 text-muted-foreground"
            />
            <input
              className={`${inputClass} w-full pl-7 font-mono`}
              placeholder="Escribe para buscar: TANQ, TURN…"
              value={form.tabla}
              onChange={(e) =>
                setForm({ ...form, tabla: e.target.value.toUpperCase() })
              }
            />
            {tabla.length >= 1 && !coincideExacta && (sugerencias ?? []).length > 0 && (
              <div className="absolute z-10 mt-1 max-h-44 w-full overflow-auto rounded-md border border-border bg-background shadow-lg">
                {(sugerencias ?? []).slice(0, 20).map((s) => (
                  <button
                    key={s.tabla}
                    type="button"
                    onClick={() =>
                      setForm({
                        ...form,
                        tabla: s.tabla,
                        nombre: form.nombre || capitalizar(s.tabla),
                      })
                    }
                    className="flex w-full items-center justify-between px-3 py-1.5 text-left font-mono text-xs hover:bg-muted"
                  >
                    <span>{s.tabla}</span>
                    <span className="text-[10px] text-muted-foreground">
                      {s.columnas} col.
                    </span>
                  </button>
                ))}
              </div>
            )}
          </div>
        </div>
        <label className="flex flex-col gap-1 text-xs text-muted-foreground">
          Columna de fecha (marca de agua, opcional)
          <input
            className={`${inputClass} font-mono`}
            placeholder="Ej: FECHA"
            value={form.columnaWatermark}
            onChange={(e) =>
              setForm({ ...form, columnaWatermark: e.target.value.toUpperCase() })
            }
          />
        </label>
        <label className="flex items-center gap-2 self-end text-sm text-foreground">
          <input
            type="checkbox"
            checked={form.activa}
            onChange={(e) => setForm({ ...form, activa: e.target.checked })}
          />
          Activa (los agentes la extraen)
        </label>
      </div>

      {/* Documentación automática de la tabla seleccionada (columnas reportadas por el agente) */}
      {detalle && detalle.columnas.length > 0 && (
        <div className="rounded-lg border border-border bg-background p-3">
          <p className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">
            Columnas de {detalle.tabla} ({detalle.columnas.length})
          </p>
          <div className="grid gap-x-4 gap-y-1 sm:grid-cols-2">
            {detalle.columnas.map((c) => (
              <button
                key={c.nombre}
                type="button"
                onClick={() => setForm({ ...form, columnaWatermark: c.nombre })}
                className="flex items-center justify-between rounded px-1.5 py-0.5 text-left text-xs hover:bg-muted"
                title="Usar como columna de fecha (marca de agua)"
              >
                <span className="font-mono text-foreground">{c.nombre}</span>
                <span className="font-mono text-[10px] text-primary">{c.tipo}</span>
              </button>
            ))}
          </div>
        </div>
      )}

      <label className="flex flex-col gap-1 text-xs text-muted-foreground">
        Descripción
        <input
          className={inputClass}
          placeholder="Para qué sirve esta tabla / qué regla la usa"
          value={form.descripcion}
          onChange={(e) => setForm({ ...form, descripcion: e.target.value })}
        />
      </label>
      {error && <p className="text-xs text-risk-high">{error}</p>}
      <div className="flex items-center gap-2">
        <button
          onClick={onGuardar}
          disabled={guardando}
          className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-60"
        >
          <Save size={14} /> Guardar
        </button>
        <button
          onClick={onCancelar}
          className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-sm text-muted-foreground hover:bg-muted"
        >
          <X size={14} /> Cancelar
        </button>
      </div>
    </div>
  );
}

/** "TANQ_REPO" → "Tanq repo" (sugerencia de nombre lógico legible). */
function capitalizar(tabla: string): string {
  const t = tabla.toLowerCase().replace(/_/g, " ").trim();
  return t.charAt(0).toUpperCase() + t.slice(1);
}

function mensajeError(e: unknown): string {
  if (
    typeof e === "object" &&
    e !== null &&
    "response" in e &&
    typeof (e as { response?: unknown }).response === "object"
  ) {
    const resp = (e as { response?: { data?: { mensaje?: string } } }).response;
    if (resp?.data?.mensaje) return resp.data.mensaje;
  }
  return "No se pudo guardar la fuente.";
}
