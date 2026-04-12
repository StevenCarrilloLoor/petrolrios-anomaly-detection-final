import {
  createContext,
  useContext,
  useState,
  useCallback,
  useEffect,
} from "react";
import type { ReactNode } from "react";
import type { LoginRequest, UsuarioInfo } from "@/types/auth";
import { authService } from "@/services/auth.service";
import {
  createSignalRConnection,
  stopSignalRConnection,
} from "@/services/signalr";

interface AuthContextType {
  user: UsuarioInfo | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
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
    const conn = createSignalRConnection(token);
    conn.start().catch(() => {
      /* SignalR no disponible */
    });
    return () => {
      stopSignalRConnection();
    };
  }, [token]);

  return (
    <AuthContext.Provider
      value={{ user, token, isAuthenticated: !!token && !!user, login, logout }}
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
