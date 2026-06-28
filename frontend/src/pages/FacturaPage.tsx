import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import {
  consultasService,
  type DocumentoFirebird,
  type DespachoFirebird,
} from "@/services/consultas.service";
import { Spinner } from "@/components/ui/Spinner";
import { nombreCombustible } from "@/lib/combustibles";
import { FileText, Printer, AlertTriangle, Fuel, X } from "lucide-react";

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

// Tipo de documento (TIP_DCTO) y forma de pago (COD_PAGO) legibles. La auditora pidió ver los nombres,
// no los códigos crudos ("¿qué es el facturado 5? no sé"). Catálogo de las capturas reales del POS.
const TIPO_DOC: Record<string, string> = {
  FV: "Factura de venta",
  DV: "Devolución / nota de crédito",
  EB: "Egreso de bodega",
};
function tipoDoc(code: unknown): string {
  const c = texto(code);
  if (c === "—") return "Documento";
  return TIPO_DOC[c.toUpperCase()] ?? c;
}
const FORMA_PAGO: Record<string, string> = {
  "001": "Contado (efectivo)",
  "002": "Tarjeta de crédito",
  "003": "Tarjeta de débito",
  "004": "Cheque",
  "020": "Pago Ya",
  "021": "Otros pagos",
  CRE: "Crédito",
  CON: "Contado islas",
  EFE: "Efectivo",
  EF: "Efectivo",
};
function formaPago(code: unknown): string {
  const c = texto(code);
  if (c === "—") return "—";
  const label = FORMA_PAGO[c.toUpperCase()];
  return label ? `${label} (${c})` : c;
}

/** Fila etiqueta→valor dentro de un bloque (cliente / documento). */
function Row({ k, v, mono }: { k: string; v: string; mono?: boolean }) {
  return (
    <div className="flex items-start justify-between gap-3 border-b border-border/40 py-1 last:border-0 print:border-black/10">
      <dt className="shrink-0 text-xs text-muted-foreground print:text-gray-600">{k}</dt>
      <dd className={`text-right text-sm font-medium text-foreground print:text-black ${mono ? "font-mono" : ""}`}>{v}</dd>
    </div>
  );
}

