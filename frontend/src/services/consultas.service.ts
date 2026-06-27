import { api } from "./api";

/** Solicitud de consulta en vivo a la Firebird de una estación. */
export interface SolicitudConsulta {
  codigoEstacion: string;
  tipoDocumento?: string | null; // "FV" | "EB" | "DV" | null = todos
  fechaDesde?: string | null; // ISO (yyyy-mm-dd)
  fechaHasta?: string | null;
  codigo?: string | null; // coincide con RUC, placa, cliente o n.º de documento
  limite?: number;
}

export interface ConsultaEstado {
  id: string;
  estado: "Pendiente" | "Listo" | "Error";
  resultadoJson?: string | null;
  error?: string | null;
}

/** Un documento (fila de DCTO) tal como lo devuelve el agente. Claves = alias del SELECT. */
export interface DocumentoFirebird {
  SecuenciaDocumento?: number;
  TipoDocumento?: string;
  NumeroDocumento?: string;
  Fecha?: string;
  Cliente?: string;
  Ruc?: string;
  Vendedor?: string;
  Placa?: string;
  FormaPago?: string;
  NumeroTurno?: number;
  TotalSinIva?: number;
  Iva?: number;
  Descuento?: number;
  TotalNeto?: number;
  [k: string]: unknown;
}

// Campos que la interfaz espera en PascalCase. Firebird/el driver pueden devolver las columnas en
// MAYÚSCULAS (SECUENCIADOCUMENTO…), así que normalizamos por nombre case-insensitive: funcione como funcione
// el alias del agente, la tabla y la factura siempre encuentran su valor.
const CAMPOS_DOC = [
  "SecuenciaDocumento", "TipoDocumento", "NumeroDocumento", "Fecha", "Cliente", "Ruc", "Vendedor",
  "Placa", "FormaPago", "NumeroTurno", "TotalSinIva", "Iva", "Descuento", "TotalNeto",
] as const;

function normalizarDoc(raw: Record<string, unknown>): DocumentoFirebird {
  const porMinuscula: Record<string, unknown> = {};
  for (const k of Object.keys(raw)) porMinuscula[k.toLowerCase()] = raw[k];
  const out: Record<string, unknown> = { ...raw };
  for (const campo of CAMPOS_DOC) out[campo] = porMinuscula[campo.toLowerCase()];
  return out as DocumentoFirebird;
}

export const consultasService = {
  async lanzar(s: SolicitudConsulta): Promise<string> {
    const { data } = await api.post<{ id: string }>("/consultas", s);
    return data.id;
  },

  async estado(id: string): Promise<ConsultaEstado> {
    const { data } = await api.get<ConsultaEstado>(`/consultas/${id}`);
    return data;
  },

  /**
   * Lanza la consulta y la sondea hasta que el agente responda (Listo/Error) o se agote el tiempo.
   * Devuelve los documentos encontrados.
   */
  async consultarDocumentos(s: SolicitudConsulta, signal?: AbortSignal): Promise<DocumentoFirebird[]> {
    const id = await this.lanzar(s);
    const inicio = Date.now();
    while (Date.now() - inicio < 30_000) {
      if (signal?.aborted) throw new Error("cancelado");
      await new Promise((r) => setTimeout(r, 700));
      const e = await this.estado(id);
      if (e.estado === "Listo") {
        const parsed = JSON.parse(e.resultadoJson ?? "{}") as { documentos?: Record<string, unknown>[] };
        return (parsed.documentos ?? []).map(normalizarDoc);
      }
      if (e.estado === "Error") throw new Error(e.error ?? "La consulta falló en la estación.");
    }
    throw new Error("La estación no respondió a tiempo (¿el agente está desconectado?).");
  },
};
