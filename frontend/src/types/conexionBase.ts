export interface ConexionActiva {
  fuente: string;
  editableDesdeUi: boolean;
  enmascarada: string;
  servidor: string | null;
  puerto: number;
  baseDatos: string | null;
  usuario: string | null;
  modoSsl: string;
}

export interface ProbarConexionRequest {
  cadena?: string;
  servidor?: string;
  puerto?: number;
  baseDatos?: string;
  usuario?: string;
  password?: string;
  modoSsl?: string;
}

export interface ProbarConexionResponse {
  ok: boolean;
  mensaje: string | null;
  version: string | null;
}

export interface GuardarConexionResponse {
  ok: boolean;
  mensaje: string;
}
