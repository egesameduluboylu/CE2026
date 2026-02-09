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
import { NewUserPage } from "@/pages/users/NewUserPage";
import { RolesPage } from "@/pages/RolesPage";
import { RoleDetailPage } from "@/pages/RoleDetailPage";

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
          { path: "roles", element: <RolesPage /> },
          { path: "roles/:id", element: <RoleDetailPage /> },

        ],
      },

      // ✅ fallback
      { path: "*", element: <Navigate to="/" replace /> },
    ],
  },
]);
