import { useState } from "react";
import type { FormEvent } from "react";
import { Navigate, useNavigate } from "react-router-dom";
import { useAuth } from "@/contexts/AuthContext";
import { Shield, Activity, Bell, Search } from "lucide-react";
import { Spinner } from "@/components/ui/Spinner";

export function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await login({ email, password });
      navigate("/dashboard", { replace: true });
    } catch {
      setError("Credenciales inválidas. Intente de nuevo.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex min-h-screen">
      {/* Panel institucional */}
      <div className="hidden flex-col justify-between bg-slate-900 p-12 text-white lg:flex lg:w-1/2">
        <div className="flex items-center gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-blue-500/20">
            <Shield size={24} className="text-blue-400" />
          </div>
          <div>
            <p className="text-lg font-bold leading-tight">PetrolRíos S.A.</p>
            <p className="text-xs uppercase tracking-wider text-slate-400">
              Sistema de Detección de Anomalías
            </p>
          </div>
        </div>

        <div className="space-y-8">
          <h1 className="max-w-md text-3xl font-bold leading-snug">
            Auditoría transaccional continua sobre 10 estaciones de servicio
          </h1>
          <div className="space-y-5">
            <FeatureRow
              icon={<Activity size={18} />}
              title="Cobertura del 100% de transacciones"
              description="13,000–15,000 transacciones diarias analizadas por 4 detectores cada 5–10 minutos."
            />
            <FeatureRow
              icon={<Bell size={18} />}
              title="Alertas en tiempo real"
              description="Notificaciones push vía SignalR, sin refrescar la página."
            />
            <FeatureRow
              icon={<Search size={18} />}
              title="Detección en minutos, no semanas"
              description="Reducción del tiempo de detección de días/semanas a menos de 10 minutos."
            />
          </div>
        </div>

        <p className="text-xs text-slate-500">
          PetrolRíos S.A. · Plataforma de Auditoría Transaccional · {new Date().getFullYear()}
        </p>
      </div>

      {/* Formulario */}
      <div className="flex flex-1 items-center justify-center bg-muted p-6">
        <div className="w-full max-w-md rounded-2xl border border-border bg-background p-8 shadow-lg">
          <div className="mb-6 flex flex-col items-center">
            <div className="mb-3 flex h-14 w-14 items-center justify-center rounded-2xl bg-primary/10">
              <Shield className="text-primary" size={28} />
            </div>
            <h2 className="text-2xl font-bold text-foreground">Bienvenido</h2>
            <p className="mt-1 text-sm text-muted-foreground">
              Ingrese sus credenciales para continuar
            </p>
          </div>
          {error && (
            <div className="mb-4 rounded-md bg-destructive/10 p-3 text-sm text-destructive">
              {error}
            </div>
          )}
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="mb-1 block text-sm font-medium" htmlFor="email">
                Correo electrónico
              </label>
              <input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full rounded-md border border-border bg-background px-3 py-2.5 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/30"
                placeholder="usuario@petrolrios.com"
                required
                disabled={loading}
              />
            </div>
            <div>
              <label
                className="mb-1 block text-sm font-medium"
                htmlFor="password"
              >
                Contraseña
              </label>
              <input
                id="password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full rounded-md border border-border bg-background px-3 py-2.5 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/30"
                required
                disabled={loading}
              />
            </div>
            <button
              type="submit"
              disabled={loading}
              className="flex w-full items-center justify-center gap-2 rounded-md bg-primary px-4 py-2.5 text-sm font-semibold text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-50"
            >
              {loading && <Spinner size="sm" />}
              Iniciar Sesión
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}

function FeatureRow({
  icon,
  title,
  description,
}: {
  icon: React.ReactNode;
  title: string;
  description: string;
}) {
  return (
    <div className="flex gap-4">
      <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-blue-500/15 text-blue-400">
        {icon}
      </div>
      <div>
        <p className="text-sm font-semibold">{title}</p>
        <p className="mt-0.5 max-w-sm text-xs leading-relaxed text-slate-400">
          {description}
        </p>
      </div>
    </div>
  );
}
