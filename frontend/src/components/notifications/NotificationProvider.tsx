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
import { useAjustes } from "@/contexts/SettingsContext";

/** Tono breve generado con Web Audio (sin archivo de audio) para alertas críticas. */
function reproducirBeepCritico(): void {
  try {
    const Ctx =
      window.AudioContext ??
      (window as unknown as { webkitAudioContext?: typeof AudioContext })
        .webkitAudioContext;
    if (!Ctx) return;
    const ctx = new Ctx();
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.type = "sine";
    osc.frequency.value = 880;
    osc.connect(gain);
    gain.connect(ctx.destination);
    const t = ctx.currentTime;
    gain.gain.setValueAtTime(0.0001, t);
    gain.gain.exponentialRampToValueAtTime(0.25, t + 0.02);
    gain.gain.exponentialRampToValueAtTime(0.0001, t + 0.45);
    osc.start(t);
    osc.stop(t + 0.45);
    osc.onended = () => void ctx.close();
  } catch {
    /* audio no disponible en este navegador/contexto */
  }
}

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
  const { token, user } = useAuth();
  const queryClient = useQueryClient();
  const [alertCount, setAlertCount] = useState(0);
  const [toasts, setToasts] = useState<Toast[]>([]);
  const recibidas = useRef(new Set<string>());
  const { ajustes } = useAjustes();
  // Se lee por ref para no re-suscribir SignalR cada vez que cambian las preferencias.
  const ajustesRef = useRef(ajustes);
  useEffect(() => {
    ajustesRef.current = ajustes;
  }, [ajustes]);
  // El usuario también por ref: el handler de asignación necesita saber si la alerta es para MÍ.
  const userRef = useRef(user);
  useEffect(() => {
    userRef.current = user;
  }, [user]);

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

    // Carril de AUDITORÍA (posible irregularidad): sube el contador del auditor y refresca la
    // bandeja de alertas y el dashboard. Son las anomalías que un auditor debe revisar.
    const handleNuevaAlerta = (alerta: AlertaResponse) => {
      if (yaRecibida(alerta)) return;
      setAlertCount((prev) => prev + 1);
      if (ajustesRef.current.sonidoAlertas && alerta.nivelRiesgo === "Critico") {
        reproducirBeepCritico();
      }
      if (ajustesRef.current.mostrarToasts) {
        addToast({
          title: `Nueva alerta: ${alerta.tipoDetector}`,
          message: alerta.descripcion,
          level: alerta.nivelRiesgo,
        });
      }
      void queryClient.invalidateQueries({ queryKey: ["alertas"] });
      void queryClient.invalidateQueries({ queryKey: ["dashboard"] });
      void queryClient.invalidateQueries({ queryKey: ["monitoreo"] });
    };

    // Carril OPERATIVO (problema de estación): NO toca el contador del auditor ni la
    // bandeja/dashboard. Solo refresca la pestaña "Problemas de estación".
    const handleProblemaEstacion = (alerta: AlertaResponse) => {
      if (yaRecibida(alerta)) return;
      if (ajustesRef.current.mostrarToasts) {
        addToast({
          title: "Nuevo problema de estación",
          message: alerta.descripcion,
          level: alerta.nivelRiesgo,
        });
      }
      void queryClient.invalidateQueries({ queryKey: ["problemas-estacion"] });
      void queryClient.invalidateQueries({ queryKey: ["monitoreo"] });
    };

    // ALERTA ASIGNADA: avisa en vivo al asignado (toast personalizado "es tuya") y, a supervisores/
    // administradores, una confirmación. Refresca las bandejas para que aparezca el responsable.
    const handleAlertaAsignada = (alerta: AlertaResponse) => {
      if (yaRecibida(alerta)) return;
      const paraMi =
        alerta.asignadoAId != null && alerta.asignadoAId === userRef.current?.id;
      if (ajustesRef.current.mostrarToasts) {
        addToast({
          title: paraMi ? "Te asignaron una alerta" : "Alerta asignada",
          message: paraMi
            ? `La alerta #${alerta.id} es ahora tuya: ${alerta.descripcion}`
            : `Alerta #${alerta.id} asignada a ${alerta.asignadoANombre ?? "un responsable"}`,
          level: alerta.nivelRiesgo,
        });
      }
      if (paraMi && ajustesRef.current.sonidoAlertas) reproducirBeepCritico();
      void queryClient.invalidateQueries({ queryKey: ["alertas"] });
    };

    const unsubscribeAlerta = subscribeSignalREvent<AlertaResponse>(
      "NuevaAlerta",
      handleNuevaAlerta,
    );
    const unsubscribeProblema = subscribeSignalREvent<AlertaResponse>(
      "ProblemaEstacion",
      handleProblemaEstacion,
    );
    const unsubscribeAsignada = subscribeSignalREvent<AlertaResponse>(
      "AlertaAsignada",
      handleAlertaAsignada,
    );

    return () => {
      unsubscribeAlerta();
      unsubscribeProblema();
      unsubscribeAsignada();
    };
  }, [token, addToast, queryClient]);

  return (
    <NotificationContext.Provider value={{ alertCount, resetCount }}>
      {children}
      <ToastContainer toasts={toasts} onRemove={removeToast} />
    </NotificationContext.Provider>
  );
}
