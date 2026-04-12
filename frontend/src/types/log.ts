export interface LogAuditoriaResponse {
  id: number;
  accion: string;
  entidad: string;
  entidadId: number | null;
  detalleJson: string | null;
  direccionIp: string;
  usuarioId: number;
  usuarioEmail: string | null;
  createdAt: string;
}
