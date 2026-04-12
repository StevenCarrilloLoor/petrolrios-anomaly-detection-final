import { useAuth } from "@/contexts/AuthContext";
import { Bell, LogOut } from "lucide-react";
import { useNotificationCount } from "@/components/notifications/NotificationProvider";

export function Header() {
  const { user, logout } = useAuth();
  const alertCount = useNotificationCount();

  return (
    <header className="flex h-16 items-center justify-between border-b border-border bg-background px-6">
      <div>
        <h2 className="text-sm font-medium text-muted-foreground">
          Bienvenido, {user?.nombreCompleto}
        </h2>
      </div>
      <div className="flex items-center gap-4">
        <div className="relative">
          <Bell size={20} className="text-muted-foreground" />
          {alertCount > 0 && (
            <span className="absolute -right-2 -top-2 flex h-5 min-w-5 items-center justify-center rounded-full bg-destructive px-1 text-xs font-bold text-destructive-foreground">
              {alertCount > 99 ? "99+" : alertCount}
            </span>
          )}
        </div>
        <span className="rounded-md bg-muted px-2 py-1 text-xs font-medium text-muted-foreground">
          {user?.rol}
        </span>
        <button
          onClick={logout}
          className="flex items-center gap-1 rounded-md px-2 py-1 text-sm text-muted-foreground hover:bg-muted hover:text-foreground"
          title="Cerrar sesión"
        >
          <LogOut size={16} />
        </button>
      </div>
    </header>
  );
}
