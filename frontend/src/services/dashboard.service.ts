import { api } from "./api";
import type {
  KpiResponse,
  AlertasPorTipoResponse,
  AlertasPorEstacionResponse,
  AlertasPorNivelResponse,
  TendenciaDiaResponse,
  TopEmpleadoResponse,
  MetricasResolucionResponse,
} from "@/types/dashboard";

// estacionId (opcional) acota el dashboard a una estación; sin él, vista global de las 10 estaciones.
export const dashboardService = {
  getKpis: (estacionId?: number) =>
    api.get<KpiResponse>("/dashboard/kpis", { params: { estacionId } }).then((r) => r.data),

  getAlertasPorTipo: (estacionId?: number) =>
    api
      .get<AlertasPorTipoResponse[]>("/dashboard/alertas-por-tipo", { params: { estacionId } })
      .then((r) => r.data),

  getAlertasPorEstacion: () =>
    api.get<AlertasPorEstacionResponse[]>("/dashboard/alertas-por-estacion").then((r) => r.data),

  getAlertasPorNivel: (estacionId?: number) =>
    api
      .get<AlertasPorNivelResponse[]>("/dashboard/alertas-por-nivel", { params: { estacionId } })
      .then((r) => r.data),

  getTendencia: (dias = 14, estacionId?: number) =>
    api
      .get<TendenciaDiaResponse[]>("/dashboard/tendencia", { params: { dias, estacionId } })
      .then((r) => r.data),

  getTopEmpleados: (top = 10, estacionId?: number) =>
    api
      .get<TopEmpleadoResponse[]>("/dashboard/top-empleados", { params: { top, estacionId } })
      .then((r) => r.data),

  getMetricasResolucion: (estacionId?: number) =>
    api
      .get<MetricasResolucionResponse>("/dashboard/metricas-resolucion", { params: { estacionId } })
      .then((r) => r.data),
};
