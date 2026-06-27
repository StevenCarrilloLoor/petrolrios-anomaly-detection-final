import {
  Sun,
  Moon,
  Monitor,
  Bell,
  Volume2,
  SlidersHorizontal,
  Database,
  Loader2,
  CheckCircle2,
  XCircle,
  AlertTriangle,
} from "lucide-react";
import { useEffect, useState } from "react";
import type { ReactNode } from "react";
import { useAjustes, type Tema, type TamanoFuente } from "@/contexts/SettingsContext";
import { useAuth } from "@/contexts/AuthContext";
import { Card, CardContent, CardHeader } from "@/components/ui/Card";
import { conexionBaseService } from "@/services/conexionBase.service";
import { operacionService, type OperacionConfig } from "@/services/operacion.service";
import type {
  ConexionActiva,
  ProbarConexionRequest,
  ProbarConexionResponse,
  GuardarConexionResponse,
} from "@/types/conexionBase";

const TEMAS: { valor: Tema; label: string; icono: ReactNode; descripcion: string }[] = [
  { valor: "sistema", label: "Sistema", icono: <Monitor size={20} />, descripcion: "Sigue tu equipo" },
  { valor: "claro", label: "Claro", icono: <Sun size={20} />, descripcion: "Siempre claro" },
  { valor: "oscuro", label: "Oscuro", icono: <Moon size={20} />, descripcion: "Siempre oscuro" },
];

const TAMANOS: { valor: TamanoFuente; label: string; descripcion: string; clase: string }[] = [
  { valor: "normal", label: "Normal", descripcion: "Tamaño estándar", clase: "text-lg" },
  { valor: "grande", label: "Grande", descripcion: "Un poco más grande", clase: "text-2xl" },
  { valor: "mayor", label: "Mayor", descripcion: "El más grande", clase: "text-3xl" },
];

export function AjustesPage() {
  const { ajustes, actualizar } = useAjustes();
  const { user } = useAuth();
  const esAdmin = user?.rol === "Administrador";

  return (
    <div className="max-w-3xl space-y-6">
      <div className="flex items-start gap-3">
        <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
          <SlidersHorizontal size={22} />
        </div>
        <div>
          <h1 className="text-2xl font-bold text-foreground">Ajustes</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Personalice la apariencia y las notificaciones. Las preferencias se guardan en este
            navegador.
          </p>
        </div>
      </div>

      <Card>
        <CardHeader title="Apariencia" subtitle="Tema de la interfaz del panel." />
        <CardContent>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            {TEMAS.map((t) => {
              const activo = ajustes.tema === t.valor;
              return (
                <button
                  key={t.valor}
                  onClick={() => actualizar({ tema: t.valor })}
                  className={`flex flex-col items-start gap-2 rounded-xl border p-4 text-left transition-colors ${
                    activo
                      ? "border-primary bg-primary/5 ring-1 ring-primary/30"
                      : "border-border hover:border-primary/50 hover:bg-muted"
                  }`}
                >
                  <span className={activo ? "text-primary" : "text-muted-foreground"}>
                    {t.icono}
                  </span>
                  <span className="text-sm font-semibold text-foreground">{t.label}</span>
                  <span className="text-xs text-muted-foreground">{t.descripcion}</span>
                </button>
              );
            })}
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader
          title="Tamaño de letra"
          subtitle="Agranda el texto de todo el panel si te cuesta leer. Se aplica al instante."
        />
        <CardContent>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            {TAMANOS.map((t) => {
              const activo = ajustes.tamanoFuente === t.valor;
              return (
                <button
                  key={t.valor}
                  onClick={() => actualizar({ tamanoFuente: t.valor })}
                  className={`flex flex-col items-start gap-1 rounded-xl border p-4 text-left transition-colors ${
                    activo
                      ? "border-primary bg-primary/5 ring-1 ring-primary/30"
                      : "border-border hover:border-primary/50 hover:bg-muted"
                  }`}
                >
                  <span className={`font-bold leading-none text-foreground ${t.clase}`}>Aa</span>
                  <span className="text-sm font-semibold text-foreground">{t.label}</span>
                  <span className="text-xs text-muted-foreground">{t.descripcion}</span>
                </button>
              );
            })}
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader
          title="Notificaciones"
          subtitle="Cómo avisarle cuando llegan alertas o problemas nuevos."
        />
        <CardContent className="p-0">
          <FilaInterruptor
            icono={<Volume2 size={18} />}
            titulo="Sonido en alertas críticas"
            descripcion="Reproduce un tono breve cuando llega una alerta de nivel crítico."
            activo={ajustes.sonidoAlertas}
            onToggle={() => actualizar({ sonidoAlertas: !ajustes.sonidoAlertas })}
          />
          <FilaInterruptor
            icono={<Bell size={18} />}
            titulo="Avisos emergentes (toasts)"
            descripcion="Muestra una tarjeta temporal en pantalla con cada alerta o problema nuevo."
            activo={ajustes.mostrarToasts}
            onToggle={() => actualizar({ mostrarToasts: !ajustes.mostrarToasts })}
            borde
          />
        </CardContent>
      </Card>

      {esAdmin && <SeccionConexionBase />}
      {esAdmin && <SeccionOperacion />}
    </div>
  );
}

