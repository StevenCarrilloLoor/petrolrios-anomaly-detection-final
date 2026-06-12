import { api } from "./api";
import type {
  ConexionEstacionResponse,
  EstadoSistemaResponse,
} from "@/types/monitoreo";

export const monitoreoService = {
  getConexiones: () =>
    api.get<ConexionEstacionResponse[]>("/monitoreo/conexiones").then((r) => r.data),

  getEstadoSistema: () =>
    api.get<EstadoSistemaResponse>("/monitoreo/sistema").then((r) => r.data),
};
