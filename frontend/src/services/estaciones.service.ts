import { api } from "./api";

export interface EstacionResponse {
  id: number;
  codigo: string;
  nombre: string;
  direccion: string;
  zona: string | null;
  activa: boolean;
  ultimoHeartbeat: string | null;
  versionAgente: string | null;
}

export interface ActualizarEstacionRequest {
  nombre: string;
  direccion?: string | null;
  zona?: string | null;
}

export interface EliminarEstacionResponse {
  eliminada: boolean;
  desactivada: boolean;
  mensaje: string;
}

export const estacionesService = {
  getAll: () => api.get<EstacionResponse[]>("/estaciones").then((r) => r.data),

  update: (id: number, data: ActualizarEstacionRequest) =>
    api.put<EstacionResponse>(`/estaciones/${id}`, data).then((r) => r.data),

  delete: (id: number) =>
    api.delete<EliminarEstacionResponse>(`/estaciones/${id}`).then((r) => r.data),
};
