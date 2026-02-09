import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getApi } from "@/lib/api";
import { Page } from "@/shared/components/Page";
import { PermissionGuard } from "@/shared/auth/PermissionGuard";
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

type AuditItem = {
  id: number;
  userId: string | null;
  email: string | null;
  type: string;
  detail: string | null;
  ipAddress: string | null;
  userAgent: string | null;
  createdAt: string;
};

type AuditPageData = {
  items: AuditItem[];
  total: number;
  page: number;
  pageSize: number;
};

export function AuditPage() {
  const [type, setType] = useState("");
  const [email, setEmail] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 50;

  const q = useQuery({
    queryKey: ["admin", "audit", type, email, page, pageSize],
    queryFn: async () => {
      const url =
        `/admin/audit?type=${encodeURIComponent(type)}` +
        `&email=${encodeURIComponent(email)}` +
        `&page=${page}&pageSize=${pageSize}`;

      const r = await getApi<AuditPageData>(url);
      return r.data as AuditPageData;
    },
  });

  const items = useMemo(() => q.data?.items ?? [], [q.data]);
  const totalPages = q.data ? Math.ceil(q.data.total / pageSize) : 0;

  return (
    <PermissionGuard permission="audit.read">
      <Page
        title="Audit"
        description="Security and audit events"
        actions={
          <div className="flex flex-wrap items-center gap-2">
            <Input
              className="w-[180px]"
              placeholder="Type…"
              value={type}
              onChange={(e) => {
                setType(e.target.value);
                setPage(1);
              }}
            />
            <Input
              className="w-[220px]"
              placeholder="Email contains…"
              value={email}
              onChange={(e) => {
                setEmail(e.target.value);
                setPage(1);
              }}
            />
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
                items.map((e) => (
                  <AuditRow key={e.id} item={e} />
                ))
              ) : (
                <div className="p-6 text-sm text-muted-foreground">No events.</div>
              )}
            </div>
          </div>
        )}

        {totalPages > 1 && (
          <div className="mt-4 flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={page <= 1}
              onClick={() => setPage((p) => p - 1)}
            >
              Previous
            </Button>
            <div className="text-sm text-muted-foreground">
              Page {page} of {totalPages}
            </div>
            <Button
              variant="outline"
              size="sm"
              disabled={page >= totalPages}
              onClick={() => setPage((p) => p + 1)}
            >
              Next
            </Button>
          </div>
        )}
      </Page>
    </PermissionGuard>
  );
}

function AuditRow({ item }: { item: AuditItem }) {
  return (
    <div className="flex items-center justify-between gap-3 p-4">
      <div className="min-w-0">
        <div className="flex items-center gap-2">
          <div className="text-sm font-medium">{item.type}</div>
          <Badge variant="outline">{new Date(item.createdAt).toLocaleString()}</Badge>
          {item.email ? <Badge variant="secondary">{item.email}</Badge> : null}
        </div>
        <div className="text-xs text-muted-foreground truncate">
          {item.detail ?? "—"}
        </div>
      </div>

      <Dialog>
        <DialogTrigger asChild>
          <Button variant="outline" size="sm">
            View
          </Button>
        </DialogTrigger>
        <DialogContent className="sm:max-w-xl">
          <DialogHeader>
            <DialogTitle>Event</DialogTitle>
            <DialogDescription>{item.type}</DialogDescription>
          </DialogHeader>

          <pre className="max-h-[420px] overflow-auto rounded-xl border bg-muted/40 p-3 text-xs">
            {JSON.stringify(item, null, 2)}
          </pre>

          <DialogFooter>
            <div />
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
