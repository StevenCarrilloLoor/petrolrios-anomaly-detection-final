import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { consultasService, type DocumentoFirebird } from "@/services/consultas.service";
import { Spinner } from "@/components/ui/Spinner";
import { FileText, Printer, AlertTriangle } from "lucide-react";

function money(v: unknown): string {
  const n = typeof v === "number" ? v : Number(v);
  return Number.isFinite(n) ? `$${n.toFixed(2)}` : "—";
}
function texto(v: unknown): string {
  const s = v == null ? "" : String(v).trim();
  return s.length > 0 ? s : "—";
}
function fecha(v: unknown): string {
  if (!v) return "—";
  const d = new Date(String(v));
  return Number.isNaN(d.getTime()) ? String(v) : d.toLocaleString("es-EC");
}

function Campo({ label, value, mono }: { label: string; value: string; mono?: boolean }) {
  return (
    <div className="rounded-lg border border-border bg-background px-3 py-2">
      <p className="text-[11px] uppercase tracking-wide text-muted-foreground">{label}</p>
      <p className={`text-sm font-medium text-foreground ${mono ? "font-mono" : ""}`}>{value}</p>
    </div>
  );
}

/** Vista de la factura COMPLETA, pensada para abrirse en una ventana nueva (comparar lado a lado, imprimir). */
export function FacturaPage() {
  const [params] = useSearchParams();
  const est = params.get("est") ?? "";
  const num = params.get("num") ?? "";

  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [doc, setDoc] = useState<DocumentoFirebird | null>(null);

  useEffect(() => {
    let activo = true;
    (async () => {
      try {
        const docs = await consultasService.consultarDocumentos({
          codigoEstacion: est,
          codigo: num,
          limite: 10,
        });
        if (!activo) return;
        const exacto = docs.find((d) => String(d.NumeroDocumento ?? "").trim() === num.trim()) ?? docs[0] ?? null;
        setDoc(exacto);
        if (!exacto) setError("No se encontró el documento en la estación.");
      } catch (e: unknown) {
        if (activo) setError(e instanceof Error ? e.message : "No se pudo consultar la factura.");
      } finally {
        if (activo) setCargando(false);
      }
    })();
    return () => {
      activo = false;
    };
  }, [est, num]);

  return (
    <div className="mx-auto max-w-3xl p-6">
      <div className="mb-4 flex items-center justify-between print:hidden">
        <h1 className="flex items-center gap-2 text-xl font-bold text-foreground">
          <FileText size={22} /> Factura {texto(num)}
        </h1>
        <button
          onClick={() => window.print()}
          className="inline-flex items-center gap-2 rounded-md border border-border px-3 py-1.5 text-sm hover:bg-muted"
        >
          <Printer size={15} /> Imprimir
        </button>
      </div>

      {cargando && (
        <div className="flex flex-col items-center gap-2 py-16 text-muted-foreground">
          <Spinner size="lg" />
          <p className="text-sm">Consultando la factura en la estación…</p>
        </div>
      )}

      {error && !cargando && (
        <div className="flex items-start gap-2 rounded-lg border border-risk-high/40 bg-risk-high/10 p-3 text-sm text-risk-high">
          <AlertTriangle size={18} className="mt-0.5 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      {doc && !cargando && (
        <div className="space-y-4">
          <div className="rounded-xl border border-border bg-card p-4">
            <p className="mb-3 text-sm font-semibold text-foreground">Datos del documento</p>
            <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
              <Campo label="N.º de documento" value={texto(doc.NumeroDocumento)} mono />
              <Campo label="Tipo" value={texto(doc.TipoDocumento)} />
              <Campo label="Fecha" value={fecha(doc.Fecha)} />
              <Campo label="Cliente" value={texto(doc.Cliente)} mono />
              <Campo label="RUC / cédula" value={texto(doc.Ruc)} mono />
              <Campo label="Placa" value={texto(doc.Placa)} mono />
              <Campo label="Despachador" value={texto(doc.Vendedor)} mono />
              <Campo label="Forma de pago" value={texto(doc.FormaPago)} />
              <Campo label="Turno" value={texto(doc.NumeroTurno)} />
            </div>
          </div>

          <div className="rounded-xl border border-border bg-card p-4">
            <p className="mb-3 text-sm font-semibold text-foreground">Importes</p>
            <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
              <Campo label="Base (sin IVA)" value={money(doc.TotalSinIva)} />
              <Campo label="IVA" value={money(doc.Iva)} />
              <Campo label="Descuento" value={money(doc.Descuento)} />
              <Campo label="Total" value={money(doc.TotalNeto)} />
            </div>
          </div>

          <p className="text-xs text-muted-foreground">
            Datos traídos en vivo de la base de la estación {texto(est)} (solo lectura). El detalle de líneas
            de despacho (DESP) se añadirá en una próxima versión.
          </p>
        </div>
      )}
    </div>
  );
}
