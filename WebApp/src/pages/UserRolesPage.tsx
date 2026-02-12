import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useParams, useNavigate } from "react-router-dom";
import { getApi, putApi } from "@/lib/api";
import { useI18n } from "@/i18n/provider";
import { Page } from "@/shared/components/Page";
import { PermissionGuard } from "@/shared/auth/PermissionGuard";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";
import { ArrowLeft, Shield, Save } from "lucide-react";

type User = { id: string; email: string; fullName?: string };
type Role = { id: string; name: string };
type UserRole = { userId: string; roleIds: string[] };

export function UserRolesPage() {
  const { t } = useI18n();
  const { userId } = useParams<{ userId: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();

  const [open, setOpen] = useState(false);
  const [selectedRoleIds, setSelectedRoleIds] = useState<string[]>([]);

  const userQ = useQuery({
    queryKey: ["admin", "users", userId],
    queryFn: async () => {
      const r = await getApi<User>(`/admin/users/${userId}`);
      return r.data as User;
    },
    enabled: !!userId,
  });

  const rolesQ = useQuery({
    queryKey: ["admin", "roles"],
    queryFn: async () => {
      const r = await getApi<Role[]>("/admin/roles");
      return r.data as Role[];
    },
  });

  const userRolesQ = useQuery({
    queryKey: ["admin", "users", userId, "roles"],
    queryFn: async () => {
      const r = await getApi<UserRole>(`/admin/users/${userId}/roles`);
      return r.data as UserRole;
    },
    enabled: !!userId,
  });

  const setRolesMut = useMutation({
    mutationFn: async (dto: { roleIds: string[] }) => {
      await putApi(`/admin/users/${userId}/roles`, dto);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["admin", "users", userId, "roles"] });
      toast.success("User roles updated");
      setOpen(false);
    },
    onError: () => toast.error("Failed to update user roles"),
  });

  const handleOpen = () => {
    setSelectedRoleIds(userRolesQ.data?.roleIds ?? []);
    setOpen(true);
  };

  const handleSave = () => {
    setRolesMut.mutate({ roleIds: selectedRoleIds });
  };

  return (
    <PermissionGuard permission="roles.read">
      <Page
        title="User Roles"
        description={`Manage role assignments for ${userQ.data?.email ?? "…"}`}
        actions={
          <PermissionGuard permission="roles.write">
            <Button size="sm" onClick={handleOpen} disabled={!userQ.data || !rolesQ.data}>
              <Shield className="w-4 h-4 mr-1" />
              Assign Roles
            </Button>
          </PermissionGuard>
        }
      >
        <div className="space-y-4">
          <Button variant="ghost" size="sm" onClick={() => navigate("/users")}>
            <ArrowLeft className="w-4 h-4 mr-1" />
            Back to Users
          </Button>

          {userQ.isLoading && <div className="text-sm text-muted-foreground">Loading user…</div>}
          {userQ.isError && <div className="text-sm text-destructive">Error loading user</div>}

          {userQ.data && (
            <div className="rounded-xl border p-4">
              <div className="font-medium">{userQ.data.email}</div>
              {userQ.data.fullName && (
                <div className="text-sm text-muted-foreground">{userQ.data.fullName}</div>
              )}
            </div>
          )}

          {userRolesQ.isLoading && <div className="text-sm text-muted-foreground">Loading roles…</div>}
          {userRolesQ.isError && <div className="text-sm text-destructive">Error loading roles</div>}

          {userRolesQ.data && rolesQ.data && (
            <div className="rounded-xl border overflow-hidden">
              <div className="divide-y">
                {rolesQ.data.length ? (
                  rolesQ.data.map((role) => (
                    <div key={role.id} className="p-4 flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <Shield className="w-5 h-5 text-muted-foreground" />
                        <span className="font-medium">{role.name}</span>
                      </div>
                      <Badge variant={userRolesQ.data.roleIds.includes(role.id) ? "default" : "secondary"}>
                        {userRolesQ.data.roleIds.includes(role.id) ? "Assigned" : "Not assigned"}
                      </Badge>
                    </div>
                  ))
                ) : (
                  <div className="p-4 text-sm text-muted-foreground">No roles defined.</div>
                )}
              </div>
            </div>
          )}
        </div>

        {/* Assign Roles Dialog */}
        <Dialog open={open} onOpenChange={setOpen}>
          <DialogContent className="max-w-md">
            <DialogHeader>
              <DialogTitle>Assign Roles</DialogTitle>
            </DialogHeader>
            {rolesQ.data && (
              <div className="space-y-2">
                {rolesQ.data.map((role) => (
                  <div key={role.id} className="flex items-center space-x-2">
                    <Checkbox
                      id={role.id}
                      checked={selectedRoleIds.includes(role.id)}
                      onCheckedChange={(checked) => {
                        if (checked) {
                          setSelectedRoleIds([...selectedRoleIds, role.id]);
                        } else {
                          setSelectedRoleIds(selectedRoleIds.filter((x) => x !== role.id));
                        }
                      }}
                    />
                    <Label htmlFor={role.id} className="font-medium">
                      {role.name}
                    </Label>
                  </div>
                ))}
              </div>
            )}
            <DialogFooter className="mt-4">
              <Button variant="outline" onClick={() => setOpen(false)}>
                Cancel
              </Button>
              <Button disabled={setRolesMut.isPending} onClick={handleSave}>
                <Save className="w-4 h-4 mr-1" />
                Save
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </Page>
    </PermissionGuard>
  );
}
