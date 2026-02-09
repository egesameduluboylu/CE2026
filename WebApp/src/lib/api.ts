const API_BASE = "/api";

export type ApiResponse<T> = { data: T; traceId?: string };

type WindowToken = { __accessToken: string | null };

function getAccessTokenSync(): string | null {
  return ((window as unknown as WindowToken).__accessToken ?? null);
}

export function setAccessToken(token: string | null): void {
  (window as unknown as WindowToken).__accessToken = token;
}

export type ApiFetchOptions = RequestInit & {
  successCode?: string; // i18n key: "users.created" vs
  success?: boolean;    // true ise default mesaj
};

/** i18n culture (tr-TR / en-GB) */
import { getCulture } from "../i18n"; // <-- yolunu düzelt

/**
 * Refresh lock:
 * - Aynı anda 10 istek 401 yerse 10 kere refresh atmasın
 * - 1 refresh çalışsın, diğerleri onu beklesin
 */
let refreshPromise: Promise<string | null> | null = null;

async function refreshToken(): Promise<string | null> {
  if (refreshPromise) return refreshPromise;

  refreshPromise = (async () => {
    const res = await fetch(`${API_BASE}/auth/refresh`, {
      method: "POST",
      credentials: "include",
      headers: {
        Accept: "application/json",
        "Accept-Language": getCulture(), // ✅ eklendi
      },
    });

    if (!res.ok) return null;

    const json = (await res.json()) as ApiResponse<{ accessToken: string }>;
    const token = json?.data?.accessToken ?? null;

    if (token) setAccessToken(token);
    return token;
  })();

  try {
    return await refreshPromise;
  } finally {
    refreshPromise = null;
  }
}

/** ProblemDetails'tan errorCode + message çıkar */
type ProblemLike = {
  title?: string;
  detail?: string;
  traceId?: string;
  errorCode?: string;
  extensions?: { errorCode?: string; traceId?: string };
};

function parseProblem(text: string): { errorCode?: string; message?: string; raw?: unknown } {
  if (!text) return { message: "Request failed" };

  try {
    const p = JSON.parse(text) as ProblemLike;

    const errorCode = p.extensions?.errorCode ?? p.errorCode;
    const message = p.detail || p.title;

    return { errorCode: errorCode ?? undefined, message: message ?? undefined, raw: p };
  } catch {
    // JSON değilse plain text döner
    return { message: text };
  }
}

export async function apiFetch<T>(
  path: string,
  options: ApiFetchOptions = {},
  retried = false
): Promise<ApiResponse<T>> {
  const token = getAccessTokenSync();

  const headers: HeadersInit = {
    Accept: "application/json",
    "Accept-Language": getCulture(), // ✅ eklendi
    ...(options.body ? { "Content-Type": "application/json" } : {}),
    ...options.headers,
  };

  if (token) (headers as Record<string, string>)["Authorization"] = `Bearer ${token}`;

  let res = await fetch(`${API_BASE}${path}`, {
    ...options,
    credentials: "include",
    headers,
  });

  // 401 -> refresh -> retry (tek sefer)
  if (res.status === 401 && !retried) {
    const newToken = await refreshToken();

    if (newToken) {
      (headers as Record<string, string>)["Authorization"] = `Bearer ${newToken}`;
      res = await fetch(`${API_BASE}${path}`, {
        ...options,
        credentials: "include",
        headers,
      });
    } else {
      setAccessToken(null);
      window.dispatchEvent(new CustomEvent("auth:logout"));
    }
  }

if (!res.ok) {
  const text = await res.text();
  const { errorCode, message, raw } = parseProblem(text);

  window.dispatchEvent(
    new CustomEvent("api:error", {
      detail: {
        code: errorCode ?? "common.unexpected_error",
        status: res.status,
      },
    })
  );

  const err = new Error(errorCode ?? message ?? `HTTP ${res.status}`);

  (err as any).status = res.status;
  (err as any).errorCode = errorCode ?? null;
  (err as any).problem = raw ?? null;

  throw err;
}

  if (res.status === 204) return { data: undefined as T };
  return (await res.json()) as ApiResponse<T>;
}

export function getApi<T = unknown>(path: string) {
  return apiFetch<T>(path, { method: "GET" });
}

export function postApi<T = unknown>(path: string, body?: unknown, opts?: { successCode?: string }) {
  return apiFetch<T>(path, {
    method: "POST",
    body: body !== undefined ? JSON.stringify(body) : undefined,
    success: !!opts?.successCode,
    successCode: opts?.successCode,
  });
}

export function putApi<T = unknown>(path: string, body?: unknown, opts?: { successCode?: string }) {
  return apiFetch<T>(path, {
    method: "PUT",
    body: body !== undefined ? JSON.stringify(body) : undefined,
    success: !!opts?.successCode,
    successCode: opts?.successCode,
  });
}

export function deleteApi<T = unknown>(path: string, opts?: { successCode?: string }) {
  return apiFetch<T>(path, { method: "DELETE", success: !!opts?.successCode, successCode: opts?.successCode });
}
