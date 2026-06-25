import { api } from "./api";
import type {
  ReglaDeteccionResponse,
  ActualizarReglaRequest,
} from "@/types/regla";

// Las reglas están definidas por el motor de detección (Strategy Pattern);
// desde la interfaz solo se parametrizan (umbral) y se activan/desactivan.
export const reglasService = {
  getAll: () =>
    api.get<ReglaDeteccionResponse[]>("/reglas").then((r) => r.data),

  getById: (id: number) =>
    api.get<ReglaDeteccionResponse>(`/reglas/${id}`).then((r) => r.data),

  update: (id: number, data: ActualizarReglaRequest) =>
    api.put<ReglaDeteccionResponse>(`/reglas/${id}`, data).then((r) => r.data),

  /** Restablece todas las reglas de un detector a sus valores predeterminados de fábrica. */
  restablecerDetector: (tipoDetector: string) =>
    api
      .post<ReglaDeteccionResponse[]>(`/reglas/restablecer/${tipoDetector}`)
      .then((r) => r.data),
};