function SeccionConexionBase() {
  const [estado, setEstado] = useState<ConexionActiva | null>(null);
  const [modo, setModo] = useState<"simple" | "avanzado">("simple");
  const [form, setForm] = useState({
    servidor: "",
    puerto: 5432,
    baseDatos: "petrolrios",
    usuario: "petrolrios",
    password: "",
    modoSsl: "Prefer",
    cadena: "",
  });
  const [prueba, setPrueba] = useState<ProbarConexionResponse | null>(null);
  const [guardado, setGuardado] = useState<GuardarConexionResponse | null>(null);
  const [cargando, setCargando] = useState<"probar" | "guardar" | null>(null);

  useEffect(() => {
    let activo = true;
    conexionBaseService
      .estado()
      .then((e) => {
        if (!activo) return;
        setEstado(e);
        setForm((f) => ({
          ...f,
          servidor: e.servidor ?? f.servidor,
          puerto: e.puerto || 5432,
          baseDatos: e.baseDatos ?? f.baseDatos,
          usuario: e.usuario ?? f.usuario,
          modoSsl: e.modoSsl || "Prefer",
        }));
      })
      .catch(() => {});
    return () => {
      activo = false;
    };
  }, []);

  const construirRequest = (): ProbarConexionRequest =>
    modo === "avanzado"
      ? { cadena: form.cadena }
      : {
          servidor: form.servidor,
          puerto: form.puerto,
          baseDatos: form.baseDatos,
          usuario: form.usuario,
          password: form.password || undefined,
          modoSsl: form.modoSsl,
        };

  const onProbar = async () => {
    setCargando("probar");
    setPrueba(null);
    setGuardado(null);
    try {
      setPrueba(await conexionBaseService.probar(construirRequest()));
    } catch {
      setPrueba({ ok: false, mensaje: "No se pudo contactar al servidor.", version: null });
    } finally {
      setCargando(null);
    }
  };

  const onGuardar = async () => {
    setCargando("guardar");
    setGuardado(null);
    try {
      const r = await conexionBaseService.guardar(construirRequest());
      setGuardado(r);
      if (r.ok) setEstado(await conexionBaseService.estado());
    } catch {
      setGuardado({ ok: false, mensaje: "No se pudo guardar (¿la conexión falló?)." });
    } finally {
      setCargando(null);
    }
  };

  const inputCls =
    "w-full rounded-lg border border-border bg-background px-3 py-2 text-sm text-foreground outline-none focus:border-primary focus:ring-1 focus:ring-primary/30";

  return (
    <Card>
      <CardHeader
        title="Conexión a la base de datos"
        subtitle="Dónde vive PostgreSQL. Pruebe y guarde sin tocar el código; se aplica al reiniciar el central."
      />
      <CardContent className="space-y-4">
        {estado && (
          <div className="rounded-lg border border-border bg-muted/40 p-3 text-xs">
            <div className="flex flex-wrap items-center gap-2 text-foreground">
              <Database size={14} className="text-primary" />
              <span className="font-medium">Conexión actual</span>
              <span className="rounded bg-primary/10 px-1.5 py-0.5 text-[11px] text-primary">
                {estado.fuente}
              </span>
            </div>
            <p className="mt-1 break-all font-mono text-muted-foreground">{estado.enmascarada}</p>
          </div>
        )}

        {estado && !estado.editableDesdeUi && (
          <div className="flex items-start gap-2 rounded-lg border border-amber-500/40 bg-amber-500/10 p-3 text-xs text-foreground">
            <AlertTriangle size={14} className="mt-0.5 shrink-0 text-amber-600" />
            <span>
              La conexión está fijada por una variable de entorno, que tiene prioridad. Puede probar
              aquí, pero para que el guardado surta efecto debe quitar esa variable del entorno.
            </span>
          </div>
        )}

        <div className="flex gap-2">
          <BotonModo activo={modo === "simple"} onClick={() => setModo("simple")} label="Campos" />
          <BotonModo
            activo={modo === "avanzado"}
            onClick={() => setModo("avanzado")}
            label="Cadena (avanzado)"
          />
        </div>

        {modo === "simple" ? (
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Campo label="Servidor (host o IP)">
              <input
                className={inputCls}
                value={form.servidor}
                onChange={(e) => setForm({ ...form, servidor: e.target.value })}
                placeholder="192.168.1.50 / db.empresa.com"
              />
            </Campo>
            <Campo label="Puerto">
              <input
                className={inputCls}
                type="number"
                value={form.puerto}
                onChange={(e) => setForm({ ...form, puerto: Number(e.target.value) || 5432 })}
              />
            </Campo>
            <Campo label="Base de datos">
              <input
                className={inputCls}
                value={form.baseDatos}
                onChange={(e) => setForm({ ...form, baseDatos: e.target.value })}
              />
            </Campo>
            <Campo label="Usuario">
              <input
                className={inputCls}
                value={form.usuario}
                onChange={(e) => setForm({ ...form, usuario: e.target.value })}
              />
            </Campo>
            <Campo label="Contraseña">
              <input
                className={inputCls}
                type="password"
                value={form.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })}
                placeholder="(sin cambios si se deja vacío)"
              />
            </Campo>
            <Campo label="SSL">
              <select
                className={inputCls}
                value={form.modoSsl}
                onChange={(e) => setForm({ ...form, modoSsl: e.target.value })}
              >
                <option value="Disable">Desactivado</option>
                <option value="Prefer">Preferir</option>
                <option value="Require">Requerir</option>
                <option value="VerifyCA">Verificar CA</option>
                <option value="VerifyFull">Verificar completo</option>
              </select>
            </Campo>
          </div>
        ) : (
          <Campo label="Cadena de conexión (Npgsql)">
            <textarea
              className={`${inputCls} font-mono`}
              rows={3}
              value={form.cadena}
              onChange={(e) => setForm({ ...form, cadena: e.target.value })}
              placeholder="Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require"
            />
          </Campo>
        )}

        <div className="flex flex-wrap items-center gap-2">
          <button
            onClick={onProbar}
            disabled={cargando !== null}
            className="inline-flex items-center gap-2 rounded-lg border border-border px-3 py-2 text-sm font-medium text-foreground hover:bg-muted disabled:opacity-50"
          >
            {cargando === "probar" ? <Loader2 size={15} className="animate-spin" /> : null}
            Probar conexión
          </button>
          <button
            onClick={onGuardar}
            disabled={cargando !== null}
            className="inline-flex items-center gap-2 rounded-lg bg-primary px-3 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
          >
            {cargando === "guardar" ? <Loader2 size={15} className="animate-spin" /> : null}
            Guardar
          </button>
        </div>

        {prueba && (
          <Resultado
            ok={prueba.ok}
            texto={
              prueba.ok
                ? `Conexión exitosa${prueba.version ? ` · PostgreSQL ${prueba.version}` : ""}.`
                : prueba.mensaje ?? "Falló la conexión."
            }
          />
        )}
        {guardado && <Resultado ok={guardado.ok} texto={guardado.mensaje} />}
      </CardContent>
    </Card>
  );
}

