import { NavLink } from "react-router-dom";
import type { ReactNode } from "react";
import { useAuth } from "@/contexts/AuthContext";
import {
  LayoutDashboard,
  AlertTriangle,
  Settings,
  Users,
  FileText,
  Shield,
} from "lucide-react";
import { cn } from "@/lib/utils";

interface NavItem {
  to: string;
  label: string;
  icon: ReactNode;
  roles?: string[];
}

const navItems: NavItem[] = [
  {
    to: "/dashboard",
    label: "Dashboard",
    icon: <LayoutDashboard size={20} />,
  },
  { to: "/alertas", label: "Alertas", icon: <AlertTriangle size={20} /> },
  {
    to: "/reglas",
    label: "Reglas",
    icon: <Settings size={20} />,
    roles: ["Supervisor", "Administrador"],
  },
  {
    to: "/usuarios",
    label: "Usuarios",
    icon: <Users size={20} />,
    roles: ["Administrador"],
  },
  {
    to: "/logs",
    label: "Logs",
    icon: <FileText size={20} />,
    roles: ["Administrador"],
  },
];

export function Sidebar() {
  const { user } = useAuth();

  const filteredItems = navItems.filter(
    (item) => !item.roles || (user && item.roles.includes(user.rol)),
  );

  return (
    <aside className="flex w-64 flex-col border-r border-border bg-background">
      <div className="flex h-16 items-center gap-2 border-b border-border px-6">
        <Shield className="text-primary" size={24} />
        <span className="text-lg font-bold text-foreground">PetrolRíos</span>
      </div>
      <nav className="flex-1 space-y-1 p-4">
        {filteredItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              cn(
                "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                isActive
                  ? "bg-primary text-primary-foreground"
                  : "text-muted-foreground hover:bg-muted hover:text-foreground",
              )
            }
          >
            {item.icon}
            {item.label}
          </NavLink>
        ))}
      </nav>
      <div className="border-t border-border p-4">
        <p className="text-xs text-muted-foreground">
          Detección de Anomalías v1.0
        </p>
      </div>
    </aside>
  );
}
