import { useState, useEffect } from "react";
import type { FormEvent } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
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
  UserCheck,
  FileWarning,
  Copy,
  Check,
  ExternalLink,
  FileText,
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
  FechaCierre: "Fecha de cierre del turno",
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
  // Regla "Placa reutilizada en el día"
  Dia: "Día",
  CantidadFacturas: "Cantidad de facturas",
  NumerosFactura: "Números de factura",
  Clientes: "Clientes",
  Rucs: "RUC / cédulas",
  Ruc: "RUC / cédula",
  Fecha: "Fecha",
  FormaPago: "Forma de pago",
  Vendedores: "Despachadores",
};

// Claves de la evidencia cuyo valor identifica una entidad (placa, RUC, cliente, despachador): se
// muestran como enlace que abre, EN VENTANA NUEVA, la consulta EN VIVO de todo lo relacionado con ese
// valor en la estación (comportamiento "ERP" que pidió la auditora: ver las facturas/despachos de ese
// cliente/placa/despachador sin perder la pantalla actual, para comparar lado a lado).
const CLAVES_BUSCABLES = new Set([
  "Placa",
  "NumeroDocumento",
  "NumerosFactura",
  "Cliente",
  "Clientes",
  "Ruc",
  "Rucs",
  "RucCliente",
  "Vendedores",
]);

// Claves cuyo valor es un NÚMERO DE FACTURA: ofrecen "Ver factura" → abre la factura COMPLETA en vivo
// (FacturaPage) en una ventana nueva, para compararla lado a lado (lo pidió la auditora). Incluye las
// LISTAS de documentos que emiten las reglas agregadas (Documentos en MultipleCombustible/DespachosRapidos,
// NumerosFactura en PlacaReutilizada) → así CADA documento de la lista es un enlace a su factura, no solo
// el primero. Es genérico (depende de la clave, no de la regla) para no chocar con el creador de reglas.
const CLAVES_FACTURA = new Set(["NumeroDocumento", "NumerosFactura", "Documentos"]);

// Evita la duplicación incómoda: si una lista (Documentos/NumerosFactura) ya incluye el "NumeroDocumento"
// suelto (que viene del Fuente automático), no se muestra el suelto — la lista ya lo enlaza como factura.
// Solo se filtra cuando el valor está contenido en la lista; cualquier otra clave se conserva. Genérico.
function evidenciaSinDuplicados(metadata: Record<string, unknown>): [string, unknown][] {
  const enLista = new Set(
    ["Documentos", "NumerosFactura"]
      .map((k) => metadata[k])
      .filter((v): v is unknown[] => Array.isArray(v))
      .flat()
      .map((v) => String(v).trim()),
  );
  return Object.entries(metadata).filter(
    ([clave, valor]) => clave !== "NumeroDocumento" || !enLista.has(String(valor ?? "").trim()),
  );
}

/**
 * Abre la consulta EN VIVO (Consultas) filtrada por un valor (RUC, placa, cliente o despachador) en la
 * estación de la alerta, en una VENTANA NUEVA. Es el comportamiento ERP que pidió la auditora: clic en
 * el dato → ver todo lo relacionado (sus facturas/despachos), comparando lado a lado.
 */
function abrirConsultaRelacionada(estacionCodigo: string, valor: string) {
  const v = valor.trim();
  if (!v || !estacionCodigo) return;
  const qs = new URLSearchParams({ est: estacionCodigo, codigo: v });
  window.open(`/consultas?${qs.toString()}`, "_blank", "noopener,width=1100,height=860");
}

