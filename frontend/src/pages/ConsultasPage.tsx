import { useEffect, useRef, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useSearchParams } from "react-router-dom";
import { estacionesService } from "@/services/estaciones.service";
import {
  consultasService,
  type DocumentoFirebird,
  type SolicitudConsulta,
} from "@/services/consultas.service";
import { Spinner } from "@/components/ui/Spinner";
import { EmptyState } from "@/components/ui/EmptyState";
import {
  FileSearch,
  Search,
  ExternalLink,
  SearchX,
  AlertTriangle,
  Printer,
  X,
} from "lucide-react";

const inputClass =
  "rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40";

// color-scheme:dark vuelve VISIBLE el icono nativo del calendario sobre el input oscuro (la auditora
// reportó que "los botones de fecha se desaparecieron porque están negros").
const dateInputClass = `${inputClass} w-full [color-scheme:dark]`;

const TIPOS = [
  { v: "", l: "Todos los tipos" },
  { v: "FV", l: "Factura de venta (FV)" },
  { v: "DV", l: "Devolución / nota de crédito (DV)" },
  { v: "EB", l: "Egreso de bodega (EB)" },
];

const TIPO_LABEL: Record<string, string> = {
  FV: "Factura de venta (FV)",
  DV: "Devolución / nota de crédito (DV)",
  EB: "Egreso de bodega (EB)",
};

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
  window.open(`/consultas/factura?est=${est}&num=${num}`, "_blank", "noopener,width=960,height=860");
}

/**
 * Abre una consulta RELACIONADA en una ventana nueva, filtrada por un valor (RUC, placa, cliente o
 * despachador) de la misma estación. Es el comportamiento "ERP" que pidió la auditora: clic en el dato
 * → ver todo lo relacionado en vivo, sin perder la pantalla anterior (comparar lado a lado).
 */
function abrirConsultaRelacionada(estacion: string, valor: string) {
  const v = valor.trim();
  if (!v || !estacion) return;
  const qs = new URLSearchParams({ est: estacion, codigo: v });
  window.open(`/consultas?${qs.toString()}`, "_blank", "noopener,width=1100,height=860");
}

