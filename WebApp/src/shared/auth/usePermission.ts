import { useAuth } from "@/contexts/AuthContext";

export function usePermission(permission: string): boolean {
  const { user } = useAuth();
  const perms = (user as any)?.permissions as string[] | undefined;
  return !!perms?.includes(permission);
}