/** Frecuencias listas para elegir (en español); cada una mapea a su expresión cron. */
const FRECUENCIAS: { label: string; cron: string }[] = [
  { label: "Cada minuto (lo más rápido, más carga)", cron: "* * * * *" },
  { label: "Cada 2 minutos", cron: "*/2 * * * *" },
  { label: "Cada 5 minutos (recomendado)", cron: "*/5 * * * *" },
  { label: "Cada 10 minutos", cron: "*/10 * * * *" },
  { label: "Cada 15 minutos", cron: "*/15 * * * *" },
  { label: "Cada 30 minutos", cron: "*/30 * * * *" },
  { label: "Cada hora", cron: "0 * * * *" },
  { label: "Cada 2 horas", cron: "0 */2 * * *" },
];
const CRON_PERSONALIZADO = "__custom__";

/** Operación del sistema (solo Admin): nivel mínimo de correo + frecuencia (cron) del job. */
function SeccionOperacion() {
  const [config, setConfig] = useState<OperacionConfig>({
    nivelMinimoCorreo: "Critico",
    cronExpression: "*/5 * * * *",
    refrescoSegundos: 1,
  });
  const [guardado, setGuardado] = useState<{ ok: boolean; texto: string } | null>(null);
  const [cargando, setCargando] = useState(false);
  const [avanzado, setAvanzado] = useState(false);

  useEffect(() => {
    let activo = true;
    operacionService
      .actual()
      .then((c) => {
        if (activo) setConfig(c);
      })
      .catch(() => {});
    return () => {
      activo = false;
    };
  }, []);

  const onGuardar = async () => {
    setCargando(true);
    setGuardado(null);
    try {
      const c = await operacionService.guardar(config);
      setConfig(c);
      setGuardado({
        ok: true,
        texto: "Guardado. La frecuencia se aplicó al instante; el nivel de correo, en el próximo ciclo.",
      });
    } catch (e: unknown) {
      const msg = (e as { response?: { data?: { mensaje?: string } } })?.response?.data?.mensaje;
      setGuardado({ ok: false, texto: msg ?? "No se pudo guardar." });
    } finally {
      setCargando(false);
    }
  };

  const inputCls =
    "w-full rounded-lg border border-border bg-background px-3 py-2 text-sm text-foreground outline-none focus:border-primary focus:ring-1 focus:ring-primary/30";

  const frecConocida = FRECUENCIAS.find((f) => f.cron === config.cronExpression);
  const valorSelect = avanzado ? CRON_PERSONALIZADO : (frecConocida?.cron ?? CRON_PERSONALIZADO);
  const mostrarRaw = valorSelect === CRON_PERSONALIZADO;

  return (
    <Card>
      <CardHeader
        title="Operación del sistema"
        subtitle="Solo administrador: desde qué nivel avisar por correo y cada cuánto corre la detección."
      />
      <CardContent className="space-y-4">
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <Campo label="Avisar por correo desde el nivel">
            <select
              className={inputCls}
              value={config.nivelMinimoCorreo}
              onChange={(e) => setConfig({ ...config, nivelMinimoCorreo: e.target.value })}
            >
              <option value="Bajo">Bajo (todas las alertas)</option>
              <option value="Medio">Medio o superior</option>
              <option value="Alto">Alto o superior</option>
              <option value="Critico">Solo críticas</option>
            </select>
          </Campo>
          <Campo label="¿Cada cuánto corre el análisis?">
            <select
              className={inputCls}
              value={valorSelect}
              onChange={(e) => {
                if (e.target.value === CRON_PERSONALIZADO) {
                  setAvanzado(true);
                } else {
                  setAvanzado(false);
                  setConfig({ ...config, cronExpression: e.target.value });
                }
              }}
            >
              {FRECUENCIAS.map((f) => (
                <option key={f.cron} value={f.cron}>
                  {f.label}
                </option>
              ))}
              <option value={CRON_PERSONALIZADO}>Personalizado… (avanzado)</option>
            </select>
          </Campo>
          <Campo label="Tasa de refresco de las pantallas">
            <select
              className={inputCls}
              value={config.refrescoSegundos}
              onChange={(e) => setConfig({ ...config, refrescoSegundos: Number(e.target.value) })}
            >
              <option value={1}>Cada 1 segundo (casi en vivo)</option>
              <option value={2}>Cada 2 segundos</option>
              <option value={5}>Cada 5 segundos</option>
              <option value={10}>Cada 10 segundos</option>
              <option value={30}>Cada 30 segundos</option>
              <option value={60}>Cada 1 minuto</option>
            </select>
            <p className="mt-1 text-xs text-muted-foreground">
              Cada cuánto TODAS las pantallas vuelven a consultar al servidor. 1 s = casi en vivo; súbelo
              si el equipo va lento.
            </p>
          </Campo>
        </div>

        {mostrarRaw ? (
          <div className="space-y-1">
            <input
              className={`${inputCls} font-mono`}
              value={config.cronExpression}
              onChange={(e) => setConfig({ ...config, cronExpression: e.target.value })}
              placeholder="*/5 * * * *"
            />
            <p className="text-xs text-muted-foreground">
              Avanzado: expresión cron de 5 campos (min hora día mes día-semana). Ejemplos:{" "}
              <span className="font-mono">* * * * *</span> cada minuto,{" "}
              <span className="font-mono">*/5 * * * *</span> cada 5 minutos,{" "}
              <span className="font-mono">0 * * * *</span> cada hora. Si no estás seguro, elige una opción
              de la lista de arriba.
            </p>
          </div>
        ) : (
          <p className="text-xs text-muted-foreground">
            El sistema revisará las reglas <span className="font-medium text-foreground">{frecConocida?.label.toLowerCase()}</span>.
            Más seguido = detecta antes pero usa más recursos. Lo normal es cada 5 minutos.
          </p>
        )}
        <button
          onClick={onGuardar}
          disabled={cargando}
          className="inline-flex items-center gap-2 rounded-lg bg-primary px-3 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
        >
          {cargando ? <Loader2 size={15} className="animate-spin" /> : null}
          Guardar
        </button>
        {guardado && <Resultado ok={guardado.ok} texto={guardado.texto} />}
      </CardContent>
    </Card>
  );
}

