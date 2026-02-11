// import { useQuery } from '@tanstack/react-query';
// import { getApi } from '@/lib/api';
// import { useAppTranslation } from '@/hooks/useTranslation';

// type ReadyData = {
//   status: string;
//   totalDuration: number;
//   entries: Record<string, { status: string; description?: string; duration: number }>;
// };

// export function Health() {
//   const { t } = useAppTranslation();
//   const ready = useQuery({
//     queryKey: ['admin', 'health', 'ready'],
//     queryFn: () => getApi<ReadyData>('/admin/health/ready').then((r) => r.data),
//   });
//   const live = useQuery({
//     queryKey: ['admin', 'health', 'live'],
//     queryFn: () => getApi<{ status: string }>('/admin/health/live').then((r) => r.data),
//   });

//   return (
//     <div>
//       <h1 className="text-2xl font-semibold text-zinc-900 dark:text-zinc-100 mb-4">{t("pages.health.title")}</h1>
//       <div className="space-y-4">
//         <div className="rounded-lg border border-zinc-200 dark:border-zinc-800 p-4">
//           <h2 className="font-medium text-zinc-900 dark:text-zinc-100 mb-2">{t("pages.health.ready_check")}</h2>
//           {ready.isLoading && <p className="text-zinc-500">{t("common.loading")}</p>}
//           {ready.error && <p className="text-red-600">{String(ready.error)}</p>}
//           {ready.data != null && (
//             <pre className="text-sm text-zinc-600 dark:text-zinc-400 overflow-auto">
//               {JSON.stringify(ready.data, null, 2)}
//             </pre>
//           )}
//         </div>
//         <div className="rounded-lg border border-zinc-200 dark:border-zinc-800 p-4">
//           <h2 className="font-medium text-zinc-900 dark:text-zinc-100 mb-2">{t("pages.health.live_check")}</h2>
//           {live.isLoading && <p className="text-zinc-500">{t("common.loading")}</p>}
//           {live.error && <p className="text-red-600">{String(live.error)}</p>}
//           {live.data != null && <p className="text-zinc-600 dark:text-zinc-400">{(live.data as { status?: string }).status ?? 'ok'}</p>}
//         </div>
//       </div>
//     </div>
//   );
// }
