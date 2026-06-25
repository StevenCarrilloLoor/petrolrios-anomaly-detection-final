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
export type TamanoFuente = "normal" | "grande" | "mayor";

export interface Ajustes {
  /** Tema de la interfaz: sigue el sistema operativo, o se fuerza claro/oscuro. */
  tema: Tema;
  /** Reproducir un sonido al recibir una alerta crítica. */
  sonidoAlertas: boolean;
  /** Mostrar avisos emergentes (toasts) cuando llega una alerta o problema. */
  mostrarToasts: boolean;
  /** Tamaño de la letra de toda la interfaz (accesibilidad para quien le cuesta ver). */
  tamanoFuente: TamanoFuente;
}

const DEFAULTS: Ajustes = {
  tema: "sistema",
  sonidoAlertas: true,
  mostrarToasts: true,
  tamanoFuente: "normal",
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

/** Escala el tamaño base de la letra (rem) de toda la interfaz, para accesibilidad. */
function aplicarTamanoFuente(t: TamanoFuente): void {
  const px = t === "mayor" ? "19px" : t === "grande" ? "17px" : "16px";
  document.documentElement.style.fontSize = px;
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

  useEffect(() => {
    aplicarTamanoFuente(ajustes.tamanoFuente);
  }, [ajustes.tamanoFuente]);

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
