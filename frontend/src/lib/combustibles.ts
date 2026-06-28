// Código de producto Contaplus → nombre real del combustible (espejo de Domain.Enums.Combustibles).
// Confirmado por precio en SanPio: 1=Súper, 2=Extra/Ecopaís, 3=Diésel. Así el auditor ve el NOMBRE,
// no el número. El código queda disponible para un tooltip (por si hay algún error de mapeo).

const MAPA: Record<string, string> = {
  "1": "Súper",
  "2": "Extra/Ecopaís",
  "3": "Diésel",
};

/** Nombre del combustible por su código (tolera "1" y "01"); si es desconocido, devuelve el código tal cual. */
export function nombreCombustible(codigo: string | number | null | undefined): string {
  const raw = String(codigo ?? "").trim();
  const norm = raw.replace(/^0+/, ""); // "01" → "1"
  return MAPA[norm] ?? raw;
}
