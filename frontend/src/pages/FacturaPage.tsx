import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import {
  consultasService,
  type DocumentoFirebird,
  type DespachoFirebird,
} from "@/services/consultas.service";
import { Spinner } from "@/components/ui/Spinner";
import { FileText, Printer, AlertTriangle, Fuel } from "lucide-react";

function money(v: unknown): string {
  const n = typeof v === "number" ? v : Number(v);
  return Number.isFinite(n) ? `$${n.toFixed(2)}` : "—";
}
function galones(v: unknown): string {
  const n = typeof v === "number" ? v : Number(v);
  return Number.isFinite(n) ? n.toFixed(2) : "—";
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
  const [lineas, setLineas] = useState<DespachoFirebird[]>([]);
  const [cargandoLineas, setCargandoLineas] = useState(false);

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

  // Líneas de surtidor (DESP) de la factura: se cargan cuando hay un nº de despacho numérico (NDO_DCTO).
  useEffect(() => {
    const desp = String(doc?.NumeroDespacho ?? "").trim();
    if (!desp || !/^\d+$/.test(desp)) {
      setLineas([]);
      return;
    }
    let activo = true;
    setCargandoLineas(true);
    consultasService
      .consultarDespachos(est, desp)
      .then((ls) => activo && setLineas(ls))
      .catch(() => activo && setLineas([]))
      .finally(() => activo && setCargandoLineas(false));
    return () => {
      activo = false;
    };
  }, [doc, est]);

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
              <Campo label="Despacho (origen)" value={texto(doc.NumeroDespacho)} mono />
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

          {/* Líneas de surtido (DESP), cruzadas por el «Despacho (origen)» = NDO_DCTO → NUM_DESP. */}
          <div className="rounded-xl border border-border bg-card p-4">
            <p className="mb-3 flex items-center gap-2 text-sm font-semibold text-foreground">
              <Fuel size={15} /> Líneas de surtido
            </p>
            {cargandoLineas ? (
              <div className="flex items-center gap-2 py-2 text-sm text-muted-foreground">
                <Spinner size="sm" /> Consultando el surtido en la estación…
              </div>
            ) : lineas.length > 0 ? (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border text-left text-[11px] uppercase tracking-wide text-muted-foreground">
                      <th className="py-1.5 pr-3 font-medium">Producto</th>
                      <th className="py-1.5 pr-3 font-medium">Manguera</th>
                      <th className="py-1.5 pr-3 text-right font-medium">Galones</th>
                      <th className="py-1.5 pr-3 text-right font-medium">Precio unit.</th>
                      <th className="py-1.5 text-right font-medium">Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    {lineas.map((l, i) => (
                      <tr key={`${l.NumeroDespacho}-${i}`} className="border-b border-border/50 last:border-0">
                        <td className="py-1.5 pr-3 text-foreground">{texto(l.Producto)}</td>
                        <td className="py-1.5 pr-3 font-mono text-xs text-muted-foreground">{texto(l.Manguera)}</td>
                        <td className="py-1.5 pr-3 text-right font-mono text-foreground">{galones(l.Galones)}</td>
                        <td className="py-1.5 pr-3 text-right font-mono text-foreground">{money(l.PrecioUnitario)}</td>
                        <td className="py-1.5 text-right font-mono font-medium text-foreground">{money(l.Total)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="py-1 text-sm text-muted-foreground">
                {texto(doc.NumeroDespacho) === "—"
                  ? "Esta factura no tiene un despacho de surtidor asociado (NDO_DCTO vacío)."
                  : `Sin líneas de surtido para el despacho ${texto(doc.NumeroDespacho)}.`}
              </p>
            )}
          </div>

          <p className="text-xs text-muted-foreground">
            Datos traídos en vivo de la base de la estación {texto(est)} (solo lectura): cabecera (DCTO),
            importes y líneas de surtido (DESP, cruzadas por «Despacho (origen)» = NDO_DCTO → NUM_DESP).
          </p>
        </div>
      )}
    </div>
  );
}
