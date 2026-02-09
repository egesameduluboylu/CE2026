import { createBrowserRouter, Navigate } from "react-router-dom";
import { RootProviders } from "@/app/RootProviders";
import { AppShell } from "@/app/layouts/AppShell";

import { Login } from "@/pages/Login";
import { Register } from "@/pages/Register";
import { ForgotPassword } from "@/pages/ForgotPassword";
import { ResetPassword } from "@/pages/ResetPassword";

import { UsersPage } from "@/pages/users/UsersPage";
import { DashboardPage } from "@/pages/DashboardPage";
import { UserDetailPage } from "@/pages/users/UserDetailPage";
import { UserRolesPage } from "@/pages/UserRolesPage";
import { NewUserPage } from "@/pages/users/NewUserPage";
import { RolesPage } from "@/pages/RolesPage";
import { RoleDetailPage } from "@/pages/RoleDetailPage";
import { TenantsPage } from "@/pages/TenantsPage";
import { TenantUsagePage } from "@/pages/TenantUsagePage";
import RateLimitPage from "@/pages/RateLimitPage";
import { PlansPage } from "@/pages/PlansPage";
import { FeatureFlagsPage } from "@/pages/FeatureFlagsPage";
import { AuditPage } from "@/pages/AuditPage";
import { SessionsPage } from "@/pages/SessionsPage";
import { ApiKeysPage } from "@/pages/ApiKeysPage";
import { WebhooksPage } from "@/pages/WebhooksPage";
import { HealthPage } from "@/pages/HealthPage";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <RootProviders />,
    children: [
      // ✅ public
      { path: "login", element: <Login /> },
      { path: "register", element: <Register /> },
      { path: "forgot-password", element: <ForgotPassword /> },
      { path: "reset-password", element: <ResetPassword /> },

      // ✅ protected shell (AppShell içinde zaten auth guard olmalı)
      {
        element: <AppShell />,
        children: [
          { index: true, element: <DashboardPage /> },
          { path: "users", element: <UsersPage /> },
          { path: "users/new", element: <NewUserPage /> },
          { path: "users/:id", element: <UserDetailPage /> },
          { path: "users/:id/roles", element: <UserRolesPage /> },
          { path: "roles", element: <RolesPage /> },
          { path: "roles/:id", element: <RoleDetailPage /> },
          { path: "tenants", element: <TenantsPage /> },
          { path: "tenants/:id/usage", element: <TenantUsagePage /> },
          { path: "rate-limit", element: <RateLimitPage /> },
          { path: "plans", element: <PlansPage /> },
          { path: "feature-flags", element: <FeatureFlagsPage /> },
          { path: "audit", element: <AuditPage /> },
          { path: "sessions", element: <SessionsPage /> },
          { path: "api-keys", element: <ApiKeysPage /> },
          { path: "webhooks", element: <WebhooksPage /> },
          { path: "health", element: <HealthPage /> },

        ],
      },

      // ✅ fallback
      { path: "*", element: <Navigate to="/" replace /> },
    ],
  },
]);
