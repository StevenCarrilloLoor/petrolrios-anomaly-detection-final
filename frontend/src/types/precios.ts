// Precios de combustible de Ecuador. Extra/Ecopaís/Diésel son regulados (precio único nacional);
// la Súper es libre mercado (referencial). Se muestra el precio del SISTEMA (efectivo) y el de la API
// (último scrapeado) lado a lado; si la API falla, el sistema conserva su precio.

export interface PrecioCombustible {
  producto: string; // "Extra" | "Ecopais" | "Diesel" | "Super"
  nombre: string;
  esRegulado: boolean;
  precioGalon: number; // precio del SISTEMA (manual/sembrado)
  fechaSistema: string; // cuándo se fijó el precio del sistema
  precioApi: number | null; // último observado por scraping
  fuenteApi: string | null;
  apiActualizadoEn: string | null;
  precioVigente: number; // EL EFECTIVO según la preferencia
  origenVigente: string; // "Sistema" | "API"
  subsidio: number;
  precioPendiente: boolean;
  vigenteDesde: string;
  vigenteHasta: string | null;
  fuente: string;
}

export interface PreciosCombustibleResponse {
  precios: PrecioCombustible[];
  moneda: string;
  consultadoEn: string;
  nota: string;
  fuentesDegradadas: string[];
}
