import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { reglasService } from "@/services/reglas.service";
import { Spinner } from "@/components/ui/Spinner";
import type { ReglaDeteccionResponse } from "@/types/regla";
import { Settings, Plus, Save, X } from "lucide-react";

export function ReglasPage() {
  const queryClient = useQueryClient();
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editValue, setEditValue] = useState("");
  const [showCreate, setShowCreate] = useState(false);

  const { data: reglas, isLoading } = useQuery({
    queryKey: ["reglas"],
    queryFn: reglasService.getAll,
  });

  const updateMutation = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: number;
      data: { valorUmbral?: number | null; activa?: boolean | null };
    }) => reglasService.update(id, data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["reglas"] });
      setEditingId(null);
    },
  });

  const toggleMutation = useMutation({
    mutationFn: ({ id, activa }: { id: number; activa: boolean }) =>
      reglasService.update(id, { activa }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["reglas"] });
    },
  });

  function startEdit(regla: ReglaDeteccionResponse) {
    setEditingId(regla.id);
    setEditValue(regla.valorUmbral.toString());
  }

  function saveEdit(id: number) {
    const val = parseFloat(editValue);
    if (!isNaN(val)) {
      updateMutation.mutate({ id, data: { valorUmbral: val } });
    }
  }

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-foreground">
          Reglas de Detección
        </h1>
        <button
          onClick={() => setShowCreate(!showCreate)}
          className="flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90"
        >
          <Plus size={16} /> Nueva Regla
        </button>
      </div>

      {showCreate && (
        <CreateReglaForm onClose={() => setShowCreate(false)} />
      )}

      <div className="overflow-x-auto rounded-lg border border-border">
        <table className="w-full text-sm">
          <thead className="bg-muted">
            <tr>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Nombre
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Detector
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Parámetro
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Umbral
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Estado
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Acciones
              </th>
            </tr>
          </thead>
          <tbody>
            {reglas?.map((regla) => (
              <tr key={regla.id} className="border-t border-border">
                <td className="px-4 py-3">
                  <div>
                    <p className="font-medium text-foreground">
                      {regla.nombre}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      {regla.descripcion}
                    </p>
                  </div>
                </td>
                <td className="px-4 py-3">{regla.tipoDetector}</td>
                <td className="px-4 py-3 font-mono text-xs">
                  {regla.parametroNombre}
                </td>
                <td className="px-4 py-3">
                  {editingId === regla.id ? (
                    <div className="flex items-center gap-2">
                      <input
                        type="number"
                        value={editValue}
                        onChange={(e) => setEditValue(e.target.value)}
                        className="w-24 rounded border border-border bg-background px-2 py-1 text-sm"
                        step="0.01"
                      />
                      <button
                        onClick={() => saveEdit(regla.id)}
                        className="text-primary hover:text-primary/80"
                      >
                        <Save size={16} />
                      </button>
                      <button
                        onClick={() => setEditingId(null)}
                        className="text-muted-foreground hover:text-foreground"
                      >
                        <X size={16} />
                      </button>
                    </div>
                  ) : (
                    <button
                      onClick={() => startEdit(regla)}
                      className="font-mono hover:text-primary"
                    >
                      {regla.valorUmbral}
                    </button>
                  )}
                </td>
                <td className="px-4 py-3">
                  <button
                    onClick={() =>
                      toggleMutation.mutate({
                        id: regla.id,
                        activa: !regla.activa,
                      })
                    }
                    className={`rounded-full px-3 py-1 text-xs font-semibold ${
                      regla.activa
                        ? "bg-green-500/20 text-green-600"
                        : "bg-gray-500/20 text-gray-500"
                    }`}
                  >
                    {regla.activa ? "Activa" : "Inactiva"}
                  </button>
                </td>
                <td className="px-4 py-3">
                  <button
                    onClick={() => startEdit(regla)}
                    className="text-muted-foreground hover:text-foreground"
                  >
                    <Settings size={16} />
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function CreateReglaForm({ onClose }: { onClose: () => void }) {
  const queryClient = useQueryClient();
  const [form, setForm] = useState({
    tipoDetector: "",
    nombre: "",
    descripcion: "",
    parametroNombre: "",
    valorUmbral: "",
  });

  const createMutation = useMutation({
    mutationFn: () =>
      reglasService.create({
        ...form,
        valorUmbral: parseFloat(form.valorUmbral),
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["reglas"] });
      onClose();
    },
  });

  return (
    <div className="rounded-lg border border-border bg-background p-6">
      <h3 className="mb-4 text-lg font-semibold">Nueva Regla</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="mb-1 block text-sm font-medium">
            Tipo Detector
          </label>
          <select
            value={form.tipoDetector}
            onChange={(e) =>
              setForm({ ...form, tipoDetector: e.target.value })
            }
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          >
            <option value="">Seleccionar...</option>
            <option value="CashFraud">CashFraud</option>
            <option value="InvoiceAnomaly">InvoiceAnomaly</option>
            <option value="PaymentFraud">PaymentFraud</option>
            <option value="ComplianceViolation">ComplianceViolation</option>
          </select>
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium">Nombre</label>
          <input
            value={form.nombre}
            onChange={(e) => setForm({ ...form, nombre: e.target.value })}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
        </div>
        <div className="sm:col-span-2">
          <label className="mb-1 block text-sm font-medium">
            Descripción
          </label>
          <input
            value={form.descripcion}
            onChange={(e) =>
              setForm({ ...form, descripcion: e.target.value })
            }
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium">Parámetro</label>
          <input
            value={form.parametroNombre}
            onChange={(e) =>
              setForm({ ...form, parametroNombre: e.target.value })
            }
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium">
            Valor Umbral
          </label>
          <input
            type="number"
            value={form.valorUmbral}
            onChange={(e) =>
              setForm({ ...form, valorUmbral: e.target.value })
            }
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
            step="0.01"
          />
        </div>
      </div>
      <div className="mt-4 flex gap-3">
        <button
          onClick={() => createMutation.mutate()}
          disabled={
            createMutation.isPending || !form.nombre || !form.tipoDetector
          }
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
        >
          Crear
        </button>
        <button
          onClick={onClose}
          className="rounded-md border border-border px-4 py-2 text-sm"
        >
          Cancelar
        </button>
      </div>
    </div>
  );
}
