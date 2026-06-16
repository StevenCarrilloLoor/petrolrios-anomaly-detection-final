import { api } from "./api";
import type {
  ReglaPersonalizadaResponse,
  GuardarReglaPersonalizadaRequest,
  CatalogoReglasResponse,
  ValidarExpresionResponse,
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
};
