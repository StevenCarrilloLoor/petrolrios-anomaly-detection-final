import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
} from "react";
import type { ReactNode } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { getConnectionReady, getSignalRConnection } from "@/services/signalr";
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

    let cancelled = false;

    const handler = (alerta: AlertaResponse) => {
      setAlertCount((prev) => prev + 1);
      addToast({
        title: `Nueva alerta: ${alerta.tipoDetector}`,
        message: alerta.descripcion,
        level: alerta.nivelRiesgo,
      });

      // Tiempo real: refrescar automáticamente las vistas afectadas
      // sin que el usuario tenga que recargar la página
      void queryClient.invalidateQueries({ queryKey: ["alertas"] });
      void queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      void queryClient.invalidateQueries({ queryKey: ["monitoreo"] });
    };

    const promise = getConnectionReady();
    if (promise) {
      promise
        .then((conn) => {
          if (cancelled) return;
          conn.on("NuevaAlerta", handler);
        })
        .catch(() => {});
    }

    return () => {
      cancelled = true;
      const conn = getSignalRConnection();
      if (conn) {
        conn.off("NuevaAlerta", handler);
      }
    };
  }, [token, addToast, queryClient]);

  return (
    <NotificationContext.Provider value={{ alertCount, resetCount }}>
      {children}
      <ToastContainer toasts={toasts} onRemove={removeToast} />
    </NotificationContext.Provider>
  );
}
