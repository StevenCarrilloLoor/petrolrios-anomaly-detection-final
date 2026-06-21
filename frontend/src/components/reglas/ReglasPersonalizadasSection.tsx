import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { reglasPersonalizadasService } from "@/services/reglasPersonalizadas.service";
import type {
  CondicionRegla,
  AgregacionRegla,
  ReglaPersonalizadaResponse,
  GuardarReglaPersonalizadaRequest,
  BacktestReglaResponse,
} from "@/types/reglaPersonalizada";
import { Card, CardContent, CardHeader } from "@/components/ui/Card";
import { Skeleton } from "@/components/ui/Skeleton";
import {
  Plus,
  Trash2,
  Pencil,
  Sparkles,
  X,
  Save,
  SigmaSquare,
  Code2,
  SlidersHorizontal,
  CheckCircle2,
  AlertCircle,
  FlaskConical,
} from "lucide-react";

const inputClass =
  "rounded-md border border-border bg-background px-2.5 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40";

const OPERADOR_LABELS: Record<string, string> = {
  ">": "mayor que",
  ">=": "mayor o igual que",
  "<": "menor que",
  "<=": "menor o igual que",
  "=": "igual a",
  "!=": "distinto de",
  contiene: "contiene",
  noContiene: "no contiene",
  vacio: "está vacío",
  noVacio: "no está vacío",
};

const FUNCION_LABELS: Record<string, string> = {
  Conteo: "Conteo de registros",
  Suma: "Suma de un campo",
  Promedio: "Promedio de un campo",
};

interface FormularioRegla {
  nombre: string;
  descripcion: string;
  fuenteDatos: string;
  modoAvanzado: boolean;
  expresion: string;
  condiciones: CondicionRegla[];
  usarAgregacion: boolean;
  agregacion: AgregacionRegla;
  riesgoBase: number;
  ambito: "Operativa" | "Auditoria";
}

const formularioVacio = (fuente: string): FormularioRegla => ({
  nombre: "",
  descripcion: "",
  fuenteDatos: fuente,
  modoAvanzado: false,
  expresion: "",
  condiciones: [],
  usarAgregacion: false,
  agregacion: { agruparPor: "", funcion: "Conteo", campo: null, operador: ">", umbral: 1 },
  riesgoBase: 50,
  ambito: "Auditoria",
});

