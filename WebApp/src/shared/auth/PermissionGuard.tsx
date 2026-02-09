import { useAuth } from "@/contexts/AuthContext";
import { Navigate } from "react-router-dom";
export function PermissionGuard({
  permission,
  children,
}: {
  permission: string;
  children: React.ReactNode;
}) {
  const { user, isLoading } = useAuth();

  // ✅ auth boot bitmeden asla forbidden gösterme
  if (isLoading) {
    return <div className="text-sm text-muted-foreground">Loading…</div>;
  }

  // login değilse
  if (!user) {
    return <Navigate to="/login" replace />;
  }

  // permission yoksa
  if (!user.permissions?.includes(permission)) {
    return (
      <div className="p-6">
        <div className="text-xl font-semibold">Forbidden</div>
        <div className="text-sm text-muted-foreground">
          You are not allowed to view this page.
        </div>
      </div>
    );
  }

  return <>{children}</>;
}
