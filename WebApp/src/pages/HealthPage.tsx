import { useEffect, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getApi } from "@/lib/api";
import { Page } from "@/shared/components/Page";
import { PermissionGuard } from "@/shared/auth/PermissionGuard";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

type HealthEntry = {
  name: string;
  status: string;
  description?: string | null;
  duration: string;
  tags?: string[] | null;
  data?: Record<string, string> | null;
  exception?: string | null;
};

type HealthResponse = {
  overall: string;
  version: string;
  timestamp: string;
  entries: HealthEntry[];
};

export function HealthPage() {
  const [autoRefresh, setAutoRefresh] = useState(true);
  const [refreshInterval, setRefreshInterval] = useState(30_000);

  const q = useQuery({
    queryKey: ["admin", "health"],
    queryFn: async () => {
      const r = await getApi<HealthResponse>("/admin/health");
      return r.data as HealthResponse;
    },
    refetchInterval: autoRefresh ? refreshInterval : false,
  });

  const entries = useMemo(() => q.data?.entries ?? [], [q.data]);

  const overallStatus = q.data?.overall ?? "Unknown";

  return (
    <PermissionGuard permission="health.read">
      <Page
        title="Health"
        description="System health checks (aggregated)"
        actions={
          <div className="flex items-center gap-2">
            <Button
              variant={autoRefresh ? "default" : "outline"}
              size="sm"
              onClick={() => setAutoRefresh((v) => !v)}
            >
              {autoRefresh ? "Auto-refresh on" : "Auto-refresh off"}
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => q.refetch()}
              disabled={q.isFetching}
            >
              Refresh now
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

        {!q.isLoading && !q.isError && q.data && (
          <div className="space-y-4">
            {/* Overall */}
            <div className="rounded-2xl border p-4">
              <div className="flex items-center justify-between mb-2">
                <div className="text-sm font-semibold">Overall</div>
                <Badge
                  variant={
                    overallStatus === "Healthy"
                      ? "default"
                      : overallStatus === "Degraded"
                      ? "secondary"
                      : "destructive"
                  }
                >
                  {overallStatus}
                </Badge>
              </div>
              <div className="text-xs text-muted-foreground">
                Version: {q.data.version} • Last checked:{" "}
                {new Date(q.data.timestamp).toLocaleString()}
              </div>
            </div>

            {/* Entries */}
            <div className="rounded-2xl border overflow-hidden">
              <div className="divide-y">
                {entries.length ? (
                  entries.map((e) => (
                    <HealthEntryRow key={e.name} entry={e} />
                  ))
                ) : (
                  <div className="p-4 text-sm text-muted-foreground">
                    No health checks found.
                  </div>
                )}
              </div>
            </div>
          </div>
        )}
      </Page>
    </PermissionGuard>
  );
}

function HealthEntryRow({ entry }: { entry: HealthEntry }) {
  const statusVariant =
    entry.status === "Healthy"
      ? "default"
      : entry.status === "Degraded"
      ? "secondary"
      : "destructive";

  return (
    <div className="p-4">
      <div className="flex items-center justify-between mb-1">
        <div className="text-sm font-medium">{entry.name}</div>
        <Badge variant={statusVariant}>{entry.status}</Badge>
      </div>
      <div className="text-xs text-muted-foreground mb-1">
        {entry.duration} • {entry.description ?? "—"}
      </div>
      {entry.exception && (
        <div className="text-xs text-destructive mb-1">{entry.exception}</div>
      )}
      {entry.data && Object.keys(entry.data).length > 0 && (
        <pre className="text-xs bg-muted/40 rounded p-2 overflow-auto">
          {JSON.stringify(entry.data, null, 2)}
        </pre>
      )}
    </div>
  );
}
