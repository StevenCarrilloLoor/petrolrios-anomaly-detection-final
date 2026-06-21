/* eslint-disable react-refresh/only-export-components -- El provider y su hook son una API cohesiva. */
import {
  createContext,
  useContext,
  useEffect,
  useState,
  useCallback,
} from "react";
import type { ReactNode } from "react";

export type Tema = "sistema" | "claro" | "oscuro";

export interface Ajustes {
  /** Tema de la interfaz: sigue el sistema operativo, o se fuerza claro/oscuro. */
  tema: Tema;
  /** Reproducir un sonido al recibir una alerta crítica. */
  sonidoAlertas: boolean;
  /** Mostrar avisos emergentes (toasts) cuando llega una alerta o problema. */
  mostrarToasts: boolean;
}

const DEFAULTS: Ajustes = {
  tema: "sistema",
  sonidoAlertas: true,
  mostrarToasts: true,
};

const STORAGE_KEY = "petrolrios.ajustes";

function cargar(): Ajustes {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return DEFAULTS;
    return { ...DEFAULTS, ...(JSON.parse(raw) as Partial<Ajustes>) };
  } catch {
    return DEFAULTS;
  }
}

/** Aplica el tema fijando (o quitando) la clase en <html>. Sin clase = sigue el sistema. */
function aplicarTema(tema: Tema): void {
  const el = document.documentElement;
  el.classList.remove("theme-claro", "theme-oscuro");
  if (tema === "claro") el.classList.add("theme-claro");
  else if (tema === "oscuro") el.classList.add("theme-oscuro");
}

interface SettingsContextType {
  ajustes: Ajustes;
  actualizar: (cambios: Partial<Ajustes>) => void;
}

const SettingsContext = createContext<SettingsContextType>({
  ajustes: DEFAULTS,
  actualizar: () => {},
});

export function useAjustes(): SettingsContextType {
  return useContext(SettingsContext);
}

export function SettingsProvider({ children }: { children: ReactNode }) {
  const [ajustes, setAjustes] = useState<Ajustes>(cargar);

  useEffect(() => {
    aplicarTema(ajustes.tema);
  }, [ajustes.tema]);

  const actualizar = useCallback((cambios: Partial<Ajustes>) => {
    setAjustes((prev) => {
      const siguiente = { ...prev, ...cambios };
      try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(siguiente));
      } catch {
        /* almacenamiento no disponible */
      }
      return siguiente;
    });
  }, []);

  return (
    <SettingsContext.Provider value={{ ajustes, actualizar }}>
      {children}
    </SettingsContext.Provider>
  );
}
