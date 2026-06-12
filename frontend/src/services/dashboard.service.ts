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

export const dashboardService = {
  getKpis: () =>
    api.get<KpiResponse>("/dashboard/kpis").then((r) => r.data),

  getAlertasPorTipo: () =>
    api.get<AlertasPorTipoResponse[]>("/dashboard/alertas-por-tipo").then((r) => r.data),

  getAlertasPorEstacion: () =>
    api.get<AlertasPorEstacionResponse[]>("/dashboard/alertas-por-estacion").then((r) => r.data),

  getAlertasPorNivel: () =>
    api.get<AlertasPorNivelResponse[]>("/dashboard/alertas-por-nivel").then((r) => r.data),

  getTendencia: (dias = 14) =>
    api
      .get<TendenciaDiaResponse[]>("/dashboard/tendencia", { params: { dias } })
      .then((r) => r.data),

  getTopEmpleados: (top = 10) =>
    api
      .get<TopEmpleadoResponse[]>("/dashboard/top-empleados", { params: { top } })
      .then((r) => r.data),

  getMetricasResolucion: () =>
    api.get<MetricasResolucionResponse>("/dashboard/metricas-resolucion").then((r) => r.data),
};
