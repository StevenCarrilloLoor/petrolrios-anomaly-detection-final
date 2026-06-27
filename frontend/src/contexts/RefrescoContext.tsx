/* eslint-disable react-refresh/only-export-components -- El provider y sus hooks son una API cohesiva. */
import { createContext, useContext } from "react";
import type { ReactNode } from "react";
import { useQuery } from "@tanstack/react-query";
import { operacionService } from "@/services/operacion.service";
import { useAuth } from "@/contexts/AuthContext";

/**
 * Tasa de refresco GLOBAL del central (configurable por el Administrador en Ajustes → Operación).
 * Todas las pantallas la usan como <c>refetchInterval</c>, así que un solo ajuste cambia la
 * frecuencia de actualización de TODA la interfaz. Por defecto 1 s (casi tiempo real).
 */
const DEFAULT_SEGUNDOS = 1;

const RefrescoContext = createContext<number>(DEFAULT_SEGUNDOS);

/** Tasa de refresco global en milisegundos (para usar como refetchInterval de TanStack Query). */
export function useRefrescoMs(): number {
  return useContext(RefrescoContext) * 1000;
}

/** Tasa de refresco global en segundos (para mostrarla en texto). */
export function useRefrescoSegundos(): number {
  return useContext(RefrescoContext);
}

export function RefrescoProvider({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useAuth();
  // Se relee cada 15 s para que un cambio del Administrador llegue a las sesiones ya abiertas.
  const { data } = useQuery({
    queryKey: ["refresco-global"],
    queryFn: () => operacionService.refresco(),
    enabled: isAuthenticated,
    refetchInterval: 15_000,
    staleTime: 10_000,
  });

  const segundos = typeof data === "number" && data > 0 ? data : DEFAULT_SEGUNDOS;
  return <RefrescoContext.Provider value={segundos}>{children}</RefrescoContext.Provider>;
}
