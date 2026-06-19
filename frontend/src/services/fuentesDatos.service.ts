import { api } from "./api";

export interface FuenteDatosResponse {
  id: number;
  nombre: string;
  tabla: string;
  columnaWatermark: string | null;
  descripcion: string;
  activa: boolean;
}

export interface CrearFuenteDatosRequest {
  nombre: string;
  tabla: string;
  columnaWatermark?: string | null;
  descripcion?: string | null;
}

export interface ActualizarFuenteDatosRequest {
  nombre: string;
  tabla: string;
  columnaWatermark?: string | null;
  descripcion?: string | null;
  activa: boolean;
}

export const fuentesDatosService = {
  getAll: () =>
    api.get<FuenteDatosResponse[]>("/fuentes-datos").then((r) => r.data),

  create: (data: CrearFuenteDatosRequest) =>
    api.post<FuenteDatosResponse>("/fuentes-datos", data).then((r) => r.data),

  update: (id: number, data: ActualizarFuenteDatosRequest) =>
    api.put<FuenteDatosResponse>(`/fuentes-datos/${id}`, data).then((r) => r.data),

  remove: (id: number) => api.delete(`/fuentes-datos/${id}`).then((r) => r.data),
};
