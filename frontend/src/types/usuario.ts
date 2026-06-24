export interface UsuarioResponse {
  id: number;
  email: string;
  nombreCompleto: string;
  rol: string;
  rolId: number;
  estacionId: number | null;
  activo: boolean;
  createdAt: string;
}

export interface CrearUsuarioRequest {
  email: string;
  nombreCompleto: string;
  password: string;
  rolId: number;
  estacionId?: number | null;
  /** Código de una estación NUEVA a crear y asignar en el momento (escala a >10 estaciones). */
  codigoEstacionNueva?: string | null;
}

export interface ActualizarUsuarioRequest {
  nombreCompleto?: string | null;
  rolId?: number | null;
  activo?: boolean | null;
  estacionId?: number | null;
  actualizarEstacion?: boolean;
}
