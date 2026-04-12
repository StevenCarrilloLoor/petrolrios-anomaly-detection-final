export interface KpiResponse {
  totalAlertas: number;
  alertasNuevas: number;
  alertasCriticas: number;
  alertasEnRevision: number;
  alertasConfirmadas: number;
  alertasFalsoPositivo: number;
  scorePromedio: number;
  estacionesActivas: number;
}

export interface AlertasPorTipoResponse {
  tipoDetector: string;
  cantidad: number;
}

export interface AlertasPorEstacionResponse {
  estacionId: number;
  estacionNombre: string;
  cantidad: number;
}
