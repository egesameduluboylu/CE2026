import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getApi, postApi } from "@/lib/api";
import { Page } from "@/shared/components/Page";
import { PermissionGuard } from "@/shared/auth/PermissionGuard";
import { usePermission } from "@/shared/auth/usePermission";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";

type SessionItem = {
  id: string;
  userId: string;
  expiresAt: string;
  revokedAt: string | null;
  active: boolean;
};

export function SessionsPage() {
  const qc = useQueryClient();
  const canWrite = usePermission("sessions.write");

  const [userId, setUserId] = useState("");
  const [activeOnly, setActiveOnly] = useState(true);

  const q = useQuery({
    queryKey: ["admin", "sessions", userId, activeOnly],
    queryFn: async () => {
      const qs = new URLSearchParams();
      if (userId.trim()) qs.set("userId", userId.trim());
      qs.set("activeOnly", activeOnly ? "true" : "false");

      const r = await getApi<SessionItem[]>(`/admin/sessions?${qs.toString()}`);
      return r.data as SessionItem[];
    },
  });

  const revokeMut = useMutation({
    mutationFn: async (id: string) => postApi(`/admin/sessions/${id}/revoke`),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ["admin", "sessions"] });
    },
  });

  const items = useMemo(() => q.data ?? [], [q.data]);

  return (
    <PermissionGuard permission="sessions.read">
      <Page
        title="Sessions"
        description="Refresh-token based sessions (MVP)"
        actions={
          <div className="flex flex-wrap items-center gap-2">
            <Input
              className="w-[280px]"
              placeholder="Filter by userId (GUID)…"
              value={userId}
              onChange={(e) => setUserId(e.target.value)}
            />
            <Button
              variant={activeOnly ? "default" : "outline"}
              onClick={() => setActiveOnly((v) => !v)}
            >
              {activeOnly ? "Active only" : "All"}
            </Button>
          </div>
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
                items.map((s) => (
                  <div key={s.id} className="flex items-center justify-between gap-3 p-4">
                    <div className="min-w-0">
                      <div className="flex items-center gap-2">
                        <Badge variant={s.active ? "default" : "secondary"}>
                          {s.active ? "Active" : "Inactive"}
                        </Badge>
                        <div className="text-sm font-medium truncate">{s.id}</div>
                      </div>
                      <div className="text-xs text-muted-foreground truncate">
                        userId: {s.userId} • expires: {new Date(s.expiresAt).toLocaleString()}
                      </div>
                    </div>

                    {canWrite ? (
                      <Button
                        size="sm"
                        variant="destructive"
                        disabled={!s.active || revokeMut.isPending}
                        onClick={() => revokeMut.mutate(s.id)}
                      >
                        Revoke
                      </Button>
                    ) : null}
                  </div>
                ))
              ) : (
                <div className="p-6 text-sm text-muted-foreground">No sessions.</div>
              )}
            </div>
          </div>
        )}

        {revokeMut.isError && (
          <div className="mt-3 rounded-xl border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
            {revokeMut.error instanceof Error
              ? revokeMut.error.message
              : String(revokeMut.error)}
          </div>
        )}
      </Page>
    </PermissionGuard>
  );
}
