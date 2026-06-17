import { useState } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import { authService } from "@/services/auth.service";
import { useAuth } from "@/contexts/AuthContext";
import { Card, CardContent } from "@/components/ui/Card";
import { QrCode, CheckCircle2, XCircle } from "lucide-react";

export function AprobarQrPage() {
  const [params] = useSearchParams();
  const codigo = params.get("codigo") ?? "";
  const { user } = useAuth();
  const navigate = useNavigate();
  const [estado, setEstado] = useState<"pendiente" | "ok" | "error">("pendiente");
  const [cargando, setCargando] = useState(false);

  async function aprobar() {
    setCargando(true);
    try {
      await authService.qrAprobar(codigo);
      setEstado("ok");
    } catch {
      setEstado("error");
    } finally {
      setCargando(false);
    }
  }

  return (
    <div className="mx-auto mt-10 max-w-md">
      <Card>
        <CardContent className="space-y-4 p-8 text-center">
          {estado === "pendiente" && (
            <>
              <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-primary/10">
                <QrCode className="text-primary" size={28} />
              </div>
              <h1 className="text-xl font-bold">Aprobar inicio de sesión</h1>
              <p className="text-sm text-muted-foreground">
                Otro dispositivo está intentando iniciar sesión con código QR.
                Si fue usted, apruebe el acceso con su cuenta{" "}
                <b>{user?.email}</b>.
              </p>
              {!codigo && (
                <p className="text-sm text-destructive">
                  Falta el código en el enlace. Escanee el QR de nuevo.
                </p>
              )}
              <button
                onClick={aprobar}
                disabled={cargando || !codigo}
                className="w-full rounded-md bg-primary px-4 py-2.5 text-sm font-semibold text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
              >
                {cargando ? "Aprobando…" : "Sí, aprobar este acceso"}
              </button>
              <button
                onClick={() => navigate("/dashboard")}
                className="text-sm text-muted-foreground hover:underline"
              >
                No fui yo / cancelar
              </button>
            </>
          )}

          {estado === "ok" && (
            <>
              <CheckCircle2 className="mx-auto text-green-500" size={40} />
              <h1 className="text-xl font-bold">Acceso aprobado</h1>
              <p className="text-sm text-muted-foreground">
                El otro dispositivo iniciará sesión automáticamente. Ya puede
                cerrar esta página.
              </p>
              <button
                onClick={() => navigate("/dashboard")}
                className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary/90"
              >
                Volver al panel
              </button>
            </>
          )}

          {estado === "error" && (
            <>
              <XCircle className="mx-auto text-destructive" size={40} />
              <h1 className="text-xl font-bold">No se pudo aprobar</h1>
              <p className="text-sm text-muted-foreground">
                El código es inválido o expiró. Genere uno nuevo en la pantalla
                de inicio de sesión.
              </p>
              <button
                onClick={() => navigate("/dashboard")}
                className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary/90"
              >
                Volver al panel
              </button>
            </>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
