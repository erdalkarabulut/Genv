import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Bags, Movements } from "@/lib/api";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Input, Select } from "@/components/ui/Input";
import { Link } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import { formatDateTime, shortId } from "@/lib/utils";
import { onCryo } from "@/lib/signalr";
import { MoveRight, Search, History } from "lucide-react";
import { EmptyState } from "@/components/ui/EmptyState";

const actionTone = (action: string): "sky" | "mint" | "amber" | "neutral" | "rose" | "brand" => {
  const a = action.toLowerCase();
  if (a.includes("store")) return "sky";
  if (a.includes("move")) return "amber";
  if (a.includes("use")) return "mint";
  if (a.includes("split")) return "brand";
  if (a.includes("discard")) return "rose";
  return "neutral";
};

export default function MovementsPage() {
  const qc = useQueryClient();
  const movements = useQuery({ queryKey: ["movements"], queryFn: () => Movements.list(0, 500) });
  const bags = useQuery({ queryKey: ["bags"], queryFn: () => Bags.list(0, 500) });

  const [q, setQ] = useState("");
  const [action, setAction] = useState<string>("all");

  useEffect(() => {
    const a = onCryo("BagStored", () => qc.invalidateQueries({ queryKey: ["movements"] }));
    const b = onCryo("BagMoved", () => qc.invalidateQueries({ queryKey: ["movements"] }));
    const c = onCryo("BagUsed", () => qc.invalidateQueries({ queryKey: ["movements"] }));
    return () => { a(); b(); c(); };
  }, [qc]);

  const bagMap = useMemo(() => {
    const m = new Map<string, { bagNumber: number; purpose: string; status: string }>();
    (bags.data?.items ?? []).forEach((b) =>
      m.set(b.id, { bagNumber: b.bagNumber, purpose: b.purpose, status: b.status }),
    );
    return m;
  }, [bags.data]);

  const actions = useMemo(() => {
    const set = new Set<string>();
    (movements.data?.items ?? []).forEach((m) => set.add(m.action));
    return Array.from(set).sort();
  }, [movements.data]);

  const filtered = useMemo(() => {
    const items = movements.data?.items ?? [];
    return items
      .filter((m) => action === "all" || m.action === action)
      .filter((m) => {
        if (!q) return true;
        const ql = q.toLowerCase();
        return (
          m.action.toLowerCase().includes(ql) ||
          m.bagId.toLowerCase().startsWith(ql) ||
          bagMap.get(m.bagId)?.bagNumber.toString() === q
        );
      })
      .sort((a, b) => (a.createdDate < b.createdDate ? 1 : -1));
  }, [movements.data, action, q, bagMap]);

  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-2xl font-semibold tracking-tight">Hareket kayıtları</h1>
        <p className="text-sm text-ink-muted mt-1">
          Tüm bag store / move / use / split olaylarının zaman damgalı audit log'u.
        </p>
      </header>

      <Card>
        <div className="flex items-center gap-3 flex-wrap">
          <div className="relative flex-1 min-w-[240px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-ink-dim" />
            <input
              className="input pl-9"
              placeholder="Action, bag id veya numara ile ara…"
              value={q}
              onChange={(e) => setQ(e.target.value)}
            />
          </div>
          <Select
            value={action}
            onChange={(e) => setAction(e.target.value)}
            options={[
              { value: "all", label: "Tüm aksiyonlar" },
              ...actions.map((a) => ({ value: a, label: a })),
            ]}
            className="min-w-[220px]"
          />
        </div>
      </Card>

      {movements.isLoading ? (
        <Card className="h-40 skeleton" />
      ) : filtered.length === 0 ? (
        <EmptyState
          icon={<History className="size-10" />}
          title="Hareket yok"
          description="Seçili filtre için audit kaydı bulunamadı."
        />
      ) : (
        <Card className="!p-0 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="text-xs text-ink-dim uppercase tracking-wide">
                <tr className="border-b border-line/60">
                  <Th>Zaman</Th>
                  <Th>Bag</Th>
                  <Th>Action</Th>
                  <Th>From</Th>
                  <Th>To</Th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((m) => {
                  const bagInfo = bagMap.get(m.bagId);
                  return (
                    <tr
                      key={m.id}
                      className="border-b border-line/40 hover:bg-bg-elevated/40 transition"
                    >
                      <td className="px-4 py-3 text-ink-muted">{formatDateTime(m.createdDate)}</td>
                      <td className="px-4 py-3">
                        <Link
                          to={`/bags/${m.bagId}`}
                          className="text-sm font-medium hover:text-brand-400"
                        >
                          {bagInfo ? `#${bagInfo.bagNumber}` : shortId(m.bagId)}
                        </Link>
                        {bagInfo && (
                          <div className="text-[11px] text-ink-dim">
                            {bagInfo.purpose} · {bagInfo.status}
                          </div>
                        )}
                      </td>
                      <td className="px-4 py-3">
                        <Badge tone={actionTone(m.action)} dot>{m.action}</Badge>
                      </td>
                      <td className="px-4 py-3 text-ink-muted">
                        {m.fromSlotId ? shortId(m.fromSlotId) : "—"}
                      </td>
                      <td className="px-4 py-3 text-ink-muted inline-flex items-center gap-1">
                        <MoveRight className="size-3 text-ink-dim" />
                        {m.toSlotId ? shortId(m.toSlotId) : "—"}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </Card>
      )}
    </div>
  );
}

function Th({ children }: { children: React.ReactNode }) {
  return <th className="text-left px-4 py-3 font-medium">{children}</th>;
}