/** Fila de un total (derecha). El total final va resaltado. */
function Total({ k, v, fuerte }: { k: string; v: string; fuerte?: boolean }) {
  return (
    <div
      className={
        fuerte
          ? "mt-1 flex items-center justify-between rounded-lg bg-primary/10 px-3 py-2 text-base font-bold text-primary print:bg-gray-100 print:text-black"
          : "flex items-center justify-between px-3 py-0.5 text-sm"
      }
    >
      <span className={fuerte ? "" : "text-muted-foreground print:text-gray-600"}>{k}</span>
      <span className="font-mono font-medium">{v}</span>
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

  const nombreCliente = doc
    ? texto(doc.ClienteNombre) !== "—"
      ? texto(doc.ClienteNombre)
      : texto(doc.ClienteRazon)
    : "—";
  const despachador = doc
    ? texto(doc.VendedorNombre) !== "—"
      ? `${texto(doc.VendedorNombre)} (${texto(doc.Vendedor)})`
      : texto(doc.Vendedor)
    : "—";

  return (
    <div className="mx-auto max-w-4xl p-4 sm:p-6 print:max-w-none print:p-0 print:text-black">
      {/* Barra de acciones (no se imprime) */}
      <div className="mb-4 flex items-center justify-between print:hidden">
        <h1 className="flex items-center gap-2 text-xl font-bold text-foreground">
          <FileText size={22} /> Factura {texto(num)}
        </h1>
        <div className="flex gap-2">
          <button
            onClick={() => window.print()}
            className="inline-flex items-center gap-2 rounded-md bg-primary px-3 py-1.5 text-sm font-semibold text-primary-foreground hover:bg-primary/90"
          >
            <Printer size={15} /> Imprimir
          </button>
          <button
            onClick={() => window.close()}
            className="inline-flex items-center gap-2 rounded-md border border-border px-3 py-1.5 text-sm hover:bg-muted"
          >
            <X size={15} /> Cerrar
          </button>
        </div>
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
        <div className="overflow-hidden rounded-2xl border border-border bg-card shadow-sm print:rounded-none print:border print:border-black/20 print:bg-white print:shadow-none">
          {/* Encabezado de la factura */}
          <div className="flex flex-col gap-4 border-b border-border bg-muted/30 p-5 sm:flex-row sm:items-start sm:justify-between print:bg-white">
            <div>
              <p className="text-[11px] font-semibold uppercase tracking-widest text-muted-foreground print:text-gray-600">
                PetrolRíos S.A. · Estación {texto(est)}
              </p>
              <h2 className="mt-1 text-lg font-bold text-foreground print:text-black">{tipoDoc(doc.TipoDocumento)}</h2>
              <p className="text-xs text-muted-foreground print:text-gray-600">
                Comprobante en vivo de la estación (solo lectura)
              </p>
            </div>
            <div className="sm:text-right">
              <p className="text-[11px] uppercase tracking-wide text-muted-foreground print:text-gray-600">N.º de documento</p>
              <p className="font-mono text-xl font-bold text-foreground print:text-black">{texto(doc.NumeroDocumento)}</p>
              <p className="mt-0.5 text-sm text-muted-foreground print:text-gray-700">{fecha(doc.Fecha)}</p>
              <span className="mt-1 inline-block rounded-full bg-primary/10 px-2 py-0.5 text-[11px] font-semibold text-primary print:bg-gray-100 print:text-black">
                {texto(doc.TipoDocumento)}
              </span>
            </div>
          </div>

          {/* Cliente + Documento */}
          <div className="grid grid-cols-1 gap-5 p-5 md:grid-cols-2">
            <section>
              <h3 className="mb-2 text-xs font-semibold uppercase tracking-wide text-muted-foreground print:text-gray-600">
                Cliente
              </h3>
              <p className="text-base font-semibold text-foreground print:text-black">{nombreCliente}</p>
              <dl className="mt-2">
                <Row k="Código" v={texto(doc.Cliente)} mono />
                <Row k="RUC / cédula" v={texto(doc.Ruc)} mono />
                <Row k="Dirección" v={texto(doc.Direccion)} />
                <Row k="Teléfono" v={texto(doc.ClienteTelefono)} mono />
                <Row k="Correo" v={texto(doc.ClienteCorreo)} />
              </dl>
            </section>

            <section>
              <h3 className="mb-2 text-xs font-semibold uppercase tracking-wide text-muted-foreground print:text-gray-600">
                Detalles del documento
              </h3>
              <dl>
                <Row k="Despachador" v={despachador} />
                <Row k="Chofer" v={texto(doc.Chofer)} mono />
                <Row k="Placa" v={texto(doc.Placa)} mono />
                <Row k="Forma de pago" v={formaPago(doc.FormaPago)} />
                <Row k="Turno" v={texto(doc.NumeroTurno)} mono />
                <Row k="Consecutivo" v={texto(doc.Consecutivo)} mono />
                <Row k="Autorización (SRI)" v={texto(doc.Autorizacion)} mono />
                <Row k="Guía" v={texto(doc.Guia)} mono />
                <Row k="Despacho (origen)" v={texto(doc.NumeroDespacho)} mono />
              </dl>
            </section>
          </div>

          {/* Líneas de surtido (DESP), cruzadas por el «Despacho (origen)» = NDO_DCTO → NUM_DESP. */}
          <div className="border-t border-border px-5 py-4 print:border-black/20">
            <h3 className="mb-3 flex items-center gap-2 text-xs font-semibold uppercase tracking-wide text-muted-foreground print:text-gray-600">
              <Fuel size={14} /> Líneas de surtido
            </h3>
            {cargandoLineas ? (
              <div className="flex items-center gap-2 py-2 text-sm text-muted-foreground">
                <Spinner size="sm" /> Consultando el surtido en la estación…
              </div>
            ) : lineas.length > 0 ? (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border text-left text-[11px] uppercase tracking-wide text-muted-foreground print:border-black/30 print:text-gray-600">
                      <th className="py-1.5 pr-3 font-medium">Producto</th>
                      <th className="py-1.5 pr-3 font-medium">Manguera</th>
                      <th className="py-1.5 pr-3 text-right font-medium">Galones</th>
                      <th className="py-1.5 pr-3 text-right font-medium">Precio unit.</th>
                      <th className="py-1.5 text-right font-medium">Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    {lineas.map((l, i) => (
                      <tr key={`${l.NumeroDespacho}-${i}`} className="border-b border-border/50 last:border-0 print:border-black/10">
                        <td
                          className="py-1.5 pr-3 text-foreground print:text-black"
                          title={l.CodigoProducto ? `Código de producto: ${l.CodigoProducto}` : undefined}
                        >
                          {nombreCombustible(l.CodigoProducto) || texto(l.Producto)}
                        </td>
                        <td className="py-1.5 pr-3 font-mono text-xs text-muted-foreground print:text-gray-600">{texto(l.Manguera)}</td>
                        <td className="py-1.5 pr-3 text-right font-mono text-foreground print:text-black">{galones(l.Galones)}</td>
                        <td className="py-1.5 pr-3 text-right font-mono text-foreground print:text-black">{money(l.PrecioUnitario)}</td>
                        <td className="py-1.5 text-right font-mono font-medium text-foreground print:text-black">{money(l.Total)}</td>
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

          {/* Importes */}
          <div className="border-t border-border p-5 print:border-black/20">
            <div className="ml-auto w-full max-w-xs">
              <Total k="Subtotal" v={money(doc.Subtotal)} />
              <Total k="Base (sin IVA)" v={money(doc.TotalSinIva)} />
              <Total k="Descuento" v={money(doc.Descuento)} />
              <Total k="IVA" v={money(doc.Iva)} />
              <Total k="TOTAL" v={money(doc.TotalNeto)} fuerte />
            </div>
          </div>

          {/* Observaciones (si las hay) */}
          {texto(doc.Observaciones) !== "—" && (
            <div className="border-t border-border px-5 py-3 text-sm print:border-black/20">
              <span className="font-semibold text-foreground print:text-black">Observaciones: </span>
              <span className="text-muted-foreground print:text-gray-700">{texto(doc.Observaciones)}</span>
            </div>
          )}

          {/* Pie */}
          <div className="border-t border-border bg-muted/20 px-5 py-3 text-[11px] text-muted-foreground print:border-black/20 print:bg-white print:text-gray-600">
            Datos traídos EN VIVO de la base de la estación {texto(est)} (Firebird, solo lectura): cabecera (DCTO)
            + cliente (CLIE) + despachador (VEND) + líneas de surtido (DESP, cruzadas por «Despacho (origen)» =
            NDO_DCTO → NUM_DESP).
          </div>
        </div>
      )}
    </div>
  );
}
