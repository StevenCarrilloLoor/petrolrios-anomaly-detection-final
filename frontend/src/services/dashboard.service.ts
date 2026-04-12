import { api } from "./api";
import type {
  KpiResponse,
  AlertasPorTipoResponse,
  AlertasPorEstacionResponse,
} from "@/types/dashboard";

export const dashboardService = {
  getKpis: () =>
    api.get<KpiResponse>("/dashboard/kpis").then((r) => r.data),

  getAlertasPorTipo: () =>
    api.get<AlertasPorTipoResponse[]>("/dashboard/alertas-por-tipo").then((r) => r.data),

  getAlertasPorEstacion: () =>
    api.get<AlertasPorEstacionResponse[]>("/dashboard/alertas-por-estacion").then((r) => r.data),
};
