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
  enLinea: boolean;
}

export interface ActualizarEstacionRequest {
  nombre: string;
  direccion?: string | null;
  zona?: string | null;
  // Configuración avanzada (solo aplica si quien edita es Administrador).
  horaApertura?: string | null;
  horaCierre?: string | null;
  correoContacto?: string | null;
  activa?: boolean | null;
}

export interface EliminarEstacionResponse {
  eliminada: boolean;
  desactivada: boolean;
  mensaje: string;
}

export interface ManifiestoVersionResponse {
  version: string;
  url: string;
  sha256?: string | null;
  notas?: string | null;
  obligatoria: boolean;
}

export const estacionesService = {
  getAll: () =>
    api.get<Omit<EstacionResponse, "enLinea">[]>("/estaciones").then((r) => {
      const ahora = Date.now();
      return r.data.map((estacion) => ({
        ...estacion,
        enLinea:
          estacion.ultimoHeartbeat !== null &&
          ahora - new Date(estacion.ultimoHeartbeat).getTime() < 3 * 60 * 1000,
      }));
    }),

  getVersionAgente: () =>
    api.get<ManifiestoVersionResponse>("/agente/version").then((r) => r.data),

  update: (id: number, data: ActualizarEstacionRequest) =>
    api.put<EstacionResponse>(`/estaciones/${id}`, data).then((r) => r.data),

  delete: (id: number) =>
    api.delete<EliminarEstacionResponse>(`/estaciones/${id}`).then((r) => r.data),
};
