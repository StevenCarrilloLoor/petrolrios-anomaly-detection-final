/* eslint-disable react-refresh/only-export-components -- el provider y su hook forman una API cohesiva */
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";
import type { ReactNode } from "react";
import { AlertTriangle } from "lucide-react";

export interface ConfirmOptions {
  /** Título del diálogo. Por defecto "¿Confirmar acción?". */
  titulo?: string;
  /** Mensaje/explicación de lo que se va a hacer. */
  mensaje: string;
  textoConfirmar?: string;
  textoCancelar?: string;
  /** Si es false, usa el estilo primario en vez del destructivo (rojo). Por defecto destructivo. */
  destructivo?: boolean;
}

type Resolver = (ok: boolean) => void;

const ConfirmContext = createContext<(opts: ConfirmOptions) => Promise<boolean>>(
  () => Promise.resolve(false),
);

/**
 * Devuelve una función `confirmar(opts) => Promise<boolean>` para reemplazar el `confirm()` nativo.
 * El nativo bloquea el hilo del renderer (rompe pruebas E2E y congela la UI); este modal no.
 *
 * Uso: `if (await confirmar({ mensaje: "¿Eliminar?" })) { ...accion... }`
 */
export function useConfirm() {
  return useContext(ConfirmContext);
}

/** Monta el modal de confirmación una sola vez y expone `useConfirm` a toda la app. */
export function ConfirmProvider({ children }: { children: ReactNode }) {
  const [opciones, setOpciones] = useState<ConfirmOptions | null>(null);
  const resolverRef = useRef<Resolver | null>(null);

  const confirmar = useCallback((opts: ConfirmOptions) => {
    setOpciones(opts);
    return new Promise<boolean>((resolve) => {
      resolverRef.current = resolve;
    });
  }, []);

  const cerrar = useCallback((ok: boolean) => {
    resolverRef.current?.(ok);
    resolverRef.current = null;
    setOpciones(null);
  }, []);

  // ESC cancela; Enter confirma (mientras el diálogo está abierto).
  useEffect(() => {
    if (!opciones) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") cerrar(false);
      if (e.key === "Enter") cerrar(true);
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [opciones, cerrar]);

  const destructivo = opciones?.destructivo !== false;

  return (
    <ConfirmContext.Provider value={confirmar}>
      {children}
      {opciones && (
        <div
          className="fixed inset-0 z-[100] flex items-center justify-center bg-black/50 p-4"
          onClick={() => cerrar(false)}
          role="presentation"
        >
          <div
            className="w-full max-w-md rounded-xl border border-border bg-background p-6 shadow-2xl"
            onClick={(e) => e.stopPropagation()}
            role="alertdialog"
            aria-modal="true"
          >
            <div className="flex items-start gap-3">
              <div
                className={`mt-0.5 shrink-0 rounded-full p-2 ${
                  destructivo
                    ? "bg-risk-critical/15 text-risk-critical"
                    : "bg-primary/15 text-primary"
                }`}
              >
                <AlertTriangle size={18} />
              </div>
              <div className="flex-1">
                <h3 className="text-base font-semibold text-foreground">
                  {opciones.titulo ?? "¿Confirmar acción?"}
                </h3>
                <p className="mt-1 text-sm text-muted-foreground">{opciones.mensaje}</p>
              </div>
            </div>
            <div className="mt-6 flex justify-end gap-3">
              <button
                onClick={() => cerrar(false)}
                className="rounded-md border border-border px-4 py-2 text-sm hover:bg-muted"
              >
                {opciones.textoCancelar ?? "Cancelar"}
              </button>
              <button
                onClick={() => cerrar(true)}
                autoFocus
                className={`rounded-md px-4 py-2 text-sm font-medium text-white ${
                  destructivo
                    ? "bg-risk-critical hover:bg-risk-critical/90"
                    : "bg-primary hover:bg-primary/90"
                }`}
              >
                {opciones.textoConfirmar ?? "Confirmar"}
              </button>
            </div>
          </div>
        </div>
      )}
    </ConfirmContext.Provider>
  );
}
