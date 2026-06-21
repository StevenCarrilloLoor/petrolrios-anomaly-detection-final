import { Sun, Moon, Monitor, Bell, Volume2, SlidersHorizontal } from "lucide-react";
import { useAjustes, type Tema } from "@/contexts/SettingsContext";
import { Card, CardContent, CardHeader } from "@/components/ui/Card";
import type { ReactNode } from "react";

const TEMAS: { valor: Tema; label: string; icono: ReactNode; descripcion: string }[] = [
  { valor: "sistema", label: "Sistema", icono: <Monitor size={20} />, descripcion: "Sigue tu equipo" },
  { valor: "claro", label: "Claro", icono: <Sun size={20} />, descripcion: "Siempre claro" },
  { valor: "oscuro", label: "Oscuro", icono: <Moon size={20} />, descripcion: "Siempre oscuro" },
];

export function AjustesPage() {
  const { ajustes, actualizar } = useAjustes();

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
