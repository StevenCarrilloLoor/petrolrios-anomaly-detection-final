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

    const yaRecibida = (alerta: AlertaResponse): boolean => {
      // La misma alerta puede llegar por el grupo de rol y el de estación; se deduplica.
      const clave =
        alerta.notificationId ??
        `${alerta.id}:${String(alerta.ambito)}:${alerta.descripcion}`;
      if (recibidas.current.has(clave)) return true;
      recibidas.current.add(clave);
      setTimeout(() => recibidas.current.delete(clave), 60_000);
      return false;
    };

    // Carril de AUDITORÍA (fraude): sube el contador del auditor y refresca la bandeja
    // de alertas y el dashboard. Son las que un auditor debe revisar.
    const handleNuevaAlerta = (alerta: AlertaResponse) => {
      if (yaRecibida(alerta)) return;
      setAlertCount((prev) => prev + 1);
      addToast({
        title: `Nueva alerta: ${alerta.tipoDetector}`,
        message: alerta.descripcion,
        level: alerta.nivelRiesgo,
      });
      void queryClient.invalidateQueries({ queryKey: ["alertas"] });
      void queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      void queryClient.invalidateQueries({ queryKey: ["monitoreo"] });
    };

    // Carril OPERATIVO (problema de estación): NO toca el contador del auditor ni la
    // bandeja/dashboard. Solo refresca la pestaña "Problemas de estación".
    const handleProblemaEstacion = (alerta: AlertaResponse) => {
      if (yaRecibida(alerta)) return;
      addToast({
        title: "Nuevo problema de estación",
        message: alerta.descripcion,
        level: alerta.nivelRiesgo,
      });
      void queryClient.invalidateQueries({ queryKey: ["problemas-estacion"] });
      void queryClient.invalidateQueries({ queryKey: ["monitoreo"] });
    };

    const unsubscribeAlerta = subscribeSignalREvent<AlertaResponse>(
      "NuevaAlerta",
      handleNuevaAlerta,
    );
    const unsubscribeProblema = subscribeSignalREvent<AlertaResponse>(
      "ProblemaEstacion",
      handleProblemaEstacion,
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
