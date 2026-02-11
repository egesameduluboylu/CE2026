import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getApi } from "@/lib/api";
import { Page } from "@/shared/components/Page";
import { PermissionGuard } from "@/shared/auth/PermissionGuard";
import { useAppTranslation } from "@/hooks/useTranslation";

type EventItem = {
  id: number;
  userId: string | null;
  email: string | null;
  type: string;
  detail: string | null;
  ipAddress: string | null;
  userAgent: string | null;
  createdAt: string;
};
type Page = { items: EventItem[]; total: number; page: number; pageSize: number };

export function SecurityEvents() {
  const { t } = useAppTranslation();
  const [type, setType] = useState('');
  const [page, setPage] = useState(1);
  const pageSize = 50;

  const { data, isLoading, error } = useQuery({
    queryKey: ['admin', 'security-events', type, page, pageSize],
    queryFn: async () => {
      const r = await getApi<Page>(
        `/admin/security-events?type=${encodeURIComponent(type)}&page=${page}&pageSize=${pageSize}`
      );
      return r.data as Page;
    },
  });

  const totalPages = data ? Math.ceil(data.total / pageSize) : 0;

  return (
    <PermissionGuard permission="audit.read">
      <Page title={t("pages.security_events.title")} description={t("descriptions.security_events")}>
        {isLoading && <div className="text-sm text-muted-foreground">{t("common.loading")}</div>}
        {error && (
          <div className="rounded-xl border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
            {t("common.error")}
          </div>
        )}
      <div className="mb-4 flex gap-4 items-center">
        <select
          value={type}
          onChange={(e) => { setType(e.target.value); setPage(1); }}
          className="rounded-md border border-zinc-300 dark:border-zinc-700 bg-white dark:bg-zinc-900 px-3 py-2 text-zinc-900 dark:text-zinc-100"
        >
          <option value="">{t("pages.security_events.all_types")}</option>
          <option value="LOGIN_SUCCESS">LOGIN_SUCCESS</option>
          <option value="LOGIN_FAIL">LOGIN_FAIL</option>
          <option value="LOCKOUT">LOCKOUT</option>
          <option value="REFRESH_REUSED">REFRESH_REUSED</option>
          <option value="REFRESH_ROTATED">REFRESH_ROTATED</option>
          <option value="LOGOUT">LOGOUT</option>
        </select>
      </div>
      <div className="rounded-lg border border-zinc-200 dark:border-zinc-800 overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-zinc-50 dark:bg-zinc-900">
            <tr>
              <th className="text-left p-3 font-medium text-zinc-700 dark:text-zinc-300">Time</th>
              <th className="text-left p-3 font-medium text-zinc-700 dark:text-zinc-300">Type</th>
              <th className="text-left p-3 font-medium text-zinc-700 dark:text-zinc-300">Email</th>
              <th className="text-left p-3 font-medium text-zinc-700 dark:text-zinc-300">Detail</th>
              <th className="text-left p-3 font-medium text-zinc-700 dark:text-zinc-300">IP</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-zinc-200 dark:divide-zinc-800">
            {data?.items?.map((e: EventItem) => (
              <tr key={e.id} className="hover:bg-zinc-50 dark:hover:bg-zinc-900/50">
                <td className="p-3 text-zinc-600 dark:text-zinc-400">{new Date(e.createdAt).toLocaleString()}</td>
                <td className="p-3 text-zinc-900 dark:text-zinc-100">{e.type}</td>
                <td className="p-3 text-zinc-600 dark:text-zinc-400">{e.email ?? '—'}</td>
                <td className="p-3 text-zinc-600 dark:text-zinc-400 max-w-xs truncate">{e.detail ?? '—'}</td>
                <td className="p-3 text-zinc-600 dark:text-zinc-400">{e.ipAddress ?? '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {totalPages > 1 && (
        <div className="mt-4 flex gap-2">
          <button
            type="button"
            disabled={page <= 1}
            onClick={() => setPage((p) => p - 1)}
            className="rounded border border-zinc-300 dark:border-zinc-700 px-3 py-1 text-sm disabled:opacity-50"
          >
            Previous
          </button>
          <span className="py-1 text-sm text-zinc-600 dark:text-zinc-400">
            Page {page} of {totalPages}
          </span>
          <button
            type="button"
            disabled={page >= totalPages}
            onClick={() => setPage((p) => p + 1)}
            className="rounded border border-zinc-300 dark:border-zinc-700 px-3 py-1 text-sm disabled:opacity-50"
          >
            Next
          </button>
        </div>
      )}
      </Page>
    </PermissionGuard>
  );
}
