/* eslint-disable react-refresh/only-export-components -- El contexto y sus hooks forman una API cohesiva. */
import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  useRef,
} from "react";
import type { ReactNode } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { subscribeSignalREvent } from "@/services/signalr";
import type { AlertaResponse } from "@/types/alert";
import { ToastContainer, type Toast } from "./ToastContainer";
import { useAuth } from "@/contexts/AuthContext";

interface NotificationContextType {
  alertCount: number;
  resetCount: () => void;
}

const NotificationContext = createContext<NotificationContextType>({
  alertCount: 0,
  resetCount: () => {},
});

export function useNotificationCount(): number {
  return useContext(NotificationContext).alertCount;
}

export function useResetNotifications(): () => void {
  return useContext(NotificationContext).resetCount;
}

export function NotificationProvider({ children }: { children: ReactNode }) {
  const { token } = useAuth();
  const queryClient = useQueryClient();
  const [alertCount, setAlertCount] = useState(0);
  const [toasts, setToasts] = useState<Toast[]>([]);
  const recibidas = useRef(new Set<string>());

  const addToast = useCallback((toast: Omit<Toast, "id">) => {
    const id = crypto.randomUUID();
    setToasts((prev) => [...prev, { ...toast, id }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, 5000);
  }, []);

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const resetCount = useCallback(() => {
    setAlertCount(0);
  }, []);

  useEffect(() => {
    if (!token) return;

    const handler = (alerta: AlertaResponse) => {
      // Un administrador adscrito a una estación pertenece al grupo de su rol y al
      // grupo de estación. La misma alerta operativa puede llegar por ambos eventos.
      const claveNotificacion =
        alerta.notificationId ??
        `${alerta.id}:${String(alerta.ambito)}:${alerta.descripcion}`;
      if (recibidas.current.has(claveNotificacion)) return;
      recibidas.current.add(claveNotificacion);
      setTimeout(() => recibidas.current.delete(claveNotificacion), 60_000);

      const operativa = esAlertaOperativa(alerta.ambito);
      setAlertCount((prev) => prev + 1);
      addToast({
        title: operativa
          ? "Nuevo problema de estación"
          : `Nueva alerta: ${alerta.tipoDetector}`,
        message: alerta.descripcion,
        level: alerta.nivelRiesgo,
      });

      // Tiempo real: refrescar automáticamente las vistas afectadas
      // sin que el usuario tenga que recargar la página
      void queryClient.invalidateQueries({ queryKey: ["alertas"] });
      void queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      void queryClient.invalidateQueries({ queryKey: ["monitoreo"] });
      if (operativa) {
        void queryClient.invalidateQueries({ queryKey: ["problemas-estacion"] });
      }
    };

    const unsubscribeAlerta = subscribeSignalREvent<AlertaResponse>(
      "NuevaAlerta",
      handler,
    );
    const unsubscribeProblema = subscribeSignalREvent<AlertaResponse>(
      "ProblemaEstacion",
      handler,
    );

    return () => {
      unsubscribeAlerta();
      unsubscribeProblema();
    };
  }, [token, addToast, queryClient]);

  return (
    <NotificationContext.Provider value={{ alertCount, resetCount }}>
      {children}
      <ToastContainer toasts={toasts} onRemove={removeToast} />
    </NotificationContext.Provider>
  );
}

function esAlertaOperativa(ambito: unknown): boolean {
  if (typeof ambito === "number") return ambito === 1;
  return String(ambito).trim().toLowerCase() === "operativa";
}
