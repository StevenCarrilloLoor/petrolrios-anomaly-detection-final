import { useEffect, useState } from "react";
import QRCode from "qrcode";
import { authService } from "@/services/auth.service";
import { Card, CardContent, CardHeader } from "@/components/ui/Card";

export function SeguridadPage() {
  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Seguridad de mi cuenta</h1>
        <p className="text-sm text-muted-foreground">
          Active la verificación en dos pasos y cambie su contraseña.
        </p>
      </div>
      <DosFactores />
      <CambiarPassword />
    </div>
  );
}

function DosFactores() {
  const [habilitado, setHabilitado] = useState<boolean | null>(null);
  const [qr, setQr] = useState<string | null>(null);
  const [secreto, setSecreto] = useState("");
  const [codigo, setCodigo] = useState("");
  const [msg, setMsg] = useState<{ ok: boolean; texto: string } | null>(null);
  const [cargando, setCargando] = useState(false);

  const refrescar = () =>
    authService.estado2fa().then((e) => setHabilitado(e.habilitado));

  useEffect(() => {
    void refrescar();
  }, []);

  async function iniciar() {
    setMsg(null);
    setCargando(true);
    try {
      const r = await authService.iniciar2fa();
      setSecreto(r.secreto);
      setQr(await QRCode.toDataURL(r.uriOtpauth, { width: 220, margin: 1 }));
    } catch {
      setMsg({ ok: false, texto: "No se pudo iniciar la configuración." });
    } finally {
      setCargando(false);
    }
  }

  async function confirmar() {
    setMsg(null);
    setCargando(true);
    try {
      await authService.confirmar2fa(codigo);
      setMsg({ ok: true, texto: "2FA activado correctamente." });
      setQr(null);
      setCodigo("");
      await refrescar();
    } catch {
      setMsg({ ok: false, texto: "Código inválido. Revise la hora del dispositivo." });
    } finally {
      setCargando(false);
    }
  }

  async function desactivar() {
    setMsg(null);
    setCargando(true);
    try {
      await authService.desactivar2fa(codigo);
      setMsg({ ok: true, texto: "2FA desactivado." });
      setCodigo("");
      await refrescar();
    } catch {
      setMsg({ ok: false, texto: "Código inválido; no se desactivó." });
    } finally {
      setCargando(false);
    }
  }

  return (
    <Card>
      <CardHeader
        title="Verificación en dos pasos (2FA)"
        action={
          habilitado != null ? (
            <span
              className={`rounded-full px-2.5 py-0.5 text-xs ${
                habilitado
                  ? "bg-green-500/15 text-green-500"
                  : "bg-muted text-muted-foreground"
              }`}
            >
              {habilitado ? "Activo" : "Inactivo"}
            </span>
          ) : undefined
        }
      />
      <CardContent className="space-y-4">
        <p className="text-sm text-muted-foreground">
          Use una app autenticadora (Google Authenticator, Authy, Microsoft
          Authenticator) para generar un código de 6 dígitos al iniciar sesión.
        </p>

        {msg && (
          <div
            className={`rounded-md p-3 text-sm ${
              msg.ok
                ? "bg-green-500/10 text-green-600"
                : "bg-destructive/10 text-destructive"
            }`}
          >
            {msg.texto}
          </div>
        )}

        {!habilitado && !qr && (
          <button
            onClick={iniciar}
            disabled={cargando}
            className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
          >
            Activar 2FA
          </button>
        )}

        {qr && (
          <div className="space-y-3 rounded-lg border border-border p-4">
            <p className="text-sm font-medium">
              1. Escanee este código QR con su app autenticadora:
            </p>
            <img src={qr} alt="Código QR para 2FA" className="rounded bg-white p-2" />
            <p className="text-xs text-muted-foreground">
              ¿No puede escanear? Ingrese esta clave manualmente:
              <code className="ml-1 break-all rounded bg-muted px-1.5 py-0.5 font-mono">
                {secreto}
              </code>
            </p>
            <p className="text-sm font-medium">
              2. Ingrese el código de 6 dígitos que muestra la app:
            </p>
            <div className="flex gap-2">
              <input
                value={codigo}
                onChange={(e) => setCodigo(e.target.value.replace(/\D/g, ""))}
                maxLength={6}
                inputMode="numeric"
                placeholder="000000"
                className="w-32 rounded-md border border-border bg-background px-3 py-2 text-center tracking-[0.3em]"
              />
              <button
                onClick={confirmar}
                disabled={cargando || codigo.length !== 6}
                className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
              >
                Confirmar y activar
              </button>
            </div>
          </div>
        )}

        {habilitado && (
          <div className="space-y-2 rounded-lg border border-border p-4">
            <p className="text-sm">
              Para desactivar el 2FA, ingrese un código actual de su app:
            </p>
            <div className="flex gap-2">
              <input
                value={codigo}
                onChange={(e) => setCodigo(e.target.value.replace(/\D/g, ""))}
                maxLength={6}
                inputMode="numeric"
                placeholder="000000"
                className="w-32 rounded-md border border-border bg-background px-3 py-2 text-center tracking-[0.3em]"
              />
              <button
                onClick={desactivar}
                disabled={cargando || codigo.length !== 6}
                className="rounded-md border border-destructive px-4 py-2 text-sm font-semibold text-destructive hover:bg-destructive/10 disabled:opacity-50"
              >
                Desactivar 2FA
              </button>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function CambiarPassword() {
  const [actual, setActual] = useState("");
  const [nueva, setNueva] = useState("");
  const [confirma, setConfirma] = useState("");
  const [msg, setMsg] = useState<{ ok: boolean; texto: string } | null>(null);
  const [cargando, setCargando] = useState(false);

  async function guardar() {
    setMsg(null);
    if (nueva.length < 8) {
      setMsg({ ok: false, texto: "La nueva contraseña debe tener al menos 8 caracteres." });
      return;
    }
    if (nueva !== confirma) {
      setMsg({ ok: false, texto: "Las contraseñas no coinciden." });
      return;
    }
    setCargando(true);
    try {
      await authService.cambiarPassword(actual, nueva);
      setMsg({ ok: true, texto: "Contraseña actualizada." });
      setActual("");
      setNueva("");
      setConfirma("");
    } catch {
      setMsg({ ok: false, texto: "No se pudo cambiar. ¿La contraseña actual es correcta?" });
    } finally {
      setCargando(false);
    }
  }

  return (
    <Card>
      <CardHeader title="Cambiar contraseña" />
      <CardContent className="space-y-3">
        {msg && (
          <div
            className={`rounded-md p-3 text-sm ${
              msg.ok
                ? "bg-green-500/10 text-green-600"
                : "bg-destructive/10 text-destructive"
            }`}
          >
            {msg.texto}
          </div>
        )}
        <input
          type="password"
          value={actual}
          onChange={(e) => setActual(e.target.value)}
          placeholder="Contraseña actual"
          className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <input
          type="password"
          value={nueva}
          onChange={(e) => setNueva(e.target.value)}
          placeholder="Nueva contraseña (mín. 8 caracteres)"
          className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <input
          type="password"
          value={confirma}
          onChange={(e) => setConfirma(e.target.value)}
          placeholder="Confirmar nueva contraseña"
          className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <button
          onClick={guardar}
          disabled={cargando || !actual || !nueva}
          className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
        >
          Guardar contraseña
        </button>
      </CardContent>
    </Card>
  );
}
