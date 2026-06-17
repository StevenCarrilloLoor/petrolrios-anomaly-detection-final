import { useEffect, useState, useRef } from "react";
import { useSearchParams, Link } from "react-router-dom";
import { authService } from "@/services/auth.service";
import { Shield, CheckCircle2, XCircle, Loader2 } from "lucide-react";

export function VerificarCorreoPage() {
  const [params] = useSearchParams();
  const token = params.get("token") ?? "";
  const [estado, setEstado] = useState<"cargando" | "ok" | "error">("cargando");
  const [mensaje, setMensaje] = useState("Verificando su correo…");
  const yaCorrio = useRef(false);

  useEffect(() => {
    if (yaCorrio.current) return;
    yaCorrio.current = true;

    if (!token) {
      setEstado("error");
      setMensaje("Falta el token en el enlace.");
      return;
    }
    authService
      .verificarEmail(token)
      .then((r) => {
        setEstado(r.ok ? "ok" : "error");
        setMensaje(r.mensaje);
      })
      .catch(() => {
        setEstado("error");
        setMensaje("El enlace es inválido o expiró.");
      });
  }, [token]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-muted p-6">
      <div className="w-full max-w-md rounded-2xl border border-border bg-background p-8 text-center shadow-lg">
        <div className="mb-4 flex justify-center">
          <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-primary/10">
            <Shield className="text-primary" size={28} />
          </div>
        </div>

        {estado === "cargando" && (
          <>
            <Loader2 className="mx-auto animate-spin text-primary" size={36} />
            <p className="mt-4 text-sm text-muted-foreground">{mensaje}</p>
          </>
        )}

        {estado === "ok" && (
          <>
            <CheckCircle2 className="mx-auto text-green-500" size={44} />
            <h1 className="mt-3 text-xl font-bold">¡Correo verificado!</h1>
            <p className="mt-1 text-sm text-muted-foreground">{mensaje}</p>
            <Link
              to="/login"
              className="mt-6 inline-block rounded-md bg-primary px-5 py-2.5 text-sm font-semibold text-primary-foreground hover:bg-primary/90"
            >
              Ir a iniciar sesión
            </Link>
          </>
        )}

        {estado === "error" && (
          <>
            <XCircle className="mx-auto text-destructive" size={44} />
            <h1 className="mt-3 text-xl font-bold">No se pudo verificar</h1>
            <p className="mt-1 text-sm text-muted-foreground">{mensaje}</p>
            <Link
              to="/login"
              className="mt-6 inline-block rounded-md border border-border px-5 py-2.5 text-sm font-semibold hover:bg-muted"
            >
              Volver al inicio
            </Link>
          </>
        )}
      </div>
    </div>
  );
}