export function ConsultasPage() {
  const [searchParams, setSearchParams] = useSearchParams();

  const [estacion, setEstacion] = useState(searchParams.get("est") ?? "");
  const [tipo, setTipo] = useState(searchParams.get("tipo") ?? "");
  const [desde, setDesde] = useState(searchParams.get("desde") ?? "");
  const [hasta, setHasta] = useState(searchParams.get("hasta") ?? "");
  const [codigo, setCodigo] = useState(searchParams.get("codigo") ?? "");

  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [docs, setDocs] = useState<DocumentoFirebird[] | null>(null);
  // Filtros con los que se obtuvo el resultado actual (para el encabezado de impresión y el resumen).
  const [filtrosAplicados, setFiltrosAplicados] = useState<SolicitudConsulta | null>(null);

  const { data: estaciones } = useQuery({
    queryKey: ["estaciones"],
    queryFn: estacionesService.getAll,
    staleTime: 5 * 60_000,
  });

  const buscar = async (
    override?: Partial<{ estacion: string; tipo: string; desde: string; hasta: string; codigo: string }>,
  ) => {
    const est = override?.estacion ?? estacion;
    const ti = override?.tipo ?? tipo;
    const de = override?.desde ?? desde;
    const ha = override?.hasta ?? hasta;
    const co = (override?.codigo ?? codigo).trim();

    if (!est) {
      setError("Elija una estación.");
      return;
    }
    setCargando(true);
    setError(null);
    setDocs(null);

    // Mantener la consulta en la URL: así la pantalla es enlazable y se puede abrir en ventana nueva.
    const qs = new URLSearchParams({ est });
    if (ti) qs.set("tipo", ti);
    if (de) qs.set("desde", de);
    if (ha) qs.set("hasta", ha);
    if (co) qs.set("codigo", co);
    setSearchParams(qs, { replace: true });

    try {
      const s: SolicitudConsulta = {
        codigoEstacion: est,
        tipoDocumento: ti || null,
        fechaDesde: de || null,
        fechaHasta: ha ? `${ha}T23:59:59` : null,
        codigo: co || null,
        limite: 500,
      };
      setDocs(await consultasService.consultarDocumentos(s));
      setFiltrosAplicados(s);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : "No se pudo consultar la estación.");
    } finally {
      setCargando(false);
    }
  };

  // Autobúsqueda al entrar con parámetros (deep-link desde una alerta / otra consulta). Una sola vez.
  const yaAutobuscado = useRef(false);
  useEffect(() => {
    if (yaAutobuscado.current) return;
    const est = searchParams.get("est");
    const tieneFiltro =
      searchParams.get("codigo") || searchParams.get("tipo") || searchParams.get("desde") || searchParams.get("hasta");
    if (est && tieneFiltro) {
      yaAutobuscado.current = true;
      void buscar({
        estacion: est,
        tipo: searchParams.get("tipo") ?? "",
        desde: searchParams.get("desde") ?? "",
        hasta: searchParams.get("hasta") ?? "",
        codigo: searchParams.get("codigo") ?? "",
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const limpiar = () => {
    setTipo("");
    setDesde("");
    setHasta("");
    setCodigo("");
    setDocs(null);
    setError(null);
    setFiltrosAplicados(null);
    setSearchParams(estacion ? new URLSearchParams({ est: estacion }) : new URLSearchParams(), { replace: true });
  };

  const nombreEstacion =
    (estaciones ?? []).find((e) => e.codigo === (filtrosAplicados?.codigoEstacion ?? estacion))?.nombre ?? "";

  const resumenFiltros: { etiqueta: string; valor: string }[] = filtrosAplicados
    ? [
        { etiqueta: "Estación", valor: `${nombreEstacion} (${filtrosAplicados.codigoEstacion})` },
        ...(filtrosAplicados.tipoDocumento
          ? [{ etiqueta: "Tipo", valor: TIPO_LABEL[filtrosAplicados.tipoDocumento] ?? filtrosAplicados.tipoDocumento }]
          : []),
        ...(filtrosAplicados.codigo ? [{ etiqueta: "Búsqueda", valor: filtrosAplicados.codigo }] : []),
        ...(filtrosAplicados.fechaDesde ? [{ etiqueta: "Desde", valor: String(filtrosAplicados.fechaDesde).slice(0, 10) }] : []),
        ...(filtrosAplicados.fechaHasta ? [{ etiqueta: "Hasta", valor: String(filtrosAplicados.fechaHasta).slice(0, 10) }] : []),
      ]
    : [];

  return (
    <div className="space-y-5 print:bg-white print:text-black">
      <div className="flex flex-wrap items-start justify-between gap-3 print:hidden">
        <div>
          <h1 className="flex items-center gap-2 text-2xl font-bold text-foreground">
            <FileSearch size={24} /> Consultas en vivo
          </h1>
          <p className="max-w-3xl text-sm text-muted-foreground">
            Busca documentos directamente en la base de la estación (en vivo, solo lectura). Filtra por tipo,
            rango de fechas y un código que coincide con RUC, placa, cliente, despachador o número de documento.
            Haz clic en un RUC, placa, cliente o despachador para ver todo lo relacionado en una ventana nueva.
          </p>
        </div>
        {docs && docs.length > 0 && (
          <button
            onClick={() => window.print()}
            className="inline-flex shrink-0 items-center gap-2 rounded-md border border-border px-3 py-2 text-sm font-medium hover:bg-muted"
            title="Imprimir / guardar como PDF lo que estás viendo con estos filtros"
          >
            <Printer size={16} /> Imprimir
          </button>
        )}
      </div>

      {/* Filtros */}
      <div className="rounded-xl border border-border bg-card p-4 print:hidden">
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
            <span className="text-xs font-medium text-muted-foreground">RUC / placa / cliente / despachador / n.º doc</span>
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
            <input type="date" className={dateInputClass} value={desde} onChange={(e) => setDesde(e.target.value)} />
          </label>
          <label className="space-y-1">
            <span className="text-xs font-medium text-muted-foreground">Hasta</span>
            <input type="date" className={dateInputClass} value={hasta} onChange={(e) => setHasta(e.target.value)} />
          </label>
          <div className="flex items-end gap-2">
            <button
              onClick={() => buscar()}
              disabled={cargando}
              className="inline-flex flex-1 items-center justify-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary/90 disabled:opacity-60"
            >
              <Search size={16} /> {cargando ? "Consultando…" : "Buscar"}
            </button>
            {(tipo || desde || hasta || codigo) && (
              <button
                onClick={limpiar}
                className="inline-flex items-center justify-center gap-1 rounded-md border border-border px-3 py-2 text-sm hover:bg-muted"
                title="Limpiar los filtros"
              >
                <X size={15} /> Limpiar
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Encabezado SOLO para impresión: muestra qué filtros se aplicaron (lo que pidió la auditora). */}
      {filtrosAplicados && (
        <div className="hidden print:block">
          <h1 className="text-xl font-bold">PetrolRíos — Consulta de documentos</h1>
          <p className="mt-1 text-sm">{resumenFiltros.map((f) => `${f.etiqueta}: ${f.valor}`).join("  ·  ")}</p>
          <p className="text-xs">
            {docs?.length ?? 0} documento(s) · generado {new Date().toLocaleString("es-EC")}
          </p>
          <hr className="my-2" />
        </div>
      )}

      {cargando && (
        <div className="flex flex-col items-center gap-2 py-12 text-muted-foreground">
          <Spinner size="lg" />
          <p className="text-sm">Consultando la estación en vivo (puede tardar unos segundos)…</p>
        </div>
      )}

      {error && !cargando && (
        <div className="flex items-start gap-2 rounded-lg border border-risk-high/40 bg-risk-high/10 p-3 text-sm text-risk-high print:hidden">
          <AlertTriangle size={18} className="mt-0.5 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      {docs && !cargando && (
        <div className="rounded-xl border border-border bg-card print:border-0">
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border px-4 py-2.5 text-sm text-muted-foreground print:hidden">
            <span>{docs.length} documento(s) encontrado(s)</span>
            {resumenFiltros.length > 0 && (
              <span className="flex flex-wrap items-center gap-1">
                {resumenFiltros.map((f) => (
                  <span key={f.etiqueta} className="rounded-full bg-muted px-2 py-0.5 text-xs">
                    <span className="text-muted-foreground">{f.etiqueta}:</span>{" "}
                    <span className="font-medium text-foreground">{f.valor}</span>
                  </span>
                ))}
              </span>
            )}
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
                    <th className="px-3 py-2">Despachador</th>
                    <th className="px-3 py-2">Turno</th>
                    <th className="px-3 py-2 text-right">Total</th>
                    <th className="px-3 py-2 print:hidden"></th>
                  </tr>
                </thead>
                <tbody>
                  {docs.map((d, i) => (
                    <tr
                      key={`${d.NumeroDocumento}-${i}`}
                      className="border-b border-border/60 odd:bg-muted/20 hover:bg-muted/40"
                    >
                      <td className="px-3 py-2 font-mono">{texto(d.NumeroDocumento)}</td>
                      <td className="whitespace-nowrap px-3 py-2">{fecha(d.Fecha)}</td>
                      <td className="px-3 py-2">{texto(d.TipoDocumento)}</td>
                      <td className="px-3 py-2">
                        <CeldaRelacionada estacion={estacion} valor={d.Cliente} />
                      </td>
                      <td className="px-3 py-2 font-mono">
                        <CeldaRelacionada estacion={estacion} valor={d.Ruc} />
                      </td>
                      <td className="px-3 py-2 font-mono">
                        <CeldaRelacionada estacion={estacion} valor={d.Placa} />
                      </td>
                      <td className="px-3 py-2 font-mono">
                        <CeldaRelacionada estacion={estacion} valor={d.Vendedor} />
                      </td>
                      <td className="px-3 py-2">{texto(d.NumeroTurno)}</td>
                      <td className="px-3 py-2 text-right font-medium">{money(d.TotalNeto)}</td>
                      <td className="px-3 py-2 print:hidden">
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

/**
 * Celda de un valor (cliente, RUC, placa, despachador) que, al hacer clic, abre una consulta
 * relacionada por ese valor en una ventana nueva. Si está vacío, muestra un guion sin enlace.
 */
function CeldaRelacionada({ estacion, valor }: { estacion: string; valor: unknown }) {
  const v = texto(valor);
  if (v === "—" || !estacion) return <span className="text-muted-foreground">{v}</span>;
  return (
    <button
      type="button"
      onClick={() => abrirConsultaRelacionada(estacion, v)}
      title={`Ver todo lo relacionado con "${v}" en una ventana nueva`}
      className="inline-flex items-center gap-1 text-left font-medium text-primary hover:underline print:text-black print:no-underline"
    >
      {v}
    </button>
  );
}
