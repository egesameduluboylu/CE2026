import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link, useParams } from "react-router-dom";
import { getApi, postApi } from "@/lib/api";
import { Page } from "@/shared/components/Page";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { usePermission } from "@/shared/auth/usePermission";

type UserDetail = {
  id: string;
  email: string;
  createdAt: string;
  isAdmin: boolean;
  failedLoginCount: number;
  lockoutUntil: string | null;
  lastFailedLoginAt?: string | null;
};

type RefreshTokenItem = {
  id: string;
  expiresAt: string;
  revokedAt: string | null;
  active: boolean;
};

type UserDetailResponse = {
  user: UserDetail;
  refreshTokens: RefreshTokenItem[];
};

function fmt(iso?: string | null) {
  if (!iso) return "—";
  const d = new Date(iso);
  return Number.isNaN(d.getTime()) ? "—" : d.toLocaleString();
}

export function UserDetailPage() {
  const { id } = useParams<{ id: string }>();
  const qc = useQueryClient();

  // ✅ permissions
  const canReadUsers = usePermission("users.read");
  const canWriteUsers = usePermission("users.write");

  // ✅ page guard
  if (!canReadUsers) {
    return (
      <Page title="User" description="—" actions={
        <Button variant="outline" asChild>
          <Link to="/">Back</Link>
        </Button>
      }>
        <div className="rounded-xl border p-3 text-sm text-muted-foreground">
          You are not allowed to view this page.
        </div>
      </Page>
    );
  }

  const q = useQuery({
    queryKey: ["admin", "users", "detail", id],
    enabled: !!id,
    queryFn: async () => {
      const r = await getApi<UserDetailResponse>(`/admin/users/${id}`);
      return r.data as UserDetailResponse;
    },
  });

  // Actions (write permission required)
  const lockMut = useMutation({
    mutationFn: async () => postApi(`/admin/users/${id}/lock`, { minutes: 30 }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["admin", "users", "detail", id] }),
  });

  const unlockMut = useMutation({
    mutationFn: async () => postApi(`/admin/users/${id}/unlock`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["admin", "users", "detail", id] }),
  });

  const adminMut = useMutation({
    mutationFn: async (isAdmin: boolean) => postApi(`/admin/users/${id}/admin`, { isAdmin }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["admin", "users", "detail", id] }),
  });

  const resetFailedMut = useMutation({
    mutationFn: async () => postApi(`/admin/users/${id}/reset-failed`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["admin", "users", "detail", id] }),
  });

  const revokeMut = useMutation({
    mutationFn: async () => postApi<{ revoked: number }>(`/admin/users/${id}/revoke-refresh-tokens`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["admin", "users", "detail", id] }),
  });

  const u = q.data?.user;
  const tokens = q.data?.refreshTokens ?? [];
  const activeCount = tokens.filter((t) => t.active).length;

  const isLocked =
    !!u?.lockoutUntil && new Date(u.lockoutUntil).getTime() > Date.now();

  return (
    <Page
      title="User"
      description={u?.email ?? "—"}
      actions={
        <div className="flex gap-2">
          <Button variant="outline" asChild>
            <Link to="/users">Back</Link>
          </Button>
        </div>
      }
    >
      {!id && (
        <div className="rounded-xl border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
          Route param not found. Check router param name (should be :id).
        </div>
      )}

      {q.isLoading && <div className="text-sm text-muted-foreground">Loading…</div>}

      {q.isError && (
        <div className="rounded-xl border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
          {q.error instanceof Error ? q.error.message : String(q.error)}
        </div>
      )}

      {u && (
        <div className="grid gap-4 md:grid-cols-2">
          <div className="rounded-2xl border p-4 space-y-3">
            <div className="flex items-center justify-between">
              <div className="text-sm font-medium">Role</div>
              {u.isAdmin ? <Badge>Admin</Badge> : <Badge variant="secondary">User</Badge>}
            </div>

            <Row label="Id" value={u.id} mono />
            <Row label="Created" value={fmt(u.createdAt)} />
            <Row label="Failed logins" value={String(u.failedLoginCount)} />
            <Row label="Lockout until" value={fmt(u.lockoutUntil)} />
            <Row label="Last failed login" value={fmt(u.lastFailedLoginAt ?? null)} />

            {/* ✅ actions only if users.write */}
            {canWriteUsers && (
              <div className="pt-2 flex flex-wrap gap-2">
                {isLocked ? (
                  <Button onClick={() => unlockMut.mutate()} disabled={unlockMut.isPending}>
                    {unlockMut.isPending ? "Unlocking..." : "Unlock"}
                  </Button>
                ) : (
                  <Button onClick={() => lockMut.mutate()} disabled={lockMut.isPending}>
                    {lockMut.isPending ? "Locking..." : "Lock 30m"}
                  </Button>
                )}

                <Button
                  variant="outline"
                  onClick={() => adminMut.mutate(!u.isAdmin)}
                  disabled={adminMut.isPending}
                >
                  {u.isAdmin ? "Revoke admin" : "Make admin"}
                </Button>

                <Button
                  variant="outline"
                  onClick={() => resetFailedMut.mutate()}
                  disabled={resetFailedMut.isPending}
                >
                  Reset failed logins
                </Button>
              </div>
            )}

            {!canWriteUsers && (
              <div className="pt-2 text-xs text-muted-foreground">
                You don’t have permission to manage this user.
              </div>
            )}
          </div>

          <div className="rounded-2xl border p-4 space-y-3">
            <div className="flex items-center justify-between">
              <div className="text-sm font-medium">Sessions</div>

              {/* ✅ revoke requires write */}
              <Button
                size="sm"
                variant="outline"
                disabled={!canWriteUsers || revokeMut.isPending || activeCount === 0}
                onClick={() => revokeMut.mutate()}
              >
                {revokeMut.isPending ? "Revoking..." : `Revoke active (${activeCount})`}
              </Button>
            </div>

            <div className="text-xs text-muted-foreground">
              Total: <span className="font-medium text-foreground">{tokens.length}</span> • Active:{" "}
              <span className="font-medium text-foreground">{activeCount}</span>
            </div>

            <div className="rounded-xl border overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-muted/40">
                  <tr className="text-left text-muted-foreground">
                    <th className="px-3 py-2 font-medium">Token</th>
                    <th className="px-3 py-2 font-medium">Expires</th>
                    <th className="px-3 py-2 font-medium">Revoked</th>
                    <th className="px-3 py-2 font-medium">Status</th>
                  </tr>
                </thead>
                <tbody>
                  {tokens.length ? (
                    tokens.map((t) => (
                      <tr key={t.id} className="border-t">
                        <td className="px-3 py-2 font-mono text-xs">{t.id}</td>
                        <td className="px-3 py-2 text-muted-foreground">{fmt(t.expiresAt)}</td>
                        <td className="px-3 py-2 text-muted-foreground">{fmt(t.revokedAt)}</td>
                        <td className="px-3 py-2">
                          {t.active ? <Badge>Active</Badge> : <Badge variant="secondary">Inactive</Badge>}
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={4} className="px-3 py-8 text-center text-sm text-muted-foreground">
                        No refresh tokens.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>

            {revokeMut.isError && (
              <div className="rounded-xl border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
                {revokeMut.error instanceof Error ? revokeMut.error.message : String(revokeMut.error)}
              </div>
            )}
          </div>
        </div>
      )}
    </Page>
  );
}

function Row({ label, value, mono }: { label: string; value: string; mono?: boolean }) {
  return (
    <div className="flex items-center justify-between border-b py-2 last:border-b-0">
      <div className="text-xs text-muted-foreground">{label}</div>
      <div className={mono ? "font-mono text-xs" : "text-sm"}>{value}</div>
    </div>
  );
}
