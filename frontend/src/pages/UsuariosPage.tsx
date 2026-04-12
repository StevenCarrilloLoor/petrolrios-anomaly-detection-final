import { useState, useEffect } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { usuariosService } from "@/services/usuarios.service";
import { Spinner } from "@/components/ui/Spinner";
import { UserPlus, Edit, Trash2 } from "lucide-react";

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

  const deleteMutation = useMutation({
    mutationFn: (id: number) => usuariosService.delete(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["usuarios"] });
    },
  });

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
                      onClick={() => deleteMutation.mutate(u.id)}
                      className="text-muted-foreground hover:text-destructive"
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
  const [form, setForm] = useState({
    email: "",
    nombreCompleto: "",
    password: "",
    rolId: 1,
  });

  const mutation = useMutation({
    mutationFn: () => usuariosService.create(form),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["usuarios"] });
      onClose();
    },
  });

  return (
    <div className="rounded-lg border border-border bg-background p-6">
      <h3 className="mb-4 text-lg font-semibold">Nuevo Usuario</h3>
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
      </div>
      <div className="mt-4 flex gap-3">
        <button
          onClick={() => mutation.mutate()}
          disabled={
            mutation.isPending ||
            !form.email ||
            !form.nombreCompleto ||
            !form.password
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

function EditUsuarioForm({
  id,
  onClose,
}: {
  id: number;
  onClose: () => void;
}) {
  const queryClient = useQueryClient();
  const [form, setForm] = useState({
    nombreCompleto: "",
    rolId: 1,
    activo: true,
  });

  const { data: usuario } = useQuery({
    queryKey: ["usuarios", id],
    queryFn: () => usuariosService.getById(id),
  });

  useEffect(() => {
    if (usuario) {
      setForm({
        nombreCompleto: usuario.nombreCompleto,
        rolId: usuario.rolId,
        activo: usuario.activo,
      });
    }
  }, [usuario]);

  const mutation = useMutation({
    mutationFn: () => usuariosService.update(id, form),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["usuarios"] });
      onClose();
    },
  });

  return (
    <div className="rounded-lg border border-border bg-background p-6">
      <h3 className="mb-4 text-lg font-semibold">Editar Usuario #{id}</h3>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
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
