import { useMemo, useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { reglasService } from "@/services/reglas.service";
import { Skeleton } from "@/components/ui/Skeleton";
import { ReglasPersonalizadasSection } from "@/components/reglas/ReglasPersonalizadasSection";
import type { ReglaDeteccionResponse, ActualizarReglaRequest } from "@/types/regla";
import { TIPO_DETECTOR_LABELS } from "@/types/alert";
import type { TipoDetector } from "@/types/alert";
import {
  Banknote,
  ReceiptText,
  CreditCard,
  ShieldAlert,
  Check,
  X,
  Pencil,
  Search,
  ChevronDown,
  SlidersHorizontal,
  Wand2,
  Building2,
  ShieldCheck,
  Layers,
  Bell,
  BellOff,
} from "lucide-react";
import type { ReactNode } from "react";

const ORDEN_DETECTORES: TipoDetector[] = [
  "CashFraud",
  "InvoiceAnomaly",
  "PaymentFraud",
  "ComplianceViolation",
];

const DETECTOR_META: Partial<
  Record<TipoDetector, { icono: ReactNode; color: string; descripcion: string }>
> = {
  CashFraud: {
    icono: <Banknote size={18} />,
    color: "text-risk-high bg-risk-high/10",
    descripcion: "Manejo de efectivo: faltantes, gineteo y ventas registradas como crédito.",
  },
  InvoiceAnomaly: {
    icono: <ReceiptText size={18} />,
    color: "text-primary bg-primary/10",
    descripcion: "Facturas: anulaciones, precios, descuentos y aritmética del comprobante.",
  },
  PaymentFraud: {
    icono: <CreditCard size={18} />,
    color: "text-risk-medium bg-risk-medium/10",
    descripcion: "Medios de pago: reversiones, duplicados, créditos y despachos sospechosos.",
  },
  ComplianceViolation: {
    icono: <ShieldAlert size={18} />,
    color: "text-risk-critical bg-risk-critical/10",
    descripcion: "Normativa ARCERNNR/SRI: placa genérica, trazabilidad y combustibles.",
  },
};

type Pestana = "motor" | "personalizadas";

export function ReglasPage() {
  const queryClient = useQueryClient();
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editValue, setEditValue] = useState("");
  const [pestana, setPestana] = useState<Pestana>("motor");
  const [busqueda, setBusqueda] = useState("");
  const [colapsados, setColapsados] = useState<Set<string>>(new Set());

  const { data: reglas, isLoading } = useQuery({
    queryKey: ["reglas"],
    queryFn: reglasService.getAll,
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: ActualizarReglaRequest }) =>
      reglasService.update(id, data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["reglas"] });
      setEditingId(null);
    },
  });

  function startEdit(regla: ReglaDeteccionResponse) {
    setEditingId(regla.id);
    setEditValue(regla.valorUmbral.toString());
  }

  function saveEdit(id: number) {
    const val = parseFloat(editValue);
    if (!isNaN(val)) updateMutation.mutate({ id, data: { valorUmbral: val } });
  }

  function toggleGrupo(detector: string) {
    setColapsados((prev) => {
      const next = new Set(prev);
      if (next.has(detector)) next.delete(detector);
      else next.add(detector);
      return next;
    });
  }

  const q = busqueda.trim().toLowerCase();
  const grupos = useMemo(() => {
    return ORDEN_DETECTORES.map((detector) => ({
      detector,
      reglas: (reglas ?? [])
        .filter((r) => r.tipoDetector === detector)
        .filter(
          (r) =>
            q === "" ||
            r.nombre.toLowerCase().includes(q) ||
            r.descripcion.toLowerCase().includes(q),
        ),
    })).filter((g) => g.reglas.length > 0);
  }, [reglas, q]);

  const totalReglas = reglas?.length ?? 0;
  const activas = (reglas ?? []).filter((r) => r.activa).length;
  const operativas = (reglas ?? []).filter((r) => r.ambito === "Operativa").length;
  const ambos = (reglas ?? []).filter((r) => r.ambito === "Ambos").length;
  const auditoria = totalReglas - operativas - ambos;

  return (
    <div className="space-y-6">
      <div className="flex items-start gap-3">
        <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
          <SlidersHorizontal size={22} />
        </div>
        <div>
          <h1 className="text-2xl font-bold text-foreground">Configuración del Motor de Detección</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Ajuste los umbrales, active o desactive reglas y elija su carril.
          </p>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
        <ResumenCard label="Reglas del motor" valor={totalReglas} />
        <ResumenCard label="Activas" valor={activas} acento="ok" />
        <ResumenCard label="Oper. / Audit. / Ambos" valor={`${operativas} / ${auditoria} / ${ambos}`} />
        <ResumenCard label="Detectores" valor={ORDEN_DETECTORES.length} />
      </div>

      <div className="flex gap-1 border-b border-border">
        <PestanaBtn activa={pestana === "motor"} onClick={() => setPestana("motor")} icon={<SlidersHorizontal size={16} />}>
          Motor de detección
        </PestanaBtn>
        <PestanaBtn activa={pestana === "personalizadas"} onClick={() => setPestana("personalizadas")} icon={<Wand2 size={16} />}>
          Reglas personalizadas
        </PestanaBtn>
      </div>

      {pestana === "personalizadas" ? (
        <ReglasPersonalizadasSection />
      ) : isLoading ? (
        <div className="space-y-3">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-16" />
          ))}
        </div>
      ) : (
        <div className="space-y-4">
          {/* Barra de herramientas: buscar + leyenda de carriles */}
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div className="relative w-full sm:max-w-xs">
              <Search size={15} className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" />
              <input
                value={busqueda}
                onChange={(e) => setBusqueda(e.target.value)}
                placeholder="Buscar regla…"
                className="w-full rounded-lg border border-border bg-background py-2 pl-9 pr-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20"
              />
            </div>
            <div className="flex items-center gap-4 text-xs text-muted-foreground">
              <span className="inline-flex items-center gap-1.5">
                <Building2 size={13} className="text-amber-500" /> Operativa = avisa a la estación
              </span>
              <span className="inline-flex items-center gap-1.5">
                <ShieldCheck size={13} className="text-primary" /> Auditoría = a revisar en el central
              </span>
            </div>
          </div>

          {grupos.length === 0 && (
            <p className="rounded-lg border border-dashed border-border py-10 text-center text-sm text-muted-foreground">
              Ninguna regla coincide con «{busqueda}».
            </p>
          )}

          {grupos.map(({ detector, reglas: reglasGrupo }) => {
            const meta = DETECTOR_META[detector];
            if (!meta) return null;
            const activasGrupo = reglasGrupo.filter((r) => r.activa).length;
            const colapsado = colapsados.has(detector) && q === "";
            return (
              <div key={detector} className="overflow-hidden rounded-xl border border-border bg-background">
                <button
                  onClick={() => toggleGrupo(detector)}
                  className="flex w-full items-center gap-3 px-4 py-3 text-left transition-colors hover:bg-muted/40"
                >
                  <div className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-lg ${meta.color}`}>
                    {meta.icono}
                  </div>
                  <div className="min-w-0 flex-1">
                    <p className="font-semibold text-foreground">{TIPO_DETECTOR_LABELS[detector]}</p>
                    <p className="truncate text-xs text-muted-foreground">{meta.descripcion}</p>
                  </div>
                  <span className="shrink-0 rounded-full bg-muted px-2.5 py-1 text-xs font-medium text-muted-foreground">
                    {activasGrupo}/{reglasGrupo.length} activas
                  </span>
                  <ChevronDown
                    size={18}
                    className={`shrink-0 text-muted-foreground transition-transform ${colapsado ? "-rotate-90" : ""}`}
                  />
                </button>

                {!colapsado && (
                  <div className="divide-y divide-border border-t border-border">
                    {reglasGrupo.map((regla) => (
                      <div
                        key={regla.id}
                        className={`flex flex-col gap-3 px-4 py-3 sm:flex-row sm:items-center sm:justify-between ${
                          !regla.activa ? "opacity-55" : ""
                        }`}
                      >
                        <div className="min-w-0 flex-1">
                          <div className="flex flex-wrap items-center gap-2">
                            <p className="text-sm font-medium text-foreground">{regla.nombre}</p>
                            <CarrilChip
                              regla={regla}
                              pendiente={updateMutation.isPending}
                              onCambiar={(ambito) => updateMutation.mutate({ id: regla.id, data: { ambito } })}
                            />
                          </div>
                          <p className="mt-0.5 truncate text-xs text-muted-foreground" title={`${regla.descripcion} · parámetro: ${regla.parametroNombre}`}>
                            {regla.descripcion}
                          </p>
                        </div>

                        <div className="flex shrink-0 items-center gap-3">
                          {editingId === regla.id ? (
                            <div
                              className="flex items-center gap-1.5"
                              title={regla.ayudaUmbral || undefined}
                            >
                              <input
                                type="number"
                                value={editValue}
                                onChange={(e) => setEditValue(e.target.value)}
                                onKeyDown={(e) => {
                                  if (e.key === "Enter") saveEdit(regla.id);
                                  if (e.key === "Escape") setEditingId(null);
                                }}
                                autoFocus
                                className="w-20 rounded-md border border-primary bg-background px-2 py-1 text-sm outline-none"
                                step="0.01"
                              />
                              {regla.unidad && regla.unidad !== "valor" && (
                                <span className="whitespace-nowrap text-[11px] font-medium text-muted-foreground">
                                  {regla.unidad}
                                </span>
                              )}
                              <button
                                onClick={() => saveEdit(regla.id)}
                                disabled={updateMutation.isPending}
                                className="rounded-md bg-primary p-1.5 text-primary-foreground hover:bg-primary/90"
                                title="Guardar"
                              >
                                <Check size={14} />
                              </button>
                              <button
                                onClick={() => setEditingId(null)}
                                className="rounded-md border border-border p-1.5 text-muted-foreground hover:bg-muted"
                                title="Cancelar"
                              >
                                <X size={14} />
                              </button>
                            </div>
                          ) : (
                            <button
                              onClick={() => startEdit(regla)}
                              className="group flex items-center gap-2 rounded-md px-2.5 py-1.5 hover:bg-muted"
                              title={`${regla.ayudaUmbral ? regla.ayudaUmbral + " " : ""}(unidad: ${regla.unidad}) · parámetro técnico: ${regla.parametroNombre}`}
                            >
                              <span className="text-xs text-muted-foreground">umbral</span>
                              <span className="font-mono text-sm font-semibold text-foreground">{regla.valorUmbral}</span>
                              {regla.unidad && regla.unidad !== "valor" && (
                                <span className="whitespace-nowrap text-[11px] font-medium text-muted-foreground">
                                  {regla.unidad}
                                </span>
                              )}
                              <Pencil size={11} className="text-muted-foreground/40 group-hover:text-primary" />
                            </button>
                          )}

                          <button
                            onClick={() =>
                              updateMutation.mutate({
                                id: regla.id,
                                data: { notificarCorreo: !regla.notificarCorreo },
                              })
                            }
                            disabled={updateMutation.isPending}
                            title={
                              regla.notificarCorreo
                                ? "Avisar por correo: ACTIVADO. Esta regla manda un correo a supervisores/administradores cuando se dispara. Clic para desactivar."
                                : "Avisar por correo cuando esta regla se dispare (además de las críticas). Clic para activar."
                            }
                            className={`rounded-md p-1.5 transition-colors disabled:opacity-50 ${
                              regla.notificarCorreo
                                ? "text-primary hover:bg-primary/10"
                                : "text-muted-foreground/40 hover:bg-muted hover:text-muted-foreground"
                            }`}
                          >
                            {regla.notificarCorreo ? <Bell size={15} /> : <BellOff size={15} />}
                          </button>

                          <button
                            role="switch"
                            aria-checked={regla.activa}
                            onClick={() => updateMutation.mutate({ id: regla.id, data: { activa: !regla.activa } })}
                            className={`relative h-6 w-11 shrink-0 rounded-full transition-colors ${
                              regla.activa ? "bg-risk-low" : "bg-muted-foreground/30"
                            }`}
                            title={regla.activa ? "Desactivar regla" : "Activar regla"}
                          >
                            <span
                              className={`absolute top-0.5 h-5 w-5 rounded-full bg-white shadow transition-all ${
                                regla.activa ? "left-[22px]" : "left-0.5"
                              }`}
                            />
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

type Carril = "Operativa" | "Auditoria" | "Ambos";

const CARRIL_ESTILO: Record<Carril, { label: string; icon: ReactNode; clase: string }> = {
  Operativa: {
    label: "Operativa",
    icon: <Building2 size={11} />,
    clase: "bg-amber-500/10 text-amber-600 ring-amber-500/30 hover:bg-amber-500/20 dark:text-amber-400",
  },
  Auditoria: {
    label: "Auditoría",
    icon: <ShieldCheck size={11} />,
    clase: "bg-primary/10 text-primary ring-primary/30 hover:bg-primary/20",
  },
  Ambos: {
    label: "Ambos",
    icon: <Layers size={11} />,
    clase: "bg-violet-500/10 text-violet-600 ring-violet-500/30 hover:bg-violet-500/20 dark:text-violet-400",
  },
};

/** Chip del carril, clicable para ciclar Operativa → Auditoría → Ambos. */
function CarrilChip({
  regla,
  pendiente,
  onCambiar,
}: {
  regla: ReglaDeteccionResponse;
  pendiente: boolean;
  onCambiar: (ambito: Carril) => void;
}) {
  const siguiente: Record<Carril, Carril> = {
    Operativa: "Auditoria",
    Auditoria: "Ambos",
    Ambos: "Operativa",
  };
  const actual: Carril = (["Operativa", "Auditoria", "Ambos"] as Carril[]).includes(
    regla.ambito as Carril,
  )
    ? (regla.ambito as Carril)
    : "Auditoria";
  const e = CARRIL_ESTILO[actual];
  const destino = siguiente[actual];
  const destinoLabel = CARRIL_ESTILO[destino].label;
  return (
    <button
      onClick={() => onCambiar(destino)}
      disabled={pendiente}
      title={`Carril ${e.label} · clic para cambiar a ${destinoLabel}. Operativa = avisa a la estación; Auditoría = bandeja del central; Ambos = los dos a la vez.`}
      className={`inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide ring-1 ring-inset transition-colors disabled:opacity-50 ${e.clase}`}
    >
      {e.icon}
      {e.label}
    </button>
  );
}

function ResumenCard({
  label,
  valor,
  acento,
}: {
  label: string;
  valor: number | string;
  acento?: "ok" | "muted";
}) {
  const color =
    acento === "ok" ? "text-risk-low" : acento === "muted" ? "text-muted-foreground" : "text-foreground";
  return (
    <div className="rounded-xl border border-border bg-background p-4">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className={`mt-1 text-2xl font-bold ${color}`}>{valor}</p>
    </div>
  );
}

function PestanaBtn({
  activa,
  onClick,
  icon,
  children,
}: {
  activa: boolean;
  onClick: () => void;
  icon: ReactNode;
  children: ReactNode;
}) {
  return (
    <button
      onClick={onClick}
      className={`-mb-px flex items-center gap-2 border-b-2 px-4 py-2.5 text-sm font-medium transition-colors ${
        activa ? "border-primary text-foreground" : "border-transparent text-muted-foreground hover:text-foreground"
      }`}
    >
      {icon}
      {children}
    </button>
  );
}
