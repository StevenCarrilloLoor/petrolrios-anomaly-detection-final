export interface KpiResponse {
  totalAlertas: number;
  alertasNuevas: number;
  alertasCriticas: number;
  alertasEnRevision: number;
  alertasConfirmadas: number;
  alertasFalsoPositivo: number;
  scorePromedio: number;
  estacionesConectadas: number;
  estacionesTotales: number;
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

export interface AlertasPorNivelResponse {
  nivelRiesgo: string;
  cantidad: number;
}

export interface TendenciaDiaResponse {
  fecha: string;
  total: number;
  criticas: number;
  altas: number;
}

export interface TopEmpleadoResponse {
  empleadoCodigo: string;
  cantidadAlertas: number;
  scorePromedio: number;
  criticas: number;
  estacionNombre: string;
}

export interface MetricasResolucionResponse {
  tiempoMedioResolucionHoras: number;
  tasaFalsosPositivos: number;
  tasaAlertasValidas: number;
  alertasUltimas24Horas: number;
  totalResueltas: number;
  totalPendientes: number;
}
