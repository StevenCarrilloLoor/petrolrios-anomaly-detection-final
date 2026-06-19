import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { usuariosService } from "@/services/usuarios.service";
import { estacionesService } from "@/services/estaciones.service";
import { Spinner } from "@/components/ui/Spinner";
import { UserPlus, Edit, Trash2 } from "lucide-react";
import type { UsuarioResponse } from "@/types/usuario";

const ROLES = [
  { id: 1, nombre: "Auditor" },
  { id: 2, nombre: "Supervisor" },
  { id: 3, nombre: "Administrador" },
];

export function UsuariosPage() {
  const queryClient = useQueryClient();
  const [showCreate, setShowCreate] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);

  const { data: usuarios, isLoading } = useQuery({
    queryKey: ["usuarios"],
    queryFn: usuariosService.getAll,
  });
  const { data: estaciones = [] } = useQuery({
    queryKey: ["estaciones"],
    queryFn: estacionesService.getAll,
  });

  const [mensaje, setMensaje] = useState<string | null>(null);
  const deleteMutation = useMutation({
    mutationFn: (id: number) => usuariosService.delete(id),
    onSuccess: () => {
      setMensaje("Usuario desactivado correctamente.");
      void queryClient.invalidateQueries({ queryKey: ["usuarios"] });
    },
    onError: (e: unknown) => {
      const d = (e as { response?: { data?: { detail?: string } } })?.response?.data?.detail;
      setMensaje(d || "No se pudo eliminar el usuario.");
    },
  });

  function eliminar(id: number, nombre: string) {
    if (window.confirm(`¿Desactivar al usuario "${nombre}"? No podrá iniciar sesión.`)) {
      setMensaje(null);
      deleteMutation.mutate(id);
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
        <h1 className="text-2xl font-bold text-foreground">Usuarios</h1>
        <button
          onClick={() => setShowCreate(!showCreate)}
          className="flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90"
        >
          <UserPlus size={16} /> Nuevo Usuario
        </button>
      </div>

      {mensaje && (
        <div className="rounded-md bg-muted p-3 text-sm text-foreground">{mensaje}</div>
      )}

      {showCreate && (
        <CreateUsuarioForm onClose={() => setShowCreate(false)} />
      )}

      <div className="overflow-x-auto rounded-lg border border-border">
        <table className="w-full text-sm">
          <thead className="bg-muted">
            <tr>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                ID
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Nombre
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Email
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Rol
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Acceso
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Estado
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Creado
              </th>
              <th className="px-4 py-3 text-left font-medium text-muted-foreground">
                Acciones
              </th>
            </tr>
          </thead>
          <tbody>
            {usuarios?.map((u) => (
              <tr key={u.id} className="border-t border-border">
                <td className="px-4 py-3 font-mono">{u.id}</td>
                <td className="px-4 py-3 font-medium">{u.nombreCompleto}</td>
                <td className="px-4 py-3">{u.email}</td>
                <td className="px-4 py-3">{u.rol}</td>
                <td className="px-4 py-3">
                  {u.estacionId == null ? (
                    <span className="text-muted-foreground">Sistema central</span>
                  ) : (
                    <span className="rounded-full bg-amber-500/15 px-2 py-1 text-xs font-medium text-amber-700 dark:text-amber-300">
                      {estaciones.find((e) => e.id === u.estacionId)?.codigo ??
                        `Estación #${u.estacionId}`}
                    </span>
                  )}
                </td>
                <td className="px-4 py-3">
                  <span
                    className={`rounded-full px-3 py-1 text-xs font-semibold ${
                      u.activo
                        ? "bg-green-500/20 text-green-600"
                        : "bg-red-500/20 text-red-600"
                    }`}
                  >
                    {u.activo ? "Activo" : "Inactivo"}
                  </span>
                </td>
                <td className="px-4 py-3 text-muted-foreground">
                  {new Date(u.createdAt).toLocaleDateString("es-EC")}
                </td>
                <td className="px-4 py-3">
                  <div className="flex gap-2">
                    <button
                      onClick={() =>
                        setEditingId(editingId === u.id ? null : u.id)
                      }
                      className="text-muted-foreground hover:text-foreground"
                    >
                      <Edit size={16} />
                    </button>
                    <button
                      onClick={() => eliminar(u.id, u.nombreCompleto)}
                      className="text-muted-foreground hover:text-destructive"
                      title="Desactivar usuario"
                    >
                      <Trash2 size={16} />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {editingId !== null && (
        <EditUsuarioForm
          id={editingId}
          onClose={() => setEditingId(null)}
        />
      )}
    </div>
  );
}

function CreateUsuarioForm({ onClose }: { onClose: () => void }) {
  const queryClient = useQueryClient();
  const { data: estaciones = [] } = useQuery({
    queryKey: ["estaciones"],
    queryFn: estacionesService.getAll,
  });
  const [form, setForm] = useState({
    email: "",
    nombreCompleto: "",
    password: "",
    rolId: 1,
    estacionId: null as number | null,
  });
  const [confirma, setConfirma] = useState("");
  const [error, setError] = useState<string | null>(null);

  const mutation = useMutation({
    mutationFn: () => usuariosService.create(form),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["usuarios"] });
      onClose();
    },
    onError: (e: unknown) => {
      const d = (e as { response?: { data?: { detail?: string } } })?.response?.data?.detail;
      setError(d || "No se pudo crear el usuario. Revisa los datos.");
    },
  });

  function crear() {
    setError(null);
    if (form.password.length < 8) {
      setError("La contraseña debe tener al menos 8 caracteres.");
      return;
    }
    if (form.password !== confirma) {
      setError("Las contraseñas no coinciden.");
      return;
    }
    mutation.mutate();
  }

  return (
    <div className="rounded-lg border border-border bg-background p-6">
      <h3 className="mb-4 text-lg font-semibold">Nuevo Usuario</h3>
      {error && (
        <div className="mb-4 rounded-md bg-destructive/10 p-3 text-sm text-destructive">
          {error}
        </div>
      )}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label className="mb-1 block text-sm font-medium">
            Nombre completo
          </label>
          <input
            value={form.nombreCompleto}
            onChange={(e) =>
              setForm({ ...form, nombreCompleto: e.target.value })
            }
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium">Email</label>
          <input
            type="email"
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium">
            Contraseña
          </label>
          <input
            type="password"
            value={form.password}
            onChange={(e) => setForm({ ...form, password: e.target.value })}
            placeholder="Mín. 8 caracteres"
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium">
            Confirmar contraseña
          </label>
          <input
            type="password"
            value={confirma}
            onChange={(e) => setConfirma(e.target.value)}
            placeholder="Repite la contraseña"
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium">Rol</label>
          <select
            value={form.rolId}
            onChange={(e) =>
              setForm({ ...form, rolId: Number(e.target.value) })
            }
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          >
            {ROLES.map((r) => (
              <option key={r.id} value={r.id}>
                {r.nombre}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium">
            Ámbito de acceso
          </label>
          <select
            value={form.estacionId ?? 0}
            onChange={(e) => {
              const value = Number(e.target.value);
              setForm({ ...form, estacionId: value === 0 ? null : value });
            }}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          >
            <option value={0}>Sistema central (sin estación fija)</option>
            {estaciones.map((estacion) => (
              <option key={estacion.id} value={estacion.id}>
                {estacion.codigo} — {estacion.nombre}
              </option>
            ))}
          </select>
          <p className="mt-1 text-xs text-muted-foreground">
            Al asignar una estación, la cuenta solo puede leer sus problemas operativos.
          </p>
        </div>
      </div>
      <div className="mt-4 flex gap-3">
        <button
          onClick={crear}
          disabled={
            mutation.isPending ||
            !form.email ||
            !form.nombreCompleto ||
            !form.password ||
            !confirma
          }
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
        >
          {mutation.isPending ? "Creando…" : "Crear"}
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

function EditUsuarioForm({
  id,
  onClose,
}: {
  id: number;
  onClose: () => void;
}) {
  const { data: usuario } = useQuery({
    queryKey: ["usuarios", id],
    queryFn: () => usuariosService.getById(id),
  });

  if (!usuario) {
    return (
      <div className="flex justify-center rounded-lg border border-border bg-background p-6">
        <Spinner />
      </div>
    );
  }

  return <EditUsuarioFormCargado key={usuario.id} usuario={usuario} onClose={onClose} />;
}

function EditUsuarioFormCargado({
  usuario,
  onClose,
}: {
  usuario: UsuarioResponse;
  onClose: () => void;
}) {
  const queryClient = useQueryClient();
  const { data: estaciones = [] } = useQuery({
    queryKey: ["estaciones"],
    queryFn: estacionesService.getAll,
  });
  const [form, setForm] = useState({
    nombreCompleto: usuario.nombreCompleto,
    rolId: usuario.rolId,
    activo: usuario.activo,
    estacionId: usuario.estacionId,
    actualizarEstacion: true,
  });

  const mutation = useMutation({
    mutationFn: () => usuariosService.update(usuario.id, form),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["usuarios"] });
      onClose();
    },
  });

  return (
    <div className="rounded-lg border border-border bg-background p-6">
      <h3 className="mb-4 text-lg font-semibold">Editar Usuario #{usuario.id}</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <div>
          <label className="mb-1 block text-sm font-medium">Nombre</label>
          <input
            value={form.nombreCompleto}
            onChange={(e) =>
              setForm({ ...form, nombreCompleto: e.target.value })
            }
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium">Rol</label>
          <select
            value={form.rolId}
            onChange={(e) =>
              setForm({ ...form, rolId: Number(e.target.value) })
            }
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          >
            {ROLES.map((r) => (
              <option key={r.id} value={r.id}>
                {r.nombre}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium">Estado</label>
          <select
            value={form.activo ? "true" : "false"}
            onChange={(e) =>
              setForm({ ...form, activo: e.target.value === "true" })
            }
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          >
            <option value="true">Activo</option>
            <option value="false">Inactivo</option>
          </select>
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium">
            Ámbito de acceso
          </label>
          <select
            value={form.estacionId ?? 0}
            onChange={(e) => {
              const value = Number(e.target.value);
              setForm({
                ...form,
                estacionId: value === 0 ? null : value,
                actualizarEstacion: true,
              });
            }}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          >
            <option value={0}>Sistema central</option>
            {estaciones.map((estacion) => (
              <option key={estacion.id} value={estacion.id}>
                {estacion.codigo} — {estacion.nombre}
              </option>
            ))}
          </select>
        </div>
      </div>
      <div className="mt-4 flex gap-3">
        <button
          onClick={() => mutation.mutate()}
          disabled={mutation.isPending}
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
        >
          Guardar
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
