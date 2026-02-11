import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { AppShell } from "@/app/layouts/AppShell";
import Login from "@/pages/Login";
import DashboardPage from "@/pages/DashboardPage";
import UsersPage from "@/pages/users/UsersPage";
import RolesPage from "@/pages/RolesPage";
import TenantsPage from "@/pages/TenantsPage";
import ApiKeysPage from "@/pages/ApiKeysPage";
import HealthPage from "@/pages/HealthPage";
import SecurityEventsPage from "@/pages/SecurityEventsPage";
import SettingsPage from "@/pages/SettingsPage";

export const router = createBrowserRouter([
  {
    path: "/login",
    element: <Login />,
  },
  {
    path: "/",
    element: <AppShell />,
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
        path: "roles",
        element: <RolesPage />,
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
        element: <SecurityEventsPage />,
      },
      {
        path: "settings",
        element: <SettingsPage />,
      },
    ],
  },
]);
