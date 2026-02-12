import { createBrowserRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AppShell } from "@/app/layouts/AppShell";
import Login from "@/pages/Login";
import DashboardPage from "@/pages/DashboardPage";
import { UsersPage } from "@/pages/users/UsersPage";
import RolesPage from "@/pages/RolesPage";
import TenantsPage from "@/pages/TenantsPage";
import ApiKeysPage from "@/pages/ApiKeysPage";
import { HealthPage } from "@/pages/HealthPage";
import { SecurityEvents } from "@/pages/SecurityEvents";
import { Settings } from "@/pages/Settings";
import { AuthProvider } from "@/contexts/AuthContext";
import { I18nProvider } from "@/i18n/provider";
import { ThemeProvider } from "@/shared/theme/ThemeProvider";
import { Toaster } from "@/components/ui/sonner";
import { AuditPage } from "@/pages/AuditPage";
import { FeatureFlagsPage } from "@/pages/FeatureFlagsPage";
import { PlansPage } from "@/pages/PlansPage";
import RateLimitPage from "@/pages/RateLimitPage";
import { SessionsPage } from "@/pages/SessionsPage";
import { WebhooksPage } from "@/pages/WebhooksPage";
import { TenantUsagePage } from "@/pages/TenantUsagePage";
import { UserRolesPage } from "@/pages/UserRolesPage";
import { NewUserPage } from "@/pages/users/NewUserPage";
import { UserDetailPage } from "@/pages/users/UserDetailPage";
import { RoleDetailPage } from "@/pages/RoleDetailPage";
import { ForgotPassword } from "@/pages/ForgotPassword";
import { Register } from "@/pages/Register";
import { ResetPassword } from "@/pages/ResetPassword";

// Create a client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60 * 1000,
      refetchOnWindowFocus: false,
    },
  },
});

// Layout component with all providers
function AppWithProviders({ children }: { children: React.ReactNode }) {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <I18nProvider>
          <AuthProvider>
            {children}
            <Toaster />
          </AuthProvider>
        </I18nProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export const router = createBrowserRouter([
  {
    path: "/login",
    element: <AppWithProviders><Login /></AppWithProviders>,
  },
  {
    path: "/forgot-password",
    element: <AppWithProviders><ForgotPassword /></AppWithProviders>,
  },
  {
    path: "/register",
    element: <AppWithProviders><Register /></AppWithProviders>,
  },
  {
    path: "/reset-password",
    element: <AppWithProviders><ResetPassword /></AppWithProviders>,
  },
  {
    path: "/",
    element: <AppWithProviders><AppShell /></AppWithProviders>,
    children: [
      {
        index: true,
        element: <DashboardPage />,
      },
      {
        path: "users",
        element: <UsersPage />,
      },
      {
        path: "users/new",
        element: <NewUserPage />,
      },
      {
        path: "users/:id",
        element: <UserDetailPage />,
      },
      {
        path: "roles",
        element: <RolesPage />,
      },
      {
        path: "roles/:id",
        element: <RoleDetailPage />,
      },
      {
        path: "tenants",
        element: <TenantsPage />,
      },
      {
        path: "api-keys",
        element: <ApiKeysPage />,
      },
      {
        path: "health",
        element: <HealthPage />,
      },
      {
        path: "security-events",
        element: <SecurityEvents />,
      },
      {
        path: "settings",
        element: <Settings />,
      },
      {
        path: "audit",
        element: <AuditPage />,
      },
      {
        path: "feature-flags",
        element: <FeatureFlagsPage />,
      },
      {
        path: "plans",
        element: <PlansPage />,
      },
      {
        path: "rate-limit",
        element: <RateLimitPage />,
      },
      {
        path: "sessions",
        element: <SessionsPage />,
      },
      {
        path: "webhooks",
        element: <WebhooksPage />,
      },
      {
        path: "tenant-usage",
        element: <TenantUsagePage />,
      },
      {
        path: "user-roles",
        element: <UserRolesPage />,
      },
    ],
  },
]);
