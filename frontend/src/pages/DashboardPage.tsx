import type { ReactNode } from "react";
import { useQuery } from "@tanstack/react-query";
import { dashboardService } from "@/services/dashboard.service";
import { Spinner } from "@/components/ui/Spinner";
import {
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Tooltip,
  Legend,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
} from "recharts";
import {
  AlertTriangle,
  AlertCircle,
  Clock,
  CheckCircle,
  XCircle,
  Building2,
  TrendingUp,
  ShieldAlert,
} from "lucide-react";

const CHART_COLORS = ["#3b82f6", "#8b5cf6", "#f97316", "#06b6d4"];

export function DashboardPage() {
  const { data: kpis, isLoading: loadingKpis } = useQuery({
    queryKey: ["dashboard", "kpis"],
    queryFn: dashboardService.getKpis,
    refetchInterval: 30_000,
  });

  const { data: porTipo, isLoading: loadingTipo } = useQuery({
    queryKey: ["dashboard", "alertas-por-tipo"],
    queryFn: dashboardService.getAlertasPorTipo,
    refetchInterval: 30_000,
  });

  const { data: porEstacion, isLoading: loadingEstacion } = useQuery({
    queryKey: ["dashboard", "alertas-por-estacion"],
    queryFn: dashboardService.getAlertasPorEstacion,
    refetchInterval: 30_000,
  });

  if (loadingKpis) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-foreground">Dashboard</h1>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <KpiCard
          icon={<ShieldAlert size={22} />}
          title="Total Alertas"
          value={kpis?.totalAlertas ?? 0}
        />
        <KpiCard
          icon={<AlertCircle size={22} />}
          title="Alertas Nuevas"
          value={kpis?.alertasNuevas ?? 0}
          color="text-blue-500"
        />
        <KpiCard
          icon={<AlertTriangle size={22} />}
          title="Alertas Críticas"
          value={kpis?.alertasCriticas ?? 0}
          color="text-risk-critical"
        />
        <KpiCard
          icon={<Clock size={22} />}
          title="En Revisión"
          value={kpis?.alertasEnRevision ?? 0}
          color="text-risk-medium"
        />
        <KpiCard
          icon={<CheckCircle size={22} />}
          title="Confirmadas"
          value={kpis?.alertasConfirmadas ?? 0}
          color="text-risk-high"
        />
        <KpiCard
          icon={<XCircle size={22} />}
          title="Falso Positivo"
          value={kpis?.alertasFalsoPositivo ?? 0}
          color="text-muted-foreground"
        />
        <KpiCard
          icon={<TrendingUp size={22} />}
          title="Score Promedio"
          value={kpis?.scorePromedio?.toFixed(1) ?? "0"}
        />
        <KpiCard
          icon={<Building2 size={22} />}
          title="Estaciones Activas"
          value={kpis?.estacionesActivas ?? 0}
          color="text-risk-low"
        />
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <div className="rounded-lg border border-border bg-background p-6">
          <h3 className="mb-4 text-lg font-semibold text-foreground">
            Alertas por Tipo de Detector
          </h3>
          {loadingTipo ? (
            <div className="flex h-64 items-center justify-center">
              <Spinner />
            </div>
          ) : porTipo && porTipo.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={porTipo}
                  dataKey="cantidad"
                  nameKey="tipoDetector"
                  cx="50%"
                  cy="50%"
                  outerRadius={100}
                  label
                >
                  {porTipo.map((_, index) => (
                    <Cell
                      key={`cell-tipo-${index}`}
                      fill={CHART_COLORS[index % CHART_COLORS.length]}
                    />
                  ))}
                </Pie>
                <Tooltip />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          ) : (
            <p className="flex h-64 items-center justify-center text-muted-foreground">
              Sin datos disponibles
            </p>
          )}
        </div>

        <div className="rounded-lg border border-border bg-background p-6">
          <h3 className="mb-4 text-lg font-semibold text-foreground">
            Alertas por Estación
          </h3>
          {loadingEstacion ? (
            <div className="flex h-64 items-center justify-center">
              <Spinner />
            </div>
          ) : porEstacion && porEstacion.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={porEstacion}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="estacionNombre" tick={{ fontSize: 11 }} />
                <YAxis />
                <Tooltip />
                <Bar dataKey="cantidad" fill="#3b82f6" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="flex h-64 items-center justify-center text-muted-foreground">
              Sin datos disponibles
            </p>
          )}
        </div>
      </div>
    </div>
  );
}

function KpiCard({
  icon,
  title,
  value,
  color = "text-foreground",
}: {
  icon: ReactNode;
  title: string;
  value: string | number;
  color?: string;
}) {
  return (
    <div className="rounded-lg border border-border bg-background p-6 shadow-sm">
      <div className="flex items-center gap-3">
        <div className="text-muted-foreground">{icon}</div>
        <div>
          <p className="text-sm text-muted-foreground">{title}</p>
          <p className={`mt-1 text-2xl font-bold ${color}`}>{value}</p>
        </div>
      </div>
    </div>
  );
}
