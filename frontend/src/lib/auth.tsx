import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from "react";
import { Auth } from "./api";
import { decodeJwtPayload } from "./jwt";
import { useQueryClient } from "@tanstack/react-query";

const TOKEN_KEY = "genv:jwt";
const EMAIL_KEY = "genv:email";

interface AuthUser {
  email: string;
}

interface AuthState {
  token: string | null;
  user: AuthUser | null;
  tokenExpiry: Date | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthState | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const queryClient = useQueryClient();
  const [token, setToken] = useState<string | null>(() => localStorage.getItem(TOKEN_KEY));
  const [email, setEmail] = useState<string | null>(() => localStorage.getItem(EMAIL_KEY));
  const [tokenExpiry, setTokenExpiry] = useState<Date | null>(null);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  /** Refresh token geçersizse oturumu temizleyip login'e yönlendir. */
  const clearSession = useCallback(() => {
    if (timerRef.current) clearTimeout(timerRef.current);
    void queryClient.cancelQueries();
    queryClient.clear();
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EMAIL_KEY);
    setToken(null);
    setEmail(null);
    setTokenExpiry(null);
    if (!location.pathname.startsWith("/login")) {
      location.replace("/login");
    }
  }, [queryClient]);

  const scheduleRefresh = useCallback(
    (expirationDate: string) => {
      if (timerRef.current) clearTimeout(timerRef.current);
      const expiry = new Date(expirationDate);
      setTokenExpiry(expiry);
      const delay = expiry.getTime() - Date.now() - 60_000;
      function doRefresh() {
        Auth.refreshToken().then((result) => {
          if (result) {
            if (timerRef.current) clearTimeout(timerRef.current);
            const nextExpiry = new Date(result.expirationDate);
            setTokenExpiry(nextExpiry);
            const nextDelay = nextExpiry.getTime() - Date.now() - 60_000;
            timerRef.current = setTimeout(doRefresh, Math.max(nextDelay, 0));
          } else {
            // Refresh token geçersiz/revoke edilmiş — oturumu sonlandır.
            clearSession();
          }
        });
      }
      timerRef.current = setTimeout(doRefresh, Math.max(delay, 0));
    },
    [clearSession],
  );

  // Mount: mevcut JWT varsa exp claim'den proaktif refresh timer kur.
  useEffect(() => {
    const t = localStorage.getItem(TOKEN_KEY);
    if (!t) return;
    const payload = decodeJwtPayload(t);
    if (!payload?.exp || typeof payload.exp !== "number") return;
    const expMs = payload.exp * 1000;
    if (expMs < Date.now()) {
      // JWT zaten expire — hemen refresh dene, başarısız olursa login'e gönder.
      Auth.refreshToken().then((result) => {
        if (result) {
          scheduleRefresh(result.expirationDate);
        } else {
          clearSession();
        }
      });
    } else {
      scheduleRefresh(new Date(expMs).toISOString());
    }
    return () => {
      if (timerRef.current) clearTimeout(timerRef.current);
    };
  }, [scheduleRefresh, clearSession]);

  // Cross-tab synchronisation.
  useEffect(() => {
    const onStorage = (e: StorageEvent) => {
      if (e.key === TOKEN_KEY) {
        setToken(e.newValue);
        if (e.newValue === null && !location.pathname.startsWith("/login")) {
          location.replace("/login");
        }
      }
      if (e.key === EMAIL_KEY) setEmail(e.newValue);
    };
    window.addEventListener("storage", onStorage);
    /** api.ts interceptor refresh sonrası aynı sekmede JWT güncellenir */
    const onJwtRefresh = (e: Event) => {
      setToken(localStorage.getItem(TOKEN_KEY));
      const exp = (e as CustomEvent<{ expirationDate?: string }>).detail?.expirationDate;
      if (exp) scheduleRefresh(exp);
    };
    window.addEventListener("genv-token-update", onJwtRefresh);
    return () => {
      window.removeEventListener("storage", onStorage);
      window.removeEventListener("genv-token-update", onJwtRefresh);
    };
  }, [scheduleRefresh]);

  const login = useCallback(async (email: string, password: string) => {
    const res = await Auth.login(email, password);
    const jwt = res?.accessToken?.token;
    if (!jwt) throw new Error("Sunucu token döndürmedi.");
    localStorage.setItem(TOKEN_KEY, jwt);
    localStorage.setItem(EMAIL_KEY, email);
    setToken(jwt);
    setEmail(email);
    if (res.accessToken.expirationDate) scheduleRefresh(res.accessToken.expirationDate);
  }, [scheduleRefresh]);

  const logout = useCallback(() => {
    // Token hâlâ localStorage'dayken revoke isteğini başlat.
    Auth.revokeToken().catch(() => {});
    clearSession();
  }, [clearSession]);

  const value = useMemo<AuthState>(
    () => ({ token, user: email ? { email } : null, tokenExpiry, login, logout }),
    [token, email, tokenExpiry, login, logout],
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
