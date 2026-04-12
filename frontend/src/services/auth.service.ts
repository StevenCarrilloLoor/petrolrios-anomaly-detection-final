import { api } from "./api";
import type { LoginRequest, LoginResponse } from "@/types/auth";

export const authService = {
  login: (data: LoginRequest) =>
    api.post<LoginResponse>("/auth/login", data).then((r) => r.data),

  refresh: (data: { refreshToken: string }) =>
    api.post<LoginResponse>("/auth/refresh", data).then((r) => r.data),
};
