import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Bags, Dashboard, Movements, Patients } from "@/lib/api";
import { Card, CardHeader, Stat } from "@/components/ui/Card";
import {
  Activity,
  Beaker,
  Snowflake,
  Users,
  Boxes as BoxesIcon,
  AlertTriangle,
  CheckCircle2,
  MoveRight,
  History,
} from "lucide-react";
import { Badge } from "@/components/ui/Badge";
import { formatNumber, formatDate, formatDateTime, shortId } from "@/lib/utils";
import { Link } from "react-router-dom";
import { useEffect, useState } from "react";
import { onConnectionState, onCryo } from "@/lib/signalr";
import { HubConnectionState } from "@microsoft/signalr";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

const statusColors: Record<string, string> = {
  Frozen: "#38bdf8",
  Stored: "#5eead4",
  Reserved: "#fbbf24",
  Used: "#9aa0b8",
  Discarded: "#fb7185",
};

export default function DashboardPage() {
  const qc = useQueryClient();
  const [live, setLive] = useState(false);
  // When the SignalR hub is down, fall back to short-interval polling so the
  // command centre stays fresh. When it reconnects, we rely on broadcasts.
  const pollingMs = live ? false : 15_000;

  useEffect(() => {
    const unsub = onConnectionState((s) => setLive(s === HubConnectionState.Connected));
    return () => unsub();
  }, []);

  const dash = useQuery({
    queryKey: ["dashboard"],
    queryFn: Dashboard.get,
    refetchInterval: pollingMs,
    refetchOnWindowFocus: true,
  });
  const patients = useQuery({ queryKey: ["patients", "recent"], queryFn: () => Patients.list(0, 5) });
  const movements = useQuery({
    queryKey: ["movements", "recent"],
    queryFn: () => Movements.list(0, 8),
    refetchInterval: pollingMs,
  });
  const bags = useQuery({
    queryKey: ["bags", "for-dashboard"],
    queryFn: () => Bags.list(0, 500),
    refetchInterval: pollingMs,
  });

  useEffect(() => {
    const a = onCryo("DashboardUpdated", () => {
      qc.invalidateQueries({ queryKey: ["dashboard"] });
      qc.invalidateQueries({ queryKey: ["movements", "recent"] });
      qc.invalidateQueries({ queryKey: ["bags", "for-dashboard"] });
    });
    const b = onCryo("BagStored", () =>
      qc.invalidateQueries({ queryKey: ["movements", "recent"] }),
    );
    const c = onCryo("BagMoved", () =>
      qc.invalidateQueries({ queryKey: ["movements", "recent"] }),
    );
    const d = onCryo("BagUsed", () =>
      qc.invalidateQueries({ queryKey: ["movements", "recent"] }),
    );
    return () => { a(); b(); c(); d(); };
  }, [qc]);

  const bagMap = (bags.data?.items ?? []).reduce<Map<string, { bagNumber: number }>>(
    (m, b) => m.set(b.id, { bagNumber: b.bagNumber }),
    new Map(),
  );

  const d = dash.data;

  const statusData = d
    ? [
        { name: "Frozen", value: d.frozenBags },
        { name: "Stored", value: d.storedBags },
        { name: "Reserved", value: d.reservedBags },
        { name: "Used", value: d.usedBags },
        { name: "Discarded", value: d.discardedBags },
      ].filter((x) => x.value > 0)
    : [];

  const occRate =
    d && d.totalSlots > 0 ? Math.round((d.occupiedSlots / d.totalSlots) * 100) : 0;

  return (
    <div className="space-y-6">
      <header className="flex items-end justify-between gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Komuta merkezi</h1>
          <p className="text-sm text-ink-muted mt-1">
            Aferez, kümülatif CD34/CD3, torba yaşam döngüsü ve cryo doluluk anlık takibi.
          </p>
        </div>
        <Link to="/patients" className="btn-primary">
          <Users className="size-4" /> Yeni hasta &rarr;
        </Link>
      </header>

      <section className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
        <Link to="/patients" className="block transition hover:scale-[1.01]">
          <Stat
            label="Hastalar"
            value={d?.totalPatients ?? "—"}
            hint={`${d?.totalSessions ?? 0} aferez seansı · detay için aç`}
            icon={<Users className="size-4" />}
          />
        </Link>
        <Link to="/bags" className="block transition hover:scale-[1.01]">
          <Stat
            label="Toplam Torba"
            value={d?.totalBags ?? "—"}
            hint={`Frozen ${d?.frozenBags ?? 0} · Stored ${d?.storedBags ?? 0} · Used ${d?.usedBags ?? 0}`}
            icon={<BoxesIcon className="size-4" />}
          />
        </Link>
        <Link to="/cryo" className="block transition hover:scale-[1.01]">
          <Stat
            label="Cryo hücre doluluk"
            value={`${occRate}%`}
            hint={`${d?.occupiedSlots ?? 0} / ${d?.totalSlots ?? 0} hücre · grid'i aç`}
            icon={<Snowflake className="size-4" />}
          />
        </Link>
        <Link to="/sessions" className="block transition hover:scale-[1.01]">
          <Stat
            label="Kümülatif CD34/kg"
            value={formatNumber(d?.totalCd34PerKg ?? 0, 2)}
            hint={`CD3/kg toplam: ${formatNumber(d?.totalCd3PerKg ?? 0, 2)} · seanslar`}
            icon={<Activity className="size-4" />}
          />
        </Link>
      </section>

      <section className="grid grid-cols-1 xl:grid-cols-3 gap-4">
        <Card className="xl:col-span-2">
          <CardHeader
            title="Torba durumu dağılımı"
            subtitle="Gerçek zamanlı yaşam döngüsü görünümü"
            right={
              <Badge tone={live ? "mint" : "amber"} dot>
                {live ? "Live" : "Polling"}
              </Badge>
            }
          />
          {statusData.length === 0 ? (
            <EmptyChart hint="Henüz torba kaydı yok." />
          ) : (
            <div className="h-64">
              <ResponsiveContainer>
                <BarChart data={statusData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                  <defs>
                    <linearGradient id="bg" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="#6366f1" stopOpacity={0.9} />
                      <stop offset="100%" stopColor="#6366f1" stopOpacity={0.4} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid stroke="#1f2138" vertical={false} />
                  <XAxis dataKey="name" stroke="#6b7191" fontSize={12} tickLine={false} axisLine={false} />
                  <YAxis stroke="#6b7191" fontSize={12} tickLine={false} axisLine={false} allowDecimals={false} />
                  <Tooltip
                    cursor={{ fill: "rgba(99,102,241,0.05)" }}
                    contentStyle={{
                      background: "#13141d",
                      border: "1px solid #23253a",
                      borderRadius: 12,
                      color: "#e6e8f3",
                    }}
                  />
                  <Bar dataKey="value" radius={[8, 8, 0, 0]}>
                    {statusData.map((s, i) => (
                      <Cell key={i} fill={statusColors[s.name] ?? "#6366f1"} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            </div>
          )}
        </Card>

        <Card>
          <CardHeader title="Risk göstergesi" subtitle="Son aferez kümülatifine göre" />
          <RiskGauge
            cd34={d?.totalCd34PerKg ?? 0}
            cd3={d?.totalCd3PerKg ?? 0}
            status={d?.riskStatus}
          />
        </Card>
      </section>

      <section className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <Card>
          <CardHeader
            title="Son hareket kayıtları"
            subtitle="Live audit log · son 8 olay"
            right={
              <Link to="/movements" className="btn-ghost text-xs">
                Tümü &rarr;
              </Link>
            }
          />
          {movements.isLoading ? (
            <div className="space-y-2">
              {Array.from({ length: 4 }).map((_, i) => (
                <div key={i} className="skeleton h-10" />
              ))}
            </div>
          ) : (movements.data?.items ?? []).length === 0 ? (
            <div className="grid h-32 place-items-center text-center">
              <div>
                <History className="mx-auto size-7 text-ink-dim" />
                <p className="mt-2 text-xs text-ink-muted">Henüz hareket yok.</p>
              </div>
            </div>
          ) : (
            <ul className="space-y-2 -mx-1">
              {(movements.data?.items ?? []).slice(0, 8).map((m) => {
                const bagInfo = bagMap.get(m.bagId);
                return (
                  <li
                    key={m.id}
                    className="flex items-center gap-2 rounded-lg px-2 py-2 hover:bg-bg-elevated/50 transition"
                  >
                    <span className="inline-grid size-8 place-items-center rounded-lg bg-brand-500/10 text-brand-400 shrink-0">
                      <MoveRight className="size-3.5" />
                    </span>
                    <div className="min-w-0 flex-1">
                      <div className="text-sm font-medium">
                        <Link to={`/bags/${m.bagId}`} className="hover:text-brand-400">
                          {bagInfo ? `Bag #${bagInfo.bagNumber}` : shortId(m.bagId)}
                        </Link>{" "}
                        <span className="text-ink-dim font-normal">— {m.action}</span>
                      </div>
                      <div className="text-[11px] text-ink-dim">
                        {formatDateTime(m.createdDate)}
                      </div>
                    </div>
                  </li>
                );
              })}
            </ul>
          )}
        </Card>

        <Card>
          <CardHeader
            title="Son hastalar"
            subtitle="En son eklenen kayıtlar"
            right={<Link to="/patients" className="btn-ghost text-xs">Tümü &rarr;</Link>}
          />
          <div className="divide-y divide-line/60 -mx-2">
            {patients.isLoading && (
              <div className="space-y-2 px-2">
                {Array.from({ length: 4 }).map((_, i) => (
                  <div key={i} className="skeleton h-10" />
                ))}
              </div>
            )}
            {patients.data?.items?.length === 0 && (
              <p className="text-sm text-ink-muted px-2 py-4">Henüz hasta yok.</p>
            )}
            {patients.data?.items?.map((p) => (
              <Link
                key={p.id}
                to={`/patients/${p.id}`}
                className="flex items-center justify-between px-2 py-3 hover:bg-bg-elevated/50 rounded-lg transition"
              >
                <div className="flex items-center gap-3 min-w-0">
                  <div className="grid place-items-center size-9 rounded-full bg-brand-500/10 text-brand-400 text-sm font-semibold shrink-0">
                    {p.fullName?.slice(0, 2).toUpperCase()}
                  </div>
                  <div className="min-w-0">
                    <div className="text-sm font-medium truncate">{p.fullName}</div>
                    <div className="text-xs text-ink-muted truncate">
                      {p.transplantType === "Autologous" ? "Otolog" : "Allogeneik"} · {p.weightKg} kg · {formatDate(p.createdDate)}
                    </div>
                  </div>
                </div>
                <Badge tone={p.transplantType === "Autologous" ? "sky" : "brand"}>
                  {p.transplantType}
                </Badge>
              </Link>
            ))}
          </div>
        </Card>

        <Card>
          <CardHeader title="Bag amaç dağılımı" subtitle="Cryo / Infusion / Backup / QC" />
          {statusData.length === 0 ? (
            <EmptyChart hint="Henüz veri yok." />
          ) : (
            <div className="h-56">
              <ResponsiveContainer>
                <PieChart>
                  <Pie
                    data={statusData}
                    dataKey="value"
                    nameKey="name"
                    innerRadius={50}
                    outerRadius={80}
                    paddingAngle={3}
                    cornerRadius={6}
                  >
                    {statusData.map((s, i) => (
                      <Cell key={i} fill={statusColors[s.name] ?? "#6366f1"} stroke="transparent" />
                    ))}
                  </Pie>
                  <Tooltip
                    contentStyle={{
                      background: "#13141d",
                      border: "1px solid #23253a",
                      borderRadius: 12,
                      color: "#e6e8f3",
                    }}
                  />
                </PieChart>
              </ResponsiveContainer>
            </div>
          )}
          <div className="mt-2 grid grid-cols-2 gap-2">
            {statusData.map((s) => (
              <div key={s.name} className="flex items-center gap-2 text-xs text-ink-muted">
                <span
                  className="inline-block size-2 rounded-full"
                  style={{ background: statusColors[s.name] ?? "#6366f1" }}
                />
                {s.name} <span className="ml-auto text-ink">{s.value}</span>
              </div>
            ))}
          </div>
        </Card>
      </section>
    </div>
  );
}

function RiskGauge({ cd34, cd3, status }: { cd34: number; cd3: number; status?: string }) {
  let tone: "mint" | "amber" | "rose" | "sky" = "amber";
  let icon = AlertTriangle;
  let title = status ?? "Veri bekleniyor";
  if (cd34 >= 4 && cd3 >= 3 && cd3 <= 8) {
    tone = "mint";
    icon = CheckCircle2;
    title = status ?? "Optimal";
  } else if (cd34 >= 4 && cd3 > 10) {
    tone = "rose";
    title = status ?? "GVHD riski";
  } else if (cd34 >= 4 && cd3 < 2) {
    tone = "sky";
    title = status ?? "Düşük bağışıklık";
  } else if (cd34 < 2) {
    tone = "rose";
    title = status ?? "Yetersiz";
  } else {
    tone = "amber";
    title = status ?? "Sınırda";
  }
  const Icon = icon;
  return (
    <div className="flex items-center gap-4">
      <div className="grid place-items-center size-16 rounded-2xl bg-bg-elevated border border-line/60">
        <Icon className={iconColor(tone)} />
      </div>
      <div className="flex-1">
        <Badge tone={tone} dot className="mb-2">
          {title}
        </Badge>
        <div className="grid grid-cols-2 gap-2">
          <Mini label="CD34/kg" value={formatNumber(cd34, 2)} />
          <Mini label="CD3/kg" value={formatNumber(cd3, 2)} />
        </div>
        <p className="mt-2 text-[11px] text-ink-dim">
          UI yalnızca veri sunar; klinik karar hekime aittir.
        </p>
      </div>
    </div>
  );
}

function Mini({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg bg-bg-elevated/70 border border-line/60 px-3 py-2">
      <div className="text-[10px] uppercase text-ink-dim tracking-wide">{label}</div>
      <div className="text-base font-semibold mt-0.5">{value}</div>
    </div>
  );
}

function iconColor(tone: "mint" | "amber" | "rose" | "sky") {
  return {
    mint: "text-accent-mint",
    amber: "text-accent-amber",
    rose: "text-accent-rose",
    sky: "text-accent-sky",
  }[tone] + " size-7";
}

function EmptyChart({ hint }: { hint: string }) {
  return (
    <div className="grid h-56 place-items-center text-center">
      <div>
        <Beaker className="mx-auto size-8 text-ink-dim" />
        <p className="mt-2 text-sm text-ink-muted">{hint}</p>
      </div>
    </div>
  );
}
