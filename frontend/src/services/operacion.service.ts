import { api } from "./api";

/** Parámetros de operación editables por el Administrador (nivel de correo + cron del job). */
export interface OperacionConfig {
  nivelMinimoCorreo: string;
  cronExpression: string;
}

export const operacionService = {
  async actual(): Promise<OperacionConfig> {
    const { data } = await api.get<OperacionConfig>("/operacion");
    return data;
  },

  async guardar(config: OperacionConfig): Promise<OperacionConfig> {
    const { data } = await api.put<OperacionConfig>("/operacion", config);
    return data;
  },
};
