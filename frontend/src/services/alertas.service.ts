import { api } from "./api";
import type { AlertaResponse, CambiarEstadoRequest, AsignarAlertaRequest } from "@/types/alert";
import type { PaginatedResponse } from "@/types/common";

interface AlertaFilters {
  page?: number;
  pageSize?: number;
  tipoDetector?: string;
  nivelRiesgo?: string;
  estado?: string;
  estacionId?: number;
}

export const alertasService = {
  getAll: (params: AlertaFilters) =>
    api.get<PaginatedResponse<AlertaResponse>>("/alertas", { params }).then((r) => r.data),

  getById: (id: number) =>
    api.get<AlertaResponse>(`/alertas/${id}`).then((r) => r.data),

  cambiarEstado: (id: number, data: CambiarEstadoRequest) =>
    api.patch<AlertaResponse>(`/alertas/${id}/estado`, data).then((r) => r.data),

  asignar: (id: number, data: AsignarAlertaRequest) =>
    api.post<AlertaResponse>(`/alertas/${id}/asignar`, data).then((r) => r.data),
};
