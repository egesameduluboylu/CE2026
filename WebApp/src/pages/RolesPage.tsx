import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { getApi, postApi } from "@/lib/api";
import { Page } from "@/shared/components/Page";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { PermissionGuard } from "@/shared/auth/PermissionGuard";
import { usePermission } from "@/shared/auth/usePermission";

type RoleListItem = { id: string; name: string };

export function RolesPage() {
  const qc = useQueryClient();
  const canWrite = usePermission("roles.write");

  const [name, setName] = useState("");

  const q = useQuery({
    queryKey: ["admin", "roles"],
    queryFn: async () => {
      const r = await getApi<RoleListItem[]>("/admin/roles");
      return r.data as RoleListItem[];
    },
  });

  const createMut = useMutation({
    mutationFn: async () =>
      postApi<RoleListItem>("/admin/roles", { name: name.trim() }),
    onSuccess: async () => {
      setName("");
      await qc.invalidateQueries({ queryKey: ["admin", "roles"] });
    },
  });

  const items = useMemo(() => q.data ?? [], [q.data]);

  return (
    <PermissionGuard permission="roles.read">
      <Page
        title="Roles"
        description="Define roles and manage permissions"
        actions={
          canWrite ? (
            <div className="flex items-center gap-2">
              <Input
                className="w-[240px]"
                placeholder="New role name…"
                value={name}
                onChange={(e) => setName(e.target.value)}
              />
              <Button
                disabled={!name.trim() || createMut.isPending}
                onClick={() => createMut.mutate()}
              >
                {createMut.isPending ? "Creating…" : "Create"}
              </Button>
            </div>
          ) : null
        }
      >
        {q.isLoading && (
          <div className="text-sm text-muted-foreground">Loading…</div>
        )}

        {q.isError && (
          <div className="rounded-xl border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
            {q.error instanceof Error ? q.error.message : String(q.error)}
          </div>
        )}

        {!q.isLoading && !q.isError && (
          <div className="rounded-2xl border overflow-hidden">
            <div className="divide-y">
              {items.length ? (
                items.map((r) => (
                  <div key={r.id} className="flex items-center justify-between p-4">
                    <div className="min-w-0">
                      <div className="text-sm font-medium truncate">{r.name}</div>
                      <div className="text-xs text-muted-foreground truncate">
                        {r.id}
                      </div>
                    </div>

                    <Button variant="outline" size="sm" asChild>
                      <Link to={`/roles/${r.id}`}>Manage</Link>
                    </Button>
                  </div>
                ))
              ) : (
                <div className="p-6 text-sm text-muted-foreground">
                  No roles yet.
                </div>
              )}
            </div>
          </div>
        )}

        {createMut.isError && (
          <div className="mt-3 rounded-xl border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
            {createMut.error instanceof Error
              ? createMut.error.message
              : String(createMut.error)}
          </div>
        )}
      </Page>
    </PermissionGuard>
  );
}
