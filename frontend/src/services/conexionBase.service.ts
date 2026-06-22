import { api } from "./api";
import type {
  ConexionActiva,
  ProbarConexionRequest,
  ProbarConexionResponse,
  GuardarConexionResponse,
} from "@/types/conexionBase";

export const conexionBaseService = {
  async estado(): Promise<ConexionActiva> {
    const { data } = await api.get<ConexionActiva>("/conexion-base");
    return data;
  },

  async probar(request: ProbarConexionRequest): Promise<ProbarConexionResponse> {
    const { data } = await api.post<ProbarConexionResponse>("/conexion-base/probar", request);
    return data;
  },

  async guardar(request: ProbarConexionRequest): Promise<GuardarConexionResponse> {
    const { data } = await api.post<GuardarConexionResponse>("/conexion-base/guardar", request);
    return data;
  },
};
