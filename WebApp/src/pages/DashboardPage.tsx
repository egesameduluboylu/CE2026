import { getApi } from "@/lib/api";
import { useEffect, useState } from "react";

type DashboardStats = {
  users: number;
  activeSessions: number;
  failedLogins24h: number;
  lockedUsers: number;
};

export function DashboardPage() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [err, setErr] = useState<string | null>(null);

  useEffect(() => {
    let mounted = true;
    getApi<DashboardStats>("/admin/stats")
      .then((r) => mounted && setStats(r.data))
      .catch((e) => mounted && setErr(e instanceof Error ? e.message : "Failed"))
      .finally(() => {});
    return () => {
      mounted = false;
    };
  }, []);

  const cards = [
    { label: "Users", value: stats?.users ?? "—" },
    { label: "Active sessions", value: stats?.activeSessions ?? "—" },
    { label: "Failed logins (24h)", value: stats?.failedLogins24h ?? "—" },
    { label: "Locked users", value: stats?.lockedUsers ?? "—" },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-semibold text-zinc-900 dark:text-zinc-100">Dashboard</h1>
        <p className="text-sm text-zinc-500 dark:text-zinc-400">Quick overview of identity/security.</p>
      </div>

      {err && (
        <div className="rounded-lg border border-red-500/30 bg-red-500/10 px-3 py-2 text-sm text-red-500">
          {err}
        </div>
      )}

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {cards.map((c) => (
          <div key={c.label} className="rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-4">
            <div className="text-xs text-zinc-500 dark:text-zinc-400">{c.label}</div>
            <div className="mt-2 text-2xl font-semibold text-zinc-900 dark:text-zinc-100">{c.value}</div>
          </div>
        ))}
      </div>

      <div className="rounded-xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-4">
        <div className="font-semibold text-zinc-900 dark:text-zinc-100">Next</div>
        <div className="mt-2 text-sm text-zinc-600 dark:text-zinc-400">
          Buraya “son security event’ler”, “rate limit hits”, “refresh reuse detection” gibi kartlar gelecek.
        </div>
      </div>
    </div>
  );
}
