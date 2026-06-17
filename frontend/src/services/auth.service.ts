import { api } from "./api";
import type { LoginRequest, LoginResponse, Iniciar2faResponse } from "@/types/auth";

export const authService = {
  login: (data: LoginRequest) =>
    api.post<LoginResponse>("/auth/login", data).then((r) => r.data),

  refresh: (data: { refreshToken: string }) =>
    api.post<LoginResponse>("/auth/refresh", data).then((r) => r.data),

  cambiarPassword: (passwordActual: string, passwordNueva: string) =>
    api.post("/auth/cambiar-password", { passwordActual, passwordNueva }),

  // 2FA (TOTP)
  estado2fa: () =>
    api.get<{ habilitado: boolean }>("/auth/2fa/estado").then((r) => r.data),
  iniciar2fa: () =>
    api.post<Iniciar2faResponse>("/auth/2fa/iniciar").then((r) => r.data),
  confirmar2fa: (codigo: string) =>
    api.post("/auth/2fa/confirmar", { codigo }),
  desactivar2fa: (codigo: string) =>
    api.post("/auth/2fa/desactivar", { codigo }),
};
