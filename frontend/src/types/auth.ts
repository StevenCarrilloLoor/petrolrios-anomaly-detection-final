export interface LoginRequest {
  email: string;
  password: string;
}

export interface UsuarioInfo {
  id: number;
  email: string;
  nombreCompleto: string;
  rol: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiration: string;
  usuario: UsuarioInfo;
}

export interface RefreshRequest {
  refreshToken: string;
}
