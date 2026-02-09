import React, { createContext, useContext, useEffect, useMemo, useState } from "react";

type Theme = "light" | "dark" | "system";
type Resolved = "light" | "dark";

type ThemeCtx = { theme: Theme; resolvedTheme: Resolved; setTheme: (t: Theme) => void; };
const Ctx = createContext<ThemeCtx | null>(null);
const KEY = "theme";

const systemTheme = (): Resolved =>
  window.matchMedia?.("(prefers-color-scheme: dark)").matches ? "dark" : "light";

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [theme, setTheme] = useState<Theme>(() => (localStorage.getItem(KEY) as Theme) ?? "system");
  const resolvedTheme = useMemo<Resolved>(() => (theme === "system" ? systemTheme() : theme), [theme]);

  useEffect(() => {
    document.documentElement.classList.toggle("dark", resolvedTheme === "dark");
  }, [resolvedTheme]);

  useEffect(() => {
    localStorage.setItem(KEY, theme);
  }, [theme]);

  return <Ctx.Provider value={{ theme, resolvedTheme, setTheme }}>{children}</Ctx.Provider>;
}

export function useTheme() {
  const v = useContext(Ctx);
  if (!v) throw new Error("useTheme must be used within ThemeProvider");
  return v;
}
