import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { Toaster } from "sonner";

export type ThemePreference = "light" | "dark" | "system";

const STORAGE_KEY = "genv:theme";

function readPreference(): ThemePreference {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw === "light" || raw === "dark" || raw === "system") return raw;
  } catch {
    /* ignore */
  }
  return "system";
}

function resolveDark(pref: ThemePreference): boolean {
  if (pref === "dark") return true;
  if (pref === "light") return false;
  return window.matchMedia("(prefers-color-scheme: dark)").matches;
}

type ThemeContextValue = {
  preference: ThemePreference;
  setPreference: (p: ThemePreference) => void;
  resolvedDark: boolean;
};

const ThemeContext = createContext<ThemeContextValue | null>(null);

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [preference, setPreferenceState] = useState<ThemePreference>(() => readPreference());
  const [resolvedDark, setResolvedDark] = useState(() =>
    typeof window !== "undefined" ? resolveDark(readPreference()) : true,
  );

  const setPreference = useCallback((p: ThemePreference) => {
    setPreferenceState(p);
    try {
      localStorage.setItem(STORAGE_KEY, p);
    } catch {
      /* ignore */
    }
    setResolvedDark(resolveDark(p));
  }, []);

  useEffect(() => {
    setResolvedDark(resolveDark(preference));
  }, [preference]);

  useEffect(() => {
    if (preference !== "system") return;
    const mq = window.matchMedia("(prefers-color-scheme: dark)");
    const onChange = () => setResolvedDark(mq.matches);
    mq.addEventListener("change", onChange);
    return () => mq.removeEventListener("change", onChange);
  }, [preference]);

  useEffect(() => {
    document.documentElement.classList.toggle("dark", resolvedDark);
  }, [resolvedDark]);

  const value = useMemo(
    () => ({ preference, setPreference, resolvedDark }),
    [preference, setPreference, resolvedDark],
  );

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme(): ThemeContextValue {
  const ctx = useContext(ThemeContext);
  if (!ctx) throw new Error("useTheme must be used within ThemeProvider");
  return ctx;
}

/** Sonner — çözülen temaya göre */
export function ThemedToaster() {
  const { resolvedDark } = useTheme();
  return (
    <Toaster
      position="bottom-right"
      theme={resolvedDark ? "dark" : "light"}
      richColors
      closeButton
      toastOptions={{
        classNames: { toast: "!bg-bg-card !border-line !text-ink" },
      }}
    />
  );
}
