import { useState } from "react";
import type { FormEvent } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { alertasService } from "@/services/alertas.service";
import { usuariosService } from "@/services/usuarios.service";
import { Badge } from "@/components/ui/Badge";
import { Skeleton } from "@/components/ui/Skeleton";
import { useAuth } from "@/contexts/AuthContext";
import {
  TIPO_DETECTOR_LABELS,
  NIVEL_RIESGO_LABELS,
  ESTADO_ALERTA_LABELS,
} from "@/types/alert";
import {
  ArrowLeft,
  MessageSquare,
  Send,
  UserPlus,
  FileWarning,
} from "lucide-react";

const STATE_TRANSITIONS: Record<string, string[]> = {
  Nueva: ["EnRevision"],
  EnRevision: ["Confirmada", "FalsoPositivo"],
  Confirmada: ["Cerrada"],
  FalsoPositivo: ["Cerrada"],
  Cerrada: [],
};

const ACTION_STYLES: Record<string, { label: string; className: string }> = {
  EnRevision: {
    label: "Tomar en Revisión",
    className: "bg-yellow-500 hover:bg-yellow-600 text-white",
  },
  Confirmada: {
    label: "Confirmar Alerta",
    className: "bg-red-500 hover:bg-red-600 text-white",
  },
  FalsoPositivo: {
    label: "Marcar Falso Positivo",
    className: "bg-gray-500 hover:bg-gray-600 text-white",
  },
  Cerrada: {
    label: "Cerrar Alerta",
    className: "bg-green-600 hover:bg-green-700 text-white",
  },
};

const METADATA_LABELS: Record<string, string> = {
  NumeroTurno: "Número de turno",
  VentasEfectivo: "Ventas en efectivo",
  DepositosEfectivo: "Depósitos en efectivo",
  Diferencia: "Diferencia",
  Umbral: "Umbral configurado",
  TotalFaltantes: "Total de faltantes",
  AlertasPrevias: "Alertas previas",
  MontoTotalFaltantes: "Monto total de faltantes",
  NumeroDocumento: "Número de documento",
  PorcentajeEfectivo: "% en efectivo",
  UmbralPorcentaje: "Umbral %",
  MontoEfectivo: "Monto en efectivo",
  CantidadTransacciones: "Cantidad de transacciones",
  DuracionMinutos: "Duración (min)",
  UmbralMinutos: "Umbral (min)",
  MontoTotal: "Monto total",
  Placa: "Placa",
  Galones: "Galones",
  GalonesMaximo: "Galones máximo",
  Monto: "Monto",
  MontoMinimo: "Monto mínimo",
  Cliente: "Cliente",
  Producto: "Producto",
  PrecioAplicado: "Precio aplicado",
  PrecioAutorizado: "Precio autorizado",
  MontoDescuento: "Monto de descuento",
  PorcentajeDescuento: "% de descuento",
  PorcentajeMaximo: "% máximo permitido",
  Subtotal: "Subtotal",
  TotalRegistrado: "Total registrado",
  TotalEsperado: "Total esperado",
  Iva: "IVA",
  Descuento: "Descuento",
  MontoCredito: "Monto del crédito",
  CodigoPago: "Código de pago",
  CodigoBanco: "Código de banco",
  MontoReversion: "Monto de reversión",
  DiferenciaMinutos: "Diferencia (min)",
};

