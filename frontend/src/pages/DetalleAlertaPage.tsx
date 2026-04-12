import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { alertasService } from "@/services/alertas.service";
import { Badge } from "@/components/ui/Badge";
import { Spinner } from "@/components/ui/Spinner";
import { useAuth } from "@/contexts/AuthContext";
import {
  TIPO_DETECTOR_LABELS,
  NIVEL_RIESGO_LABELS,
  ESTADO_ALERTA_LABELS,
} from "@/types/alert";
import { ArrowLeft } from "lucide-react";

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

export function DetalleAlertaPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const [assignAuditorId, setAssignAuditorId] = useState("");

  const { data: alerta, isLoading } = useQuery({
    queryKey: ["alertas", Number(id)],
    queryFn: () => alertasService.getById(Number(id)),
    enabled: !!id,
  });

  const cambiarEstadoMutation = useMutation({
    mutationFn: (nuevoEstado: string) =>
      alertasService.cambiarEstado(Number(id), { estado: nuevoEstado }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["alertas"] });
    },
  });

  const asignarMutation = useMutation({
    mutationFn: () =>
      alertasService.asignar(Number(id), {
        auditorId: Number(assignAuditorId),
        comentario: null,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["alertas"] });
      setAssignAuditorId("");
    },
  });

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Spinner size="lg" />
      </div>
    );
  }

  if (!alerta) {
    return (
      <div className="flex h-64 flex-col items-center justify-center gap-2">
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
  const canAssign =
    user?.rol === "Supervisor" || user?.rol === "Administrador";

  let parsedMetadata: string | null = null;
  if (alerta.metadataJson) {
    try {
      parsedMetadata = JSON.stringify(JSON.parse(alerta.metadataJson), null, 2);
    } catch {
      parsedMetadata = alerta.metadataJson;
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

      <div className="rounded-lg border border-border bg-background p-6">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <h1 className="text-2xl font-bold text-foreground">
              Alerta #{alerta.id}
            </h1>
            <p className="mt-1 text-muted-foreground">{alerta.descripcion}</p>
          </div>
          <div className="flex gap-2">
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
          <InfoField
            label="Score de Riesgo"
            value={alerta.score.toFixed(1)}
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
            <InfoField
              label="Referencia"
              value={alerta.transaccionReferencia}
            />
          )}
        </div>

        {parsedMetadata && (
          <div className="mt-6">
            <h3 className="mb-2 text-sm font-semibold text-foreground">
              Metadata
            </h3>
            <pre className="overflow-x-auto rounded-md bg-muted p-4 text-xs text-muted-foreground">
              {parsedMetadata}
            </pre>
          </div>
        )}
      </div>

      {transitions.length > 0 && (
        <div className="rounded-lg border border-border bg-background p-6">
          <h3 className="mb-4 text-lg font-semibold text-foreground">
            Acciones
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
                  className={`rounded-md px-4 py-2 text-sm font-medium disabled:opacity-50 ${style.className}`}
                >
                  {style.label}
                </button>
              );
            })}
          </div>
        </div>
      )}

      {canAssign && alerta.estado !== "Cerrada" && (
        <div className="rounded-lg border border-border bg-background p-6">
          <h3 className="mb-4 text-lg font-semibold text-foreground">
            Asignar a Auditor
          </h3>
          <div className="flex gap-3">
            <input
              type="number"
              value={assignAuditorId}
              onChange={(e) => setAssignAuditorId(e.target.value)}
              placeholder="ID del auditor"
              className="w-48 rounded-md border border-border bg-background px-3 py-2 text-sm"
            />
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
