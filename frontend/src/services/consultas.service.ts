import { api } from "./api";

/** Solicitud de consulta en vivo a la Firebird de una estación. */
export interface SolicitudConsulta {
  codigoEstacion: string;
  tipoDocumento?: string | null; // "FV" | "EB" | "DV" | null = todos
  fechaDesde?: string | null; // ISO (yyyy-mm-dd)
  fechaHasta?: string | null;
  codigo?: string | null; // coincide con RUC, placa, cliente, despachador o n.º de documento
  limite?: number;
  tabla?: string; // "DCTO" (defecto) | "DESP" (líneas de surtidor por NUM_DESP)
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
  NumeroDespacho?: string; // NDO_DCTO: el despacho (DESP) que originó la factura
  // Enriquecido (etapa C): nombre/contacto del cliente (CLIE), nombre del despachador (VEND) y más DCTO.
  ClienteNombre?: string;
  ClienteRazon?: string;
  ClienteCorreo?: string;
  ClienteTelefono?: string;
  Direccion?: string;
  Chofer?: string;
  VendedorNombre?: string;
  Consecutivo?: number;
  Autorizacion?: string;
  Guia?: string;
  Observaciones?: string;
  Subtotal?: number;
  [k: string]: unknown;
}

// Campos que la interfaz espera en PascalCase. Firebird/el driver pueden devolver las columnas en
// MAYÚSCULAS (SECUENCIADOCUMENTO…), así que normalizamos por nombre case-insensitive: funcione como funcione
// el alias del agente, la tabla y la factura siempre encuentran su valor.
const CAMPOS_DOC = [
  "SecuenciaDocumento", "TipoDocumento", "NumeroDocumento", "Fecha", "Cliente", "Ruc", "Vendedor",
  "Placa", "FormaPago", "NumeroTurno", "TotalSinIva", "Iva", "Descuento", "TotalNeto", "NumeroDespacho",
  "ClienteNombre", "ClienteRazon", "ClienteCorreo", "ClienteTelefono", "Direccion", "Chofer",
  "VendedorNombre", "Consecutivo", "Autorizacion", "Guia", "Observaciones", "Subtotal",
] as const;

function normalizarDoc(raw: Record<string, unknown>): DocumentoFirebird {
  const porMinuscula: Record<string, unknown> = {};
  for (const k of Object.keys(raw)) porMinuscula[k.toLowerCase()] = raw[k];
  const out: Record<string, unknown> = { ...raw };
  for (const campo of CAMPOS_DOC) out[campo] = porMinuscula[campo.toLowerCase()];
  return out as DocumentoFirebird;
}

/** Una línea de surtidor (fila de DESP) de una factura: producto, galones, precio. */
export interface DespachoFirebird {
  NumeroDespacho?: number;
  CodigoProducto?: string;
  Producto?: string;
  Manguera?: string;
  Galones?: number;
  PrecioUnitario?: number;
  Total?: number;
  Fecha?: string;
  FormaPago?: string;
  Cliente?: string;
  [k: string]: unknown;
}

const CAMPOS_DESP = [
  "NumeroDespacho", "CodigoProducto", "Producto", "Manguera", "Galones", "PrecioUnitario",
  "Total", "Fecha", "FormaPago", "Cliente",
] as const;

function normalizarDespacho(raw: Record<string, unknown>): DespachoFirebird {
  const porMinuscula: Record<string, unknown> = {};
  for (const k of Object.keys(raw)) porMinuscula[k.toLowerCase()] = raw[k];
  const out: Record<string, unknown> = { ...raw };
  for (const campo of CAMPOS_DESP) out[campo] = porMinuscula[campo.toLowerCase()];
  return out as DespachoFirebird;
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
   * Devuelve las filas crudas del sobre { documentos }. Lo usan las consultas de documentos y de surtidor.
   */
  async sondearFilas(s: SolicitudConsulta, signal?: AbortSignal): Promise<Record<string, unknown>[]> {
    const id = await this.lanzar(s);
    const inicio = Date.now();
    while (Date.now() - inicio < 30_000) {
      if (signal?.aborted) throw new Error("cancelado");
      await new Promise((r) => setTimeout(r, 700));
      const e = await this.estado(id);
      if (e.estado === "Listo") {
        const parsed = JSON.parse(e.resultadoJson ?? "{}") as { documentos?: Record<string, unknown>[] };
        return parsed.documentos ?? [];
      }
      if (e.estado === "Error") throw new Error(e.error ?? "La consulta falló en la estación.");
    }
    throw new Error("La estación no respondió a tiempo (¿el agente está desconectado?).");
  },

  /** Documentos (DCTO) que coinciden con el filtro. */
  async consultarDocumentos(s: SolicitudConsulta, signal?: AbortSignal): Promise<DocumentoFirebird[]> {
    return (await this.sondearFilas(s, signal)).map(normalizarDoc);
  },

  /** Líneas de surtidor (DESP) de una factura, por su número de despacho (NDO_DCTO → NUM_DESP). */
  async consultarDespachos(
    codigoEstacion: string,
    numeroDespacho: string,
    signal?: AbortSignal,
  ): Promise<DespachoFirebird[]> {
    const filas = await this.sondearFilas(
      { codigoEstacion, codigo: numeroDespacho, limite: 50, tabla: "DESP" },
      signal,
    );
    return filas.map(normalizarDespacho);
  },

  /**
   * Consulta GENÉRICA de CUALQUIER tabla de la estación (auto-estructurada): devuelve las filas crudas
   * tal cual (todas las columnas), para renderizarlas con columnas dinámicas. El agente valida que la
   * tabla exista (lista blanca anti-inyección) y corre `SELECT FIRST n * FROM "tabla"` SOLO LECTURA.
   */
  async consultarTabla(
    codigoEstacion: string,
    tabla: string,
    limite = 100,
    signal?: AbortSignal,
  ): Promise<Record<string, unknown>[]> {
    return this.sondearFilas({ codigoEstacion, tabla: tabla.trim(), limite }, signal);
  },
};
