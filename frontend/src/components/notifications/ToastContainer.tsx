import { AlertTriangle, Info, XCircle, X } from "lucide-react";
import type { NivelRiesgo } from "@/types/alert";
import { cn } from "@/lib/utils";

export interface Toast {
  id: string;
  title: string;
  message: string;
  level: NivelRiesgo;
}

interface ToastContainerProps {
  toasts: Toast[];
  onRemove: (id: string) => void;
}

const levelConfig: Record<
  NivelRiesgo,
  { icon: typeof AlertTriangle; color: string; bg: string }
> = {
  Bajo: {
    icon: Info,
    color: "text-risk-low",
    bg: "border-risk-low/30 bg-risk-low/10",
  },
  Medio: {
    icon: AlertTriangle,
    color: "text-risk-medium",
    bg: "border-risk-medium/30 bg-risk-medium/10",
  },
  Alto: {
    icon: AlertTriangle,
    color: "text-risk-high",
    bg: "border-risk-high/30 bg-risk-high/10",
  },
  Critico: {
    icon: XCircle,
    color: "text-risk-critical",
    bg: "border-risk-critical/30 bg-risk-critical/10",
  },
};

export function ToastContainer({ toasts, onRemove }: ToastContainerProps) {
  if (toasts.length === 0) return null;

  return (
    <div className="fixed right-4 top-4 z-50 flex flex-col gap-2">
      {toasts.map((toast) => {
        const config = levelConfig[toast.level];
        const Icon = config.icon;
        return (
          <div
            key={toast.id}
            className={cn(
              "flex w-80 items-start gap-3 rounded-lg border p-4 shadow-lg animate-slide-in",
              config.bg,
            )}
          >
            <Icon size={20} className={cn("mt-0.5 shrink-0", config.color)} />
            <div className="min-w-0 flex-1">
              <p className="text-sm font-semibold text-foreground">
                {toast.title}
              </p>
              <p className="mt-1 line-clamp-2 text-xs text-muted-foreground">
                {toast.message}
              </p>
            </div>
            <button
              onClick={() => onRemove(toast.id)}
              className="shrink-0 text-muted-foreground hover:text-foreground"
            >
              <X size={16} />
            </button>
          </div>
        );
      })}
    </div>
  );
}
