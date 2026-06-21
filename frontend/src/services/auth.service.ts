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

  // Login por QR (estilo Steam)
  qrIniciar: () =>
    api
      .post<{ codigo: string; expiraSegundos: number }>("/auth/qr/iniciar")
      .then((r) => r.data),
  qrEstado: (codigo: string) =>
    api
      .get<{ estado: string; login: LoginResponse | null }>("/auth/qr/estado", {
        params: { codigo },
      })
      .then((r) => r.data),
  qrAprobar: (codigo: string) => api.post("/auth/qr/aprobar", { codigo }),

  // Verificación de correo
  verificarEmail: (token: string) =>
    api.post<{ ok: boolean; mensaje: string }>("/auth/verificar-email", { token }).then((r) => r.data),
  reenviarVerificacion: (email: string) =>
    api.post("/auth/reenviar-verificacion", { email }),

  // Login con autenticador (sin contraseña)
  loginTotp: (email: string, codigoTotp: string) =>
    api.post<LoginResponse>("/auth/login-totp", { email, codigoTotp }).then((r) => r.data),

  // Recuperación de contraseña
  olvidePassword: (email: string) =>
    api.post<{ ok: boolean; mensaje: string }>("/auth/olvide-password", { email }).then((r) => r.data),
  restablecerPassword: (token: string, nuevaPassword: string) =>
    api.post<{ ok: boolean; mensaje: string }>("/auth/restablecer-password", { token, nuevaPassword }).then((r) => r.data),

  // Desbloqueo de cuenta (tras bloqueo por intentos fallidos)
  solicitarDesbloqueo: (email: string) =>
    api.post<{ ok: boolean; mensaje: string }>("/auth/solicitar-desbloqueo", { email }).then((r) => r.data),
  desbloquearCuenta: (token: string) =>
    api.post<{ ok: boolean; mensaje: string }>("/auth/desbloquear-cuenta", { token }).then((r) => r.data),
};
