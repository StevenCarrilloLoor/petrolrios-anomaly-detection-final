import { Database } from "lucide-react";
import { FuentesDatosSection } from "@/components/reglas/FuentesDatosSection";

/**
 * Página dedicada al catálogo de fuentes de datos (tablas extra de Firebird).
 * Se separó de "Reglas" porque registrar tablas y configurar reglas son tareas
 * distintas: aquí se administra QUÉ datos extraen los agentes; en Reglas se define
 * QUÉ se considera una anomalía sobre esos datos.
 */
export function FuentesDatosPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-start gap-3">
        <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
          <Database size={22} />
        </div>
        <div>
          <h1 className="text-2xl font-bold text-foreground">Fuentes de datos</h1>
          <p className="mt-1 max-w-3xl text-sm text-muted-foreground">
            Catálogo central de tablas de Firebird (Contaplus) que los agentes extraen y envían,
            además de las estándar. Regístrelas una sola vez aquí y todas las estaciones las reciben
            automáticamente; luego podrá crear reglas personalizadas sobre sus campos desde la
            pantalla de <span className="font-medium text-foreground">Reglas</span>.
          </p>
        </div>
      </div>

      <FuentesDatosSection />
    </div>
  );
}
