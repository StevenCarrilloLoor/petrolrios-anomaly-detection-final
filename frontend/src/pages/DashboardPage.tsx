import { useState } from "react";
import type { ReactNode } from "react";
import { useQuery, keepPreviousData } from "@tanstack/react-query";
import { Link, useNavigate } from "react-router-dom";
import { dashboardService } from "@/services/dashboard.service";
import { alertasService } from "@/services/alertas.service";
import { useRefrescoMs } from "@/contexts/RefrescoContext";
import { Card, CardContent, CardHeader } from "@/components/ui/Card";
import { Skeleton } from "@/components/ui/Skeleton";
import { EmptyState } from "@/components/ui/EmptyState";
import { PreciosCombustibleCard } from "@/components/PreciosCombustibleCard";
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
  ArrowUpRight,
  ArrowDownRight,
  ChevronRight,
  Bell,
} from "lucide-react";
import { TIPO_DETECTOR_LABELS, NIVEL_RIESGO_LABELS, ESTADO_ALERTA_LABELS } from "@/types/alert";
import type { TipoDetector, NivelRiesgo, EstadoAlerta } from "@/types/alert";

const CHART_COLORS = ["#3b82f6", "#8b5cf6", "#f97316", "#06b6d4", "#ec4899"];

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
function labelEstado(estado: string): string {
  return ESTADO_ALERTA_LABELS[estado as EstadoAlerta] ?? estado;
}

function timeAgo(iso: string): string {
  const d = new Date(iso);
  const s = (Date.now() - d.getTime()) / 1000;
  if (Number.isNaN(s)) return "";
  if (s < 60) return "hace instantes";
  if (s < 3600) return `hace ${Math.floor(s / 60)} min`;
  if (s < 86400) return `hace ${Math.floor(s / 3600)} h`;
  return `hace ${Math.floor(s / 86400)} d`;
}

const PERIODOS = [
  { d: 7, l: "7 días" },
  { d: 14, l: "14 días" },
  { d: 30, l: "30 días" },
  { d: 90, l: "90 días" },
];

