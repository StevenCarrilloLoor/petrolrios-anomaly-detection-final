import { api } from "./api";
import type { PaginatedResponse } from "@/types/common";
import type { DatoRecibidoResponse, TipoRecibidoOption } from "@/types/datoRecibido";

export interface DatosRecibidosParams {
  tipo?: string;
  estacionId?: number;
  procesada?: boolean;
  q?: string;
  page?: number;
  pageSize?: number;
}

export const datosRecibidosService = {
  getAll: (params: DatosRecibidosParams) =>
    api
      .get<PaginatedResponse<DatoRecibidoResponse>>("/datos-recibidos", { params })
      .then((r) => r.data),
  getTipos: () =>
    api.get<TipoRecibidoOption[]>("/datos-recibidos/tipos").then((r) => r.data),
};
