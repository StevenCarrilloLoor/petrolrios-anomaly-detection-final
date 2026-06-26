export interface DatoRecibidoResponse {
  id: number;
  tipoTransaccion: string;
  tipoNatural: string;
  tabla: string;
  estacionId: number;
  estacionCodigo: string;
  estacionNombre: string;
  fechaOriginal: string;
  procesada: boolean;
  dataJson: string;
  createdAt: string;
}

/** Opción del desplegable de tipos: valor crudo para filtrar + etiqueta "Natural (TABLA)". */
export interface TipoRecibidoOption {
  tipo: string;
  etiqueta: string;
}
