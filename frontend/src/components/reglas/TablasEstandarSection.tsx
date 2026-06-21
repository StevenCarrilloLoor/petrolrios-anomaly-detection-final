import { Layers } from "lucide-react";

/**
 * Tablas estándar del modelo Contaplus que el agente extrae SIEMPRE en cada ciclo
 * (no se registran; alimentan los 4 detectores integrados). Se muestran junto al
 * catálogo de tablas extra configurables para que se vea TODO lo que se está usando.
 * La lista refleja las consultas fijas del extractor (solo lectura).
 */
const TABLAS_ESTANDAR = [
  { nombre: "Facturas / documentos", tabla: "DCTO", cursor: "FEC_DCTO", uso: "Ventas, créditos, placas y formas de pago" },
  { nombre: "Despachos", tabla: "DESP", cursor: "FIN_DESP", uso: "Galones servidos, mangueras y facturación" },
  { nombre: "Turnos", tabla: "TURN", cursor: "FFI_TURN", uso: "Apertura/cierre, faltantes y vendedor" },
  { nombre: "Depósitos de turno", tabla: "TURN_DEPO", cursor: "FFI_TURN", uso: "Efectivo depositado por turno" },
  { nombre: "Anulaciones", tabla: "ANUL", cursor: "FECHAANULACION", uso: "Comprobantes anulados" },
  { nombre: "Créditos", tabla: "CRED_CABE", cursor: "FEC_CABE", uso: "Cabeceras de crédito, garante y autorización" },
  { nombre: "Tarjetas de turno", tabla: "TURN_TARJ", cursor: "FFI_TURN", uso: "Cobros con tarjeta por turno" },
];

export function TablasEstandarSection() {
  return (
    <div className="overflow-hidden rounded-xl border border-border bg-background">
      <div className="flex items-center gap-3 border-b border-border px-5 py-4">
        <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-muted text-muted-foreground">
          <Layers size={16} />
        </div>
        <div className="min-w-0">
          <h2 className="font-semibold text-foreground">Tablas estándar del modelo</h2>
          <p className="text-xs text-muted-foreground">
            Se extraen siempre en cada ciclo (solo lectura), sin necesidad de registrarlas.
            Alimentan los cuatro detectores integrados.
          </p>
        </div>
        <span className="ml-auto shrink-0 rounded-full bg-muted px-2.5 py-1 text-xs font-medium text-muted-foreground">
          {TABLAS_ESTANDAR.length} tablas
        </span>
      </div>

      <div className="divide-y divide-border">
        {TABLAS_ESTANDAR.map((t) => (
          <div
            key={t.tabla}
            className="flex flex-col gap-1 px-5 py-3 sm:flex-row sm:items-center sm:justify-between"
          >
            <div className="min-w-0">
              <div className="flex items-center gap-2">
                <p className="text-sm font-medium text-foreground">{t.nombre}</p>
                <span className="rounded bg-primary/10 px-1.5 py-0.5 font-mono text-[10px] font-semibold text-primary">
                  {t.tabla}
                </span>
              </div>
              <p className="truncate text-xs text-muted-foreground">{t.uso}</p>
            </div>
            <div className="flex shrink-0 items-center gap-2 text-xs text-muted-foreground">
              <span>cursor incremental</span>
              <span className="font-mono text-foreground">{t.cursor}</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
