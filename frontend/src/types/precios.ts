// Precios oficiales de los combustibles regulados de Ecuador (Extra/Ecopaís/Diésel).
// Espejo de PreciosCombustibleResponse del central. La Súper se excluye (precio no regulado).

export interface PrecioCombustible {
  producto: string; // "Extra" | "Ecopais" | "Diesel"
  nombre: string; // "Gasolina Extra"
  precioGalon: number;
  subsidio: number;
  vigenteDesde: string;
  vigenteHasta: string | null;
  fuente: string;
}

export interface PreciosCombustibleResponse {
  precios: PrecioCombustible[];
  moneda: string;
  consultadoEn: string;
  nota: string;
}
