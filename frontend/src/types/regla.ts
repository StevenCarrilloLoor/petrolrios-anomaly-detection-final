export type ModoProgramacion = "CadaCiclo" | "Intervalo" | "Calendario";
export type UnidadIntervalo =
  | "Segundos"
  | "Minutos"
  | "Horas"
  | "Dias"
  | "Semanas"
  | "Meses";
export type TipoCalendario = "Diario" | "Semanal" | "Mensual";

/** Cadencia de ejecución de una regla (espejo de ProgramacionDto del backend). */
export interface ProgramacionDto {
  modo: ModoProgramacion;
  /** Modo Intervalo: cantidad (≥ 1). */
  intervaloN: number;
  intervaloUnidad: UnidadIntervalo;
  /** Modo Calendario. */
  calendarioTipo: TipoCalendario;
  hora: number;
  minuto: number;
  /** 0 = domingo … 6 = sábado (modo semanal). */
  diaSemana: number;
  /** 1–31 (modo mensual). */
  diaMes: number;
  /** Mensual: usar el último día del mes (sin importar cuántos tenga). */
  ultimoDiaDelMes: boolean;
  /** Texto legible en español (solo lectura, lo calcula el backend o el selector). */
  descripcion: string;
}

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
  /** Cadencia de ejecución (cada ciclo / intervalo / calendario). */
  programacion: ProgramacionDto;
  /** Próxima ejecución programada (ISO UTC); null en "cada ciclo" o recién configurada. */
  proximaEjecucion: string | null;
  /** Última ejecución programada (ISO UTC); null si nunca o si es "cada ciclo". */
  ultimaEjecucion: string | null;
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
  /** Cambia la cadencia de ejecución; al cambiarla el job reancla la próxima ejecución. */
  programacion?: ProgramacionDto | null;
}
