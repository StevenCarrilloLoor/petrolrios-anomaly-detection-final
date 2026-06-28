import { api } from "./api";
import type { PreciosCombustibleResponse } from "@/types/precios";

// Precios oficiales vigentes de los combustibles regulados (para el dashboard).
export const preciosService = {
  getVigentes: () =>
    api.get<PreciosCombustibleResponse>("/precios-combustible").then((r) => r.data),
};
