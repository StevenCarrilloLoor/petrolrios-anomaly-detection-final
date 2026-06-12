import { api } from "./api";
import type {
  UsuarioResponse,
  CrearUsuarioRequest,
  ActualizarUsuarioRequest,
} from "@/types/usuario";

export const usuariosService = {
  getAll: () =>
    api.get<UsuarioResponse[]>("/usuarios").then((r) => r.data),

  getAuditores: () =>
    api.get<UsuarioResponse[]>("/usuarios/auditores").then((r) => r.data),

  getById: (id: number) =>
    api.get<UsuarioResponse>(`/usuarios/${id}`).then((r) => r.data),

  create: (data: CrearUsuarioRequest) =>
    api.post<UsuarioResponse>("/usuarios", data).then((r) => r.data),

  update: (id: number, data: ActualizarUsuarioRequest) =>
    api.put<UsuarioResponse>(`/usuarios/${id}`, data).then((r) => r.data),

  delete: (id: number) => api.delete(`/usuarios/${id}`),
};
