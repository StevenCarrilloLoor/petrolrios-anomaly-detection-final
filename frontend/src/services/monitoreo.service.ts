import { api } from "./api";
import type {
  ConexionEstacionResponse,
  EstadoSistemaResponse,
} from "@/types/monitoreo";

export interface UsuarioConectado {
  usuarioId: string;
  nombre: string;
  rol: string;
  estacionId: string | null;
  desde: string;
}

export const monitoreoService = {
  getConexiones: () =>
    api.get<ConexionEstacionResponse[]>("/monitoreo/conexiones").then((r) => r.data),

  getEstadoSistema: () =>
    api.get<EstadoSistemaResponse>("/monitoreo/sistema").then((r) => r.data),

  getUsuariosConectados: () =>
    api.get<UsuarioConectado[]>("/monitoreo/usuarios-conectados").then((r) => r.data),
};
