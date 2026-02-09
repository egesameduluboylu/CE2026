import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getApi, postApi } from "@/lib/api";
import { Page } from "@/shared/components/Page";
import { PermissionGuard } from "@/shared/auth/PermissionGuard";
import { usePermission } from "@/shared/auth/usePermission";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";

type PlanListItem = {
  id: string;
  name: string;
  maxUsers: number;
  apiCallsPerMinute: number;
  webhooksCount: number;
  isActive: boolean;
  createdAt: string;
};

export function PlansPage() {
  const qc = useQueryClient();
  const canWrite = usePermission("billing.write");

  const [name, setName] = useState("");
  const [maxUsers, setMaxUsers] = useState("10");
  const [apiCallsPerMinute, setApiCallsPerMinute] = useState("60");
  const [webhooksCount, setWebhooksCount] = useState("5");

  const q = useQuery({
    queryKey: ["admin", "plans"],
    queryFn: async () => {
      const r = await getApi<PlanListItem[]>("/admin/plans");
      return r.data as PlanListItem[];
    },
  });

  const createMut = useMutation({
    mutationFn: async () =>
      postApi("/admin/plans", {
        name: name.trim(),
        maxUsers: Number(maxUsers) || 0,
        apiCallsPerMinute: Number(apiCallsPerMinute) || 0,
        webhooksCount: Number(webhooksCount) || 0,
      }),
    onSuccess: async () => {
      setName("");
      await qc.invalidateQueries({ queryKey: ["admin", "plans"] });
    },
  });

  const items = useMemo(() => q.data ?? [], [q.data]);

  return (
    <PermissionGuard permission="billing.read">
      <Page
        title="Plans"
        description="Define tenant plans and limits"
        actions={
          canWrite ? (
            <div className="flex flex-wrap items-center gap-2">
              <Input
                className="w-[200px]"
                placeholder="Plan name…"
                value={name}
                onChange={(e) => setName(e.target.value)}
              />
              <Input
                className="w-[120px]"
                placeholder="Max users"
                value={maxUsers}
                onChange={(e) => setMaxUsers(e.target.value)}
              />
              <Input
                className="w-[140px]"
                placeholder="API/min"
                value={apiCallsPerMinute}
                onChange={(e) => setApiCallsPerMinute(e.target.value)}
              />
              <Input
                className="w-[140px]"
                placeholder="Webhooks"
                value={webhooksCount}
                onChange={(e) => setWebhooksCount(e.target.value)}
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
                items.map((p) => (
                  <div key={p.id} className="flex items-center justify-between gap-3 p-4">
                    <div className="min-w-0">
                      <div className="flex items-center gap-2">
                        <div className="text-sm font-medium truncate">{p.name}</div>
                        <Badge variant={p.isActive ? "default" : "secondary"}>
                          {p.isActive ? "Active" : "Inactive"}
                        </Badge>
                      </div>
                      <div className="text-xs text-muted-foreground">
                        maxUsers: {p.maxUsers} • api/min: {p.apiCallsPerMinute} • webhooks: {p.webhooksCount}
                      </div>
                      <div className="text-xs text-muted-foreground truncate">{p.id}</div>
                    </div>
                  </div>
                ))
              ) : (
                <div className="p-6 text-sm text-muted-foreground">No plans yet.</div>
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
