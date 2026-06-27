import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { estacionesService } from "@/services/estaciones.service";
import {
  consultasService,
  type DocumentoFirebird,
  type SolicitudConsulta,
} from "@/services/consultas.service";
import { Spinner } from "@/components/ui/Spinner";
import { EmptyState } from "@/components/ui/EmptyState";
import { FileSearch, Search, ExternalLink, SearchX, AlertTriangle } from "lucide-react";

const inputClass =
  "rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40";

const TIPOS = [
  { v: "", l: "Todos los tipos" },
  { v: "FV", l: "Factura de venta (FV)" },
  { v: "DV", l: "Devolución / nota de crédito (DV)" },
  { v: "EB", l: "Egreso de bodega (EB)" },
];

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

/** Abre la factura completa en una VENTANA NUEVA (lo que pidió la auditora: comparar lado a lado). */
function abrirFactura(estacion: string, doc: DocumentoFirebird) {
  const num = encodeURIComponent(String(doc.NumeroDocumento ?? ""));
  const est = encodeURIComponent(estacion);
  window.open(`/consultas/factura?est=${est}&num=${num}`, "_blank", "noopener,width=900,height=800");
}

export function ConsultasPage() {
  const [estacion, setEstacion] = useState("");
  const [tipo, setTipo] = useState("");
  const [desde, setDesde] = useState("");
  const [hasta, setHasta] = useState("");
  const [codigo, setCodigo] = useState("");

  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [docs, setDocs] = useState<DocumentoFirebird[] | null>(null);

  const { data: estaciones } = useQuery({
    queryKey: ["estaciones"],
    queryFn: estacionesService.getAll,
    staleTime: 5 * 60_000,
  });

  const buscar = async () => {
    if (!estacion) {
      setError("Elija una estación.");
      return;
    }
    setCargando(true);
    setError(null);
    setDocs(null);
    try {
      const s: SolicitudConsulta = {
        codigoEstacion: estacion,
        tipoDocumento: tipo || null,
        fechaDesde: desde || null,
        fechaHasta: hasta ? `${hasta}T23:59:59` : null,
        codigo: codigo.trim() || null,
        limite: 200,
      };
      setDocs(await consultasService.consultarDocumentos(s));
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : "No se pudo consultar la estación.");
    } finally {
      setCargando(false);
    }
  };

  return (
    <div className="space-y-5">
      <div>
        <h1 className="flex items-center gap-2 text-2xl font-bold text-foreground">
          <FileSearch size={24} /> Consultas en vivo
        </h1>
        <p className="text-sm text-muted-foreground">
          Busca documentos directamente en la base de la estación (en vivo, solo lectura). Filtra por tipo,
          rango de fechas y un código que coincide con RUC, placa, cliente o número de documento.
        </p>
      </div>

      <div className="rounded-xl border border-border bg-card p-4">
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
          <label className="space-y-1">
            <span className="text-xs font-medium text-muted-foreground">Estación</span>
            <select className={`${inputClass} w-full`} value={estacion} onChange={(e) => setEstacion(e.target.value)}>
              <option value="">Elija una estación…</option>
              {(estaciones ?? []).map((es) => (
                <option key={es.codigo} value={es.codigo}>
                  {es.nombre} ({es.codigo})
                </option>
              ))}
            </select>
          </label>
          <label className="space-y-1">
            <span className="text-xs font-medium text-muted-foreground">Tipo de documento</span>
            <select className={`${inputClass} w-full`} value={tipo} onChange={(e) => setTipo(e.target.value)}>
              {TIPOS.map((t) => (
                <option key={t.v} value={t.v}>
                  {t.l}
                </option>
              ))}
            </select>
          </label>
          <label className="space-y-1">
            <span className="text-xs font-medium text-muted-foreground">RUC / placa / cliente / n.º de documento</span>
            <input
              className={`${inputClass} w-full`}
              value={codigo}
              onChange={(e) => setCodigo(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && buscar()}
              placeholder="p. ej. 1790012345001"
            />
          </label>
          <label className="space-y-1">
            <span className="text-xs font-medium text-muted-foreground">Desde</span>
            <input type="date" className={`${inputClass} w-full`} value={desde} onChange={(e) => setDesde(e.target.value)} />
          </label>
          <label className="space-y-1">
            <span className="text-xs font-medium text-muted-foreground">Hasta</span>
            <input type="date" className={`${inputClass} w-full`} value={hasta} onChange={(e) => setHasta(e.target.value)} />
          </label>
          <div className="flex items-end">
            <button
              onClick={buscar}
              disabled={cargando}
              className="inline-flex w-full items-center justify-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary/90 disabled:opacity-60"
            >
              <Search size={16} /> {cargando ? "Consultando…" : "Buscar"}
            </button>
          </div>
        </div>
      </div>

      {cargando && (
        <div className="flex flex-col items-center gap-2 py-12 text-muted-foreground">
          <Spinner size="lg" />
          <p className="text-sm">Consultando la estación en vivo (puede tardar unos segundos)…</p>
        </div>
      )}

      {error && !cargando && (
        <div className="flex items-start gap-2 rounded-lg border border-risk-high/40 bg-risk-high/10 p-3 text-sm text-risk-high">
          <AlertTriangle size={18} className="mt-0.5 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      {docs && !cargando && (
        <div className="rounded-xl border border-border bg-card">
          <div className="border-b border-border px-4 py-2.5 text-sm text-muted-foreground">
            {docs.length} documento(s) encontrado(s)
          </div>
          {docs.length === 0 ? (
            <EmptyState
              icon={<SearchX size={40} />}
              title="Sin resultados"
              description="No hay documentos que coincidan con esos filtros en la estación."
            />
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-muted-foreground">
                    <th className="px-3 py-2">N.º documento</th>
                    <th className="px-3 py-2">Fecha</th>
                    <th className="px-3 py-2">Tipo</th>
                    <th className="px-3 py-2">Cliente</th>
                    <th className="px-3 py-2">RUC</th>
                    <th className="px-3 py-2">Placa</th>
                    <th className="px-3 py-2">Turno</th>
                    <th className="px-3 py-2 text-right">Total</th>
                    <th className="px-3 py-2"></th>
                  </tr>
                </thead>
                <tbody>
                  {docs.map((d, i) => (
                    <tr key={`${d.NumeroDocumento}-${i}`} className="border-b border-border/60 hover:bg-muted/40">
                      <td className="px-3 py-2 font-mono">{texto(d.NumeroDocumento)}</td>
                      <td className="px-3 py-2">{fecha(d.Fecha)}</td>
                      <td className="px-3 py-2">{texto(d.TipoDocumento)}</td>
                      <td className="px-3 py-2">{texto(d.Cliente)}</td>
                      <td className="px-3 py-2 font-mono">{texto(d.Ruc)}</td>
                      <td className="px-3 py-2 font-mono">{texto(d.Placa)}</td>
                      <td className="px-3 py-2">{texto(d.NumeroTurno)}</td>
                      <td className="px-3 py-2 text-right font-medium">{money(d.TotalNeto)}</td>
                      <td className="px-3 py-2">
                        <button
                          onClick={() => abrirFactura(estacion, d)}
                          className="inline-flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs hover:bg-muted"
                          title="Abrir la factura completa en una ventana nueva"
                        >
                          <ExternalLink size={13} /> Ver factura
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
