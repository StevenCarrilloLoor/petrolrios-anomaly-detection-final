import { useState } from "react";
import type { ReactNode } from "react";
import { useQuery, keepPreviousData } from "@tanstack/react-query";
import { dashboardService } from "@/services/dashboard.service";
import { useRefrescoMs } from "@/contexts/RefrescoContext";
import { Card, CardContent, CardHeader } from "@/components/ui/Card";
import { Skeleton } from "@/components/ui/Skeleton";
import { EmptyState } from "@/components/ui/EmptyState";
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
  AreaChart,
  Area,
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
  Timer,
  Target,
  Activity,
  Users,
  Printer,
} from "lucide-react";
import { TIPO_DETECTOR_LABELS, NIVEL_RIESGO_LABELS } from "@/types/alert";
import type { TipoDetector, NivelRiesgo } from "@/types/alert";

const CHART_COLORS = ["#3b82f6", "#8b5cf6", "#f97316", "#06b6d4"];

const NIVEL_COLORS: Record<string, string> = {
  Bajo: "#22c55e",
  Medio: "#eab308",
  Alto: "#f97316",
  Critico: "#ef4444",
};

function labelTipo(tipo: string): string {
  return TIPO_DETECTOR_LABELS[tipo as TipoDetector] ?? tipo;
}

function labelNivel(nivel: string): string {
  return NIVEL_RIESGO_LABELS[nivel as NivelRiesgo] ?? nivel;
}

