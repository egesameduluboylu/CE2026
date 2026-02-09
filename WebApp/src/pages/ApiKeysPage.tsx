import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getApi, postApi } from "@/lib/api";
import { Page } from "@/shared/components/Page";
import { PermissionGuard } from "@/shared/auth/PermissionGuard";
import { usePermission } from "@/shared/auth/usePermission";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";

type ApiKeyItem = {
  id: string;
  name: string;
  prefix: string;
  scopes: string;
  expiresAt: string | null;
  revokedAt: string | null;
  lastUsedAt: string | null;
  createdAt: string;
};

type CreateResponse = {
  id: string;
  name: string;
  prefix: string;
  scopes: string[];
  expiresAt: string | null;
  secret: string;
};

export function ApiKeysPage() {
  const qc = useQueryClient();
  const canWrite = usePermission("api_keys.write");

  const [name, setName] = useState("");
  const [scopes, setScopes] = useState("*");

  const [lastSecret, setLastSecret] = useState<string | null>(null);

  const q = useQuery({
    queryKey: ["admin", "api-keys"],
    queryFn: async () => {
      const r = await getApi<ApiKeyItem[]>("/admin/api-keys");
      return r.data as ApiKeyItem[];
    },
  });

  const createMut = useMutation({
    mutationFn: async () => {
      const parsedScopes = scopes
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean);

      const r = await postApi<CreateResponse>("/admin/api-keys", {
        name: name.trim(),
        scopes: parsedScopes,
        expiresAt: null,
      });

      return r.data as CreateResponse;
    },
    onSuccess: async (data) => {
      setName("");
      setLastSecret(data.secret);
      await qc.invalidateQueries({ queryKey: ["admin", "api-keys"] });
    },
  });

  const revokeMut = useMutation({
    mutationFn: async (id: string) => postApi(`/admin/api-keys/${id}/revoke`),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ["admin", "api-keys"] });
    },
  });

  const items = useMemo(() => q.data ?? [], [q.data]);

  return (
    <PermissionGuard permission="api_keys.read">
      <Page
        title="API Keys"
        description="Server-to-server access keys (MVP)"
        actions={
          canWrite ? (
            <div className="flex flex-wrap items-center gap-2">
              <Input
                className="w-[200px]"
                placeholder="Key name…"
                value={name}
                onChange={(e) => setName(e.target.value)}
              />
              <Input
                className="w-[260px]"
                placeholder="Scopes (comma-separated)…"
                value={scopes}
                onChange={(e) => setScopes(e.target.value)}
              />
              <Button
                disabled={!name.trim() || createMut.isPending}
                onClick={() => createMut.mutate()}
              >
                {createMut.isPending ? "Creating…" : "Create"}
              </Button>

              <Dialog>
                <DialogTrigger asChild>
                  <Button variant="outline" disabled={!lastSecret}>
                    Show last secret
                  </Button>
                </DialogTrigger>
                <DialogContent className="sm:max-w-xl">
                  <DialogHeader>
                    <DialogTitle>API key secret</DialogTitle>
                    <DialogDescription>
                      This secret is shown only once when creating the key. Store it securely.
                    </DialogDescription>
                  </DialogHeader>

                  <pre className="rounded-xl border bg-muted/40 p-3 text-xs overflow-auto">
                    {lastSecret ?? "—"}
                  </pre>

                  <DialogFooter>
                    <div />
                  </DialogFooter>
                </DialogContent>
              </Dialog>
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
                items.map((k) => (
                  <div key={k.id} className="flex items-center justify-between gap-3 p-4">
                    <div className="min-w-0">
                      <div className="flex items-center gap-2">
                        <div className="text-sm font-medium truncate">{k.name}</div>
                        <Badge variant={k.revokedAt ? "secondary" : "default"}>
                          {k.revokedAt ? "Revoked" : "Active"}
                        </Badge>
                        <Badge variant="outline">{k.prefix}</Badge>
                      </div>
                      <div className="text-xs text-muted-foreground truncate">
                        scopes: {k.scopes || "—"}
                      </div>
                      <div className="text-xs text-muted-foreground truncate">{k.id}</div>
                    </div>

                    {canWrite ? (
                      <Button
                        size="sm"
                        variant="destructive"
                        disabled={!!k.revokedAt || revokeMut.isPending}
                        onClick={() => revokeMut.mutate(k.id)}
                      >
                        Revoke
                      </Button>
                    ) : null}
                  </div>
                ))
              ) : (
                <div className="p-6 text-sm text-muted-foreground">No API keys.</div>
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
