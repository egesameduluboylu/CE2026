// import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
// import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
// import { AuthProvider, useAuth } from "@/contexts/AuthContext";

// import { Layout } from "@/components/Layout";
// import { Login } from "@/pages/Login";
// import { Register } from "@/pages/Register";
// import { UsersPage } from "@/pages/UsersPage";
// import { UserDetailPage } from "@/pages/UserDetailPage";
// import { DashboardPage } from "@/pages/DashboardPage";
// import { SecurityEvents } from "@/pages/SecurityEvents";
// import { Health } from "@/pages/Health";
// import { Settings } from "@/pages/Settings";
// import { NewUserPage } from "@/pages/users/NewUserPage";
// import { ForgotPassword } from "@/pages/ForgotPassword";
// import { ResetPassword } from "@/pages/ResetPassword";

// const queryClient = new QueryClient({
//   defaultOptions: {
//     queries: {
//       retry: (_, err) => !(err instanceof Error && err.message.includes("401")),
//       refetchOnWindowFocus: false,
//     },
//   },
// });

// function ProtectedRoute({ children }: { children: React.ReactNode }) {
//   const { accessToken, isLoading } = useAuth();

//   if (isLoading) {
//     return (
//       <div className="flex items-center justify-center min-h-screen">
//         Loading…
//       </div>
//     );
//   }

//   if (!accessToken) return <Navigate to="/login" replace />;

//   return <>{children}</>;
// }

// function AppRoutes() {
//   return (
//     <Routes>
//       {/* public */}
//       <Route path="/login" element={<Login />} />
//       <Route path="/register" element={<Register />} />
//       <Route path="/forgot-password" element={<ForgotPassword />} />
//       <Route path="/reset-password" element={<ResetPassword />} />

//       {/* protected */}
//       <Route
//         path="/"
//         element={
//           <ProtectedRoute>
//             <Layout />
//           </ProtectedRoute>
//         }
//       >
//         <Route index element={<DashboardPage />} />

//         <Route path="users" element={<UsersPage />} />
//         <Route path="users/new" element={<NewUserPage />} />
//         <Route path="users/:id" element={<UserDetailPage />} />

//         <Route path="security-events" element={<SecurityEvents />} />
//         <Route path="health" element={<Health />} />
//         <Route path="settings" element={<Settings />} />
//       </Route>

//       {/* fallback */}
//       <Route path="*" element={<Navigate to="/" replace />} />
//     </Routes>
//   );
// }

// export default function App() {
//   return (
//     <QueryClientProvider client={queryClient}>
//       <AuthProvider>
//         <BrowserRouter>
//           <AppRoutes />
//         </BrowserRouter>
//       </AuthProvider>
//     </QueryClientProvider>
//   );
// }
