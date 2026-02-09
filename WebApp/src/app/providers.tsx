// import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
// import { ReactQueryDevtools } from "@tanstack/react-query-devtools"
// import { ThemeProvider } from "@/shared/theme/ThemeProvider"
// import { useState } from "react"

// export function AppProviders({ children }: { children: React.ReactNode }) {
//   const [queryClient] = useState(() =>
//     new QueryClient({
//       defaultOptions: {
//         queries: {
//           refetchOnWindowFocus: false,
//           retry: 1,
//           staleTime: 1000 * 30,
//         },
//       },
//     })
//   )

//   return (
//     <QueryClientProvider client={queryClient}>
//       <ThemeProvider>
//         {children}
//       </ThemeProvider>
//       <ReactQueryDevtools initialIsOpen={false} />
//     </QueryClientProvider>
//   )
// }