export function DetalleAlertaPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const queryClient = useQueryClient();

  // Origen de la navegación: si vinimos desde "Problemas de estación", el botón Volver regresa allí
  // (con la estación aún expandida) en vez de mandar siempre a la bandeja de alertas.
  const volver = (location.state as { volverA?: string; volverLabel?: string } | null) ?? null;
  const volverA = volver?.volverA ?? "/alertas";
  const volverLabel = volver?.volverLabel ?? "Volver a alertas";
  const { user } = useAuth();
  const [assignAuditorId, setAssignAuditorId] = useState("");
  const [nuevoComentario, setNuevoComentario] = useState("");

  const alertaId = Number(id);
  const canAssign = user?.rol === "Supervisor" || user?.rol === "Administrador";

  // Al abrir el detalle, marcar la alerta como vista POR ESTE usuario (no afecta a lo que ven los
  // demás) y refrescar el set de vistas para que la lista la deje de marcar como "nueva para ti".
  useEffect(() => {
    if (!id) return;
    alertasService
      .marcarVista(alertaId)
      .then(() => queryClient.invalidateQueries({ queryKey: ["alertas", "vistas"] }))
      .catch(() => {});
  }, [alertaId, id, queryClient]);

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
          onClick={() => navigate(volverA)}
          className="text-primary hover:underline"
        >
          {volverLabel}
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
      <div className="flex items-center justify-between">
        <button
          onClick={() => navigate(volverA)}
          className="flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft size={16} /> {volverLabel}
        </button>
        {/* Abrir esta misma alerta en otra pestaña, para compararla lado a lado (lo pidió auditoría). */}
        <a
          href={`/alertas/${alertaId}`}
          target="_blank"
          rel="noopener noreferrer"
          className="flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
          title="Abrir esta alerta en una ventana nueva"
        >
          <ExternalLink size={15} /> Abrir en ventana nueva
        </a>
      </div>

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
            <InfoField
              label="Empleado"
              value={
                alerta.empleadoNombre
                  ? `${alerta.empleadoNombre} (${alerta.empleadoCodigo})`
                  : alerta.empleadoCodigo
              }
            />
          )}
          {alerta.transaccionReferencia && (
            <div>
              <p className="text-xs font-medium text-muted-foreground">Referencia</p>
              <p className="mt-1 flex items-center gap-1.5 text-sm font-medium text-foreground">
                <span className="break-all font-mono text-xs">
                  {alerta.transaccionReferencia}
                </span>
                <CopyButton value={alerta.transaccionReferencia} />
              </p>
            </div>
          )}
        </div>

        {metadata && Object.keys(metadata).length > 0 && (
          <div className="mt-6">
            <h3 className="mb-3 text-sm font-semibold text-foreground">
              Evidencia de la detección
            </h3>
            <div className="grid grid-cols-1 gap-x-6 gap-y-2 rounded-lg bg-muted/50 p-4 sm:grid-cols-2 lg:grid-cols-3">
              {evidenciaSinDuplicados(metadata).map(([clave, valor]) => (
                <div
                  key={clave}
                  className="flex items-start justify-between gap-3 border-b border-border/50 py-1.5 last:border-0"
                >
                  <span className="shrink-0 text-xs text-muted-foreground">
                    {METADATA_LABELS[clave] ?? clave}
                  </span>
                  <ValorEvidencia
                    clave={clave}
                    valor={valor}
                    estacionCodigo={alerta.estacionCodigo}
                    onRelacionar={(v) =>
                      alerta.estacionCodigo
                        ? abrirConsultaRelacionada(alerta.estacionCodigo, v)
                        : navigate(`/alertas?buscar=${encodeURIComponent(v)}`)
                    }
                  />
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Asignación actual: a quién está asignada, por quién y cuándo (visible para todos). */}
      {alerta.asignadoANombre && (
        <div className="flex items-center gap-3 rounded-xl border border-primary/30 bg-primary/5 px-5 py-3">
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-primary/15 text-primary">
            <UserCheck size={18} />
          </div>
          <div className="text-sm">
            <span className="text-muted-foreground">Asignada a </span>
            <span className="font-semibold text-foreground">{alerta.asignadoANombre}</span>
            {alerta.asignadoARol && (
              <span className="text-muted-foreground"> ({alerta.asignadoARol})</span>
            )}
            {alerta.asignadoPorNombre && (
              <span className="text-muted-foreground">
                {" "}
                · asignada por {alerta.asignadoPorNombre}
              </span>
            )}
            {alerta.fechaAsignacion && (
              <span className="text-muted-foreground">
                {" "}
                · {new Date(alerta.fechaAsignacion).toLocaleString("es-EC")}
              </span>
            )}
          </div>
        </div>
      )}

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
            <h3 className="mb-1 flex items-center gap-2 text-base font-semibold text-foreground">
              <UserPlus size={17} />{" "}
              {alerta.asignadoANombre ? "Reasignar alerta" : "Asignar a auditor"}
            </h3>
            <p className="mb-4 text-xs text-muted-foreground">
              {alerta.asignadoANombre
                ? `Actualmente asignada a ${alerta.asignadoANombre}. Al reasignar se le avisará por correo al nuevo responsable.`
                : "El auditor asignado recibirá un correo y un aviso en tiempo real."}
            </p>
            <div className="flex gap-3">
              <select
                value={assignAuditorId}
                onChange={(e) => setAssignAuditorId(e.target.value)}
                className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
              >
                <option value="">Seleccione un responsable…</option>
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
                {alerta.asignadoANombre ? "Reasignar" : "Asignar"}
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

/** Botón pequeño para copiar un valor al portapapeles, con confirmación breve (✓). */
function CopyButton({ value }: { value: string }) {
  const [copiado, setCopiado] = useState(false);
  return (
    <button
      type="button"
      onClick={async () => {
        try {
          await navigator.clipboard.writeText(value);
          setCopiado(true);
          setTimeout(() => setCopiado(false), 1200);
        } catch {
          /* el portapapeles puede no estar disponible; no es crítico */
        }
      }}
      title="Copiar al portapapeles"
      aria-label="Copiar al portapapeles"
      className="shrink-0 text-muted-foreground transition-colors hover:text-foreground"
    >
      {copiado ? (
        <Check size={12} className="text-green-600" />
      ) : (
        <Copy size={12} />
      )}
    </button>
  );
}

/**
 * Renderiza un valor de la evidencia. Si la clave identifica una entidad (placa, RUC, cliente,
 * despachador), el valor se muestra como enlace que abre, EN VENTANA NUEVA, la consulta EN VIVO de todo
 * lo relacionado con ese valor + botón copiar. Si es un n° de factura, ofrece "Ver factura". Los valores
 * de lista (varios n° de factura, clientes, despachadores) se muestran como pastillas.
 */
function ValorEvidencia({
  clave,
  valor,
  estacionCodigo,
  onRelacionar,
}: {
  clave: string;
  valor: unknown;
  estacionCodigo: string;
  onRelacionar: (valor: string) => void;
}) {
  const buscable = CLAVES_BUSCABLES.has(clave);
  const esFactura = CLAVES_FACTURA.has(clave);

  if (Array.isArray(valor)) {
    const items = valor.map(String).filter((v) => v.length > 0);
    if (items.length === 0)
      return <span className="text-right font-mono text-xs text-foreground">—</span>;
    return (
      <span className="flex flex-wrap justify-end gap-1">
        {items.map((v, i) => (
          <ChipValor
            key={`${v}-${i}`}
            value={v}
            buscable={buscable}
            esFactura={esFactura}
            estacionCodigo={estacionCodigo}
            onRelacionar={onRelacionar}
          />
        ))}
      </span>
    );
  }

  const texto = formatearValor(valor);
  if ((buscable || esFactura) && texto !== "—") {
    return (
      <span className="flex justify-end">
        <ChipValor
          value={texto}
          buscable={buscable}
          esFactura={esFactura}
          estacionCodigo={estacionCodigo}
          onRelacionar={onRelacionar}
        />
      </span>
    );
  }
  return (
    <span className="text-right font-mono text-xs font-medium text-foreground">
      {texto}
    </span>
  );
}

/**
 * Pastilla de un valor. Si es un n° de factura, ofrece "Ver factura" (abre la factura COMPLETA en vivo
 * en una ventana nueva). Si identifica una entidad (placa/RUC/cliente/despachador), el valor es un
 * enlace que abre la consulta EN VIVO de todo lo relacionado en una ventana nueva. Siempre con copiar.
 */
function ChipValor({
  value,
  buscable,
  esFactura,
  estacionCodigo,
  onRelacionar,
}: {
  value: string;
  buscable: boolean;
  esFactura: boolean;
  estacionCodigo: string;
  onRelacionar: (valor: string) => void;
}) {
  const verFactura = esFactura && estacionCodigo.length > 0;
  // El nº de factura usa su propio enlace "Ver factura"; el resto de entidades abren la consulta relacionada.
  const relacionable = buscable && !esFactura;
  return (
    <span className="inline-flex items-center gap-1 rounded bg-background px-1.5 py-0.5 font-mono text-xs ring-1 ring-border">
      {relacionable ? (
        <button
          type="button"
          onClick={() => onRelacionar(value)}
          title={`Ver todo lo relacionado con "${value}" en una ventana nueva`}
          className="inline-flex items-center gap-1 font-medium text-primary hover:underline"
        >
          <ExternalLink size={11} className="shrink-0 opacity-70" />
          {value}
        </button>
      ) : (
        <span className="font-medium text-foreground">{value}</span>
      )}
      {verFactura && (
        <a
          href={`/consultas/factura?est=${encodeURIComponent(estacionCodigo)}&num=${encodeURIComponent(value)}`}
          target="_blank"
          rel="noopener noreferrer"
          title={`Ver la factura ${value} completa en una ventana nueva`}
          className="inline-flex shrink-0 items-center gap-0.5 rounded bg-primary/10 px-1 py-0.5 text-[10px] font-semibold text-primary hover:bg-primary/20"
        >
          <FileText size={11} /> factura
        </a>
      )}
      <CopyButton value={value} />
    </span>
  );
}