function BotonModo({
  activo,
  onClick,
  label,
}: {
  activo: boolean;
  onClick: () => void;
  label: string;
}) {
  return (
    <button
      onClick={onClick}
      className={`rounded-lg border px-3 py-1.5 text-xs font-medium transition-colors ${
        activo
          ? "border-primary bg-primary/5 text-primary"
          : "border-border text-muted-foreground hover:bg-muted"
      }`}
    >
      {label}
    </button>
  );
}

function Campo({ label, children }: { label: string; children: ReactNode }) {
  return (
    <label className="block space-y-1">
      <span className="text-xs font-medium text-muted-foreground">{label}</span>
      {children}
    </label>
  );
}

function Resultado({ ok, texto }: { ok: boolean; texto: string }) {
  return (
    <div
      className={`flex items-start gap-2 rounded-lg border p-3 text-xs text-foreground ${
        ok ? "border-green-500/40 bg-green-500/10" : "border-red-500/40 bg-red-500/10"
      }`}
    >
      {ok ? (
        <CheckCircle2 size={14} className="mt-0.5 shrink-0 text-green-600" />
      ) : (
        <XCircle size={14} className="mt-0.5 shrink-0 text-red-600" />
      )}
      <span>{texto}</span>
    </div>
  );
}

function FilaInterruptor({
  icono,
  titulo,
  descripcion,
  activo,
  onToggle,
  borde,
}: {
  icono: ReactNode;
  titulo: string;
  descripcion: string;
  activo: boolean;
  onToggle: () => void;
  borde?: boolean;
}) {
  return (
    <div
      className={`flex items-center justify-between gap-4 px-6 py-4 ${
        borde ? "border-t border-border" : ""
      }`}
    >
      <div className="flex items-start gap-3">
        <span className="mt-0.5 text-muted-foreground">{icono}</span>
        <div>
          <p className="text-sm font-medium text-foreground">{titulo}</p>
          <p className="mt-0.5 text-xs text-muted-foreground">{descripcion}</p>
        </div>
      </div>
      <button
        role="switch"
        aria-checked={activo}
        onClick={onToggle}
        className={`relative h-6 w-11 shrink-0 rounded-full transition-colors ${
          activo ? "bg-risk-low" : "bg-muted-foreground/30"
        }`}
        title={activo ? "Desactivar" : "Activar"}
      >
        <span
          className={`absolute top-0.5 h-5 w-5 rounded-full bg-white shadow transition-all ${
            activo ? "left-[22px]" : "left-0.5"
          }`}
        />
      </button>
    </div>
  );
}
