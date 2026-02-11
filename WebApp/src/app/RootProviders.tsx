import { Navigate, Outlet, useLocation } from "react-router-dom";
import { ThemeProvider } from "@/shared/theme/ThemeProvider";
import { AuthProvider, useAuth } from "@/contexts/AuthContext";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { useState } from "react";

import { Toaster } from "sonner";
import { GlobalApiErrorToast } from "@/components/GlobalApiErrorToast";
import { GlobalApiSuccessToast } from "@/components/GlobalApiSuccessToast";

const PUBLIC_PATHS = ["/login", "/register", "/forgot-password", "/reset-password"];

function AuthGate({ children }: { children: React.ReactNode }) {
  const { isLoading, user } = useAuth();
  const { pathname } = useLocation();

  if (isLoading) {
    return <div className="p-6 text-sm text-muted-foreground">Loading…</div>;
  }

  const isPublic = PUBLIC_PATHS.some((p) => pathname.startsWith(p));

  if (!user && !isPublic) {
    return <Navigate to="/login" replace />;
  }

  if (user && isPublic) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
}

export function RootProviders() {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            refetchOnWindowFocus: false,
            retry: 1,
            staleTime: 30_000,
          },
          mutations: {
            retry: 0,
          },
        },
      })
  );

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AuthProvider>
          <AuthGate>
            <Outlet />
          </AuthGate>

          <GlobalApiErrorToast />
          <GlobalApiSuccessToast />
          <Toaster richColors position="top-right" />
        </AuthProvider>
      </ThemeProvider>

      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}
