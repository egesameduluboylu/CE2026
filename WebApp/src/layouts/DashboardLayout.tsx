import { Outlet, NavLink } from "react-router-dom";
import { useAuth } from "@/contexts/AuthContext";
import { Button } from "@/components/ui/button";
import { NotificationDropdown } from "@/components/Notifications/NotificationDropdown";

const nav = [
  { to: "/dashboard", label: "Overview" },
  { to: "/dashboard/users", label: "Users" },
  { to: "/dashboard/sessions", label: "Sessions" },
  { to: "/dashboard/notifications", label: "Notifications" },
  { to: "/dashboard/background-jobs", label: "Background Jobs" },
  { to: "/dashboard/error-dashboard", label: "Error Dashboard" },
  { to: "/dashboard/settings", label: "Settings" },
];

export function DashboardLayout() {
  const { user, logout } = useAuth();

  return (
    <div className="min-h-screen bg-background">
      <div className="grid min-h-screen grid-cols-[260px_1fr]">
        {/* Sidebar */}
        <aside className="border-r bg-card">
          <div className="flex h-16 items-center justify-between px-4">
            <div className="flex items-center gap-2">
              <div className="flex h-9 w-9 items-center justify-center rounded-xl border bg-background font-bold">
                A
              </div>
              <div className="leading-tight">
                <div className="text-sm font-semibold">Platform Admin</div>
                <div className="text-xs text-muted-foreground">Dashboard</div>
              </div>
            </div>
          </div>

          <nav className="px-2 py-2">
            {nav.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.to === "/dashboard"}
                className={({ isActive }) =>
                  [
                    "flex items-center rounded-xl px-3 py-2 text-sm transition",
                    isActive
                      ? "bg-muted font-semibold"
                      : "text-muted-foreground hover:bg-muted/60 hover:text-foreground",
                  ].join(" ")
                }
              >
                {item.label}
              </NavLink>
            ))}
          </nav>

          <div className="mt-auto px-4 py-4">
            <div className="rounded-2xl border bg-background p-3">
              <div className="text-xs text-muted-foreground">Signed in as</div>
              <div className="text-sm font-semibold truncate">{user?.email}</div>
              <Button
                variant="outline"
                className="mt-3 w-full rounded-xl"
                onClick={logout}
              >
                Logout
              </Button>
            </div>
          </div>
        </aside>

        {/* Main */}
        <main className="min-w-0">
          {/* Topbar */}
          <header className="sticky top-0 z-10 flex h-16 items-center justify-between border-b bg-background/80 px-6 backdrop-blur">
            <div className="text-sm text-muted-foreground">
              Welcome back{" "}
              <span className="font-semibold text-foreground">
                {user?.email}
              </span>
            </div>

            <div className="flex items-center gap-2">
              <NotificationDropdown />
              <div className="hidden text-xs text-muted-foreground sm:block">
                JWT + Refresh rotation enabled
              </div>
            </div>
          </header>

          <div className="p-6">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
}
