export interface ReglaDeteccionResponse {
  id: number;
  tipoDetector: string;
  nombre: string;
  descripcion: string;
  parametroNombre: string;
  valorUmbral: number;
  activa: boolean;
  /** Carril: "Operativa" (problema de estación) o "Auditoria" (fraude). */
  ambito: "Operativa" | "Auditoria";
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
  /** Cambia el carril: "Operativa" o "Auditoria". */
  ambito?: "Operativa" | "Auditoria" | null;
}
