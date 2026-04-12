export interface UsuarioResponse {
  id: number;
  email: string;
  nombreCompleto: string;
  rol: string;
  rolId: number;
  activo: boolean;
  createdAt: string;
}

export interface CrearUsuarioRequest {
  email: string;
  nombreCompleto: string;
  password: string;
  rolId: number;
}

export interface ActualizarUsuarioRequest {
  nombreCompleto?: string | null;
  rolId?: number | null;
  activo?: boolean | null;
}
