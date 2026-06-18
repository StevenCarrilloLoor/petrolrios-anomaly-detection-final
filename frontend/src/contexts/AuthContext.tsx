import {
  createContext,
  useContext,
  useState,
  useCallback,
  useEffect,
} from "react";
import type { ReactNode } from "react";
import type { LoginRequest, LoginResponse, UsuarioInfo } from "@/types/auth";
import { authService } from "@/services/auth.service";
import {
  createSignalRConnection,
  stopSignalRConnection,
} from "@/services/signalr";

interface AuthContextType {
  user: UsuarioInfo | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (credentials: LoginRequest) => Promise<LoginResponse>;
  establecerSesion: (response: LoginResponse) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UsuarioInfo | null>(() => {
    const json = localStorage.getItem("user");
    if (json) {
      try {
        return JSON.parse(json) as UsuarioInfo;
      } catch {
        return null;
      }
    }
    return null;
  });
  const [token, setToken] = useState<string | null>(
    () => localStorage.getItem("token"),
  );

  const login = useCallback(async (credentials: LoginRequest) => {
    const response = await authService.login(credentials);
    // Si el servidor pide 2FA, aún no hay token: el formulario pedirá el código.
    if (response.requiere2Fa || !response.token) {
      return response;
    }
    localStorage.setItem("token", response.token);
    localStorage.setItem("refreshToken", response.refreshToken);
    localStorage.setItem("user", JSON.stringify(response.usuario));
    setToken(response.token);
    setUser(response.usuario);
    return response;
  }, []);

  const establecerSesion = useCallback((response: LoginResponse) => {
    localStorage.setItem("token", response.token);
    localStorage.setItem("refreshToken", response.refreshToken);
    localStorage.setItem("user", JSON.stringify(response.usuario));
    setToken(response.token);
    setUser(response.usuario);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("user");
    setToken(null);
    setUser(null);
    stopSignalRConnection();
  }, []);

  useEffect(() => {
    if (!token) return;
    createSignalRConnection(token, user ?? undefined);
    return () => {
      stopSignalRConnection();
    };
  }, [token, user]);

  return (
    <AuthContext.Provider
      value={{ user, token, isAuthenticated: !!token && !!user, login, establecerSesion, logout }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth debe usarse dentro de AuthProvider");
  }
  return context;
}
