import { NavLink, Outlet, useLocation, useNavigate } from "react-router-dom";
import {
  LayoutDashboard,
  Users,
  Boxes,
  Snowflake,
  Activity,
  Wifi,
  WifiOff,
  HeartHandshake,
  Beaker,
  History,
  Database,
  Cpu,
  SlidersHorizontal,
  LogOut,
  UserCircle2,
  Menu,
  X,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { useEffect, useState } from "react";
import { getCryoConnection, onConnectionState, onCryo } from "@/lib/signalr";
import { HubConnectionState } from "@microsoft/signalr";
import { useAuth } from "@/lib/auth";
import { hasJwtRole } from "@/lib/jwt";
import { useQueryClient } from "@tanstack/react-query";
import { ThemeToggle } from "@/components/ThemeToggle";

const nav: Array<{
  to: string;
  label: string;
  icon: any;
  section?: string;
  adminOnly?: boolean;
}> = [
  { to: "/", label: "Dashboard", icon: LayoutDashboard, section: "Özet" },
  { to: "/patients", label: "Hastalar", icon: Users, section: "Operasyon" },
  { to: "/donors", label: "Donorlar", icon: HeartHandshake, section: "Operasyon" },
  { to: "/sessions", label: "Aferez seansları", icon: Beaker, section: "Operasyon" },
  { to: "/bags", label: "Torbalar", icon: Boxes, section: "Cryo" },
  { to: "/cryo", label: "Cryo Grid", icon: Snowflake, section: "Cryo" },
  { to: "/movements", label: "Hareket kayıtları", icon: History, section: "Cryo" },
  { to: "/inventory", label: "Envanter yönetimi", icon: Database, section: "Yönetim" },
  { to: "/plc", label: "PLC entegrasyonu", icon: Cpu, section: "Yönetim", adminOnly: true },
  {
    to: "/clinical-settings",
    label: "Klinik eşikleri",
    icon: SlidersHorizontal,
    section: "Yönetim",
    adminOnly: true,
  },
  { to: "/users", label: "Kullanıcı yönetimi", icon: Users, section: "Yönetim", adminOnly: true },
];

export default function Layout() {
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout, token } = useAuth();
  const isAdmin = Boolean(token && hasJwtRole(token, "Admin"));
  const qc = useQueryClient();
  const [connState, setConnState] = useState<HubConnectionState>(HubConnectionState.Disconnected);
  const [mobileNavOpen, setMobileNavOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate("/login", { replace: true });
  };

  useEffect(() => {
    let cancelled = false;
    getCryoConnection().catch(() => {
      /* reconnection loop will handle retries */
    });
    const unsub = onConnectionState((s) => {
      if (cancelled) return;
      setConnState(s);
      // On (re)connect, refresh everything that depends on realtime events
      // to cover any broadcasts missed while the socket was down.
      if (s === HubConnectionState.Connected) {
        qc.invalidateQueries({ queryKey: ["dashboard"] });
        qc.invalidateQueries({ queryKey: ["bags"] });
        qc.invalidateQueries({ queryKey: ["cryo-grid"] });
        qc.invalidateQueries({ queryKey: ["movements"] });
      }
    });
    return () => {
      cancelled = true;
      unsub();
    };
  }, [qc]);

  useEffect(() => {
    setMobileNavOpen(false);
  }, [location.pathname]);

  // Global SignalR event handlers — invalidate common caches on any cryo/dashboard change
  // and prevent the "No client method found" warnings by ensuring all events are registered app-wide.
  useEffect(() => {
    const bag = () => {
      qc.invalidateQueries({ queryKey: ["bags"] });
      qc.invalidateQueries({ queryKey: ["cryo-grid"] });
      qc.invalidateQueries({ queryKey: ["dashboard"] });
      qc.invalidateQueries({ queryKey: ["movements"] });
    };
    const offs = [
      onCryo("DashboardUpdated", () => qc.invalidateQueries({ queryKey: ["dashboard"] })),
      onCryo("BagStored", bag),
      onCryo("BagMoved", bag),
      onCryo("BagUsed", bag),
    ];
    return () => offs.forEach((off) => off());
  }, [qc]);

  return (
    <div className="flex min-h-screen">
      <aside className="hidden lg:flex w-64 shrink-0 flex-col border-r border-line/60 bg-bg-subtle/60 backdrop-blur-xl">
        <div className="flex items-center gap-2.5 px-5 py-5">
          <Logo />
          <div>
            <div className="text-sm font-semibold tracking-tight">CryoFlow</div>
            <div className="text-[11px] text-ink-dim">Cryo & Apheresis</div>
          </div>
        </div>
        <nav className="flex-1 overflow-y-auto px-3 py-2 space-y-1">
          {groupedSections(nav).map(([section, items]) => (
            <div key={section} className="mb-2">
              <div className="px-3 pt-2 pb-1 text-[10px] uppercase tracking-wider text-ink-dim">
                {section}
              </div>
              {items.filter((n) => !n.adminOnly || isAdmin).map((n) => {
                const active =
                  n.to === "/"
                    ? location.pathname === "/"
                    : location.pathname.startsWith(n.to);
                const Icon = n.icon;
                return (
                  <NavLink
                    key={n.to}
                    to={n.to}
                    className={cn(
                      "group flex items-center gap-3 rounded-xl px-3 py-2 text-sm transition",
                      active
                        ? "bg-brand-500/10 text-brand-400 border border-brand-500/20 shadow-glow"
                        : "text-ink-muted hover:text-ink hover:bg-bg-elevated/70 border border-transparent",
                    )}
                  >
                    <Icon className="size-4" />
                    {n.label}
                  </NavLink>
                );
              })}
            </div>
          ))}
        </nav>
        <div className="border-t border-line/60 p-4 space-y-3">
          <ConnPill state={connState} />
          {user && (
            <div className="flex items-center gap-2.5 rounded-xl border border-line/60 bg-bg-elevated/40 px-2.5 py-2">
              <div className="grid place-items-center size-8 rounded-full bg-bg-subtle border border-line/60">
                <UserCircle2 className="size-4 text-ink-muted" />
              </div>
              <div className="min-w-0 flex-1">
                <div className="text-[11px] text-ink-dim">Oturum</div>
                <div className="text-xs text-ink truncate" title={user.email}>
                  {user.email}
                </div>
              </div>
              <button
                type="button"
                onClick={handleLogout}
                title="Çıkış yap"
                className="rounded-lg p-1.5 text-ink-dim hover:text-accent-rose hover:bg-rose-500/10"
              >
                <LogOut className="size-3.5" />
              </button>
            </div>
          )}
        </div>
      </aside>

      {mobileNavOpen && (
        <>
          <button
            type="button"
            aria-label="Menüyü kapat"
            className="lg:hidden fixed inset-0 z-40 bg-black/40"
            onClick={() => setMobileNavOpen(false)}
          />
          <aside className="lg:hidden fixed inset-y-0 left-0 z-50 w-72 max-w-[85vw] flex flex-col border-r border-line/60 bg-bg/95 backdrop-blur-xl">
            <div className="flex items-center justify-between px-5 py-5 border-b border-line/60">
              <div className="flex items-center gap-2.5">
                <Logo />
                <div>
                  <div className="text-sm font-semibold tracking-tight">CryoFlow</div>
                  <div className="text-[11px] text-ink-dim">Cryo & Apheresis</div>
                </div>
              </div>
              <button
                type="button"
                onClick={() => setMobileNavOpen(false)}
                className="rounded-lg p-1.5 text-ink-dim hover:text-ink hover:bg-bg-elevated"
                aria-label="Menüyü kapat"
              >
                <X className="size-4" />
              </button>
            </div>
            <nav className="flex-1 overflow-y-auto px-3 py-3 space-y-1">
              {groupedSections(nav).map(([section, items]) => (
                <div key={section} className="mb-2">
                  <div className="px-3 pt-2 pb-1 text-[10px] uppercase tracking-wider text-ink-dim">
                    {section}
                  </div>
                  {items.filter((n) => !n.adminOnly || isAdmin).map((n) => {
                    const active =
                      n.to === "/"
                        ? location.pathname === "/"
                        : location.pathname.startsWith(n.to);
                    const Icon = n.icon;
                    return (
                      <NavLink
                        key={n.to}
                        to={n.to}
                        className={cn(
                          "group flex items-center gap-3 rounded-xl px-3 py-2 text-sm transition",
                          active
                            ? "bg-brand-500/10 text-brand-400 border border-brand-500/20 shadow-glow"
                            : "text-ink-muted hover:text-ink hover:bg-bg-elevated/70 border border-transparent",
                        )}
                        onClick={() => setMobileNavOpen(false)}
                      >
                        <Icon className="size-4" />
                        {n.label}
                      </NavLink>
                    );
                  })}
                </div>
              ))}
            </nav>
            <div className="border-t border-line/60 p-4 space-y-3">
              <ConnPill state={connState} />
              {user && (
                <div className="flex items-center gap-2.5 rounded-xl border border-line/60 bg-bg-elevated/40 px-2.5 py-2">
                  <div className="grid place-items-center size-8 rounded-full bg-bg-subtle border border-line/60">
                    <UserCircle2 className="size-4 text-ink-muted" />
                  </div>
                  <div className="min-w-0 flex-1">
                    <div className="text-[11px] text-ink-dim">Oturum</div>
                    <div className="text-xs text-ink truncate" title={user.email}>
                      {user.email}
                    </div>
                  </div>
                  <button
                    type="button"
                    onClick={handleLogout}
                    title="Çıkış yap"
                    className="rounded-lg p-1.5 text-ink-dim hover:text-accent-rose hover:bg-rose-500/10"
                  >
                    <LogOut className="size-3.5" />
                  </button>
                </div>
              )}
            </div>
          </aside>
        </>
      )}

      <main className="flex-1 min-w-0">
        <header className="sticky top-0 z-30 border-b border-line/60 bg-bg/70 backdrop-blur-xl">
          <div className="flex items-center justify-between px-5 lg:px-8 h-14">
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={() => setMobileNavOpen(true)}
                className="lg:hidden inline-flex items-center justify-center rounded-lg border border-line/60 bg-bg-elevated/50 p-1.5 text-ink-muted hover:text-ink"
                aria-label="Menüyü aç"
              >
                <Menu className="size-4" />
              </button>
              <span className="lg:hidden flex items-center gap-2">
                <Logo />
                <span className="text-sm font-semibold">CryoFlow</span>
              </span>
              <span className="text-xs text-ink-dim hidden sm:inline-flex items-center gap-1">
                <Activity className="size-3.5" /> Live operasyon paneli
              </span>
            </div>
            <div className="flex items-center gap-3">
              <ThemeToggle />
              <span className="hidden md:inline lg:hidden"><ConnPill state={connState} compact /></span>
              <span className="text-xs text-ink-dim">v0.1 · demo</span>
              {user && (
                <button
                  type="button"
                  onClick={handleLogout}
                  className="lg:hidden inline-flex items-center gap-1.5 rounded-lg border border-line/60 bg-bg-elevated/40 px-2.5 py-1.5 text-xs text-ink-muted hover:text-accent-rose hover:border-rose-500/30"
                >
                  <LogOut className="size-3.5" /> Çıkış
                </button>
              )}
            </div>
          </div>
        </header>
        <div className="px-5 lg:px-8 py-6 max-w-[1400px] mx-auto">
          <Outlet />
        </div>
      </main>
    </div>
  );
}

