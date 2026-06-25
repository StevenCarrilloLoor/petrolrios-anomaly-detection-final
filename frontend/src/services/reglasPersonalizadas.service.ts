import { api } from "./api";
import type {
  ReglaPersonalizadaResponse,
  GuardarReglaPersonalizadaRequest,
  CatalogoReglasResponse,
  ValidarExpresionResponse,
  BacktestReglaRequest,
  BacktestReglaResponse,
} from "@/types/reglaPersonalizada";

export const reglasPersonalizadasService = {
  getCatalogo: () =>
    api
      .get<CatalogoReglasResponse>("/reglas-personalizadas/catalogo")
      .then((r) => r.data),

  validarExpresion: (fuenteDatos: string, expresion: string) =>
    api
      .post<ValidarExpresionResponse>("/reglas-personalizadas/validar-expresion", {
        fuenteDatos,
        expresion,
      })
      .then((r) => r.data),

  getAll: () =>
    api
      .get<ReglaPersonalizadaResponse[]>("/reglas-personalizadas")
      .then((r) => r.data),

  create: (data: GuardarReglaPersonalizadaRequest) =>
    api
      .post<ReglaPersonalizadaResponse>("/reglas-personalizadas", data)
      .then((r) => r.data),

  update: (id: number, data: GuardarReglaPersonalizadaRequest) =>
    api
      .put<ReglaPersonalizadaResponse>(`/reglas-personalizadas/${id}`, data)
      .then((r) => r.data),

  delete: (id: number) => api.delete(`/reglas-personalizadas/${id}`),

  backtest: (data: BacktestReglaRequest) =>
    api
      .post<BacktestReglaResponse>("/reglas-personalizadas/backtest", data)
      .then((r) => r.data),
};

/** Relación entre dos tablas/fuentes para enriquecer alertas. */
export interface RelacionTablaResponse {
  id: number;
  fuenteOrigen: string;
  fuenteDestino: string;
  campoOrigen: string;
  campoDestino: string;
  etiqueta: string;
  activa: boolean;
  esAutomatica: boolean;
}

export const relacionesService = {
  getAll: () =>
    api.get<RelacionTablaResponse[]>("/relaciones-tabla").then((r) => r.data),

  /** Ejecuta el autodescubridor (cruza llaves compartidas + valida por solapamiento de valores). */
  descubrir: () =>
    api
      .post<{ creadas: number; mensaje: string }>("/relaciones-tabla/descubrir")
      .then((r) => r.data),

  delete: (id: number) => api.delete(`/relaciones-tabla/${id}`),
};
