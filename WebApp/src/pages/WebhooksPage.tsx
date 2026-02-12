import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getApi, postApi } from "@/lib/api";
import { useI18n } from "@/i18n/provider";
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

type WebhookItem = {
  id: string;
  name: string;
  url: string;
  events: string;
  isActive: boolean;
  disabledAt: string | null;
  createdAt: string;
};

type CreateResponse = {
  id: string;
  name: string;
  url: string;
  events: string[];
  secret: string;
  isActive: boolean;
  createdAt: string;
};

export function WebhooksPage() {
  const { t } = useI18n();
  const qc = useQueryClient();
  const canWrite = usePermission("webhooks.write");

  const [name, setName] = useState("");
  const [url, setUrl] = useState("");
  const [events, setEvents] = useState("user.created,user.updated");

  const [lastSecret, setLastSecret] = useState<string | null>(null);

  const q = useQuery({
    queryKey: ["admin", "webhooks"],
    queryFn: async () => {
      const r = await getApi<WebhookItem[]>("/admin/webhooks");
      return r.data as WebhookItem[];
    },
  });

  const createMut = useMutation({
    mutationFn: async () => {
      const parsedEvents = events
        .split(",")
        .map((e) => e.trim())
        .filter(Boolean);

      const r = await postApi<CreateResponse>("/admin/webhooks", {
        name: name.trim(),
        url: url.trim(),
        events: parsedEvents,
      });

      return r.data as CreateResponse;
    },
    onSuccess: async (data) => {
      setName("");
      setUrl("");
      setLastSecret(data.secret);
      await qc.invalidateQueries({ queryKey: ["admin", "webhooks"] });
    },
  });

  const disableMut = useMutation({
    mutationFn: async (id: string) => postApi(`/admin/webhooks/${id}/disable`),
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: ["admin", "webhooks"] });
    },
  });

  const triggerMut = useMutation({
    mutationFn: async (id: string) =>
      postApi(`/admin/webhooks/${id}/trigger`, {
        eventType: "manual.test",
        payload: "{}",
      }),
  });

  const items = useMemo(() => q.data ?? [], [q.data]);

  return (
    <PermissionGuard permission="webhooks.read">
      <Page
        title="Webhooks"
        description="Outbound event delivery (MVP)"
        actions={
          canWrite ? (
            <div className="flex flex-wrap items-center gap-2">
              <Input
                className="w-[200px]"
                placeholder="Name…"
                value={name}
                onChange={(e) => setName(e.target.value)}
              />
              <Input
                className="w-[260px]"
                placeholder="URL…"
                value={url}
                onChange={(e) => setUrl(e.target.value)}
              />
              <Input
                className="w-[280px]"
                placeholder="Events (comma)…"
                value={events}
                onChange={(e) => setEvents(e.target.value)}
              />
              <Button
                disabled={!name.trim() || !url.trim() || createMut.isPending}
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
                    <DialogTitle>Webhook signing secret</DialogTitle>
                    <DialogDescription>
                      Use this secret to verify HMAC signatures. It is shown only once.
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
                items.map((w) => (
                  <WebhookRow
                    key={w.id}
                    item={w}
                    canWrite={canWrite}
                    onDisable={() => disableMut.mutate(w.id)}
                    onTrigger={() => triggerMut.mutate(w.id)}
                  />
                ))
              ) : (
                <div className="p-6 text-sm text-muted-foreground">No webhooks.</div>
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

function WebhookRow({
  item,
  canWrite,
  onDisable,
  onTrigger,
}: {
  item: WebhookItem;
  canWrite: boolean;
  onDisable: () => void;
  onTrigger: () => void;
}) {
  return (
    <div className="flex items-center justify-between gap-3 p-4">
      <div className="min-w-0">
        <div className="flex items-center gap-2">
          <div className="text-sm font-medium truncate">{item.name}</div>
          <Badge variant={item.isActive ? "default" : "secondary"}>
            {item.isActive ? "Active" : "Disabled"}
          </Badge>
        </div>
        <div className="text-xs text-muted-foreground truncate">{item.url}</div>
        <div className="text-xs text-muted-foreground truncate">events: {item.events}</div>
        <div className="text-xs text-muted-foreground truncate">{item.id}</div>
      </div>

      {canWrite ? (
        <div className="flex items-center gap-2">
          <Button
            size="sm"
            variant="outline"
            disabled={!item.isActive}
            onClick={onTrigger}
          >
            Trigger
          </Button>
          <Button
            size="sm"
            variant="destructive"
            disabled={!item.isActive}
            onClick={onDisable}
          >
            Disable
          </Button>
        </div>
      ) : null}
    </div>
  );
}
