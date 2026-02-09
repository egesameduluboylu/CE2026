import { useState } from "react";
import { NavLink, Outlet } from "react-router-dom";
import { cn } from "@/shared/lib/cn";
import { ThemeToggle } from "@/shared/theme/ThemeToggle";

import { Sheet, SheetContent, SheetTrigger } from "@/components/ui/sheet";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

import {
  LayoutDashboard,
  Menu,
  Users,
  LogOut,
  User as UserIcon,
  ChevronDown,
  Shield, // ✅ eklendi
  Building2,
} from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";
import { useTranslation } from "react-i18next";
import { LanguageMenu } from "@/components/LanguageMenu";
import { usePermission } from "@/shared/auth/usePermission";

type NavItem = {
  to: string;
  label: string;
  icon: any;
  end: boolean;
};

export function AppShell() {
  const { t } = useTranslation();

  const canUsersRead = usePermission("users.read");
  const canRolesRead = usePermission("roles.read"); // ✅ eklendi
  const canTenantsRead = usePermission("tenants.read");
  const canBillingRead = usePermission("billing.read");
  const canFlagsRead = usePermission("flags.read");
  const canAuditRead = usePermission("audit.read");
  const canSessionsRead = usePermission("sessions.read");
  const canApiKeysRead = usePermission("api_keys.read");
  const canWebhooksRead = usePermission("webhooks.read");
  const canHealthRead = usePermission("health.read");
  const canRateLimitRead = usePermission("rate_limit.read");

  const nav: NavItem[] = [
    { to: "/", label: t("nav.dashboard"), icon: LayoutDashboard, end: true },

    ...(canUsersRead
      ? [{ to: "/users", label: t("nav.users"), icon: Users, end: false } as NavItem]
      : []),

    // ✅ admin/izinli: Roles & Permissions ekranı
    ...(canRolesRead
      ? [
          { to: "/roles", label: t("nav.roles") ?? "Roles", icon: Shield, end: false } as NavItem,
        ]
      : []),

    ...(canTenantsRead
      ? [{ to: "/tenants", label: t("nav.tenants") ?? "Tenants", icon: Building2, end: false } as NavItem]
      : []),
    ...(canRateLimitRead
      ? [{ to: "/rate-limit", label: "Rate Limit", icon: Shield, end: false } as NavItem]
      : []),

    ...(canBillingRead
      ? [{ to: "/plans", label: t("nav.plans") ?? "Plans", icon: Shield, end: false } as NavItem]
      : []),

    ...(canFlagsRead
      ? [{ to: "/feature-flags", label: t("nav.flags") ?? "Feature Flags", icon: Shield, end: false } as NavItem]
      : []),

    ...(canAuditRead
      ? [{ to: "/audit", label: t("nav.audit") ?? "Audit", icon: Shield, end: false } as NavItem]
      : []),

    ...(canSessionsRead
      ? [{ to: "/sessions", label: t("nav.sessions") ?? "Sessions", icon: Shield, end: false } as NavItem]
      : []),

    ...(canApiKeysRead
      ? [{ to: "/api-keys", label: t("nav.api_keys") ?? "API Keys", icon: Shield, end: false } as NavItem]
      : []),

    ...(canWebhooksRead
      ? [{ to: "/webhooks", label: t("nav.webhooks") ?? "Webhooks", icon: Shield, end: false } as NavItem]
      : []),

    ...(canHealthRead
      ? [{ to: "/health", label: t("nav.health") ?? "Health", icon: Shield, end: false } as NavItem]
      : []),
  ];

  return (
    <div className="min-h-screen bg-background">
      {/* Desktop sidebar */}
      <aside className="hidden lg:fixed lg:inset-y-0 lg:flex lg:w-72 lg:flex-col border-r bg-card/60 backdrop-blur">
        <Sidebar nav={nav} />
      </aside>

      <div className="lg:pl-72">
        <header className="sticky top-0 z-50 h-16 border-b bg-background/70 backdrop-blur">
          <div className="mx-auto flex h-16 items-center justify-between px-4">
            <div className="flex items-center gap-2">
              {/* Mobile menu */}
              <div className="lg:hidden">
                <Sheet>
                  <SheetTrigger asChild>
                    <Button variant="outline" size="icon" aria-label="Open menu">
                      <Menu className="h-4 w-4" />
                    </Button>
                  </SheetTrigger>

                  <SheetContent side="left" className="p-0">
                    <Sidebar nav={nav} />
                  </SheetContent>
                </Sheet>
              </div>

              <div className="hidden sm:block">
                <div className="text-sm font-semibold">{t("app.title")}</div>
                <div className="text-xs text-muted-foreground">
                  {t("app.subtitle")}
                </div>
              </div>
            </div>

            <div className="flex items-center gap-1">
              {canTenantsRead ? <TenantSwitcher /> : null}
              <ThemeToggle />
              <LanguageMenu />
              <UserMenu />
            </div>
          </div>
        </header>

        <main className="mx-auto p-4 sm:p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

function TenantSwitcher() {
  const [open, setOpen] = useState(false);
  const [value, setValue] = useState<string>(() => {
    try {
      return window.localStorage.getItem("tenantId") ?? "";
    } catch {
      return "";
    }
  });

  const apply = () => {
    try {
      const v = value.trim();
      if (v) window.localStorage.setItem("tenantId", v);
      else window.localStorage.removeItem("tenantId");
    } catch {
    }
    setOpen(false);
  };

  const clear = () => {
    try {
      window.localStorage.removeItem("tenantId");
    } catch {
    }
    setValue("");
  };

  const current = value.trim();

  return (
    <DropdownMenu open={open} onOpenChange={setOpen}>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" className="h-9 rounded-xl px-2 gap-2">
          <Building2 className="h-4 w-4 text-muted-foreground" />
          <span className="hidden sm:inline text-sm">
            {current ? current.slice(0, 8) + "…" : "Tenant"}
          </span>
          <ChevronDown className="hidden sm:block h-4 w-4 text-muted-foreground" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" sideOffset={8} className="w-72 p-2">
        <div className="space-y-2">
          <div className="text-xs text-muted-foreground">X-Tenant-Id</div>
          <Input
            value={value}
            onChange={(e) => setValue(e.target.value)}
            placeholder="Tenant GUID…"
            className="h-9"
          />
          <div className="flex items-center gap-2">
            <Button size="sm" onClick={apply}>
              Apply
            </Button>
            <Button size="sm" variant="outline" onClick={clear}>
              Clear
            </Button>
          </div>
        </div>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

function Sidebar({ nav }: { nav: NavItem[] }) {
  const { t } = useTranslation();

  return (
    <div className="flex h-full flex-col">
      <div className="flex h-16 items-center gap-3 px-4">
        <div className="flex h-10 w-10 items-center justify-center rounded-2xl bg-primary text-primary-foreground font-bold">
          A
        </div>
        <div className="leading-tight">
          <div className="text-sm font-semibold">{t("app.title")}</div>
          <div className="text-xs text-muted-foreground">
            {t("app.admin_console")}
          </div>
        </div>
      </div>

      <div className="px-3">
        <Separator className="my-2" />
        <nav className="space-y-1">
          {nav.map((n) => {
            const Icon = n.icon;
            return (
              <NavLink
                key={n.to}
                to={n.to}
                end={n.end}
                className={({ isActive }) =>
                  cn(
                    "flex items-center gap-3 rounded-xl px-3 py-2 text-sm transition",
                    "hover:bg-muted",
                    isActive ? "bg-muted font-semibold" : ""
                  )
                }
              >
                <Icon className="h-4 w-4 text-muted-foreground" />
                {n.label}
              </NavLink>
            );
          })}
        </nav>
      </div>

      <div className="mt-auto p-4">
        <Separator className="mb-3" />
        <div className="rounded-2xl border bg-card p-3">
          <div className="text-sm font-semibold">{t("app.status")}</div>
          <div className="mt-1 text-xs text-muted-foreground">
            {t("app.backend")}: http://localhost:5211
          </div>
        </div>
      </div>
    </div>
  );
}

function UserMenu() {
  const { user, logout } = useAuth();
  const { t } = useTranslation();

  const email = user?.email ?? "Account";
  const initials = (email?.[0] ?? "A").toUpperCase();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" className="h-9 rounded-xl px-2 gap-2">
          <Avatar className="h-7 w-7">
            <AvatarFallback>{initials}</AvatarFallback>
          </Avatar>

          <span className="hidden sm:inline max-w-[160px] truncate text-sm text-foreground">
            {email}
          </span>

          <ChevronDown className="hidden sm:block h-4 w-4 text-muted-foreground" />
        </Button>
      </DropdownMenuTrigger>

      <DropdownMenuContent align="end" sideOffset={8} className="w-64 p-1">
        <div className="px-2 py-2">
          <div className="flex items-center gap-2">
            <UserIcon className="h-4 w-4 text-muted-foreground" />
            <div className="min-w-0">
              <div className="text-sm font-medium truncate">{email}</div>
              <div className="text-xs text-muted-foreground truncate">
                {user?.userId ?? ""}
              </div>
            </div>
          </div>
        </div>

        <DropdownMenuSeparator />

        <DropdownMenuItem
          onSelect={(e) => {
            e.preventDefault();
            logout();
          }}
          className="gap-2 text-destructive focus:text-destructive"
        >
          <LogOut className="h-4 w-4" />
          {t("user.logout")}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
