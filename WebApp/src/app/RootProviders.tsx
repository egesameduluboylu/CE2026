import { Outlet } from "react-router-dom";
import { ThemeProvider } from "@/shared/theme/ThemeProvider";
import { AuthProvider, useAuth } from "@/contexts/AuthContext";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { useState } from "react";

import { Toaster } from "sonner";
import { GlobalApiErrorToast } from "@/components/GlobalApiErrorToast";
import { GlobalApiSuccessToast } from "@/components/GlobalApiSuccessToast";

function AuthGate({ children }: { children: React.ReactNode }) {
  const { isLoading } = useAuth();
  if (isLoading) {
    return <div className="p-6 text-sm text-muted-foreground">Loading…</div>;
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
