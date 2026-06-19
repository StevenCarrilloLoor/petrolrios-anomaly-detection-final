import { api } from "./api";

export interface ColumnaEsquema {
  nombre: string;
  tipo: string;
  longitud: number;
  nullable: boolean;
}

export interface TablaResumen {
  tabla: string;
  columnas: number;
}

export interface TablaDetalle {
  tabla: string;
  columnas: ColumnaEsquema[];
  estacionCodigo: string | null;
  actualizado: string | null;
}

export const esquemaService = {
  /** Busca tablas por nombre en el esquema reportado por los agentes. */
  buscarTablas: (buscar: string) =>
    api
      .get<TablaResumen[]>("/esquema/tablas", { params: { buscar } })
      .then((r) => r.data),

  /** Columnas (documentación automática) de una tabla. */
  getTabla: (nombre: string) =>
    api.get<TablaDetalle>(`/esquema/tabla/${encodeURIComponent(nombre)}`).then((r) => r.data),

  /** Pide a una estación conectada que reporte su esquema (lo envía en su próximo latido). */
  solicitar: (codigoEstacion: string) =>
    api
      .post(`/esquema/solicitar/${encodeURIComponent(codigoEstacion)}`)
      .then((r) => r.data),
};
