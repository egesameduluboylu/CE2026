import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { getApi, postApi, putApi, deleteApi } from "@/lib/api";
import { Page } from "@/shared/components/Page";
import { PermissionGuard } from "@/shared/auth/PermissionGuard";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
import { Settings, Trash2, Plus, Edit, Shield } from "lucide-react";

type Role = { id: string; name: string };
type Permission = { key: string; description: string };
type RoleDetail = Role & { permissions: string[] };

type CreateRoleDto = { name: string };
type UpdateRoleDto = { name: string };
type UpdatePermissionsDto = { permissions: string[] };

export function RolesPage() {
  const [createOpen, setCreateOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [permissionsOpen, setPermissionsOpen] = useState(false);
  const [selectedRole, setSelectedRole] = useState<RoleDetail | null>(null);
  const [selectedPermissions, setSelectedPermissions] = useState<string[]>([]);

  const qc = useQueryClient();

  const rolesQ = useQuery({
    queryKey: ["admin", "roles"],
    queryFn: async () => {
      const r = await getApi<Role[]>("/admin/roles");
      return r.data as Role[];
    },
  });

  const permissionsQ = useQuery({
    queryKey: ["admin", "roles", "permissions"],
    queryFn: async () => {
      const r = await getApi<Permission[]>("/admin/roles/permissions");
      return r.data as Permission[];
    },
  });

  const roleDetailQ = useQuery({
    queryKey: ["admin", "roles", selectedRole?.id],
    queryFn: async () => {
      if (!selectedRole) return null;
      const r = await getApi<RoleDetail>(`/admin/roles/${selectedRole.id}`);
      return r.data as RoleDetail;
    },
    enabled: !!selectedRole,
  });

  const createMut = useMutation({
    mutationFn: async (dto: CreateRoleDto) => {
      await postApi("/admin/roles", dto);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["admin", "roles"] });
      toast.success("Role created");
      setCreateOpen(false);
    },
    onError: () => toast.error("Failed to create role"),
  });

  const updateMut = useMutation({
    mutationFn: async ({ id, dto }: { id: string; dto: UpdateRoleDto }) => {
      await putApi(`/admin/roles/${id}`, dto);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["admin", "roles"] });
      toast.success("Role updated");
      setEditOpen(false);
      setSelectedRole(null);
    },
    onError: () => toast.error("Failed to update role"),
  });

  const setPermissionsMut = useMutation({
    mutationFn: async ({ id, dto }: { id: string; dto: UpdatePermissionsDto }) => {
      await putApi(`/admin/roles/${id}/permissions`, dto);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["admin", "roles"] });
      qc.invalidateQueries({ queryKey: ["admin", "roles", selectedRole?.id] });
      toast.success("Permissions updated");
      setPermissionsOpen(false);
      setSelectedRole(null);
    },
    onError: () => toast.error("Failed to update permissions"),
  });

  const deleteMut = useMutation({
    mutationFn: async (id: string) => {
      await deleteApi(`/admin/roles/${id}`);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["admin", "roles"] });
      toast.success("Role deleted");
    },
    onError: () => toast.error("Failed to delete role"),
  });

  const handleEdit = (role: Role) => {
    setSelectedRole({ ...role, permissions: [] });
    setEditOpen(true);
  };

  const handlePermissions = async (role: Role) => {
    setSelectedRole({ ...role, permissions: [] });
    const detail = await getApi<RoleDetail>(`/admin/roles/${role.id}`);
    setSelectedRole(detail.data as RoleDetail);
    setSelectedPermissions((detail.data as RoleDetail).permissions);
    setPermissionsOpen(true);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this role? Users assigned to this role will lose these permissions.")) {
      deleteMut.mutate(id);
    }
  };

  return (
    <PermissionGuard permission="roles.read">
      <Page
        title="Roles"
        description="Manage roles and their permissions"
        actions={
          <PermissionGuard permission="roles.write">
            <Button size="sm" onClick={() => setCreateOpen(true)}>
              <Plus className="w-4 h-4 mr-1" />
              Create Role
            </Button>
          </PermissionGuard>
        }
      >
        {rolesQ.isLoading && <div className="text-sm text-muted-foreground">Loading…</div>}
        {rolesQ.isError && <div className="text-sm text-destructive">Error loading roles</div>}

        {!rolesQ.isLoading && !rolesQ.isError && (
          <div className="rounded-xl border overflow-hidden">
            <div className="divide-y">
              {rolesQ.data?.map((role) => (
                <div key={role.id} className="p-4 flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <Shield className="w-5 h-5 text-muted-foreground" />
                    <span className="font-medium">{role.name}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <PermissionGuard permission="roles.read">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handlePermissions(role)}
                        disabled={roleDetailQ.isLoading}
                      >
                        <Settings className="w-4 h-4 mr-1" />
                        Permissions
                      </Button>
                    </PermissionGuard>
                    <PermissionGuard permission="roles.write">
                      <Button variant="outline" size="sm" onClick={() => handleEdit(role)}>
                        <Edit className="w-4 h-4" />
                      </Button>
                      <Button variant="outline" size="sm" onClick={() => handleDelete(role.id)}>
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    </PermissionGuard>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Create Role Dialog */}
        <Dialog open={createOpen} onOpenChange={setCreateOpen}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Create Role</DialogTitle>
            </DialogHeader>
            <form
              onSubmit={(e) => {
                e.preventDefault();
                const fd = new FormData(e.currentTarget);
                createMut.mutate({ name: fd.get("name") as string });
              }}
            >
              <Label htmlFor="name">Name</Label>
              <Input id="name" name="name" required autoFocus />
              <DialogFooter className="mt-4">
                <Button type="button" variant="outline" onClick={() => setCreateOpen(false)}>
                  Cancel
                </Button>
                <Button type="submit" disabled={createMut.isPending}>
                  Create
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>

        {/* Edit Role Dialog */}
        <Dialog open={editOpen} onOpenChange={setEditOpen}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Rename Role</DialogTitle>
            </DialogHeader>
            {selectedRole && (
              <form
                onSubmit={(e) => {
                  e.preventDefault();
                  const fd = new FormData(e.currentTarget);
                  updateMut.mutate({
                    id: selectedRole.id,
                    dto: { name: fd.get("name") as string },
                  });
                }}
              >
                <Label htmlFor="edit-name">Name</Label>
                <Input id="edit-name" name="name" defaultValue={selectedRole.name} required autoFocus />
                <DialogFooter className="mt-4">
                  <Button type="button" variant="outline" onClick={() => setEditOpen(false)}>
                    Cancel
                  </Button>
                  <Button type="submit" disabled={updateMut.isPending}>
                    Save
                  </Button>
                </DialogFooter>
              </form>
            )}
          </DialogContent>
        </Dialog>

        {/* Permissions Dialog */}
        <Dialog open={permissionsOpen} onOpenChange={setPermissionsOpen}>
          <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
            <DialogHeader>
              <DialogTitle>Permissions for {selectedRole?.name}</DialogTitle>
            </DialogHeader>
            {permissionsQ.data && selectedRole && (
              <div className="space-y-2">
                {permissionsQ.data.map((p) => (
                  <div key={p.key} className="flex items-start space-x-2">
                    <Checkbox
                      id={p.key}
                      checked={selectedPermissions.includes(p.key)}
                      onCheckedChange={(checked) => {
                        if (checked) {
                          setSelectedPermissions([...selectedPermissions, p.key]);
                        } else {
                          setSelectedPermissions(selectedPermissions.filter((x) => x !== p.key));
                        }
                      }}
                    />
                    <div className="grid gap-1.5 leading-none">
                      <Label htmlFor={p.key} className="font-medium">
                        {p.key}
                      </Label>
                      <p className="text-sm text-muted-foreground">{p.description}</p>
                    </div>
                  </div>
                ))}
              </div>
            )}
            <DialogFooter className="mt-4">
              <Button variant="outline" onClick={() => setPermissionsOpen(false)}>
                Cancel
              </Button>
              <Button
                disabled={setPermissionsMut.isPending}
                onClick={() => {
                  if (selectedRole) {
                    setPermissionsMut.mutate({
                      id: selectedRole.id,
                      dto: { permissions: selectedPermissions },
                    });
                  }
                }}
              >
                Save
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </Page>
    </PermissionGuard>
  );
}
