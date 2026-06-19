import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { reglasService } from "@/services/reglas.service";
import { Skeleton } from "@/components/ui/Skeleton";
import { Card, CardContent, CardHeader } from "@/components/ui/Card";
import { ReglasPersonalizadasSection } from "@/components/reglas/ReglasPersonalizadasSection";
import { FuentesDatosSection } from "@/components/reglas/FuentesDatosSection";
import type { ReglaDeteccionResponse } from "@/types/regla";
import { TIPO_DETECTOR_LABELS } from "@/types/alert";
import type { TipoDetector } from "@/types/alert";
import {
  Banknote,
  ReceiptText,
  CreditCard,
  ShieldAlert,
  Save,
  X,
  Pencil,
  Info,
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
    descripcion:
      "Irregularidades en el manejo de efectivo: faltantes, gineteo y ventas registradas como crédito.",
  },
  InvoiceAnomaly: {
    icono: <ReceiptText size={18} />,
    color: "text-primary bg-primary/10",
    descripcion:
      "Discrepancias documentales: anulaciones, precios, descuentos y aritmética de la factura.",
  },
  PaymentFraud: {
    icono: <CreditCard size={18} />,
    color: "text-risk-medium bg-risk-medium/10",
    descripcion:
      "Manipulación de medios de pago: reversiones, duplicados, créditos y despachos sospechosos.",
  },
  ComplianceViolation: {
    icono: <ShieldAlert size={18} />,
    color: "text-risk-critical bg-risk-critical/10",
    descripcion:
      "Incumplimiento normativo ARCERNNR/SRI: placa genérica, trazabilidad y combustibles.",
  },
};

export function ReglasPage() {
  const queryClient = useQueryClient();
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editValue, setEditValue] = useState("");

  const { data: reglas, isLoading } = useQuery({
    queryKey: ["reglas"],
    queryFn: reglasService.getAll,
  });

  const updateMutation = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: number;
      data: { valorUmbral?: number | null; activa?: boolean | null };
    }) => reglasService.update(id, data),
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
    if (!isNaN(val)) {
      updateMutation.mutate({ id, data: { valorUmbral: val } });
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-48" />
        ))}
      </div>
    );
  }

  const grupos = ORDEN_DETECTORES.map((detector) => ({
    detector,
    reglas: (reglas ?? []).filter((r) => r.tipoDetector === detector),
  })).filter((g) => g.reglas.length > 0);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground">
          Configuración del Motor de Detección
        </h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Parametrice las reglas del motor y cree sus propias reglas de negocio
        </p>
      </div>

      <div className="flex items-start gap-3 rounded-xl border border-primary/30 bg-primary/5 p-4">
        <Info size={18} className="mt-0.5 shrink-0 text-primary" />
        <p className="text-sm text-muted-foreground">
          Las reglas del motor se{" "}
          <span className="font-medium text-foreground">
            parametrizan (umbral) y se habilitan o deshabilitan
          </span>{" "}
          desde esta pantalla. Además, en{" "}
          <span className="font-medium text-foreground">Reglas personalizadas</span>{" "}
          puede crear nuevas reglas de negocio adaptadas a sus necesidades sin
          modificar el sistema. Cada regla indica su carril:{" "}
          <span className="font-medium text-amber-600 dark:text-amber-400">Operativa</span>{" "}
          (problema de estación → avisa al administrador de la estación) o{" "}
          <span className="font-medium text-primary">Auditoría</span> (fraude → central).
          Todo cambio se aplica en el siguiente ciclo de análisis y queda registrado
          en los logs de auditoría.
        </p>
      </div>

      <FuentesDatosSection />

      <ReglasPersonalizadasSection />

      {grupos.map(({ detector, reglas: reglasGrupo }) => {
        const meta = DETECTOR_META[detector];
        if (!meta) return null;
        const activas = reglasGrupo.filter((r) => r.activa).length;
        return (
          <Card key={detector}>
            <CardHeader
              title={TIPO_DETECTOR_LABELS[detector]}
              subtitle={meta.descripcion}
              action={
                <div className="flex items-center gap-3">
                  <span className="text-xs text-muted-foreground">
                    {activas}/{reglasGrupo.length} activas
                  </span>
                  <div className={`rounded-lg p-2 ${meta.color}`}>{meta.icono}</div>
                </div>
              }
            />
            <CardContent className="p-0">
              {reglasGrupo.map((regla, idx) => (
                <div
                  key={regla.id}
                  className={`flex flex-col gap-3 px-6 py-4 sm:flex-row sm:items-center sm:justify-between ${
                    idx > 0 ? "border-t border-border" : ""
                  } ${!regla.activa ? "opacity-60" : ""}`}
                >
                  <div className="min-w-0 flex-1">
                    <div className="flex flex-wrap items-center gap-2">
                      <p className="font-medium text-foreground">{regla.nombre}</p>
                      {regla.ambito === "Operativa" ? (
                        <span
                          className="inline-flex items-center rounded-full bg-amber-500/10 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-amber-600 dark:text-amber-400"
                          title="Genera un problema de estación (se avisa al administrador de la estación)"
                        >
                          Operativa
                        </span>
                      ) : (
                        <span
                          className="inline-flex items-center rounded-full bg-primary/10 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-primary"
                          title="Genera una alerta de auditoría/fraude (carril central)"
                        >
                          Auditoría
                        </span>
                      )}
                    </div>
                    <p className="mt-0.5 text-xs text-muted-foreground">
                      {regla.descripcion}
                    </p>
                    <p className="mt-1 font-mono text-[10px] text-muted-foreground/60">
                      {regla.parametroNombre}
                    </p>
                  </div>

                  <div className="flex shrink-0 items-center gap-4">
                    {/* Umbral */}
                    {editingId === regla.id ? (
                      <div className="flex items-center gap-1.5">
                        <input
                          type="number"
                          value={editValue}
                          onChange={(e) => setEditValue(e.target.value)}
                          onKeyDown={(e) => {
                            if (e.key === "Enter") saveEdit(regla.id);
                            if (e.key === "Escape") setEditingId(null);
                          }}
                          autoFocus
                          className="w-24 rounded-md border border-primary bg-background px-2 py-1.5 text-sm focus:outline-none"
                          step="0.01"
                        />
                        <button
                          onClick={() => saveEdit(regla.id)}
                          disabled={updateMutation.isPending}
                          className="rounded-md bg-primary p-1.5 text-primary-foreground hover:bg-primary/90"
                          title="Guardar"
                        >
                          <Save size={14} />
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
                        className="group flex items-center gap-2 rounded-md border border-border px-3 py-1.5 hover:border-primary"
                        title="Editar umbral"
                      >
                        <span className="text-xs text-muted-foreground">Umbral</span>
                        <span className="font-mono text-sm font-semibold text-foreground">
                          {regla.valorUmbral}
                        </span>
                        <Pencil
                          size={12}
                          className="text-muted-foreground/50 group-hover:text-primary"
                        />
                      </button>
                    )}

                    {/* Switch activa */}
                    <button
                      role="switch"
                      aria-checked={regla.activa}
                      onClick={() =>
                        updateMutation.mutate({
                          id: regla.id,
                          data: { activa: !regla.activa },
                        })
                      }
                      className={`relative h-6 w-11 rounded-full transition-colors ${
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
                    <span
                      className={`w-14 text-xs font-semibold ${
                        regla.activa ? "text-risk-low" : "text-muted-foreground"
                      }`}
                    >
                      {regla.activa ? "Activa" : "Inactiva"}
                    </span>
                  </div>
                </div>
              ))}
            </CardContent>
          </Card>
        );
      })}
    </div>
  );
}