export function ReglasPersonalizadasSection() {
  const queryClient = useQueryClient();
  const [editando, setEditando] = useState<number | null>(null); // null=cerrado, 0=nueva, >0=editar
  const [form, setForm] = useState<FormularioRegla | null>(null);
  const [errores, setErrores] = useState<string[]>([]);
  const [backtest, setBacktest] = useState<BacktestReglaResponse | null>(null);

  const { data: catalogo } = useQuery({
    queryKey: ["reglas-personalizadas", "catalogo"],
    queryFn: reglasPersonalizadasService.getCatalogo,
    staleTime: Infinity,
  });

  const { data: reglas, isLoading } = useQuery({
    queryKey: ["reglas-personalizadas"],
    queryFn: reglasPersonalizadasService.getAll,
  });

  const invalidar = () => {
    void queryClient.invalidateQueries({ queryKey: ["reglas-personalizadas"] });
  };

  const guardarMutation = useMutation({
    mutationFn: (payload: { id: number; data: GuardarReglaPersonalizadaRequest }) =>
      payload.id === 0
        ? reglasPersonalizadasService.create(payload.data)
        : reglasPersonalizadasService.update(payload.id, payload.data),
    onSuccess: () => {
      invalidar();
      cerrarFormulario();
    },
    onError: (error: unknown) => {
      const respuesta = (error as { response?: { data?: { errores?: string[] } } })
        .response?.data?.errores;
      setErrores(respuesta ?? ["No se pudo guardar la regla. Revise los datos."]);
    },
  });

  const toggleMutation = useMutation({
    mutationFn: (regla: ReglaPersonalizadaResponse) =>
      reglasPersonalizadasService.update(regla.id, {
        nombre: regla.nombre,
        descripcion: regla.descripcion,
        fuenteDatos: regla.fuenteDatos,
        condiciones: regla.condiciones,
        agregacion: regla.agregacion,
        expresionAvanzada: regla.expresionAvanzada,
        riesgoBase: regla.riesgoBase,
        ambito: regla.ambito,
        activa: !regla.activa,
      }),
    onSuccess: invalidar,
  });

  const eliminarMutation = useMutation({
    mutationFn: (id: number) => reglasPersonalizadasService.delete(id),
    onSuccess: invalidar,
  });

  // Backtest / vista previa: corre la regla borrador contra los últimos días sin guardarla.
  const backtestMutation = useMutation({
    mutationFn: (data: GuardarReglaPersonalizadaRequest) =>
      reglasPersonalizadasService.backtest({ regla: data, dias: 7 }),
    onSuccess: (res) => setBacktest(res),
    onError: () =>
      setBacktest({
        valida: false,
        errores: ["No se pudo ejecutar la prueba. Intente de nuevo."],
        ventanaDias: 7,
        registrosEvaluados: 0,
        totalCoincidencias: 0,
        bajo: 0,
        medio: 0,
        alto: 0,
        critico: 0,
        muestra: [],
      }),
  });

  function abrirNueva() {
    if (!catalogo) return;
    setForm(formularioVacio(catalogo.fuentes[0]?.nombre ?? "Factura"));
    setEditando(0);
    setErrores([]);
  }

  function abrirEdicion(regla: ReglaPersonalizadaResponse) {
    setForm({
      nombre: regla.nombre,
      descripcion: regla.descripcion,
      fuenteDatos: regla.fuenteDatos,
      modoAvanzado: !!regla.expresionAvanzada,
      expresion: regla.expresionAvanzada ?? "",
      condiciones: [...regla.condiciones],
      usarAgregacion: regla.agregacion !== null,
      agregacion: regla.agregacion ?? {
        agruparPor: "",
        funcion: "Conteo",
        campo: null,
        operador: ">",
        umbral: 1,
      },
      riesgoBase: regla.riesgoBase,
      ambito: regla.ambito ?? "Auditoria",
    });
    setEditando(regla.id);
    setErrores([]);
  }

  function cerrarFormulario() {
    setEditando(null);
    setForm(null);
    setErrores([]);
    setBacktest(null);
  }

  /// Arma el payload de la regla a partir del formulario (lo comparten guardar y probar).
  function datosRegla(f: FormularioRegla): GuardarReglaPersonalizadaRequest {
    return {
      nombre: f.nombre,
      descripcion: f.descripcion,
      fuenteDatos: f.fuenteDatos,
      condiciones: f.modoAvanzado ? [] : f.condiciones,
      agregacion: f.usarAgregacion ? f.agregacion : null,
      expresionAvanzada: f.modoAvanzado ? f.expresion : null,
      riesgoBase: f.riesgoBase,
      ambito: f.ambito,
      activa: true,
    };
  }

  function probar() {
    if (!form) return;
    setBacktest(null);
    backtestMutation.mutate(datosRegla(form));
  }

  function guardar() {
    if (!form || editando === null) return;

    // Validación previa: evita persistir reglas malformadas (p. ej. una condición
    // "CAMPO >" sin valor, que el motor de detección no puede evaluar).
    const erroresValidacion: string[] = [];
    if (!form.nombre.trim()) {
      erroresValidacion.push("El nombre es obligatorio.");
    }
    if (form.modoAvanzado) {
      if (!form.expresion.trim()) {
        erroresValidacion.push("La expresión avanzada no puede estar vacía.");
      }
    } else {
      const requiereValor = (op: string) => op !== "vacio" && op !== "noVacio";
      form.condiciones.forEach((c, i) => {
        if (requiereValor(c.operador) && !String(c.valor).trim()) {
          erroresValidacion.push(
            `La condición ${i + 1} (${c.campo || "campo sin elegir"}) necesita un valor.`,
          );
        }
        if (!c.campo) {
          erroresValidacion.push(`La condición ${i + 1} no tiene un campo elegido.`);
        }
      });
      if (form.condiciones.length === 0 && !form.usarAgregacion) {
        erroresValidacion.push(
          "Agregue al menos una condición o active una agregación con umbral.",
        );
      }
    }
    if (form.usarAgregacion && !form.agregacion.agruparPor.trim()) {
      erroresValidacion.push("Elija el campo por el cual agrupar en la agregación.");
    }

    if (erroresValidacion.length > 0) {
      setErrores(erroresValidacion);
      return;
    }

    guardarMutation.mutate({ id: editando, data: datosRegla(form) });
  }

  const camposFuente = (fuente: string) =>
    catalogo?.fuentes.find((f) => f.nombre === fuente)?.campos ?? [];

  const operadoresPara = (fuente: string, campo: string) => {
    const info = camposFuente(fuente).find((c) => c.nombre === campo);
    if (!info || !catalogo) return [];
    return info.tipo === "numero" ? catalogo.operadoresNumero : catalogo.operadoresTexto;
  };

  return (
    <Card>
      <CardHeader
        title="Reglas personalizadas"
        subtitle="Cree sus propias reglas de negocio sin tocar código: filtre cualquier fuente de datos y, si lo necesita, agrupe y compare contra un umbral"
        action={
          <div className="flex items-center gap-3">
            <div className="rounded-lg bg-violet-500/10 p-2 text-violet-400">
              <Sparkles size={18} />
            </div>
            <button
              onClick={abrirNueva}
              disabled={!catalogo}
              className="flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
            >
              <Plus size={15} /> Nueva regla
            </button>
          </div>
        }
      />
      <CardContent className="p-0">
        {/* Builder */}
        {form && catalogo && (
          <div className="border-b border-border bg-muted/30 px-6 py-5">
            <h4 className="mb-4 text-sm font-semibold text-foreground">
              {editando === 0 ? "Nueva regla personalizada" : "Editar regla"}
            </h4>

            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <label className="space-y-1">
                <span className="text-xs font-medium text-muted-foreground">Nombre *</span>
                <input
                  value={form.nombre}
                  onChange={(e) => setForm({ ...form, nombre: e.target.value })}
                  placeholder="Ej.: Despachos nocturnos de alto valor"
                  className={`${inputClass} w-full`}
                />
              </label>
              <label className="space-y-1">
                <span className="text-xs font-medium text-muted-foreground">
                  Fuente de datos *
                </span>
                <select
                  value={form.fuenteDatos}
                  onChange={(e) =>
                    setForm({
                      ...formularioVacio(e.target.value),
                      nombre: form.nombre,
                      descripcion: form.descripcion,
                      modoAvanzado: form.modoAvanzado,
                      expresion: form.expresion,
                      riesgoBase: form.riesgoBase,
                      ambito: form.ambito,
                    })
                  }
                  className={`${inputClass} w-full`}
                >
                  {catalogo.fuentes.map((f) => (
                    <option key={f.nombre} value={f.nombre}>
                      {f.etiqueta}
                    </option>
                  ))}
                </select>
              </label>
              <label className="space-y-1 sm:col-span-2">
                <span className="text-xs font-medium text-muted-foreground">Descripción</span>
                <input
                  value={form.descripcion}
                  onChange={(e) => setForm({ ...form, descripcion: e.target.value })}
                  placeholder="Qué situación de negocio detecta esta regla"
                  className={`${inputClass} w-full`}
                />
              </label>
              <div className="space-y-1 sm:col-span-2">
                <span className="text-xs font-medium text-muted-foreground">
                  Carril de la alerta
                </span>
                <div className="flex gap-2 rounded-lg border border-border bg-background p-1">
                  <button
                    type="button"
                    onClick={() => setForm({ ...form, ambito: "Auditoria" })}
                    className={`flex-1 rounded-md px-3 py-2 text-sm font-medium transition-colors ${
                      form.ambito === "Auditoria"
                        ? "bg-primary text-primary-foreground"
                        : "text-muted-foreground hover:bg-muted"
                    }`}
                  >
                    Auditoría (fraude → central)
                  </button>
                  <button
                    type="button"
                    onClick={() => setForm({ ...form, ambito: "Operativa" })}
                    className={`flex-1 rounded-md px-3 py-2 text-sm font-medium transition-colors ${
                      form.ambito === "Operativa"
                        ? "bg-amber-500 text-white"
                        : "text-muted-foreground hover:bg-muted"
                    }`}
                  >
                    Operativa (problema de estación)
                  </button>
                </div>
              </div>
            </div>

            {/* Selector de modo: básico (visual) vs avanzado (expresión) */}
            <div className="mt-5 flex gap-2 rounded-lg border border-border bg-background p-1">
              <button
                onClick={() => setForm({ ...form, modoAvanzado: false })}
                className={`flex flex-1 items-center justify-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors ${
                  !form.modoAvanzado
                    ? "bg-primary text-primary-foreground"
                    : "text-muted-foreground hover:bg-muted"
                }`}
              >
                <SlidersHorizontal size={15} /> Modo básico (visual)
              </button>
              <button
                onClick={() => setForm({ ...form, modoAvanzado: true })}
                className={`flex flex-1 items-center justify-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors ${
                  form.modoAvanzado
                    ? "bg-violet-600 text-white"
                    : "text-muted-foreground hover:bg-muted"
                }`}
              >
                <Code2 size={15} /> Modo avanzado (expresión)
              </button>
            </div>

            {/* MODO AVANZADO: editor de expresión */}
            {form.modoAvanzado && (
              <EditorExpresion
                fuente={form.fuenteDatos}
                campos={camposFuente(form.fuenteDatos)}
                expresion={form.expresion}
                onChange={(expr) => setForm({ ...form, expresion: expr })}
              />
            )}

            {/* MODO BÁSICO: condiciones visuales */}
            {!form.modoAvanzado && (
            <div className="mt-5">
              <div className="mb-2 flex items-center justify-between">
                <span className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                  Condiciones (todas deben cumplirse)
                </span>
                <button
                  onClick={() =>
                    setForm({
                      ...form,
                      condiciones: [
                        ...form.condiciones,
                        {
                          campo: camposFuente(form.fuenteDatos)[0]?.nombre ?? "",
                          operador: ">",
                          valor: "",
                        },
                      ],
                    })
                  }
                  className="flex items-center gap-1 rounded-md border border-border px-2.5 py-1 text-xs hover:bg-muted"
                >
                  <Plus size={12} /> Agregar condición
                </button>
              </div>

              {form.condiciones.length === 0 && (
                <p className="rounded-md border border-dashed border-border px-3 py-2 text-xs text-muted-foreground">
                  Sin condiciones: se considerarán todos los registros de la fuente
                  (útil combinado con una agregación).
                </p>
              )}

              <div className="space-y-2">
                {form.condiciones.map((condicion, idx) => {
                  const campoInfo = camposFuente(form.fuenteDatos).find(
                    (c) => c.nombre === condicion.campo,
                  );
                  const sinValor =
                    condicion.operador === "vacio" || condicion.operador === "noVacio";
                  return (
                    <div key={idx} className="flex flex-wrap items-center gap-2">
                      <span className="w-10 text-right font-mono text-[10px] text-muted-foreground">
                        {idx === 0 ? "SI" : "Y"}
                      </span>
                      <select
                        value={condicion.campo}
                        onChange={(e) => {
                          const nuevas = [...form.condiciones];
                          const operadores = operadoresPara(form.fuenteDatos, e.target.value);
                          nuevas[idx] = {
                            campo: e.target.value,
                            operador: operadores[0] ?? ">",
                            valor: "",
                          };
                          setForm({ ...form, condiciones: nuevas });
                        }}
                        className={inputClass}
                      >
                        {camposFuente(form.fuenteDatos).map((c) => (
                          <option key={c.nombre} value={c.nombre}>
                            {c.etiqueta}
                          </option>
                        ))}
                      </select>
                      <select
                        value={condicion.operador}
                        onChange={(e) => {
                          const nuevas = [...form.condiciones];
                          nuevas[idx] = { ...condicion, operador: e.target.value };
                          setForm({ ...form, condiciones: nuevas });
                        }}
                        className={inputClass}
                      >
                        {operadoresPara(form.fuenteDatos, condicion.campo).map((op) => (
                          <option key={op} value={op}>
                            {OPERADOR_LABELS[op] ?? op}
                          </option>
                        ))}
                      </select>
                      {!sinValor && (
                        <input
                          value={condicion.valor}
                          onChange={(e) => {
                            const nuevas = [...form.condiciones];
                            nuevas[idx] = { ...condicion, valor: e.target.value };
                            setForm({ ...form, condiciones: nuevas });
                          }}
                          placeholder={campoInfo?.tipo === "numero" ? "0" : "valor"}
                          type={campoInfo?.tipo === "numero" ? "number" : "text"}
                          className={`${inputClass} w-32`}
                        />
                      )}
                      <button
                        onClick={() =>
                          setForm({
                            ...form,
                            condiciones: form.condiciones.filter((_, i) => i !== idx),
                          })
                        }
                        className="rounded-md p-1.5 text-muted-foreground hover:bg-risk-critical/10 hover:text-risk-critical"
                        title="Quitar condición"
                      >
                        <Trash2 size={14} />
                      </button>
                    </div>
                  );
                })}
              </div>
            </div>
            )}

            {/* Agregación (disponible en ambos modos) */}
            <div className="mt-5">
              <label className="flex items-center gap-2 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                <input
                  type="checkbox"
                  checked={form.usarAgregacion}
                  onChange={(e) => setForm({ ...form, usarAgregacion: e.target.checked })}
                  className="h-3.5 w-3.5"
                />
                <SigmaSquare size={14} />
                Agrupar y comparar contra un umbral (opcional)
              </label>

              {form.usarAgregacion && (
                <div className="mt-2 flex flex-wrap items-center gap-2 pl-6 text-sm">
                  <span className="text-xs text-muted-foreground">Agrupar por</span>
                  <select
                    value={form.agregacion.agruparPor}
                    onChange={(e) =>
                      setForm({
                        ...form,
                        agregacion: { ...form.agregacion, agruparPor: e.target.value },
                      })
                    }
                    className={inputClass}
                  >
                    <option value="">Seleccione…</option>
                    {camposFuente(form.fuenteDatos).map((c) => (
                      <option key={c.nombre} value={c.nombre}>
                        {c.etiqueta}
                      </option>
                    ))}
                  </select>
                  <select
                    value={form.agregacion.funcion}
                    onChange={(e) =>
                      setForm({
                        ...form,
                        agregacion: {
                          ...form.agregacion,
                          funcion: e.target.value,
                          campo: e.target.value === "Conteo" ? null : form.agregacion.campo,
                        },
                      })
                    }
                    className={inputClass}
                  >
                    {catalogo.funciones.map((f) => (
                      <option key={f} value={f}>
                        {FUNCION_LABELS[f] ?? f}
                      </option>
                    ))}
                  </select>
                  {form.agregacion.funcion !== "Conteo" && (
                    <select
                      value={form.agregacion.campo ?? ""}
                      onChange={(e) =>
                        setForm({
                          ...form,
                          agregacion: { ...form.agregacion, campo: e.target.value || null },
                        })
                      }
                      className={inputClass}
                    >
                      <option value="">Campo…</option>
                      {camposFuente(form.fuenteDatos)
                        .filter((c) => c.tipo === "numero")
                        .map((c) => (
                          <option key={c.nombre} value={c.nombre}>
                            {c.etiqueta}
                          </option>
                        ))}
                    </select>
                  )}
                  <select
                    value={form.agregacion.operador}
                    onChange={(e) =>
                      setForm({
                        ...form,
                        agregacion: { ...form.agregacion, operador: e.target.value },
                      })
                    }
                    className={inputClass}
                  >
                    {catalogo.operadoresNumero.map((op) => (
                      <option key={op} value={op}>
                        {OPERADOR_LABELS[op] ?? op}
                      </option>
                    ))}
                  </select>
                  <input
                    type="number"
                    value={form.agregacion.umbral}
                    onChange={(e) =>
                      setForm({
                        ...form,
                        agregacion: {
                          ...form.agregacion,
                          umbral: parseFloat(e.target.value) || 0,
                        },
                      })
                    }
                    className={`${inputClass} w-24`}
                  />
                </div>
              )}
            </div>

            {/* Riesgo base */}
            <div className="mt-5 flex items-center gap-3">
              <span className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                Riesgo base
              </span>
              <input
                type="range"
                min={1}
                max={100}
                value={form.riesgoBase}
                onChange={(e) => setForm({ ...form, riesgoBase: parseInt(e.target.value) })}
                className="w-48"
              />
              <span
                className={`rounded-full px-2.5 py-0.5 font-mono text-xs font-bold ${
                  form.riesgoBase > 75
                    ? "bg-risk-critical/15 text-risk-critical"
                    : form.riesgoBase > 50
                      ? "bg-risk-high/15 text-risk-high"
                      : form.riesgoBase > 25
                        ? "bg-risk-medium/15 text-risk-medium"
                        : "bg-risk-low/15 text-risk-low"
                }`}
              >
                {form.riesgoBase}
              </span>
              <span className="text-xs text-muted-foreground">
                (el motor de scoring aplica multiplicadores por monto y reincidencia)
              </span>
            </div>

            {errores.length > 0 && (
              <div className="mt-4 rounded-md border border-risk-critical/30 bg-risk-critical/10 px-4 py-3">
                {errores.map((e, i) => (
                  <p key={i} className="text-sm text-risk-critical">
                    • {e}
                  </p>
                ))}
              </div>
            )}

            <div className="mt-5 flex flex-wrap gap-3">
              <button
                onClick={guardar}
                disabled={guardarMutation.isPending || !form.nombre.trim()}
                className="flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
              >
                <Save size={14} />
                {editando === 0 ? "Crear regla" : "Guardar cambios"}
              </button>
              <button
                onClick={probar}
                disabled={backtestMutation.isPending}
                className="flex items-center gap-2 rounded-md border border-primary/40 px-4 py-2 text-sm font-medium text-primary hover:bg-primary/10 disabled:opacity-50"
                title="Prueba la regla contra los datos reales de los últimos 7 días, sin guardarla"
              >
                <FlaskConical size={14} />
                {backtestMutation.isPending ? "Probando…" : "Probar regla"}
              </button>
              <button
                onClick={cerrarFormulario}
                className="flex items-center gap-2 rounded-md border border-border px-4 py-2 text-sm hover:bg-muted"
              >
                <X size={14} /> Cancelar
              </button>
            </div>

            {backtest && (
              <div className="mt-4 rounded-md border border-border bg-muted/40 p-4">
                {!backtest.valida ? (
                  <div className="flex items-start gap-2 text-sm text-risk-critical">
                    <AlertCircle size={16} className="mt-0.5 shrink-0" />
                    <div>
                      <p className="font-medium">No se pudo probar la regla:</p>
                      {backtest.errores.map((e, i) => (
                        <p key={i}>• {e}</p>
                      ))}
                    </div>
                  </div>
                ) : (
                  <>
                    <p className="text-sm">
                      En los últimos{" "}
                      <span className="font-semibold">{backtest.ventanaDias} días</span> esta regla
                      habría generado{" "}
                      <span className="font-bold text-primary">{backtest.totalCoincidencias}</span>{" "}
                      alerta(s) sobre {backtest.registrosEvaluados} registro(s) evaluado(s).
                    </p>
                    {backtest.totalCoincidencias > 0 && (
                      <div className="mt-2 flex flex-wrap gap-2 text-xs font-medium">
                        <span className="rounded-full bg-risk-critical/15 px-2.5 py-0.5 text-risk-critical">
                          Crítico {backtest.critico}
                        </span>
                        <span className="rounded-full bg-risk-high/15 px-2.5 py-0.5 text-risk-high">
                          Alto {backtest.alto}
                        </span>
                        <span className="rounded-full bg-risk-medium/15 px-2.5 py-0.5 text-risk-medium">
                          Medio {backtest.medio}
                        </span>
                        <span className="rounded-full bg-risk-low/15 px-2.5 py-0.5 text-risk-low">
                          Bajo {backtest.bajo}
                        </span>
                      </div>
                    )}
                    {backtest.muestra.length > 0 && (
                      <div className="mt-3 space-y-1">
                        <p className="text-xs font-semibold text-muted-foreground">
                          Ejemplos de coincidencias:
                        </p>
                        {backtest.muestra.map((m, i) => (
                          <p key={i} className="text-xs text-muted-foreground">
                            <span className="font-mono font-semibold">
                              {m.nivel} · {m.score}
                            </span>{" "}
                            — {m.descripcion}
                            {m.estacion ? ` · ${m.estacion}` : ""}
                          </p>
                        ))}
                      </div>
                    )}
                    {backtest.totalCoincidencias === 0 && (
                      <p className="mt-2 text-xs text-muted-foreground">
                        No coincidió con datos recientes: puede que el umbral sea muy estricto o que
                        no haya datos de esa fuente en la ventana de prueba.
                      </p>
                    )}
                  </>
                )}
              </div>
            )}
          </div>
        )}

        {/* Lista */}
        {isLoading ? (
          <div className="space-y-2 p-6">
            <Skeleton className="h-12" />
            <Skeleton className="h-12" />
          </div>
        ) : (reglas ?? []).length === 0 && !form ? (
          <p className="px-6 py-8 text-center text-sm text-muted-foreground">
            Aún no hay reglas personalizadas. Cree la primera con "Nueva regla" —
            por ejemplo: <span className="italic">facturas en efectivo de más de $500</span> o{" "}
            <span className="italic">más de 5 despachos del mismo cliente en un ciclo</span>.
          </p>
        ) : (
          (reglas ?? []).map((regla, idx) => (
            <div
              key={regla.id}
              className={`flex flex-col gap-3 px-6 py-4 sm:flex-row sm:items-center sm:justify-between ${
                idx > 0 || form ? "border-t border-border" : ""
              } ${!regla.activa ? "opacity-60" : ""}`}
            >
              <div className="min-w-0 flex-1">
                <div className="flex flex-wrap items-center gap-2">
                  <p className="font-medium text-foreground">{regla.nombre}</p>
                  {regla.ambito === "Operativa" ? (
                    <span className="inline-flex items-center rounded-full bg-amber-500/10 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-amber-600 dark:text-amber-400">
                      Operativa
                    </span>
                  ) : (
                    <span className="inline-flex items-center rounded-full bg-primary/10 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-primary">
                      Auditoría
                    </span>
                  )}
                </div>
                <p className="mt-0.5 text-xs text-muted-foreground">
                  {regla.descripcion || resumenRegla(regla)}
                </p>
                <p className="mt-1 font-mono text-[10px] text-muted-foreground/60">
                  {resumenRegla(regla)}
                </p>
              </div>
              <div className="flex shrink-0 items-center gap-3">
                <span className="rounded-full bg-muted px-2 py-0.5 font-mono text-[10px] text-muted-foreground">
                  riesgo {regla.riesgoBase}
                </span>
                <button
                  onClick={() => abrirEdicion(regla)}
                  className="rounded-md border border-border p-2 text-muted-foreground hover:border-primary hover:text-primary"
                  title="Editar"
                >
                  <Pencil size={14} />
                </button>
                <button
                  onClick={() => {
                    if (confirm(`¿Eliminar la regla "${regla.nombre}"?`))
                      eliminarMutation.mutate(regla.id);
                  }}
                  className="rounded-md border border-border p-2 text-muted-foreground hover:border-risk-critical hover:text-risk-critical"
                  title="Eliminar"
                >
                  <Trash2 size={14} />
                </button>
                <button
                  role="switch"
                  aria-checked={regla.activa}
                  onClick={() => toggleMutation.mutate(regla)}
                  className={`relative h-6 w-11 rounded-full transition-colors ${
                    regla.activa ? "bg-risk-low" : "bg-muted-foreground/30"
                  }`}
                  title={regla.activa ? "Desactivar" : "Activar"}
                >
                  <span
                    className={`absolute top-0.5 h-5 w-5 rounded-full bg-white shadow transition-all ${
                      regla.activa ? "left-[22px]" : "left-0.5"
                    }`}
                  />
                </button>
                <span
                  className={`w-14 text-xs font-semibold ${
                    regla.activa ? "text-risk-low" : "text-muted-foreground"
                  }`}
                >
                  {regla.activa ? "Activa" : "Inactiva"}
                </span>
              </div>
            </div>
          ))
        )}
      </CardContent>
    </Card>
  );
}

