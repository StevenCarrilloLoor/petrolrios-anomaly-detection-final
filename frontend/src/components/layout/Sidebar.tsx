import { NavLink } from "react-router-dom";
import type { ReactNode } from "react";
import { useAuth } from "@/contexts/AuthContext";
import {
  LayoutDashboard,
  AlertTriangle,
  Settings,
  Database,
  Users,
  FileText,
  Shield,
  ShieldCheck,
  BarChart3,
  Radio,
  Wrench,
  SlidersHorizontal,
} from "lucide-react";
import { cn } from "@/lib/utils";

interface NavItem {
  to: string;
  label: string;
  icon: ReactNode;
  roles?: string[];
}

interface NavSection {
  title: string;
  items: NavItem[];
}

const navSections: NavSection[] = [
  {
    title: "Monitoreo",
    items: [
      { to: "/dashboard", label: "Dashboard", icon: <LayoutDashboard size={20} /> },
      { to: "/alertas", label: "Alertas", icon: <AlertTriangle size={20} /> },
      {
        to: "/problemas-estacion",
        label: "Problemas de estación",
        icon: <Wrench size={20} />,
      },
      { to: "/conexiones", label: "Conexiones", icon: <Radio size={20} /> },
    ],
  },
  {
    title: "Gestión",
    items: [
      {
        to: "/reportes",
        label: "Reportes",
        icon: <BarChart3 size={20} />,
        roles: ["Supervisor", "Administrador"],
      },
      {
        to: "/reglas",
        label: "Reglas",
        icon: <Settings size={20} />,
        roles: ["Supervisor", "Administrador"],
      },
      {
        to: "/fuentes-datos",
        label: "Fuentes de datos",
        icon: <Database size={20} />,
        roles: ["Supervisor", "Administrador"],
      },
    ],
  },
  {
    title: "Administración",
    items: [
      {
        to: "/usuarios",
        label: "Usuarios",
        icon: <Users size={20} />,
        roles: ["Administrador"],
      },
      {
        to: "/logs",
        label: "Logs de Auditoría",
        icon: <FileText size={20} />,
        roles: ["Administrador"],
      },
    ],
  },
  {
    title: "Mi cuenta",
    items: [
      { to: "/seguridad", label: "Seguridad (2FA)", icon: <ShieldCheck size={20} /> },
      { to: "/ajustes", label: "Ajustes", icon: <SlidersHorizontal size={20} /> },
    ],
  },
];

export function Sidebar() {
  const { user } = useAuth();

  const sections = navSections
    .map((section) => ({
      ...section,
      items: section.items.filter(
        (item) => !item.roles || (user && item.roles.includes(user.rol)),
      ),
    }))
    .filter((section) => section.items.length > 0);

  return (
    <aside className="flex w-64 flex-col border-r border-border bg-background">
      <div className="flex h-16 items-center gap-2.5 border-b border-border px-6">
        <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
          <Shield className="text-primary" size={20} />
        </div>
        <div>
          <span className="block text-base font-bold leading-tight text-foreground">
            PetrolRíos
          </span>
          <span className="block text-[10px] uppercase tracking-wider text-muted-foreground">
            Anomaly Detection
          </span>
        </div>
      </div>
      <nav className="flex-1 space-y-5 overflow-y-auto p-4">
        {sections.map((section) => (
          <div key={section.title}>
            <p className="mb-1.5 px-3 text-[10px] font-semibold uppercase tracking-wider text-muted-foreground/70">
              {section.title}
            </p>
            <div className="space-y-1">
              {section.items.map((item) => (
                <NavLink
                  key={item.to}
                  to={item.to}
                  className={({ isActive }) =>
                    cn(
                      "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                      isActive
                        ? "bg-primary text-primary-foreground shadow-sm"
                        : "text-muted-foreground hover:bg-muted hover:text-foreground",
                    )
                  }
                >
                  {item.icon}
                  {item.label}
                </NavLink>
              ))}
            </div>
          </div>
        ))}
      </nav>
      <div className="border-t border-border p-4">
        <p className="text-xs text-muted-foreground">
          Detección de Anomalías v2.0
        </p>
        <p className="mt-0.5 text-[10px] text-muted-foreground/60">
          PetrolRíos S.A. · monitoreo continuo
        </p>
      </div>
    </aside>
  );
}
