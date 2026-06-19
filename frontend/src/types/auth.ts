export interface LoginRequest {
  email: string;
  password: string;
  codigoTotp?: string;
}

export interface UsuarioInfo {
  id: number;
  email: string;
  nombreCompleto: string;
  rol: string;
  estacionId: number | null;
  estacionCodigo: string | null;
  estacionNombre: string | null;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiration: string;
  usuario: UsuarioInfo;
  debeCambiarPassword?: boolean;
  requiere2Fa?: boolean;
}

export interface Iniciar2faResponse {
  secreto: string;
  uriOtpauth: string;
}

export interface RefreshRequest {
  refreshToken: string;
}