function groupedSections<T extends { section?: string }>(items: T[]): [string, T[]][] {
  const order: string[] = [];
  const map = new Map<string, T[]>();
  for (const it of items) {
    const key = it.section ?? "";
    if (!map.has(key)) {
      map.set(key, []);
      order.push(key);
    }
    map.get(key)!.push(it);
  }
  return order.map((k) => [k, map.get(k)!]);
}

function Logo() {
  return (
    <div className="size-8 rounded-lg overflow-hidden">
      <img src="/favicon.svg" alt="CryoFlow" className="size-full" />
    </div>
  );
}

function ConnPill({ state, compact }: { state: HubConnectionState; compact?: boolean }) {
  const ok = state === HubConnectionState.Connected;
  const reconnecting = state === HubConnectionState.Reconnecting;
  return (
    <span
      className={cn(
        "inline-flex items-center gap-2 rounded-full px-2.5 py-1 text-[11px] border",
        ok
          ? "border-emerald-500/30 bg-emerald-500/10 text-accent-mint"
          : reconnecting
            ? "border-amber-500/30 bg-amber-500/10 text-accent-amber animate-pulseGlow"
            : "border-rose-500/30 bg-rose-500/10 text-accent-rose",
      )}
    >
      {ok ? <Wifi className="size-3" /> : <WifiOff className="size-3" />}
      {compact ? (ok ? "live" : reconnecting ? "…" : "off") : ok ? "Real-time bağlı" : reconnecting ? "Yeniden bağlanıyor…" : "Bağlantı yok"}
    </span>
  );
}
