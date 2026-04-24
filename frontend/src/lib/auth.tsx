import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { Auth } from "./api";

const TOKEN_KEY = "genv:jwt";
const EMAIL_KEY = "genv:email";

interface AuthUser {
  email: string;
}

interface AuthState {
  token: string | null;
  user: AuthUser | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthState | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem(TOKEN_KEY));
  const [email, setEmail] = useState<string | null>(() => localStorage.getItem(EMAIL_KEY));

  // Cross-tab synchronisation.
  useEffect(() => {
    const onStorage = (e: StorageEvent) => {
      if (e.key === TOKEN_KEY) setToken(e.newValue);
      if (e.key === EMAIL_KEY) setEmail(e.newValue);
    };
    window.addEventListener("storage", onStorage);
    /** api.ts interceptor refresh sonrası aynı sekmede JWT güncellenir */
    const onJwtRefresh = () => setToken(localStorage.getItem(TOKEN_KEY));
    window.addEventListener("genv-token-update", onJwtRefresh);
    return () => {
      window.removeEventListener("storage", onStorage);
      window.removeEventListener("genv-token-update", onJwtRefresh);
    };
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const res = await Auth.login(email, password);
    const jwt = res?.accessToken?.token;
    if (!jwt) throw new Error("Sunucu token döndürmedi.");
    localStorage.setItem(TOKEN_KEY, jwt);
    localStorage.setItem(EMAIL_KEY, email);
    setToken(jwt);
    setEmail(email);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EMAIL_KEY);
    setToken(null);
    setEmail(null);
  }, []);

  const value = useMemo<AuthState>(
    () => ({ token, user: email ? { email } : null, login, logout }),
    [token, email, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}

export function getStoredToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}
