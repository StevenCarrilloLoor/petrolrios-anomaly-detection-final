export interface CondicionRegla {
  campo: string;
  operador: string;
  valor: string;
}

export interface AgregacionRegla {
  agruparPor: string;
  funcion: string;
  campo: string | null;
  operador: string;
  umbral: number;
}

export interface ReglaPersonalizadaResponse {
  id: number;
  nombre: string;
  descripcion: string;
  fuenteDatos: string;
  condiciones: CondicionRegla[];
  combinadorCondiciones: "Y" | "O";
  agregacion: AgregacionRegla | null;
  expresionAvanzada: string | null;
  riesgoBase: number;
  ambito: "Operativa" | "Auditoria";
  activa: boolean;
}

export interface GuardarReglaPersonalizadaRequest {
  nombre: string;
  descripcion: string;
  fuenteDatos: string;
  condiciones: CondicionRegla[];
  combinadorCondiciones: "Y" | "O";
  agregacion: AgregacionRegla | null;
  expresionAvanzada: string | null;
  riesgoBase: number;
  ambito: "Operativa" | "Auditoria";
  activa: boolean;
}

export interface ValidarExpresionResponse {
  valida: boolean;
  errores: string[];
}

export interface BacktestReglaRequest {
  regla: GuardarReglaPersonalizadaRequest;
  dias: number;
}

export interface BacktestCoincidencia {
  nivel: string;
  score: number;
  descripcion: string;
  empleado: string | null;
  estacion: string | null;
}

export interface BacktestReglaResponse {
  valida: boolean;
  errores: string[];
  ventanaDias: number;
  registrosEvaluados: number;
  totalCoincidencias: number;
  bajo: number;
  medio: number;
  alto: number;
  critico: number;
  muestra: BacktestCoincidencia[];
}

export interface CampoCatalogo {
  nombre: string;
  etiqueta: string;
  tipo: "numero" | "texto";
}

export interface FuenteCatalogo {
  nombre: string;
  etiqueta: string;
  campos: CampoCatalogo[];
}

export interface CatalogoReglasResponse {
  fuentes: FuenteCatalogo[];
  operadoresNumero: string[];
  operadoresTexto: string[];
  funciones: string[];
}
