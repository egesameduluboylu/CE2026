import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useParams, useNavigate } from "react-router-dom";
import { getApi } from "@/lib/api";
import { useI18n } from "@/i18n/provider";
import { Page } from "@/shared/components/Page";
import { PermissionGuard } from "@/shared/auth/PermissionGuard";
import { Button } from "@/components/ui/button";
import { ArrowLeft, BarChart3 } from "lucide-react";

type DailyUsage = { date: string; apiCalls: number };

export function TenantUsagePage() {
  const { t } = useI18n();
  const { tenantId } = useParams<{ tenantId: string }>();
  const navigate = useNavigate();

  const usageQ = useQuery({
    queryKey: ["admin", "tenants", tenantId, "usage-chart"],
    queryFn: async () => {
      const r = await getApi<DailyUsage[]>(`/admin/tenants/${tenantId}/usage-chart`);
      return r.data as DailyUsage[];
    },
    enabled: !!tenantId,
  });

  const data = useMemo(() => {
    const items = usageQ.data ?? [];
    // Fill missing dates with 0 for last 30 days
    const today = new Date();
    const last30 = Array.from({ length: 30 }, (_, i) => {
      const d = new Date(today);
      d.setDate(today.getDate() - (29 - i));
      return d.toISOString().split("T")[0];
    });
    return last30.map((date) => {
      const found = items.find((x) => x.date === date);
      return { date, apiCalls: found?.apiCalls ?? 0 };
    });
  }, [usageQ.data]);

  const maxCalls = Math.max(...data.map((d) => d.apiCalls), 1);

  return (
    <PermissionGuard permission="billing.read">
      <Page
        title="Tenant Usage"
        description="API calls over the last 30 days"
        actions={
          <Button variant="outline" size="sm" onClick={() => navigate("/tenants")}>
            <ArrowLeft className="w-4 h-4 mr-1" />
            Back to Tenants
          </Button>
        }
      >
        <div className="space-y-4">
          {usageQ.isLoading && <div className="text-sm text-muted-foreground">Loading usage…</div>}
          {usageQ.isError && <div className="text-sm text-destructive">Error loading usage</div>}

          {data.length > 0 && (
            <div className="rounded-xl border p-4">
              <div className="flex items-center gap-2 mb-4">
                <BarChart3 className="w-5 h-5 text-muted-foreground" />
                <span className="font-medium">API Calls (Last 30 Days)</span>
              </div>
              {/* Simple bar chart using CSS */}
              <div className="space-y-2">
                {data.map((d) => (
                  <div key={d.date} className="flex items-center gap-2">
                    <div className="w-20 text-xs text-muted-foreground">{d.date}</div>
                    <div className="flex-1 bg-muted rounded-sm h-6 relative overflow-hidden">
                      <div
                        className="absolute left-0 top-0 h-full bg-primary transition-all"
                        style={{ width: `${(d.apiCalls / maxCalls) * 100}%` }}
                      />
                      <span className="absolute inset-0 flex items-center justify-center text-xs font-medium">
                        {d.apiCalls}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </Page>
    </PermissionGuard>
  );
}