export function DashboardPage() {
  // Filtro por estación ("no mezclar estaciones", lo pidió auditoría). "" = todas.
  const [estacionFiltro, setEstacionFiltro] = useState("");
  const estId = estacionFiltro ? Number(estacionFiltro) : undefined;
  const [dias, setDias] = useState(14);
  const refrescoMs = useRefrescoMs();
  const navigate = useNavigate();

  // Sufijo de estación para los drill-downs (la URL de alertas lee ?estacionId).
  const suf = estId ? `&estacionId=${estId}` : "";
  const irAlertas = (qs: string) => navigate(`/alertas?${qs}${suf}`.replace(/^\/alertas\?&/, "/alertas?"));

  const { data: kpis, isLoading: loadingKpis } = useQuery({
    queryKey: ["dashboard", "kpis", estId],
    queryFn: () => dashboardService.getKpis(estId),
    refetchInterval: refrescoMs,
    placeholderData: keepPreviousData,
  });

  const { data: metricas } = useQuery({
    queryKey: ["dashboard", "metricas-resolucion", estId],
    queryFn: () => dashboardService.getMetricasResolucion(estId),
    refetchInterval: refrescoMs,
  });

  const { data: tendencia, isLoading: loadingTendencia } = useQuery({
    queryKey: ["dashboard", "tendencia", dias, estId],
    queryFn: () => dashboardService.getTendencia(dias, estId),
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

  // Feed "Atención inmediata": las alertas más recientes (triage: qué revisar primero).
  const { data: feed } = useQuery({
    queryKey: ["dashboard", "feed", estId],
    queryFn: () => alertasService.getAll({ page: 1, pageSize: 7, estacionId: estId }),
    refetchInterval: refrescoMs,
    placeholderData: keepPreviousData,
  });
  const recientes = (feed?.items ?? [])
    .slice()
    .sort((a, b) => new Date(b.fechaDeteccion).getTime() - new Date(a.fechaDeteccion).getTime())
    .slice(0, 7);

  const estacionNombre = (porEstacion ?? []).find(
    (e) => String(e.estacionId) === estacionFiltro,
  )?.estacionNombre;

  // Delta de la tendencia: mitad reciente vs mitad anterior del período (contexto "¿subiendo o bajando?").
  const serie = tendencia ?? [];
  const mitad = Math.floor(serie.length / 2);
  const prevTotal = serie.slice(0, mitad).reduce((s, d) => s + d.total, 0);
  const currTotal = serie.slice(mitad).reduce((s, d) => s + d.total, 0);
  const delta = prevTotal > 0 ? Math.round(((currTotal - prevTotal) / prevTotal) * 100) : currTotal > 0 ? 100 : 0;

  if (loadingKpis) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-56" />
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <Skeleton key={i} className="h-28" />
          ))}
        </div>
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
          <Skeleton className="h-80 lg:col-span-2" />
          <Skeleton className="h-80" />
        </div>
      </div>
    );
  }

  const tendenciaFormateada = serie.map((t) => ({
    ...t,
    dia: new Date(t.fecha).toLocaleDateString("es-EC", { day: "2-digit", month: "short" }),
  }));

  const nivelData = (porNivel ?? []).map((n) => ({
    nombre: labelNivel(n.nivelRiesgo),
    nivel: n.nivelRiesgo,
    cantidad: n.cantidad,
    color: NIVEL_COLORS[n.nivelRiesgo] ?? "#94a3b8",
  }));

  const tipoData = (porTipo ?? []).map((t) => ({ ...t, nombre: labelTipo(t.tipoDetector) }));

  const tasaValidas = metricas?.tasaAlertasValidas ?? 0;

  return (
    <div className="space-y-6 print:bg-white print:text-black">
      {/* Cabecera */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Centro de Monitoreo</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {estacionNombre
              ? `Anomalías de ${estacionNombre} — vista por estación`
              : "Detección de anomalías transaccionales en tiempo real — 10 estaciones"}
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-2 print:hidden">
          <select
            value={estacionFiltro}
            onChange={(e) => setEstacionFiltro(e.target.value)}
            aria-label="Filtrar el dashboard por estación"
            className="rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
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
            className="flex items-center gap-1.5 rounded-md border border-border bg-background px-3 py-2 text-sm text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
          >
            <Printer size={15} /> Imprimir / PDF
          </button>
          <div className="flex items-center gap-2 rounded-full border border-border bg-muted px-3 py-1.5">
            <span className="relative flex h-2.5 w-2.5">
              <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-risk-low opacity-60" />
              <span className="relative inline-flex h-2.5 w-2.5 rounded-full bg-risk-low" />
            </span>
            <span className="text-xs font-medium text-muted-foreground">En vivo</span>
          </div>
        </div>
      </div>

      {/* KPIs principales (clic → bandeja de alertas filtrada) */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <KpiCard
          icon={<ShieldAlert size={20} />}
          title="Total Alertas"
          value={kpis?.totalAlertas ?? 0}
          detail={`${metricas?.alertasUltimas24Horas ?? 0} en las últimas 24 h`}
          to={`/alertas?${suf.replace(/^&/, "")}`}
        />
        <KpiCard
          icon={<AlertCircle size={20} />}
          title="Alertas Nuevas"
          value={kpis?.alertasNuevas ?? 0}
          color="text-blue-500"
          detail="Pendientes de revisión"
          to={`/alertas?estado=Nueva${suf}`}
        />
        <KpiCard
          icon={<AlertTriangle size={20} />}
          title="Alertas Críticas"
          value={kpis?.alertasCriticas ?? 0}
          color="text-risk-critical"
          accent="border-l-4 border-l-risk-critical"
          detail="Score 76–100 · requieren acción"
          to={`/alertas?nivel=Critico${suf}`}
        />
        <KpiCard
          icon={<Clock size={20} />}
          title="En Revisión"
          value={kpis?.alertasEnRevision ?? 0}
          color="text-risk-medium"
          detail="Asignadas a auditores"
          to={`/alertas?estado=EnRevision${suf}`}
        />
        <KpiCard
          icon={<CheckCircle size={20} />}
          title="Confirmadas"
          value={kpis?.alertasConfirmadas ?? 0}
          color="text-risk-high"
          detail="Irregularidades reales"
          to={`/alertas?estado=Confirmada${suf}`}
        />
        <KpiCard
          icon={<XCircle size={20} />}
          title="Falsos Positivos"
          value={kpis?.alertasFalsoPositivo ?? 0}
          color="text-muted-foreground"
          detail={`Tasa: ${metricas?.tasaFalsosPositivos ?? 0}%`}
          to={`/alertas?estado=FalsoPositivo${suf}`}
        />
        <KpiCard
          icon={<TrendingUp size={20} />}
          title="Score Promedio"
          value={kpis?.scorePromedio?.toFixed(1) ?? "0"}
          detail="Escala de riesgo 0–100"
        />
        <KpiCard
          icon={<Building2 size={20} />}
          title="Estaciones en Línea"
          value={`${kpis?.estacionesConectadas ?? 0} de ${kpis?.estacionesTotales ?? 0}`}
          color={(kpis?.estacionesConectadas ?? 0) > 0 ? "text-risk-low" : "text-risk-critical"}
          detail="Agentes con señal de vida"
          to="/conexiones"
        />
      </div>

      {/* Métricas de efectividad (con semántica de color: verde = en meta, rojo = fuera) */}
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
          value={`${tasaValidas}%`}
          description="Confirmadas sobre el total resuelto (meta institucional: > 90%)"
          tone={tasaValidas >= 90 ? "ok" : tasaValidas >= 70 ? "warn" : "bad"}
        />
        <MetricaCard
          icon={<Activity size={18} />}
          title="Pendientes vs resueltas"
          value={`${metricas?.totalPendientes ?? 0} / ${metricas?.totalResueltas ?? 0}`}
          description="Carga de trabajo actual del equipo de auditoría"
        />
      </div>

      {/* Tendencia + Feed de atención inmediata */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <CardHeader
            title="Tendencia de alertas"
            subtitle="Evolución diaria del total, con detalle de críticas"
            action={
              <div className="flex items-center gap-2 print:hidden">
                {delta !== 0 && (
                  <span
                    className={`inline-flex items-center gap-0.5 rounded-full px-2 py-0.5 text-xs font-semibold ${
                      delta > 0 ? "bg-risk-critical/15 text-risk-critical" : "bg-risk-low/15 text-risk-low"
                    }`}
                    title="Variación de la segunda mitad del período vs la primera"
                  >
                    {delta > 0 ? <ArrowUpRight size={13} /> : <ArrowDownRight size={13} />}
                    {Math.abs(delta)}%
                  </span>
                )}
                <select
                  value={dias}
                  onChange={(e) => setDias(Number(e.target.value))}
                  aria-label="Período de la tendencia"
                  className="rounded-md border border-border bg-background px-2 py-1 text-xs focus:outline-none focus:ring-2 focus:ring-primary/40"
                >
                  {PERIODOS.map((p) => (
                    <option key={p.d} value={p.d}>
                      {p.l}
                    </option>
                  ))}
                </select>
              </div>
            }
          />
          <CardContent>
            {loadingTendencia ? (
              <Skeleton className="h-64" />
            ) : tendenciaFormateada.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
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
                  <XAxis dataKey="dia" tick={{ fontSize: 11 }} minTickGap={20} />
                  <YAxis allowDecimals={false} tick={{ fontSize: 11 }} />
                  <Tooltip />
                  <Legend />
                  <Area type="monotone" dataKey="total" name="Total" stroke="#3b82f6" fill="url(#gradTotal)" strokeWidth={2} />
                  <Area type="monotone" dataKey="criticas" name="Críticas" stroke="#ef4444" fill="url(#gradCriticas)" strokeWidth={2} />
                </AreaChart>
              </ResponsiveContainer>
            ) : (
              <EmptyState description="Aún no hay alertas registradas en este período." />
            )}
          </CardContent>
        </Card>

        {/* Feed: Atención inmediata (triage) */}
        <Card>
          <CardHeader
            title="Atención inmediata"
            subtitle="Lo más reciente — qué revisar primero"
            action={<Bell size={16} className="text-muted-foreground" />}
          />
          <CardContent className="p-0">
            {recientes.length > 0 ? (
              <ul className="divide-y divide-border">
                {recientes.map((a) => (
                  <li key={a.id}>
                    <Link
                      to={`/alertas/${a.id}`}
                      className="flex items-start gap-2 px-4 py-2.5 transition-colors hover:bg-muted/50"
                    >
                      <span
                        className="mt-1 h-2.5 w-2.5 shrink-0 rounded-full"
                        style={{ backgroundColor: NIVEL_COLORS[a.nivelRiesgo] ?? "#94a3b8" }}
                        title={labelNivel(a.nivelRiesgo)}
                      />
                      <div className="min-w-0 flex-1">
                        <p className="truncate text-sm font-medium text-foreground">{a.descripcion}</p>
                        <p className="mt-0.5 truncate text-xs text-muted-foreground">
                          {a.estacionNombre}
                          {a.empleadoNombre ? ` · ${a.empleadoNombre}` : a.empleadoCodigo ? ` · ${a.empleadoCodigo}` : ""} ·{" "}
                          {timeAgo(a.fechaDeteccion)}
                        </p>
                      </div>
                      <span className="shrink-0 text-right">
                        <span className="block font-mono text-sm font-bold" style={{ color: NIVEL_COLORS[a.nivelRiesgo] }}>
                          {Math.round(a.score)}
                        </span>
                        <span className="text-[10px] text-muted-foreground">{labelEstado(a.estado)}</span>
                      </span>
                    </Link>
                  </li>
                ))}
              </ul>
            ) : (
              <div className="p-6">
                <EmptyState description="Sin alertas recientes." />
              </div>
            )}
            <Link
              to={`/alertas?${suf.replace(/^&/, "")}`}
              className="flex items-center justify-center gap-1 border-t border-border px-4 py-2.5 text-sm font-medium text-primary hover:bg-muted/50 print:hidden"
            >
              Ver todas las alertas <ChevronRight size={15} />
            </Link>
          </CardContent>
        </Card>
      </div>

      {/* Precios oficiales de combustible de Ecuador (Extra/Ecopaís/Diésel) */}
      <PreciosCombustibleCard />

      {/* Distribución por tipo / nivel (clic en una barra/segmento → bandeja filtrada) */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader title="Alertas por tipo de detector" subtitle="Clic en un segmento para ver esas alertas" />
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
                    onClick={(d) => {
                      const x = d as { tipoDetector?: string; payload?: { tipoDetector?: string } };
                      const v = x.tipoDetector ?? x.payload?.tipoDetector;
                      if (v) irAlertas(`tipo=${v}`);
                    }}
                    className="cursor-pointer"
                  >
                    {tipoData.map((_, index) => (
                      <Cell key={`cell-tipo-${index}`} fill={CHART_COLORS[index % CHART_COLORS.length]} />
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
          <CardHeader title="Alertas por nivel de riesgo" subtitle="Clic en una barra para ver esas alertas" />
          <CardContent>
            {nivelData.length > 0 ? (
              <ResponsiveContainer width="100%" height={280}>
                <BarChart data={nivelData} layout="vertical">
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" />
                  <XAxis type="number" allowDecimals={false} tick={{ fontSize: 11 }} />
                  <YAxis type="category" dataKey="nombre" width={70} tick={{ fontSize: 12 }} />
                  <Tooltip />
                  <Bar
                    dataKey="cantidad"
                    name="Alertas"
                    radius={[0, 6, 6, 0]}
                    className="cursor-pointer"
                    onClick={(d) => {
                      const x = d as { nivel?: string; payload?: { nivel?: string } };
                      const v = x.nivel ?? x.payload?.nivel;
                      if (v) irAlertas(`nivel=${v}`);
                    }}
                  >
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
          <CardHeader title="Alertas por estación" subtitle="Comparativo entre estaciones · clic en una barra" />
          <CardContent>
            {loadingEstacion ? (
              <Skeleton className="h-64" />
            ) : porEstacion && porEstacion.length > 0 ? (
              <ResponsiveContainer width="100%" height={280}>
                <BarChart data={porEstacion}>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" />
                  <XAxis dataKey="estacionNombre" tick={{ fontSize: 10 }} angle={-25} textAnchor="end" height={70} />
                  <YAxis allowDecimals={false} tick={{ fontSize: 11 }} />
                  <Tooltip />
                  <Bar
                    dataKey="cantidad"
                    name="Alertas"
                    fill="#3b82f6"
                    radius={[6, 6, 0, 0]}
                    className="cursor-pointer"
                    onClick={(d) => {
                      const x = d as { estacionId?: number; payload?: { estacionId?: number } };
                      const v = x.estacionId ?? x.payload?.estacionId;
                      if (v) navigate(`/alertas?estacionId=${v}`);
                    }}
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
            subtitle="Ranking de riesgo · clic para ver sus alertas"
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
                      onClick={() => navigate(`/alertas?buscar=${encodeURIComponent(emp.empleadoCodigo)}`)}
                      className="cursor-pointer border-b border-border last:border-0 hover:bg-muted/50"
                    >
                      <td className="px-6 py-2.5">
                        {emp.empleadoNombre ? (
                          <div className="flex flex-col leading-tight">
                            <span className="font-medium">{emp.empleadoNombre}</span>
                            <span className="font-mono text-xs text-muted-foreground">{emp.empleadoCodigo}</span>
                          </div>
                        ) : (
                          <span className="font-mono font-medium">{emp.empleadoCodigo}</span>
                        )}
                      </td>
                      <td className="px-4 py-2.5 text-muted-foreground">{emp.estacionNombre}</td>
                      <td className="px-4 py-2.5 text-right font-semibold">{emp.cantidadAlertas}</td>
                      <td className="px-4 py-2.5 text-right">
                        {emp.criticas > 0 ? (
                          <span className="font-semibold text-risk-critical">{emp.criticas}</span>
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
  to,
}: {
  icon: ReactNode;
  title: string;
  value: string | number;
  color?: string;
  accent?: string;
  detail?: string;
  to?: string;
}) {
  const inner = (
    <>
      <div className="flex items-center justify-between">
        <p className="text-sm font-medium text-muted-foreground">{title}</p>
        <div className="text-muted-foreground/70">{icon}</div>
      </div>
      <p className={`mt-2 text-3xl font-bold tracking-tight ${color}`}>{value}</p>
      {detail && <p className="mt-1 text-xs text-muted-foreground">{detail}</p>}
    </>
  );
  const cls = `block rounded-xl border border-border bg-background p-5 shadow-sm transition-all ${accent}`;
  return to ? (
    <Link to={to} className={`${cls} hover:-translate-y-0.5 hover:shadow-md`}>
      {inner}
    </Link>
  ) : (
    <div className={cls}>{inner}</div>
  );
}

function MetricaCard({
  icon,
  title,
  value,
  description,
  tone,
}: {
  icon: ReactNode;
  title: string;
  value: string;
  description: string;
  tone?: "ok" | "warn" | "bad";
}) {
  const toneCls =
    tone === "ok"
      ? "bg-risk-low/10 text-risk-low"
      : tone === "warn"
        ? "bg-risk-medium/10 text-risk-medium"
        : tone === "bad"
          ? "bg-risk-critical/10 text-risk-critical"
          : "bg-primary/10 text-primary";
  return (
    <div className="flex items-start gap-4 rounded-xl border border-border bg-background p-5 shadow-sm">
      <div className={`rounded-lg p-2.5 ${toneCls}`}>{icon}</div>
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
    <span className={`inline-block rounded-full px-2.5 py-0.5 font-mono text-xs font-semibold ${color}`}>
      {score.toFixed(1)}
    </span>
  );
}
