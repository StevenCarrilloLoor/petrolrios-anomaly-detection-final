import { useState } from "react";
import { useSearchParams, Link, useNavigate } from "react-router-dom";
import { authService } from "@/services/auth.service";
import { Shield, CheckCircle2 } from "lucide-react";

export function RestablecerPasswordPage() {
  const [params] = useSearchParams();
  const token = params.get("token") ?? "";
  const navigate = useNavigate();
  const [pass, setPass] = useState("");
  const [confirma, setConfirma] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState(false);
  const [loading, setLoading] = useState(false);

  async function guardar() {
    setError(null);
    if (pass.length < 8) {
      setError("La contraseña debe tener al menos 8 caracteres.");
      return;
    }
    if (pass !== confirma) {
      setError("Las contraseñas no coinciden.");
      return;
    }
    setLoading(true);
    try {
      const r = await authService.restablecerPassword(token, pass);
      if (r.ok) {
        setOk(true);
        setTimeout(() => navigate("/login"), 2500);
      } else {
        setError(r.mensaje);
      }
    } catch {
      setError("El enlace es inválido o expiró. Solicita uno nuevo.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-muted p-6">
      <div className="w-full max-w-md rounded-2xl border border-border bg-background p-8 shadow-lg">
        <div className="mb-5 flex flex-col items-center">
          <div className="mb-3 flex h-14 w-14 items-center justify-center rounded-2xl bg-primary/10">
            <Shield className="text-primary" size={28} />
          </div>
          <h1 className="text-xl font-bold">Restablecer contraseña</h1>
        </div>

        {ok ? (
          <div className="text-center">
            <CheckCircle2 className="mx-auto text-green-500" size={44} />
            <p className="mt-3 text-sm text-muted-foreground">
              Contraseña actualizada. Te llevamos al inicio de sesión…
            </p>
          </div>
        ) : !token ? (
          <p className="text-center text-sm text-destructive">
            Falta el token en el enlace. Solicita un nuevo correo de recuperación.
          </p>
        ) : (
          <div className="space-y-3">
            {error && (
              <div className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">
                {error}
              </div>
            )}
            <input
              type="password"
              value={pass}
              onChange={(e) => setPass(e.target.value)}
              placeholder="Nueva contraseña (mín. 8 caracteres)"
              className="w-full rounded-md border border-border bg-background px-3 py-2.5 text-sm"
            />
            <input
              type="password"
              value={confirma}
              onChange={(e) => setConfirma(e.target.value)}
              placeholder="Confirmar nueva contraseña"
              className="w-full rounded-md border border-border bg-background px-3 py-2.5 text-sm"
            />
            <button
              onClick={guardar}
              disabled={loading || !pass}
              className="w-full rounded-md bg-primary px-4 py-2.5 text-sm font-semibold text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
            >
              {loading ? "Guardando…" : "Guardar nueva contraseña"}
            </button>
            <Link to="/login" className="block text-center text-sm text-muted-foreground hover:underline">
              Volver al inicio de sesión
            </Link>
          </div>
        )}
      </div>
    </div>
  );
}
