import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import { useNavigate } from "react-router-dom";
import { getApi, postApi } from "@/lib/api";
import { loadStoredToken, persistToken } from "@/lib/auth";

type User = { userId: string; email: string | null; permissions: string[] };

type AuthContextValue = {
  user: User | null;
  accessToken: string | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  registerUser: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  setToken: (token: string | null) => void;
  refreshMe: () => Promise<void>;
};

type MeDto = {
  userId: string;
  email?: string | null;
  permissions?: string[];
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [accessToken, setAccessTokenState] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();

  // ✅ Token yönetimi (persistToken window.__accessToken vs. ayarlıyor)
  const setToken = useCallback((token: string | null) => {
    persistToken(token);
    setAccessTokenState(token);
  }, []);

  const fetchMe = useCallback(async () => {
    const me = await getApi<MeDto>("/auth/me");
    const d = me?.data;

    setUser(
      d
        ? {
            userId: d.userId,
            email: d.email ?? null,
            permissions: d.permissions ?? [],
          }
        : null
    );
  }, []);

  // ✅ Refresh: HttpOnly cookie ile yeni access token al
  const refresh = useCallback(async () => {
    const r = await postApi<{ accessToken: string }>("/auth/refresh");
    const token = r.data?.accessToken ?? null;
    if (!token) throw new Error("No token from refresh");
    setToken(token);
  }, [setToken]);

  const login = useCallback(
    async (email: string, password: string) => {
      const res = await postApi<{ accessToken: string }>(
        "/auth/login",
        { email, password },
        { successCode: "auth.logged_in" }
      );

      const token = res.data?.accessToken ?? null;
      if (!token) throw new Error("No token received");

      setToken(token);
      await fetchMe();

      navigate("/", { replace: true });
    },
    [fetchMe, navigate, setToken]
  );

  const registerUser = useCallback(async (email: string, password: string) => {
    await postApi(
      "/auth/register",
      { email, password },
      { successCode: "auth.registered" }
    );
  }, []);

  const logout = useCallback(async () => {
    await postApi("/auth/logout", undefined, {
      successCode: "auth.logged_out",
    }).catch(() => {});
    setToken(null);
    setUser(null);
    navigate("/login", { replace: true });
  }, [navigate, setToken]);

  // ✅ api.ts refresh fail olunca window event atıyorsa burada yakala

  const bootedRef = useRef(false);
useEffect(() => {
  if (bootedRef.current) return;
  bootedRef.current = true;
    const onLogout = () => {
      setToken(null);
      setUser(null);
      navigate("/login", { replace: true });
    };
    window.addEventListener("auth:logout", onLogout);
    return () => window.removeEventListener("auth:logout", onLogout);
  }, [navigate, setToken]);

  // ✅ App açılış bootstrap:
  // - storage token varsa yükle (opsiyonel)
  // - her durumda refresh dene (cookie ile)
  // - sonra me çek
  useEffect(() => {
    let alive = true;

    const boot = async () => {
      try {
        const stored = loadStoredToken();
        if (stored) setToken(stored);

        await refresh();   // 👈 kritik
        await fetchMe();   // 👈 user/permissions doldur
      } catch {
        if (!alive) return;
        setToken(null);
        setUser(null);
      } finally {
        if (!alive) return;
        setIsLoading(false);
      }
    };

    boot();

    return () => {
      alive = false;
    };
  }, [fetchMe, refresh, setToken]);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      accessToken,
      isLoading,
      login,
      registerUser,
      logout,
      setToken,
      refreshMe: fetchMe,
    }),
    [user, accessToken, isLoading, login, registerUser, logout, setToken, fetchMe]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
