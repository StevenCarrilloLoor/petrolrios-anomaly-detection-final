import { api } from "./api";
import type {
  ReglaDeteccionResponse,
  CrearReglaRequest,
  ActualizarReglaRequest,
} from "@/types/regla";

export const reglasService = {
  getAll: () =>
    api.get<ReglaDeteccionResponse[]>("/reglas").then((r) => r.data),

  getById: (id: number) =>
    api.get<ReglaDeteccionResponse>(`/reglas/${id}`).then((r) => r.data),

  create: (data: CrearReglaRequest) =>
    api.post<ReglaDeteccionResponse>("/reglas", data).then((r) => r.data),

  update: (id: number, data: ActualizarReglaRequest) =>
    api.put<ReglaDeteccionResponse>(`/reglas/${id}`, data).then((r) => r.data),

  delete: (id: number) => api.delete(`/reglas/${id}`),
};
