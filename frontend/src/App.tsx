import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "@/contexts/AuthContext";
import { RefrescoProvider } from "@/contexts/RefrescoContext";
import { SettingsProvider } from "@/contexts/SettingsContext";
import { NotificationProvider } from "@/components/notifications/NotificationProvider";
import { ConfirmProvider } from "@/components/ui/ConfirmDialog";
import { ProtectedRoute } from "@/components/auth/ProtectedRoute";
import { AppLayout } from "@/components/layout/AppLayout";
import { LoginPage } from "@/pages/LoginPage";
import { DashboardPage } from "@/pages/DashboardPage";
import { AlertasPage } from "@/pages/AlertasPage";
import { ProblemasEstacionPage } from "@/pages/ProblemasEstacionPage";
import { DetalleAlertaPage } from "@/pages/DetalleAlertaPage";
import { ConexionesPage } from "@/pages/ConexionesPage";
import { DatosRecibidosPage } from "@/pages/DatosRecibidosPage";
import { ReglasPage } from "@/pages/ReglasPage";
import { FuentesDatosPage } from "@/pages/FuentesDatosPage";
import { ReportesPage } from "@/pages/ReportesPage";
import { UsuariosPage } from "@/pages/UsuariosPage";
import { LogsPage } from "@/pages/LogsPage";
import { SeguridadPage } from "@/pages/SeguridadPage";
import { AjustesPage } from "@/pages/AjustesPage";
import { AprobarQrPage } from "@/pages/AprobarQrPage";
import { VerificarCorreoPage } from "@/pages/VerificarCorreoPage";
import { RestablecerPasswordPage } from "@/pages/RestablecerPasswordPage";
import { DesbloquearCuentaPage } from "@/pages/DesbloquearCuentaPage";
import { NotFoundPage } from "@/pages/NotFoundPage";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <RefrescoProvider>
        <SettingsProvider>
          <NotificationProvider>
            <ConfirmProvider>
            <BrowserRouter>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/verificar-correo" element={<VerificarCorreoPage />} />
              <Route path="/restablecer-password" element={<RestablecerPasswordPage />} />
              <Route path="/desbloquear-cuenta" element={<DesbloquearCuentaPage />} />

              <Route element={<ProtectedRoute />}>
                <Route element={<AppLayout />}>
                  <Route path="/dashboard" element={<DashboardPage />} />
                  <Route path="/alertas" element={<AlertasPage />} />
                  <Route path="/problemas-estacion" element={<ProblemasEstacionPage />} />
                  <Route path="/alertas/:id" element={<DetalleAlertaPage />} />
                  <Route path="/conexiones" element={<ConexionesPage />} />
                  <Route path="/datos-recibidos" element={<DatosRecibidosPage />} />
                  <Route path="/seguridad" element={<SeguridadPage />} />
                  <Route path="/ajustes" element={<AjustesPage />} />
                  <Route path="/aprobar-qr" element={<AprobarQrPage />} />
                </Route>
              </Route>

              <Route
                element={
                  <ProtectedRoute
                    allowedRoles={["Supervisor", "Administrador"]}
                  />
                }
              >
                <Route element={<AppLayout />}>
                  <Route path="/reglas" element={<ReglasPage />} />
                  <Route path="/fuentes-datos" element={<FuentesDatosPage />} />
                  <Route path="/reportes" element={<ReportesPage />} />
                </Route>
              </Route>

              <Route
                element={
                  <ProtectedRoute allowedRoles={["Administrador"]} />
                }
              >
                <Route element={<AppLayout />}>
                  <Route path="/usuarios" element={<UsuariosPage />} />
                  <Route path="/logs" element={<LogsPage />} />
                </Route>
              </Route>

              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route path="*" element={<NotFoundPage />} />
            </Routes>
            </BrowserRouter>
            </ConfirmProvider>
          </NotificationProvider>
        </SettingsProvider>
        </RefrescoProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
