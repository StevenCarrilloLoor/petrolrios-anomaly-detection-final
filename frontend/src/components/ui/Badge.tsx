import type { ReactNode } from "react";
import { cn } from "@/lib/utils";
import type { NivelRiesgo, EstadoAlerta } from "@/types/alert";

const riskColors: Record<NivelRiesgo, string> = {
  Bajo: "bg-risk-low/20 text-risk-low border-risk-low/30",
  Medio: "bg-risk-medium/20 text-risk-medium border-risk-medium/30",
  Alto: "bg-risk-high/20 text-risk-high border-risk-high/30",
  Critico: "bg-risk-critical/20 text-risk-critical border-risk-critical/30",
};

const statusColors: Record<EstadoAlerta, string> = {
  Nueva: "bg-blue-500/20 text-blue-600 border-blue-500/30",
  EnRevision: "bg-yellow-500/20 text-yellow-600 border-yellow-500/30",
  Confirmada: "bg-red-500/20 text-red-600 border-red-500/30",
  FalsoPositivo: "bg-gray-500/20 text-gray-600 border-gray-500/30",
  Cerrada: "bg-green-500/20 text-green-600 border-green-500/30",
};

interface BadgeProps {
  children: ReactNode;
  variant?: "default" | "risk" | "status";
  riskLevel?: NivelRiesgo;
  status?: EstadoAlerta;
  className?: string;
}

export function Badge({
  children,
  variant = "default",
  riskLevel,
  status,
  className,
}: BadgeProps) {
  let colorClasses = "bg-muted text-muted-foreground border-border";

  if (variant === "risk" && riskLevel) {
    colorClasses = riskColors[riskLevel];
  } else if (variant === "status" && status) {
    colorClasses = statusColors[status];
  }

  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold",
        colorClasses,
        className,
      )}
    >
      {children}
    </span>
  );
}
