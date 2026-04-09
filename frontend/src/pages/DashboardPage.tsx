export function DashboardPage() {
  return (
    <div className="min-h-screen bg-background">
      <header className="border-b border-border px-6 py-4">
        <div className="flex items-center justify-between">
          <h1 className="text-xl font-bold text-foreground">
            PetrolRios - Dashboard
          </h1>
          <span className="text-sm text-muted-foreground">
            Sistema de Deteccion de Anomalias
          </span>
        </div>
      </header>
      <main className="p-6">
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
          <KpiCard title="Alertas Criticas" value="0" color="text-risk-critical" />
          <KpiCard title="Alertas Altas" value="0" color="text-risk-high" />
          <KpiCard title="Alertas Medias" value="0" color="text-risk-medium" />
          <KpiCard title="Alertas Bajas" value="0" color="text-risk-low" />
        </div>
        <p className="mt-8 text-center text-muted-foreground">
          El dashboard se conectara al backend en bloques posteriores.
        </p>
      </main>
    </div>
  );
}

function KpiCard({ title, value, color }: { title: string; value: string; color: string }) {
  return (
    <div className="rounded-lg border border-border bg-background p-6 shadow-sm">
      <p className="text-sm text-muted-foreground">{title}</p>
      <p className={`mt-2 text-3xl font-bold ${color}`}>{value}</p>
    </div>
  );
}
