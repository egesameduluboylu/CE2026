import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { getApi, postApi, putApi } from "@/lib/api";
import { Page } from "@/shared/components/Page";
import { PermissionGuard } from "@/shared/auth/PermissionGuard";
import { usePermission } from "@/shared/auth/usePermission";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { BarChart3 } from "lucide-react";

type TenantListItem = {
  id: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  planId?: string | null;
  planName?: string | null;
  usage?: {
    apiCalls?: number;
    webhooksCount?: number;
  };
};

type PlanListItem = {
  id: string;
  name: string;
  isActive: boolean;
};

export function TenantsPage() {
  const qc = useQueryClient();
  const canWrite = usePermission("tenants.write");
  const canBillingWrite = usePermission("billing.write");

  const [name, setName] = useState("");

  const q = useQuery({
    queryKey: ["admin", "tenants"],
    queryFn: async () => {
      const r = await getApi<TenantListItem[]>("/admin/tenants");
      return r.data as TenantListItem[];
    },
  });

  const plansQ = useQuery({
    queryKey: ["admin", "plans"],
    queryFn: async () => {
      const r = await getApi<PlanListItem[]>("/admin/plans");
      return (r.data as any as PlanListItem[]).filter((x) => x.isActive);
    },
  });

  const usageQ = useQuery({
    queryKey: ["admin", "tenants", "usage"],
    queryFn: async () => {
      const r = await getApi<Record<string, { apiCalls?: number; webhooksCount?: number }>>("/admin/tenants/usage");
      return r.data as Record<string, { apiCalls?: number; webhooksCount?: number }>;
    },
  });

  const createMut = useMutation({
    mutationFn: async () => postApi("/admin/tenants", { name: name.trim() }),
    onSuccess: async () => {
      setName("");
      await qc.invalidateQueries({ queryKey: ["admin", "tenants"] });
    },
  });

  const renameMut = useMutation({
    mutationFn: async (p: { id: string; name: string }) =>
      putApi(`/admin/tenants/${p.id}`, { name: p.name.trim() }),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ["admin", "tenants"] });
    },
  });

  const statusMut = useMutation({
    mutationFn: async (p: { id: string; isActive: boolean }) =>
      putApi(`/admin/tenants/${p.id}/status`, { isActive: p.isActive }),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ["admin", "tenants"] });
    },
  });

  const setPlanMut = useMutation({
    mutationFn: async (p: { tenantId: string; planId: string }) =>
      putApi(`/admin/tenants/${p.tenantId}/plan`, { planId: p.planId }),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ["admin", "tenants"] });
    },
  });

  const items = useMemo(() => {
    const tenants = q.data ?? [];
    const usage = usageQ.data ?? {};
    return tenants.map((t) => ({ ...t, usage: usage[t.id] }));
  }, [q.data, usageQ.data]);

  return (
    <PermissionGuard permission="tenants.read">
      <Page
        title="Tenants"
        description="Manage tenants (MVP header-based resolution: X-Tenant-Id)"
        actions={
          canWrite ? (
            <div className="flex items-center gap-2">
              <Input
                className="w-[240px]"
                placeholder="New tenant name…"
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
                items.map((t) => (
                  <TenantRow
                    key={t.id}
                    item={t}
                    canWrite={canWrite}
                    canBillingWrite={canBillingWrite}
                    plans={plansQ.data ?? []}
                    renamePending={renameMut.isPending}
                    statusPending={statusMut.isPending}
                    planPending={setPlanMut.isPending}
                    onRename={(newName) => renameMut.mutate({ id: t.id, name: newName })}
                    onSetStatus={(isActive) => statusMut.mutate({ id: t.id, isActive })}
                    onSetPlan={(planId) => setPlanMut.mutate({ tenantId: t.id, planId })}
                  />
                ))
              ) : (
                <div className="p-6 text-sm text-muted-foreground">
                  No tenants yet.
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

function TenantRow({
  item,
  canWrite,
  canBillingWrite,
  plans,
  renamePending,
  statusPending,
  planPending,
  onRename,
  onSetStatus,
  onSetPlan,
}: {
  item: TenantListItem;
  canWrite: boolean;
  canBillingWrite: boolean;
  plans: PlanListItem[];
  renamePending: boolean;
  statusPending: boolean;
  planPending: boolean;
  onRename: (name: string) => void;
  onSetStatus: (isActive: boolean) => void;
  onSetPlan: (planId: string) => void;
}) {
  const [editing, setEditing] = useState(false);
  const [name, setName] = useState(item.name);

  const save = () => {
    const next = name.trim();
    if (!next || next === item.name) {
      setEditing(false);
      setName(item.name);
      return;
    }
    onRename(next);
    setEditing(false);
  };

  return (
    <div className="flex items-center justify-between gap-3 p-4">
      <div className="min-w-0">
        <div className="flex items-center gap-2">
          {editing ? (
            <Input
              className="h-8 w-[240px]"
              value={name}
              onChange={(e) => setName(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") save();
                if (e.key === "Escape") {
                  setEditing(false);
                  setName(item.name);
                }
              }}
              disabled={!canWrite || renamePending}
            />
          ) : (
            <div className="text-sm font-medium truncate">{item.name}</div>
          )}

          <Badge variant={item.isActive ? "default" : "secondary"}>
            {item.isActive ? "Active" : "Suspended"}
          </Badge>

          {item.planName ? (
            <Badge variant="outline">{item.planName}</Badge>
          ) : (
            <Badge variant="outline">No plan</Badge>
          )}
          {item.usage && (
          <div className="flex gap-2 mt-1">
            {typeof item.usage.apiCalls === "number" && (
              <Badge variant="secondary" className="text-xs">
                API calls: {item.usage.apiCalls}
              </Badge>
            )}
            {typeof item.usage.webhooksCount === "number" && (
              <Badge variant="secondary" className="text-xs">
                Webhooks: {item.usage.webhooksCount}
              </Badge>
            )}
          </div>
        )}
        </div>

        <div className="text-xs text-muted-foreground truncate">{item.id}</div>
      </div>

      {canWrite ? (
        <div className="flex items-center gap-2">
          {editing ? (
            <>
              <Button size="sm" onClick={save} disabled={renamePending}>
                Save
              </Button>
              <Button
                size="sm"
                variant="outline"
                onClick={() => {
                  setEditing(false);
                  setName(item.name);
                }}
              >
                Cancel
              </Button>
            </>
          ) : (
            <>
              <Button
                size="sm"
                variant="outline"
                onClick={() => setEditing(true)}
              >
                Rename
              </Button>
              <Button
                size="sm"
                variant={item.isActive ? "destructive" : "default"}
                disabled={statusPending}
                onClick={() => onSetStatus(!item.isActive)}
              >
                {item.isActive ? "Suspend" : "Activate"}
              </Button>
            </>
          )}
        </div>
      ) : null}

      {canBillingWrite ? (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button size="sm" variant="outline" disabled={planPending || !plans.length}>
              Set plan
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" sideOffset={8} className="w-56">
            {plans.map((p) => (
              <DropdownMenuItem
                key={p.id}
                onSelect={(e) => {
                  e.preventDefault();
                  onSetPlan(p.id);
                }}
              >
                {p.name}
              </DropdownMenuItem>
            ))}
          </DropdownMenuContent>

      {canBillingWrite && (
        <Button size="sm" variant="outline" asChild>
          <Link to={`/tenants/${item.id}/usage`}>
            <BarChart3 className="w-4 h-4 mr-1" />
            Usage
          </Link>
        </Button>
      )}
        </DropdownMenu>
      ) : null}
    </div>
  );
}
