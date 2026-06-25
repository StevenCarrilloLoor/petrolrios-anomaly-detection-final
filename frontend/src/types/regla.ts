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
  /** Si true, la regla envía correo a supervisores/administradores cuando se dispara. */
  notificarCorreo: boolean;
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
  /** Activa/desactiva el aviso por correo de esta regla. */
  notificarCorreo?: boolean | null;
}
