import { api } from "./api";
import type {
  AlertaResponse,
  CambiarEstadoRequest,
  AsignarAlertaRequest,
  ComentarioResponse,
  AgregarComentarioRequest,
  ProblemaEstacionGrupo,
} from "@/types/alert";
import type { PaginatedResponse } from "@/types/common";

export interface AlertaFilters {
  page?: number;
  pageSize?: number;
  tipoDetector?: string;
  nivelRiesgo?: string;
  estado?: string;
  estacionId?: number;
  fechaDesde?: string;
  fechaHasta?: string;
}

export const alertasService = {
  getAll: (filters: AlertaFilters) =>
    api
      .get<PaginatedResponse<AlertaResponse>>("/alertas", {
        params: {
          page: filters.page,
          pageSize: filters.pageSize,
          // El backend espera el parámetro "tipo"
          tipo: filters.tipoDetector,
          nivelRiesgo: filters.nivelRiesgo,
          estado: filters.estado,
          estacionId: filters.estacionId,
          fechaDesde: filters.fechaDesde,
          fechaHasta: filters.fechaHasta,
        },
      })
      .then((r) => r.data),

  getProblemasEstacion: (estacionId?: number, dias = 7) =>
    api
      .get<ProblemaEstacionGrupo[]>("/alertas/problemas-estacion", {
        params: { estacionId, dias },
      })
      .then((r) => r.data),

  getById: (id: number) =>
    api.get<AlertaResponse>(`/alertas/${id}`).then((r) => r.data),

  cambiarEstado: (id: number, data: CambiarEstadoRequest) =>
    api.patch<AlertaResponse>(`/alertas/${id}/estado`, data).then((r) => r.data),

  asignar: (id: number, data: AsignarAlertaRequest) =>
    api.post<AlertaResponse>(`/alertas/${id}/asignar`, data).then((r) => r.data),

  getComentarios: (id: number) =>
    api.get<ComentarioResponse[]>(`/alertas/${id}/comentarios`).then((r) => r.data),

  agregarComentario: (id: number, data: AgregarComentarioRequest) =>
    api.post<ComentarioResponse>(`/alertas/${id}/comentarios`, data).then((r) => r.data),

  /** Marca una alerta como vista por el usuario actual (estado leído/no leído por usuario). */
  marcarVista: (id: number) =>
    api.post<void>(`/alertas/${id}/marcar-vista`).then((r) => r.data),

  /** IDs de las alertas que el usuario actual ya vio. */
  getVistas: () => api.get<number[]>("/alertas/vistas").then((r) => r.data),
};