export function DashboardPage() {
  // Filtro por estación ("no mezclar estaciones", lo pidió auditoría). "" = todas. Acota KPIs,
  // tendencia, distribución por tipo/nivel, métricas y ranking de empleados. La comparativa
  // "alertas por estación" queda global (es la vista cruzada) y alimenta el selector.
  const [estacionFiltro, setEstacionFiltro] = useState("");
  const estId = estacionFiltro ? Number(estacionFiltro) : undefined;
  const refrescoMs = useRefrescoMs();

  const { data: kpis, isLoading: loadingKpis } = useQuery({
    queryKey: ["dashboard", "kpis", estId],
    queryFn: () => dashboardService.getKpis(estId),
    refetchInterval: refrescoMs,
    // Al cambiar de estación, conservar la vista previa mientras llega la nueva (sin parpadeo a esqueleto).
    placeholderData: keepPreviousData,
  });

  const { data: metricas } = useQuery({
    queryKey: ["dashboard", "metricas-resolucion", estId],
    queryFn: () => dashboardService.getMetricasResolucion(estId),
    refetchInterval: refrescoMs,
  });

  const { data: tendencia, isLoading: loadingTendencia } = useQuery({
    queryKey: ["dashboard", "tendencia", estId],
    queryFn: () => dashboardService.getTendencia(14, estId),
    refetchInterval: refrescoMs,
    placeholderData: keepPreviousData,
  });

  const { data: porTipo, isLoading: loadingTipo } = useQuery({
    queryKey: ["dashboard", "alertas-por-tipo", estId],
    queryFn: () => dashboardService.getAlertasPorTipo(estId),
    refetchInterval: refrescoMs,
    placeholderData: keepPreviousData,
  });

  const { data: porEstacion, isLoading: loadingEstacion } = useQuery({
    queryKey: ["dashboard", "alertas-por-estacion"],
    queryFn: dashboardService.getAlertasPorEstacion,
    refetchInterval: refrescoMs,
  });

  const { data: porNivel } = useQuery({
    queryKey: ["dashboard", "alertas-por-nivel", estId],
    queryFn: () => dashboardService.getAlertasPorNivel(estId),
    refetchInterval: refrescoMs,
  });

  const { data: topEmpleados, isLoading: loadingEmpleados } = useQuery({
    queryKey: ["dashboard", "top-empleados", estId],
    queryFn: () => dashboardService.getTopEmpleados(8, estId),
    refetchInterval: refrescoMs,
    placeholderData: keepPreviousData,
  });

  const estacionNombre = (porEstacion ?? []).find(
    (e) => String(e.estacionId) === estacionFiltro,
  )?.estacionNombre;

  if (loadingKpis) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-56" />
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <Skeleton key={i} className="h-28" />
          ))}
        </div>
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
          <Skeleton className="h-80" />
          <Skeleton className="h-80" />
        </div>
      </div>
    );
  }

  const tendenciaFormateada = (tendencia ?? []).map((t) => ({
    ...t,
    dia: new Date(t.fecha).toLocaleDateString("es-EC", {
      day: "2-digit",
      month: "short",
    }),
  }));

  const nivelData = (porNivel ?? []).map((n) => ({
    nombre: labelNivel(n.nivelRiesgo),
    cantidad: n.cantidad,
    color: NIVEL_COLORS[n.nivelRiesgo] ?? "#94a3b8",
  }));

  const tipoData = (porTipo ?? []).map((t) => ({
    ...t,
    nombre: labelTipo(t.tipoDetector),
  }));

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">
            Centro de Monitoreo
          </h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {estacionNombre
              ? `Anomalías de ${estacionNombre} — vista por estación`
              : "Detección de anomalías transaccionales en tiempo real — 10 estaciones"}
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <select
            value={estacionFiltro}
            onChange={(e) => setEstacionFiltro(e.target.value)}
            aria-label="Filtrar el dashboard por estación"
            className="rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 print:hidden"
          >
            <option value="">Todas las estaciones</option>
            {(porEstacion ?? []).map((est) => (
              <option key={est.estacionId} value={est.estacionId}>
                {est.estacionNombre}
              </option>
            ))}
          </select>
          <button
            type="button"
            onClick={() => window.print()}
            title="Imprimir o guardar como PDF esta vista del dashboard"
            className="flex items-center gap-1.5 rounded-md border border-border bg-background px-3 py-2 text-sm text-muted-foreground transition-colors hover:bg-muted hover:text-foreground print:hidden"
          >
            <Printer size={15} /> Imprimir / PDF
          </button>
          <div className="flex items-center gap-2 rounded-full border border-border bg-muted px-3 py-1.5 print:hidden">
            <span className="relative flex h-2.5 w-2.5">
              <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-risk-low opacity-60" />
              <span className="relative inline-flex h-2.5 w-2.5 rounded-full bg-risk-low" />
            </span>
            <span className="text-xs font-medium text-muted-foreground">
              Monitoreo activo
            </span>
          </div>
        </div>
      </div>

      {/* KPIs principales */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <KpiCard
          icon={<ShieldAlert size={20} />}
          title="Total Alertas"
          value={kpis?.totalAlertas ?? 0}
          detail={`${metricas?.alertasUltimas24Horas ?? 0} en las últimas 24 h`}
        />
        <KpiCard
          icon={<AlertCircle size={20} />}
          title="Alertas Nuevas"
          value={kpis?.alertasNuevas ?? 0}
          color="text-blue-500"
          detail="Pendientes de revisión"
        />
        <KpiCard
          icon={<AlertTriangle size={20} />}
          title="Alertas Críticas"
          value={kpis?.alertasCriticas ?? 0}
          color="text-risk-critical"
          accent="border-l-4 border-l-risk-critical"
          detail="Score 76–100"
        />
        <KpiCard
          icon={<Clock size={20} />}
          title="En Revisión"
          value={kpis?.alertasEnRevision ?? 0}
          color="text-risk-medium"
          detail="Asignadas a auditores"
        />
        <KpiCard
          icon={<CheckCircle size={20} />}
          title="Confirmadas"
          value={kpis?.alertasConfirmadas ?? 0}
          color="text-risk-high"
          detail="Irregularidades reales"
        />
        <KpiCard
          icon={<XCircle size={20} />}
          title="Falsos Positivos"
          value={kpis?.alertasFalsoPositivo ?? 0}
          color="text-muted-foreground"
          detail={`Tasa: ${metricas?.tasaFalsosPositivos ?? 0}%`}
        />
        <KpiCard
          icon={<TrendingUp size={20} />}
          title="Score Promedio"
          value={kpis?.scorePromedio?.toFixed(1) ?? "0"}
          detail="Escala 0–100"
        />
        <KpiCard
          icon={<Building2 size={20} />}
          title="Estaciones en Línea"
          value={`${kpis?.estacionesConectadas ?? 0} de ${kpis?.estacionesTotales ?? 0}`}
          color={
            (kpis?.estacionesConectadas ?? 0) > 0
              ? "text-risk-low"
              : "text-risk-critical"
          }
          detail="Agentes con señal de vida (las estaciones se registran solas)"
        />
      </div>

      {/* Métricas de efectividad (CU-13) */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <MetricaCard
          icon={<Timer size={18} />}
          title="Tiempo medio de resolución"
          value={
            metricas
              ? metricas.tiempoMedioResolucionHoras >= 48
                ? `${(metricas.tiempoMedioResolucionHoras / 24).toFixed(1)} días`
                : `${metricas.tiempoMedioResolucionHoras} h`
              : "—"
          }
          description="Desde la detección hasta el cierre de la alerta"
        />
        <MetricaCard
          icon={<Target size={18} />}
          title="Tasa de alertas válidas"
          value={`${metricas?.tasaAlertasValidas ?? 0}%`}
          description="Alertas confirmadas sobre el total resuelto (meta institucional: > 90%)"
        />
        <MetricaCard
          icon={<Activity size={18} />}
          title="Pendientes vs resueltas"
          value={`${metricas?.totalPendientes ?? 0} / ${metricas?.totalResueltas ?? 0}`}
          description="Carga de trabajo actual del equipo de auditoría"
        />
      </div>

      {/* Tendencia 14 días */}
      <Card>
        <CardHeader
          title="Tendencia de alertas — últimos 14 días"
          subtitle="Evolución diaria del total de alertas, con detalle de críticas y altas"
        />
        <CardContent>
          {loadingTendencia ? (
            <Skeleton className="h-64" />
          ) : tendenciaFormateada.length > 0 ? (
            <ResponsiveContainer width="100%" height={280}>
              <AreaChart data={tendenciaFormateada}>
                <defs>
                  <linearGradient id="gradTotal" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.5} />
                    <stop offset="95%" stopColor="#3b82f6" stopOpacity={0.02} />
                  </linearGradient>
                  <linearGradient id="gradCriticas" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#ef4444" stopOpacity={0.5} />
                    <stop offset="95%" stopColor="#ef4444" stopOpacity={0.02} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" />
                <XAxis dataKey="dia" tick={{ fontSize: 11 }} />
                <YAxis allowDecimals={false} tick={{ fontSize: 11 }} />
                <Tooltip />
                <Legend />
                <Area
                  type="monotone"
                  dataKey="total"
                  name="Total"
                  stroke="#3b82f6"
                  fill="url(#gradTotal)"
                  strokeWidth={2}
                />
                <Area
                  type="monotone"
                  dataKey="criticas"
                  name="Críticas"
                  stroke="#ef4444"
                  fill="url(#gradCriticas)"
                  strokeWidth={2}
                />
              </AreaChart>
            </ResponsiveContainer>
          ) : (
            <EmptyState description="Aún no hay alertas registradas en este período." />
          )}
        </CardContent>
      </Card>

      {/* Distribución por tipo / nivel */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader
            title="Alertas por tipo de detector"
            subtitle="Distribución entre los 4 detectores del motor"
          />
          <CardContent>
            {loadingTipo ? (
              <Skeleton className="h-64" />
            ) : tipoData.length > 0 ? (
              <ResponsiveContainer width="100%" height={280}>
                <PieChart>
                  <Pie
                    data={tipoData}
                    dataKey="cantidad"
                    nameKey="nombre"
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={100}
                    paddingAngle={3}
                    label
                  >
                    {tipoData.map((_, index) => (
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
              <EmptyState description="Los detectores aún no han generado alertas." />
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader
            title="Alertas por nivel de riesgo"
            subtitle="Clasificación según el motor de scoring (0–100)"
          />
          <CardContent>
            {nivelData.length > 0 ? (
              <ResponsiveContainer width="100%" height={280}>
                <BarChart data={nivelData} layout="vertical">
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" />
                  <XAxis type="number" allowDecimals={false} tick={{ fontSize: 11 }} />
                  <YAxis
                    type="category"
                    dataKey="nombre"
                    width={70}
                    tick={{ fontSize: 12 }}
                  />
                  <Tooltip />
                  <Bar dataKey="cantidad" name="Alertas" radius={[0, 6, 6, 0]}>
                    {nivelData.map((entry, index) => (
                      <Cell key={`cell-nivel-${index}`} fill={entry.color} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <EmptyState description="Sin alertas clasificadas todavía." />
            )}
          </CardContent>
        </Card>
      </div>

      {/* Estaciones + empleados */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader
            title="Alertas por estación"
            subtitle="Comparativo entre las 10 estaciones piloto"
          />
          <CardContent>
            {loadingEstacion ? (
              <Skeleton className="h-64" />
            ) : porEstacion && porEstacion.length > 0 ? (
              <ResponsiveContainer width="100%" height={280}>
                <BarChart data={porEstacion}>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" />
                  <XAxis
                    dataKey="estacionNombre"
                    tick={{ fontSize: 10 }}
                    angle={-25}
                    textAnchor="end"
                    height={70}
                  />
                  <YAxis allowDecimals={false} tick={{ fontSize: 11 }} />
                  <Tooltip />
                  <Bar
                    dataKey="cantidad"
                    name="Alertas"
                    fill="#3b82f6"
                    radius={[6, 6, 0, 0]}
                  />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <EmptyState description="Sin alertas registradas por estación." />
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader
            title="Empleados con más alertas"
            subtitle="Ranking de riesgo por despachador"
            action={<Users size={18} className="text-muted-foreground" />}
          />
          <CardContent className="p-0">
            {loadingEmpleados ? (
              <div className="p-6">
                <Skeleton className="h-64" />
              </div>
            ) : topEmpleados && topEmpleados.length > 0 ? (
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border text-left text-xs text-muted-foreground">
                    <th className="px-6 py-3 font-medium">Empleado</th>
                    <th className="px-4 py-3 font-medium">Estación</th>
                    <th className="px-4 py-3 text-right font-medium">Alertas</th>
                    <th className="px-4 py-3 text-right font-medium">Críticas</th>
                    <th className="px-6 py-3 text-right font-medium">Score prom.</th>
                  </tr>
                </thead>
                <tbody>
                  {topEmpleados.map((emp) => (
                    <tr
                      key={`${emp.empleadoCodigo}-${emp.estacionNombre}`}
                      className="border-b border-border last:border-0"
                    >
                      <td className="px-6 py-2.5">
                        {emp.empleadoNombre ? (
                          <div className="flex flex-col leading-tight">
                            <span className="font-medium">{emp.empleadoNombre}</span>
                            <span className="font-mono text-xs text-muted-foreground">
                              {emp.empleadoCodigo}
                            </span>
                          </div>
                        ) : (
                          <span className="font-mono font-medium">{emp.empleadoCodigo}</span>
                        )}
                      </td>
                      <td className="px-4 py-2.5 text-muted-foreground">
                        {emp.estacionNombre}
                      </td>
                      <td className="px-4 py-2.5 text-right font-semibold">
                        {emp.cantidadAlertas}
                      </td>
                      <td className="px-4 py-2.5 text-right">
                        {emp.criticas > 0 ? (
                          <span className="font-semibold text-risk-critical">
                            {emp.criticas}
                          </span>
                        ) : (
                          <span className="text-muted-foreground">0</span>
                        )}
                      </td>
                      <td className="px-6 py-2.5 text-right">
                        <ScorePill score={emp.scorePromedio} />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <EmptyState description="Sin alertas asociadas a empleados todavía." />
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

function KpiCard({
  icon,
  title,
  value,
  color = "text-foreground",
  accent = "",
  detail,
}: {
  icon: ReactNode;
  title: string;
  value: string | number;
  color?: string;
  accent?: string;
  detail?: string;
}) {
  return (
    <div
      className={`rounded-xl border border-border bg-background p-5 shadow-sm transition-shadow hover:shadow-md ${accent}`}
    >
      <div className="flex items-center justify-between">
        <p className="text-sm font-medium text-muted-foreground">{title}</p>
        <div className="text-muted-foreground/70">{icon}</div>
      </div>
      <p className={`mt-2 text-3xl font-bold tracking-tight ${color}`}>{value}</p>
      {detail && (
        <p className="mt-1 text-xs text-muted-foreground">{detail}</p>
      )}
    </div>
  );
}

function MetricaCard({
  icon,
  title,
  value,
  description,
}: {
  icon: ReactNode;
  title: string;
  value: string;
  description: string;
}) {
  return (
    <div className="flex items-start gap-4 rounded-xl border border-border bg-background p-5 shadow-sm">
      <div className="rounded-lg bg-primary/10 p-2.5 text-primary">{icon}</div>
      <div>
        <p className="text-sm font-medium text-muted-foreground">{title}</p>
        <p className="mt-0.5 text-xl font-bold text-foreground">{value}</p>
        <p className="mt-0.5 text-xs text-muted-foreground">{description}</p>
      </div>
    </div>
  );
}

function ScorePill({ score }: { score: number }) {
  const color =
    score > 75
      ? "bg-risk-critical/15 text-risk-critical"
      : score > 50
        ? "bg-risk-high/15 text-risk-high"
        : score > 25
          ? "bg-risk-medium/15 text-risk-medium"
          : "bg-risk-low/15 text-risk-low";

  return (
    <span
      className={`inline-block rounded-full px-2.5 py-0.5 font-mono text-xs font-semibold ${color}`}
    >
      {score.toFixed(1)}
    </span>
  );
}
