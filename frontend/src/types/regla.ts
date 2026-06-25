export interface ReglaDeteccionResponse {
  id: number;
  tipoDetector: string;
  nombre: string;
  descripcion: string;
  parametroNombre: string;
  valorUmbral: number;
  /** Unidad del umbral: "horas", "minutos", "días", "%", "USD ($)", "galones", "veces", "1 = activado". */
  unidad: string;
  /** Explicación corta de qué representa el umbral (tooltip del editor). */
  ayudaUmbral: string;
  activa: boolean;
  /** Carril: "Operativa" (estación), "Auditoria" (central) o "Ambos" (los dos). */
  ambito: "Operativa" | "Auditoria" | "Ambos";
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
  /** Cambia el carril: "Operativa", "Auditoria" o "Ambos". */
  ambito?: "Operativa" | "Auditoria" | "Ambos" | null;
}
