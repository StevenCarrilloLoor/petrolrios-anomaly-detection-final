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
  agregacion: AgregacionRegla | null;
  riesgoBase: number;
  activa: boolean;
}

export interface GuardarReglaPersonalizadaRequest {
  nombre: string;
  descripcion: string;
  fuenteDatos: string;
  condiciones: CondicionRegla[];
  agregacion: AgregacionRegla | null;
  riesgoBase: number;
  activa: boolean;
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