export function DetalleAlertaPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const [assignAuditorId, setAssignAuditorId] = useState("");
  const [nuevoComentario, setNuevoComentario] = useState("");

  const alertaId = Number(id);
  const canAssign = user?.rol === "Supervisor" || user?.rol === "Administrador";

  const { data: alerta, isLoading } = useQuery({
    queryKey: ["alertas", alertaId],
    queryFn: () => alertasService.getById(alertaId),
    enabled: !!id,
  });

  const { data: comentarios } = useQuery({
    queryKey: ["alertas", alertaId, "comentarios"],
    queryFn: () => alertasService.getComentarios(alertaId),
    enabled: !!id,
  });

  const { data: auditores } = useQuery({
    queryKey: ["usuarios", "auditores"],
    queryFn: usuariosService.getAuditores,
    enabled: canAssign,
  });

  const cambiarEstadoMutation = useMutation({
    mutationFn: (nuevoEstado: string) =>
      alertasService.cambiarEstado(alertaId, { estado: nuevoEstado }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["alertas"] });
      void queryClient.invalidateQueries({ queryKey: ["dashboard"] });
    },
  });

  const asignarMutation = useMutation({
    mutationFn: () =>
      alertasService.asignar(alertaId, {
        auditorId: Number(assignAuditorId),
        comentario: null,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["alertas"] });
      setAssignAuditorId("");
    },
  });

  const comentarioMutation = useMutation({
    mutationFn: () =>
      alertasService.agregarComentario(alertaId, { texto: nuevoComentario }),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: ["alertas", alertaId, "comentarios"],
      });
      setNuevoComentario("");
    },
  });

  function handleComentarioSubmit(e: FormEvent) {
    e.preventDefault();
    if (nuevoComentario.trim()) comentarioMutation.mutate();
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-6 w-40" />
        <Skeleton className="h-64 w-full" />
        <Skeleton className="h-32 w-full" />
      </div>
    );
  }

  if (!alerta) {
    return (
      <div className="flex h-64 flex-col items-center justify-center gap-2">
        <FileWarning size={36} className="text-muted-foreground/50" />
        <p className="text-muted-foreground">Alerta no encontrada.</p>
        <button
          onClick={() => navigate("/alertas")}
          className="text-primary hover:underline"
        >
          Volver a alertas
        </button>
      </div>
    );
  }

  const transitions = STATE_TRANSITIONS[alerta.estado] ?? [];

  let metadata: Record<string, unknown> | null = null;
  if (alerta.metadataJson) {
    try {
      metadata = JSON.parse(alerta.metadataJson) as Record<string, unknown>;
    } catch {
      metadata = null;
    }
  }

  return (
    <div className="space-y-6">
      <button
        onClick={() => navigate("/alertas")}
        className="flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
      >
        <ArrowLeft size={16} /> Volver a alertas
      </button>

      {/* Cabecera */}
      <div className="rounded-xl border border-border bg-background p-6 shadow-sm">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div className="flex items-start gap-4">
            <ScoreGauge score={alerta.score} />
            <div>
              <h1 className="text-2xl font-bold text-foreground">
                Alerta #{alerta.id}
              </h1>
              <p className="mt-1 max-w-2xl text-muted-foreground">
                {alerta.descripcion}
              </p>
            </div>
          </div>
          <div className="flex shrink-0 gap-2">
            <Badge variant="risk" riskLevel={alerta.nivelRiesgo}>
              {NIVEL_RIESGO_LABELS[alerta.nivelRiesgo]}
            </Badge>
            <Badge variant="status" status={alerta.estado}>
              {ESTADO_ALERTA_LABELS[alerta.estado]}
            </Badge>
          </div>
        </div>

        <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <InfoField
            label="Tipo de Detector"
            value={TIPO_DETECTOR_LABELS[alerta.tipoDetector]}
          />
          <InfoField label="Estación" value={alerta.estacionNombre} />
          <InfoField
            label="Fecha de Detección"
            value={new Date(alerta.fechaDeteccion).toLocaleString("es-EC")}
          />
          {alerta.empleadoCodigo && (
            <InfoField label="Empleado" value={alerta.empleadoCodigo} />
          )}
          {alerta.transaccionReferencia && (
            <InfoField label="Referencia" value={alerta.transaccionReferencia} />
          )}
        </div>

        {metadata && Object.keys(metadata).length > 0 && (
          <div className="mt-6">
            <h3 className="mb-3 text-sm font-semibold text-foreground">
              Evidencia de la detección
            </h3>
            <div className="grid grid-cols-1 gap-x-6 gap-y-2 rounded-lg bg-muted/50 p-4 sm:grid-cols-2 lg:grid-cols-3">
              {Object.entries(metadata).map(([clave, valor]) => (
                <div
                  key={clave}
                  className="flex items-baseline justify-between gap-3 border-b border-border/50 py-1.5 last:border-0"
                >
                  <span className="text-xs text-muted-foreground">
                    {METADATA_LABELS[clave] ?? clave}
                  </span>
                  <span className="text-right font-mono text-xs font-medium text-foreground">
                    {formatearValor(valor)}
                  </span>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Acciones */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {transitions.length > 0 && (
          <div className="rounded-xl border border-border bg-background p-6 shadow-sm">
            <h3 className="mb-4 text-base font-semibold text-foreground">
              Clasificar alerta
            </h3>
            <div className="flex flex-wrap gap-3">
              {transitions.map((nextState) => {
                const style = ACTION_STYLES[nextState];
                if (!style) return null;
                return (
                  <button
                    key={nextState}
                    onClick={() => cambiarEstadoMutation.mutate(nextState)}
                    disabled={cambiarEstadoMutation.isPending}
                    className={`rounded-md px-4 py-2 text-sm font-medium transition-colors disabled:opacity-50 ${style.className}`}
                  >
                    {style.label}
                  </button>
                );
              })}
            </div>
          </div>
        )}

        {canAssign && alerta.estado !== "Cerrada" && (
          <div className="rounded-xl border border-border bg-background p-6 shadow-sm">
            <h3 className="mb-4 flex items-center gap-2 text-base font-semibold text-foreground">
              <UserPlus size={17} /> Asignar a auditor
            </h3>
            <div className="flex gap-3">
              <select
                value={assignAuditorId}
                onChange={(e) => setAssignAuditorId(e.target.value)}
                className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
              >
                <option value="">Seleccione un auditor…</option>
                {(auditores ?? []).map((a) => (
                  <option key={a.id} value={a.id}>
                    {a.nombreCompleto} ({a.rol})
                  </option>
                ))}
              </select>
              <button
                onClick={() => asignarMutation.mutate()}
                disabled={!assignAuditorId || asignarMutation.isPending}
                className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
              >
                Asignar
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Comentarios (CU-07) */}
      <div className="rounded-xl border border-border bg-background p-6 shadow-sm">
        <h3 className="mb-4 flex items-center gap-2 text-base font-semibold text-foreground">
          <MessageSquare size={17} /> Comentarios de auditoría
          {comentarios && comentarios.length > 0 && (
            <span className="rounded-full bg-muted px-2 py-0.5 text-xs font-medium text-muted-foreground">
              {comentarios.length}
            </span>
          )}
        </h3>

        <div className="space-y-4">
          {(comentarios ?? []).map((c) => (
            <div key={c.id} className="flex gap-3">
              <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">
                {iniciales(c.usuarioNombre)}
              </div>
              <div className="flex-1 rounded-lg bg-muted/50 px-4 py-3">
                <div className="flex items-baseline justify-between gap-3">
                  <span className="text-sm font-semibold text-foreground">
                    {c.usuarioNombre}
                  </span>
                  <span className="text-xs text-muted-foreground">
                    {new Date(c.fecha).toLocaleString("es-EC")}
                  </span>
                </div>
                <p className="mt-1 whitespace-pre-wrap text-sm text-foreground">
                  {c.texto}
                </p>
              </div>
            </div>
          ))}

          {comentarios && comentarios.length === 0 && (
            <p className="py-2 text-sm text-muted-foreground">
              Aún no hay comentarios. Documente aquí sus hallazgos y conclusiones.
            </p>
          )}
        </div>

        <form onSubmit={handleComentarioSubmit} className="mt-4 flex gap-3">
          <textarea
            value={nuevoComentario}
            onChange={(e) => setNuevoComentario(e.target.value)}
            placeholder="Escriba un comentario con sus hallazgos…"
            rows={2}
            className="flex-1 resize-none rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
          />
          <button
            type="submit"
            disabled={!nuevoComentario.trim() || comentarioMutation.isPending}
            className="flex items-center gap-2 self-end rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
          >
            <Send size={14} /> Comentar
          </button>
        </form>
      </div>
    </div>
  );
}

function InfoField({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-xs font-medium text-muted-foreground">{label}</p>
      <p className="mt-1 text-sm font-medium text-foreground">{value}</p>
    </div>
  );
}

function ScoreGauge({ score }: { score: number }) {
  const color =
    score > 75
      ? "text-risk-critical border-risk-critical"
      : score > 50
        ? "text-risk-high border-risk-high"
        : score > 25
          ? "text-risk-medium border-risk-medium"
          : "text-risk-low border-risk-low";

  return (
    <div
      className={`flex h-16 w-16 shrink-0 flex-col items-center justify-center rounded-full border-4 ${color}`}
    >
      <span className="text-lg font-bold leading-none">{Math.round(score)}</span>
      <span className="text-[9px] uppercase tracking-wide opacity-70">score</span>
    </div>
  );
}

function iniciales(nombre: string): string {
  return nombre
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((p) => p[0]?.toUpperCase())
    .join("");
}

function formatearValor(valor: unknown): string {
  if (valor === null || valor === undefined) return "—";
  if (typeof valor === "number")
    return Number.isInteger(valor) ? String(valor) : valor.toFixed(2);
  if (typeof valor === "boolean") return valor ? "Sí" : "No";
  if (Array.isArray(valor)) return valor.map(String).join(", ");
  return String(valor);
}
