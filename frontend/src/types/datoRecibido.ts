export interface DatoRecibidoResponse {
  id: number;
  tipoTransaccion: string;
  estacionId: number;
  estacionCodigo: string;
  estacionNombre: string;
  fechaOriginal: string;
  procesada: boolean;
  dataJson: string;
  createdAt: string;
}
