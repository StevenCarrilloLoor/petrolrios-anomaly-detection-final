export interface ReglaDeteccionResponse {
  id: number;
  tipoDetector: string;
  nombre: string;
  descripcion: string;
  parametroNombre: string;
  valorUmbral: number;
  activa: boolean;
}

export interface CrearReglaRequest {
  tipoDetector: string;
  nombre: string;
  descripcion: string;
  parametroNombre: string;
  valorUmbral: number;
}

export interface ActualizarReglaRequest {
  valorUmbral?: number | null;
  activa?: boolean | null;
}