function resumenRegla(regla: ReglaPersonalizadaResponse): string {
  const filtro = regla.expresionAvanzada
    ? regla.expresionAvanzada
    : regla.condiciones
        .map((c) => `${c.campo} ${c.operador} ${c.valor}`.trim())
        .join(" Y ") || "todos los registros";
  const agregacion = regla.agregacion
    ? ` → ${regla.agregacion.funcion}${regla.agregacion.campo ? `(${regla.agregacion.campo})` : ""} por ${regla.agregacion.agruparPor} ${regla.agregacion.operador} ${regla.agregacion.umbral}`
    : "";
  const prefijo = regla.expresionAvanzada ? "⚡ " : "";
  return `${prefijo}${regla.fuenteDatos}: ${filtro}${agregacion}`;
}

/// <summary>Editor del modo avanzado: expresión + referencia de campos/operadores + validación en vivo.</summary>
function EditorExpresion({
  fuente,
  campos,
  expresion,
  onChange,
}: {
  fuente: string;
  campos: { nombre: string; etiqueta: string; tipo: string }[];
  expresion: string;
  onChange: (expr: string) => void;
}) {
  const [validacion, setValidacion] = useState<{
    valida: boolean;
    errores: string[];
  } | null>(null);
  const [validando, setValidando] = useState(false);

  async function validar() {
    if (!expresion.trim()) return;
    setValidando(true);
    try {
      const r = await reglasPersonalizadasService.validarExpresion(fuente, expresion);
      setValidacion(r);
    } catch {
      setValidacion({ valida: false, errores: ["No se pudo validar."] });
    }
    setValidando(false);
  }

  function insertar(texto: string) {
    onChange((expresion ? expresion + " " : "") + texto);
    setValidacion(null);
  }

  return (
    <div className="mt-5 space-y-3">
      <div className="rounded-lg border border-violet-500/30 bg-violet-500/5 p-3 text-xs text-muted-foreground">
        Escriba una <span className="font-medium text-foreground">expresión lógica</span> que
        combine campos con operadores. Ejemplos:
        <span className="ml-1 font-mono text-violet-300">
          TotalNeto &gt; 400 &amp;&amp; CodigoPago == 'EF'
        </span>
        {" · "}
        <span className="font-mono text-violet-300">Descuento / Subtotal &gt; 0.1</span>
        {" · "}
        <span className="font-mono text-violet-300">vacio(Placa) || longitud(RucCliente) &lt; 10</span>
      </div>

      <textarea
        value={expresion}
        onChange={(e) => {
          onChange(e.target.value);
          setValidacion(null);
        }}
        rows={3}
        placeholder="Ej.: TotalNeto > 400 && (CodigoPago == 'EF' || Descuento / Subtotal > 0.15)"
        className="w-full rounded-md border border-border bg-[#0a0f1c] px-3 py-2.5 font-mono text-sm text-foreground focus:border-violet-500 focus:outline-none"
      />

      <div className="flex flex-wrap items-center gap-2">
        <button
          onClick={validar}
          disabled={validando || !expresion.trim()}
          className="flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs hover:bg-muted disabled:opacity-50"
        >
          <CheckCircle2 size={13} /> Validar expresión
        </button>
        {validacion?.valida && (
          <span className="flex items-center gap-1 text-xs font-medium text-risk-low">
            <CheckCircle2 size={13} /> Expresión válida
          </span>
        )}
      </div>

      {validacion && !validacion.valida && (
        <div className="rounded-md border border-risk-critical/30 bg-risk-critical/10 px-3 py-2">
          {validacion.errores.map((e, i) => (
            <p key={i} className="flex items-start gap-1.5 text-xs text-risk-critical">
              <AlertCircle size={13} className="mt-0.5 shrink-0" /> {e}
            </p>
          ))}
        </div>
      )}

      {/* Referencia: campos, operadores y funciones disponibles */}
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
        <div className="rounded-lg border border-border bg-background p-3">
          <p className="mb-1.5 text-[10px] font-semibold uppercase tracking-wide text-muted-foreground">
            Campos ({fuente})
          </p>
          <div className="flex flex-wrap gap-1">
            {campos.map((c) => (
              <button
                key={c.nombre}
                onClick={() => insertar(c.nombre)}
                title={c.etiqueta}
                className="rounded bg-muted px-1.5 py-0.5 font-mono text-[10px] text-foreground hover:bg-primary hover:text-primary-foreground"
              >
                {c.nombre}
              </button>
            ))}
          </div>
        </div>
        <div className="rounded-lg border border-border bg-background p-3">
          <p className="mb-1.5 text-[10px] font-semibold uppercase tracking-wide text-muted-foreground">
            Operadores
          </p>
          <div className="flex flex-wrap gap-1">
            {[">", ">=", "<", "<=", "==", "!=", "&&", "||", "!", "+", "-", "*", "/", "(", ")"].map(
              (op) => (
                <button
                  key={op}
                  onClick={() => insertar(op)}
                  className="rounded bg-muted px-1.5 py-0.5 font-mono text-[10px] text-foreground hover:bg-primary hover:text-primary-foreground"
                >
                  {op}
                </button>
              ),
            )}
          </div>
        </div>
        <div className="rounded-lg border border-border bg-background p-3">
          <p className="mb-1.5 text-[10px] font-semibold uppercase tracking-wide text-muted-foreground">
            Funciones
          </p>
          <div className="flex flex-wrap gap-1">
            {[
              "vacio()",
              "contiene(,)",
              "empieza(,)",
              "abs()",
              "longitud()",
              "minusculas()",
              "redondear()",
            ].map((fn) => (
              <button
                key={fn}
                onClick={() => insertar(fn)}
                className="rounded bg-muted px-1.5 py-0.5 font-mono text-[10px] text-foreground hover:bg-primary hover:text-primary-foreground"
              >
                {fn}
              </button>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
