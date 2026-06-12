export type NivelRiesgo = "Bajo" | "Medio" | "Alto" | "Critico";

export type TipoDetector =
  | "CashFraud"
  | "InvoiceAnomaly"
  | "PaymentFraud"
  | "ComplianceViolation"
  | "Personalizada";

export type EstadoAlerta =
  | "Nueva"
  | "EnRevision"
  | "Confirmada"
  | "FalsoPositivo"
  | "Cerrada";

export interface AlertaResponse {
  id: number;
  tipoDetector: TipoDetector;
  nivelRiesgo: NivelRiesgo;
  estado: EstadoAlerta;
  descripcion: string;
  score: number;
  fechaDeteccion: string;
  empleadoCodigo: string | null;
  transaccionReferencia: string | null;
  estacionId: number;
  estacionNombre: string;
  metadataJson: string | null;
}

export interface CambiarEstadoRequest {
  estado: string;
}

export interface ComentarioResponse {
  id: number;
  alertaId: number;
  usuarioId: number;
  usuarioNombre: string;
  texto: string;
  fecha: string;
}

export interface AgregarComentarioRequest {
  texto: string;
}

export interface AsignarAlertaRequest {
  auditorId: number;
  comentario: string | null;
}

export const NIVEL_RIESGO_OPTIONS: NivelRiesgo[] = [
  "Bajo",
  "Medio",
  "Alto",
  "Critico",
];

export const TIPO_DETECTOR_OPTIONS: TipoDetector[] = [
  "CashFraud",
  "InvoiceAnomaly",
  "PaymentFraud",
  "ComplianceViolation",
  "Personalizada",
];

export const ESTADO_ALERTA_OPTIONS: EstadoAlerta[] = [
  "Nueva",
  "EnRevision",
  "Confirmada",
  "FalsoPositivo",
  "Cerrada",
];

export const TIPO_DETECTOR_LABELS: Record<TipoDetector, string> = {
  CashFraud: "Fraude de Efectivo",
  InvoiceAnomaly: "Anomalía de Factura",
  PaymentFraud: "Fraude de Pago",
  ComplianceViolation: "Violación Normativa",
  Personalizada: "Regla Personalizada",
};

export const NIVEL_RIESGO_LABELS: Record<NivelRiesgo, string> = {
  Bajo: "Bajo",
  Medio: "Medio",
  Alto: "Alto",
  Critico: "Crítico",
};

export const ESTADO_ALERTA_LABELS: Record<EstadoAlerta, string> = {
  Nueva: "Nueva",
  EnRevision: "En Revisión",
  Confirmada: "Confirmada",
  FalsoPositivo: "Falso Positivo",
  Cerrada: "Cerrada",
};
