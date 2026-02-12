import { useEffect, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link, useParams, useNavigate } from "react-router-dom";
import { getApi, putApi } from "@/lib/api";
import { useI18n } from "@/i18n/provider";
import { Page } from "@/shared/components/Page";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import { Checkbox } from "@/components/ui/checkbox";
import { PermissionGuard } from "@/shared/auth/PermissionGuard";
import { usePermission } from "@/shared/auth/usePermission";
import { toast } from "sonner";

type PermissionItem = { key: string; description?: string | null };

type RoleDetail = {
  id: string;
  name: string;
  permissions: string[];
};

export function RoleDetailPage() {
  const { t } = useI18n();
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();
  const canWrite = usePermission("roles.write");

  const roleQ = useQuery({
    queryKey: ["admin", "roles", "detail", id],
    enabled: !!id,
    queryFn: async () => {
      const r = await getApi<RoleDetail>(`/admin/roles/${id}`);
      return r.data as RoleDetail;
    },
  });

  const permsQ = useQuery({
    queryKey: ["admin", "roles", "permissions"],
    queryFn: async () => {
      const r = await getApi<PermissionItem[]>("/admin/roles/permissions");
      return r.data as PermissionItem[];
    },
  });

  const [name, setName] = useState("");
  const [selected, setSelected] = useState<Set<string>>(new Set());

  useEffect(() => {
    if (roleQ.data) {
      setName(roleQ.data.name);
      setSelected(new Set(roleQ.data.permissions ?? []));
    }
  }, [roleQ.data]);

  const renameMut = useMutation({
    mutationFn: async () => putApi(`/admin/roles/${id}`, { name: name.trim() }),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ["admin", "roles"] });
      await qc.invalidateQueries({ queryKey: ["admin", "roles", "detail", id] });
    },
  });

const savePermsMut = useMutation({
  mutationFn: async () =>
    putApi(`/admin/roles/${id}/permissions`, {
      permissions: Array.from(selected.values()),
    }),
  onSuccess: async () => {
    toast.success("Permissions updated successfully");
    await qc.invalidateQueries({ queryKey: ["admin", "roles"] });
    navigate("/roles");
  },
  onError: (err: any) => {
    toast.error(err?.message ?? "Failed to update permissions");
  },
});

  const allPerms = useMemo(() => permsQ.data ?? [], [permsQ.data]);

  const toggle = (k: string, on: boolean) => {
    setSelected((prev) => {
      const next = new Set(prev);
      if (on) next.add(k);
      else next.delete(k);
      return next;
    });
  };

  return (
    <PermissionGuard permission="roles.read">
      <Page
        title="Role"
        description={roleQ.data?.name ?? "—"}
        actions={
          <div className="flex gap-2">
            <Button variant="outline" asChild>
              <Link to="/roles">Back</Link>
            </Button>

            {canWrite && (
              <Button
                disabled={
                  savePermsMut.isPending ||
                  roleQ.isLoading ||
                  permsQ.isLoading
                }
                onClick={() => savePermsMut.mutate()}
              >
                {savePermsMut.isPending ? "Saving…" : "Save permissions"}
              </Button>
            )}
          </div>
        }
      >
        {(roleQ.isLoading || permsQ.isLoading) && (
          <div className="text-sm text-muted-foreground">Loading…</div>
        )}

        {(roleQ.isError || permsQ.isError) && (
          <div className="rounded-xl border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
            {roleQ.error instanceof Error
              ? roleQ.error.message
              : permsQ.error instanceof Error
              ? permsQ.error.message
              : "Failed to load"}
          </div>
        )}

        {roleQ.data && (
          <div className="grid gap-4 lg:grid-cols-2">
            {/* Rename */}
            <div className="rounded-2xl border p-4 space-y-3">
              <div className="text-sm font-semibold">Role name</div>

              <div className="flex gap-2">
                <Input value={name} onChange={(e) => setName(e.target.value)} />
                <Button
                  variant="outline"
                  disabled={!canWrite || !name.trim() || renameMut.isPending}
                  onClick={() => renameMut.mutate()}
                >
                  {renameMut.isPending ? "Renaming…" : "Rename"}
                </Button>
              </div>

              {!canWrite && (
                <div className="text-xs text-muted-foreground">
                  You don’t have permission to edit roles.
                </div>
              )}

              {renameMut.isError && (
                <div className="rounded-xl border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
                  {renameMut.error instanceof Error
                    ? renameMut.error.message
                    : String(renameMut.error)}
                </div>
              )}
            </div>

            {/* Permissions */}
            <div className="rounded-2xl border p-4">
              <div className="flex items-center justify-between">
                <div className="text-sm font-semibold">Permissions</div>
                <div className="text-xs text-muted-foreground">
                  Selected: {selected.size}
                </div>
              </div>

              <Separator className="my-3" />

              <div className="space-y-2">
                {allPerms.length ? (
                  allPerms.map((p) => {
                    const checked = selected.has(p.key);
                    return (
                      <label
                        key={p.key}
                        className="flex items-start gap-3 rounded-xl border p-3 hover:bg-muted/40 cursor-pointer"
                      >
                        <Checkbox
                          checked={checked}
                          onCheckedChange={(v) => toggle(p.key, v === true)}
                          disabled={!canWrite}
                        />
                        <div className="min-w-0">
                          <div className="text-sm font-medium">{p.key}</div>
                          <div className="text-xs text-muted-foreground">
                            {p.description ?? "—"}
                          </div>
                        </div>
                      </label>
                    );
                  })
                ) : (
                  <div className="text-sm text-muted-foreground">
                    No permissions found.
                  </div>
                )}
              </div>

              {savePermsMut.isError && (
                <div className="mt-3 rounded-xl border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
                  {savePermsMut.error instanceof Error
                    ? savePermsMut.error.message
                    : String(savePermsMut.error)}
                </div>
              )}
            </div>
          </div>
        )}
      </Page>
    </PermissionGuard>
  );
}
