import { api } from "./api";

/**
 * Parámetros de operación editables por el Administrador: nivel de correo, cron del job y la
 * tasa de refresco (segundos) con la que TODAS las pantallas vuelven a consultar al servidor.
 */
export interface OperacionConfig {
  nivelMinimoCorreo: string;
  cronExpression: string;
  refrescoSegundos: number;
  // "Auto" | "Api" | "Sistema": qué fuente manda como precio de combustible efectivo.
  preferenciaPreciosCombustible: string;
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

  /** Tasa de refresco (segundos). Endpoint liviano accesible a cualquier rol autenticado. */
  async refresco(): Promise<number> {
    const { data } = await api.get<{ refrescoSegundos: number }>("/refresco");
    return data.refrescoSegundos;
  },
};
