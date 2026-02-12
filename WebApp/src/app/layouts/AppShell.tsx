import { NavLink, Outlet } from "react-router-dom";
import { cn } from "@/shared/lib/cn";
import { ThemeToggle } from "@/shared/theme/ThemeToggle";
import { useI18n } from "@/i18n/provider";
import LanguageSwitcher from "@/components/LanguageSwitcher";

import { Sheet, SheetContent, SheetTrigger } from "@/components/ui/sheet";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
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
  LogOut,
  User as UserIcon,
  ChevronDown,
  Shield,
  Bell,
  Activity,
  AlertTriangle,
  Users,
  Building,
  Key,
  Heart,
  FileText,
  Settings as SettingsIcon,
  Eye,
  Flag,
  CreditCard,
  Clock,
  Webhook,
  BarChart,
  UserCheck,
} from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";
import { PermissionGuard } from "@/shared/auth/PermissionGuard";

type NavItem = {
  to: string;
  label: string;
  icon: any;
  end: boolean;
};

export function AppShell() {
  const { t } = useI18n();
  
  const nav: NavItem[] = [
    { to: "/", label: t("nav.dashboard"), icon: LayoutDashboard, end: false },
    { to: "/users", label: t("nav.users"), icon: Users, end: false },
    { to: "/roles", label: t("nav.roles"), icon: UserCheck, end: false },
    { to: "/tenants", label: t("nav.tenants"), icon: Building, end: false },
    { to: "/api-keys", label: t("nav.api_keys"), icon: Key, end: false },
    { to: "/health", label: t("nav.health"), icon: Heart, end: false },
    { to: "/settings", label: t("nav.settings"), icon: SettingsIcon, end: false },
    { to: "/security-events", label: t("nav.security_events"), icon: Shield, end: false },
    { to: "/audit", label: t("nav.audit"), icon: FileText, end: false },
    { to: "/feature-flags", label: t("nav.feature_flags"), icon: Flag, end: false },
    { to: "/plans", label: t("nav.plans"), icon: CreditCard, end: false },
    { to: "/rate-limit", label: t("nav.rate_limit"), icon: Eye, end: false },
    { to: "/sessions", label: t("nav.sessions"), icon: Clock, end: false },
    { to: "/webhooks", label: t("nav.webhooks"), icon: Webhook, end: false },
    { to: "/tenant-usage", label: t("nav.tenant_usage"), icon: BarChart, end: false },
    { to: "/user-roles", label: t("nav.user_roles"), icon: UserCheck, end: true },
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
                <div className="text-sm font-semibold">Platform</div>
                <div className="text-xs text-muted-foreground">
                  Management Console
                </div>
              </div>
            </div>

            <div className="flex items-center gap-1">
              <ThemeToggle />
              <LanguageSwitcher />
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

function Sidebar({ nav }: { nav: NavItem[] }) {
  return (
    <div className="flex h-full flex-col">
      <div className="flex h-16 items-center gap-3 px-4">
        <div className="flex h-10 w-10 items-center justify-center rounded-2xl bg-primary text-primary-foreground font-bold">
          A
        </div>
        <div className="leading-tight">
          <div className="text-sm font-semibold">Platform</div>
          <div className="text-xs text-muted-foreground">
            Admin Console
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
          <div className="text-sm font-semibold">Status</div>
          <div className="mt-1 text-xs text-muted-foreground">
            Backend: {import.meta.env.VITE_BACKEND_URL || "http://localhost:5001"}
          </div>
        </div>
      </div>
    </div>
  );
}

function UserMenu() {
  const { user, logout } = useAuth();
  const { t } = useI18n();

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
          {t("common.logout")}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
