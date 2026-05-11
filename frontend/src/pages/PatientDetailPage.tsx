import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate, useParams, Link } from "react-router-dom";
import { ApheresisPlans, BagCells, Bags, ClinicalSettingsApi, Dashboard, Donors, Patients, Sessions } from "@/lib/api";
import { Card, CardHeader, Stat } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { formatDate, formatNumber, calculateAbsoluteCellCount, calculateCellDose } from "@/lib/utils";
import {
  ArrowLeft,
  Beaker,
  Boxes,
  Calculator,
  CheckCircle2,
  ChevronRight,
  Droplet,
  ExternalLink,
  MapPin,
  Pencil,
  Plus,
  Scissors,
  Snowflake,
  Sparkles,
  StickyNote,
  Trash,
  TimerReset,
  Trash2,
  TriangleAlert,
} from "lucide-react";
import { Modal } from "@/components/ui/Modal";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { PatientFormView } from "./PatientsPage";
import { useEffect, useMemo, useState } from "react";
import { onCryo } from "@/lib/signalr";
import { Input, Select } from "@/components/ui/Input";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import {
  Area,
  AreaChart,
  CartesianGrid,
  ReferenceLine,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import type { ApheresisPlanResponse, Bag, CryoGridResponse } from "@/lib/types";

export default function PatientDetailPage() {
  const { id } = useParams<{ id: string }>();
  const nav = useNavigate();
  const qc = useQueryClient();

  const patient = useQuery({ queryKey: ["patient", id], queryFn: () => Patients.byId(id!), enabled: !!id });
  const plan = useQuery({
    queryKey: ["apheresis-plan", id],
    queryFn: () => ApheresisPlans.byPatient(id!),
    enabled: !!id,
  });

  const [sessionOpen, setSessionOpen] = useState(false);
  const [customSplitOpen, setCustomSplitOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);

  const donors = useQuery({ queryKey: ["donors"], queryFn: () => Donors.list(0, 500) });

  const removePatient = useMutation({
    mutationFn: () => Patients.remove(id!),
    onSuccess: () => {
      toast.success("Hasta silindi");
      nav("/patients");
    },
  });

  const refresh = async () => {
    await Promise.all([
      qc.invalidateQueries({ queryKey: ["apheresis-plan", id] }),
      qc.invalidateQueries({ queryKey: ["patient", id] }),
      qc.invalidateQueries({ queryKey: ["dashboard"] }),
      qc.invalidateQueries({ queryKey: ["bags"] }),
      qc.invalidateQueries({ queryKey: ["cryo-grid"] }),
      qc.invalidateQueries({ queryKey: ["patients"] }),
    ]);
    await plan.refetch();
  };

  useEffect(() => {
    const off = onCryo("DashboardUpdated", () => {
      qc.invalidateQueries({ queryKey: ["apheresis-plan", id] });
      qc.invalidateQueries({ queryKey: ["patient", id] });
    });
    return off;
  }, [id, qc]);

  if (!id) return null;

  const p = plan.data;

  return (
    <div className="space-y-6">
      <button
        onClick={() => nav("/patients")}
        className="text-xs text-ink-muted hover:text-ink inline-flex items-center gap-1"
      >
        <ArrowLeft className="size-3.5" /> Hastalar
      </button>

      <header className="flex items-end justify-between gap-4 flex-wrap">
        <div className="flex items-center gap-4">
          <div className="grid place-items-center size-14 rounded-2xl bg-gradient-to-br from-brand-500/30 to-emerald-400/20 text-brand-400 text-lg font-semibold">
            {patient.data?.fullName?.slice(0, 2).toUpperCase() ?? "—"}
          </div>
          <div>
            <h1 className="text-2xl font-semibold tracking-tight">
              {patient.data?.fullName ?? "Hasta"}
            </h1>
            <div className="mt-1 flex items-center gap-2 text-sm text-ink-muted">
              <Badge tone={p?.isAutologous ? "sky" : "brand"} dot>
                {p?.transplantType ?? patient.data?.transplantType}
              </Badge>
              <span>· {patient.data?.weightKg ?? "—"} kg</span>
              {patient.data?.protocolNo && <span>· {patient.data.protocolNo}</span>}
            </div>
          </div>
        </div>
        <div className="flex items-center gap-2 flex-wrap">
          <Button
            variant="soft"
            icon={<Pencil className="size-4" />}
            onClick={() => setEditOpen(true)}
          >
            Düzenle
          </Button>
          <Button
            variant="danger"
            icon={<Trash2 className="size-4" />}
            onClick={() => setDeleteOpen(true)}
          >
            Sil
          </Button>
          <Button
            variant="soft"
            icon={<Plus className="size-4" />}
            onClick={() => setSessionOpen(true)}
          >
            Aferez seansı ekle
          </Button>
          <Button
            variant="soft"
            icon={<Snowflake className="size-4" />}
            disabled={!p?.completedSessions?.length}
            onClick={() => setCustomSplitOpen(true)}
            title="Hacim ve WBC değerlerini kendiniz girerek özel torba oluşturun ve istediğiniz slotta dondurun"
          >
            Özel torba + Dondur
          </Button>
        </div>
      </header>

      {p && <RecommendationBanner plan={p} />}

      <section className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <Stat
          label="Maks. aferez günü"
          value={p?.maxCollectionDays ?? "—"}
          hint={p?.isAutologous ? "Otolog protokol" : "Allogeneik protokol"}
          icon={<TimerReset className="size-4" />}
        />
        <Stat
          label="Tamamlanan gün"
          value={`${p?.completedDays ?? 0} / ${p?.maxCollectionDays ?? 0}`}
          hint={`Kalan: ${p?.remainingDays ?? 0}`}
          icon={<CheckCircle2 className="size-4" />}
        />
        <Stat
          label="Kümülatif CD34/kg"
          value={formatNumber(p?.cumulativeCd34PerKg ?? 0, 2)}
          hint={`Hedef ≥ ${formatNumber(p?.targetCd34PerKg ?? 0, 1)} · İdeal ${formatNumber(
            p?.idealCd34PerKg ?? 0,
            1,
          )}`}
          icon={<Droplet className="size-4" />}
        />
        {p?.isAutologous ? (
          <Stat
            label="Hedef CD34/kg"
            value={formatNumber(p?.idealCd34PerKg ?? 0, 1)}
            hint="Otolog · CD3 takibi uygulanmaz"
            icon={<Beaker className="size-4" />}
          />
        ) : (
          <Stat
            label="Kümülatif CD3/kg"
            value={formatNumber(p?.cumulativeCd3PerKg ?? 0, 2)}
            hint="3-8 ideal · >10 GVHD"
            icon={<Beaker className="size-4" />}
          />
        )}
      </section>

      <section className="grid grid-cols-1 xl:grid-cols-3 gap-4">
        <Card className="xl:col-span-2">
          <CardHeader
            title="Kümülatif CD34/CD3 trendi"
            subtitle="Tamamlanan günler dolu, planlananlar kesik çizgi ile"
            right={
              p?.isOptimal ? (
                <Badge tone="mint" dot>İdeal</Badge>
              ) : p?.isSufficient ? (
                <Badge tone="amber" dot>Sınırda</Badge>
              ) : (
                <Badge tone="rose" dot>Yetersiz</Badge>
              )
            }
          />
          <CumulativeChart plan={p} />
        </Card>

        <Card>
          <CardHeader title="Aferez timeline" subtitle="Gün gün ilerleme" />
          <Timeline
            plan={p}
            patientId={id}
            onChanged={refresh}
            patientWeight={p?.weightKg ?? patient.data?.weightKg}
            isAutologous={p?.isAutologous ?? patient.data?.transplantType === "Autologous"}
          />
        </Card>
      </section>

      <PatientBagsCard
        plan={p}
        isAutologous={p?.isAutologous ?? patient.data?.transplantType === "Autologous"}
      />

      <Modal
        open={sessionOpen}
        onClose={() => setSessionOpen(false)}
        title="Aferez seansı"
        description={`Sonraki gün: ${p?.nextDay ?? 1}. gün · Pre-procedure (PK) + post-procedure (ÜRÜN) birlikte kaydedilir`}
        size="xl"
      >
        {sessionOpen && (
          <CreateSessionForm
            key={`session-form-${sessionOpen ? "open" : "closed"}-${p?.nextDay ?? 1}`}
            patientId={id}
            weightKg={p?.weightKg ?? patient.data?.weightKg}
            defaultDay={p?.nextDay ?? (p ? (p.completedDays ?? 0) + 1 : 1)}
            isAutologous={p?.isAutologous ?? patient.data?.transplantType === "Autologous"}
            onCancel={() => setSessionOpen(false)}
            onCreated={async () => {
              await refresh();
              setSessionOpen(false);
              toast.success("Aferez seansı kaydedildi ve hesaplandı");
            }}
          />
        )}
      </Modal>

      <Modal
        open={customSplitOpen}
        onClose={() => setCustomSplitOpen(false)}
        title="Özel torba oluştur + Dondur"
        description="Her torba için kendi hacim / WBC / yüzde değerlerinizi girin (ör. 32 + 32 = 64 ml). Torbayı istediğiniz slota dondurarak yerleştirin."
        size="xl"
      >
        {customSplitOpen && (
          <CustomSplitForm
            key={`custom-split-${customSplitOpen ? "open" : "closed"}`}
            plan={p}
            onCancel={() => setCustomSplitOpen(false)}
            onDone={() => {
              refresh();
              setCustomSplitOpen(false);
            }}
          />
        )}
      </Modal>

      <Card>
        <CardHeader
          title="Hızlı linkler"
          subtitle="Bu hastaya bağlı operasyonel ekranlar"
        />
        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          <QuickLink to="/cryo" label="Cryo grid" hint="Slot doluluğunu görüntüle" />
          <QuickLink to="/bags" label="Torbalar" hint="Tüm torba operasyonları" />
          <QuickLink to="/sessions" label="Seanslar" hint="Tüm aferez seansları" />
        </div>
      </Card>

      <Modal open={editOpen} onClose={() => setEditOpen(false)} title="Hastayı düzenle">
        {patient.data && (
          <PatientFormView
            initial={patient.data}
            donors={donors.data?.items ?? []}
            onCancel={() => setEditOpen(false)}
            onSubmit={async (d) => {
              await Patients.update({
                id: patient.data!.id,
                ...d,
                weightKg: Number(d.weightKg),
                birthDate: d.birthDate ? new Date(d.birthDate).toISOString() : undefined,
                donorId: d.donorId || undefined,
              });
              toast.success("Hasta güncellendi");
              await refresh();
              setEditOpen(false);
            }}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={deleteOpen}
        onClose={() => setDeleteOpen(false)}
        onConfirm={() => removePatient.mutate()}
        loading={removePatient.isPending}
        description={`"${patient.data?.fullName ?? ""}" adlı hastayı silmek istediğinize emin misiniz? Bağlı seans ve torba kayıtları varsa işlem başarısız olabilir.`}
      />
    </div>
  );
}

function PatientBagsCard({
  plan,
  isAutologous,
}: {
  plan?: ApheresisPlanResponse;
  isAutologous?: boolean;
}) {
  const sessionIds = (plan?.completedSessions ?? [])
    .map((s) => s.sessionId)
    .filter((x): x is string => !!x);

  const bagsQ = useQuery({
    queryKey: ["bags", "all"],
    queryFn: () => Bags.list(0, 500),
    enabled: sessionIds.length > 0,
  });
  const gridQ = useQuery({
    queryKey: ["cryo-grid"],
    queryFn: () => Dashboard.cryoGrid(),
    enabled: sessionIds.length > 0,
  });

  const sessionIdSet = useMemo(() => new Set(sessionIds), [sessionIds]);
  const bags = useMemo(
    () => (bagsQ.data?.items ?? []).filter((b) => sessionIdSet.has(b.sessionId)),
    [bagsQ.data, sessionIdSet],
  );
  const slotLocations = useMemo(
    () => buildSlotLocationMap(gridQ.data),
    [gridQ.data],
  );

  const sessionMeta = useMemo(() => {
    const m = new Map<string, { day: number; date: string }>();
    (plan?.completedSessions ?? []).forEach((s) => {
      if (s.sessionId) m.set(s.sessionId, { day: s.day, date: s.date });
    });
    return m;
  }, [plan]);

  const grouped = useMemo(() => {
    const g = new Map<string, Bag[]>();
    bags.forEach((b) => {
      const arr = g.get(b.sessionId) ?? [];
      arr.push(b);
      g.set(b.sessionId, arr);
    });
    return [...g.entries()]
      .map(([sid, items]) => ({
        sessionId: sid,
        meta: sessionMeta.get(sid),
        items: items.sort((a, b) => a.bagNumber - b.bagNumber),
      }))
      .sort((a, b) => (a.meta?.day ?? 0) - (b.meta?.day ?? 0));
  }, [bags, sessionMeta]);

  const totals = useMemo(() => {
    return {
      count: bags.length,
      cryo: bags.filter((b) => b.purpose === "Cryo").length,
      stored: bags.filter((b) => b.status === "Stored" || b.status === "Frozen").length,
      reserved: bags.filter((b) => b.status === "Reserved").length,
      used: bags.filter((b) => b.status === "Used").length,
      volumeMl: bags.reduce((s, b) => s + (b.volumeMl || 0), 0),
    };
  }, [bags]);

  return (
    <Card>
      <CardHeader
        title="Hastaya ait torbalar"
        subtitle="Tüm seanslardan üretilen torbalar ve cryo-grid konumları"
        right={
          totals.count > 0 ? (
            <div className="flex items-center gap-1.5 flex-wrap">
              <Badge tone="neutral">{totals.count} torba</Badge>
              <Badge tone="sky">{totals.cryo} Cryo</Badge>
              <Badge tone="brand">{totals.stored} depoda</Badge>
              {totals.reserved > 0 && <Badge tone="amber">{totals.reserved} bekliyor</Badge>}
              {totals.used > 0 && <Badge tone="mint">{totals.used} kullanıldı</Badge>}
            </div>
          ) : null
        }
      />

      {bagsQ.isLoading || gridQ.isLoading ? (
        <div className="skeleton h-24" />
      ) : bags.length === 0 ? (
        <p className="text-sm text-ink-muted">
          Bu hastaya ait torba yok. Aferez seansı tamamlandıktan sonra{" "}
          <b>Özel torba + Dondur</b> ile torba oluşturabilirsiniz.
        </p>
      ) : (
        <div className="space-y-4">
          {grouped.map((g) => (
            <div key={g.sessionId} className="space-y-2">
              <div className="flex items-center gap-2 text-[11px] uppercase tracking-wide text-ink-dim border-b border-line/60 pb-1">
                <Beaker className="size-3.5" />
                Gün {g.meta?.day ?? "—"} · {formatDate(g.meta?.date)}
                <span className="ml-auto normal-case text-ink-dim">
                  {g.items.length} torba ·{" "}
                  {formatNumber(
                    g.items.reduce((s, b) => s + b.volumeMl, 0),
                    1,
                  )}{" "}
                  ml
                </span>
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-2">
                {g.items.map((b) => (
                  <BagMiniCard
                    key={b.id}
                    bag={b}
                    location={b.bagCellId ? slotLocations.get(b.bagCellId) : undefined}
                    isAutologous={isAutologous}
                  />
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
}

function BagMiniCard({
  bag,
  location,
  isAutologous,
}: {
  bag: Bag;
  location?: SlotLocation;
  isAutologous?: boolean;
}) {
  return (
    <Link
      to={`/bags/${bag.id}`}
      className="block rounded-xl border border-line/60 bg-bg-elevated/40 p-3 transition hover:border-brand-500/40 hover:shadow-glow"
    >
      <div className="flex items-start justify-between gap-2">
        <div className="flex items-center gap-2">
          <div className="grid place-items-center size-8 rounded-lg bg-brand-500/10 text-brand-400 text-xs font-semibold">
            #{bag.bagNumber}
          </div>
          <div>
            <div className="text-sm font-medium inline-flex items-center gap-1">
              Bag #{bag.bagNumber}
              <ExternalLink className="size-3 text-ink-dim" />
            </div>
            <div className="text-[10px] text-ink-dim">
              {formatNumber(bag.volumeMl, 1)} ml · {purposeShort(bag.purpose)}
            </div>
          </div>
        </div>
        <Badge tone={statusToneForBag(bag.status)} dot>
          {statusLabel(bag.status)}
        </Badge>
      </div>

      <div className="mt-2.5 grid grid-cols-2 gap-1.5 text-[11px]">
        <MiniStat label="CD34/kg" value={formatNumber(bag.cd34PerKg, 2)} />
        {!isAutologous && <MiniStat label="CD3/kg" value={formatNumber(bag.cd3PerKg, 2)} />}
        {bag.wbc != null && <MiniStat label="WBC" value={formatNumber(bag.wbc, 2)} />}
        {bag.cd34Percent != null && (
          <MiniStat label="%CD34" value={formatNumber(bag.cd34Percent, 2)} />
        )}
      </div>

      <div className="mt-2.5 border-t border-line/60 pt-2">
        {location ? (
          <div className="flex items-center gap-1.5 text-[11px] text-ink">
            <MapPin className="size-3 text-brand-400 shrink-0" />
            <span className="truncate">
              <span className="text-ink-dim">{location.tankName}</span>
              <span className="mx-1 text-ink-dim">/</span>
              <span className="text-ink-dim">{location.rackName}</span>
              <span className="mx-1 text-ink-dim">/</span>
              <span className="text-ink-dim">{location.rackSlotName}</span>
              <span className="mx-1 text-ink-dim">/</span>
              <span className="text-ink-dim">{location.boxName}</span>
              <span className="mx-1 text-ink-dim">·</span>
              <span className="font-semibold text-brand-400">{location.position}</span>
            </span>
          </div>
        ) : (
          <div className="flex items-center gap-1.5 text-[11px] text-ink-dim">
            <Boxes className="size-3 shrink-0" />
            {bag.status === "Reserved"
              ? "Henüz hücreye yerleştirilmedi (Reserved)"
              : bag.status === "Used"
                ? "Kullanıldı — hücre boşaldı"
                : "Konum bilgisi yok"}
          </div>
        )}
        {bag.compositionNote && (
          <div className="mt-1.5 flex items-start gap-1.5 text-[11px] text-ink-dim">
            <StickyNote className="size-3 shrink-0 mt-0.5" />
            <span className="line-clamp-2">{bag.compositionNote}</span>
          </div>
        )}
      </div>
    </Link>
  );
}

function MiniStat({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md bg-bg-subtle/50 border border-line/50 px-2 py-1">
      <div className="text-[9px] uppercase tracking-wide text-ink-dim">{label}</div>
      <div className="text-[12px] font-medium text-ink">{value}</div>
    </div>
  );
}

type SlotLocation = {
  tankName: string;
  rackName: string;
  rackSlotName: string;
  boxName: string;
  position: string;
};

function buildSlotLocationMap(grid?: CryoGridResponse) {
  const map = new Map<string, SlotLocation>();
  if (!grid) return map;
  for (const tank of grid.tanks) {
    for (const rack of tank.racks) {
      for (const rackSlot of rack.slots) {
        for (const box of rackSlot.boxes) {
          for (const cell of box.bagCells) {
            map.set(cell.id, {
              tankName: tank.name,
              rackName: rack.name,
              rackSlotName: rackSlot.name,
              boxName: box.name,
              position: cell.position,
            });
          }
        }
      }
    }
  }
  return map;
}

function purposeShort(p: string) {
  switch (p) {
    case "Cryo":
      return "Cryo";
    case "Infusion":
      return "İnfüzyon";
    case "Backup":
      return "Yedek";
    case "QualityControl":
      return "QC";
    default:
      return p;
  }
}

function statusLabel(s: string) {
  switch (s) {
    case "Frozen":
      return "Donduruldu";
    case "Stored":
      return "Depoda";
    case "Reserved":
      return "Bekliyor";
    case "Used":
      return "Kullanıldı";
    case "Discarded":
      return "Elenmiş";
    default:
      return s;
  }
}

function statusToneForBag(
  s: string,
): "brand" | "mint" | "amber" | "rose" | "sky" | "neutral" {
  switch (s) {
    case "Frozen":
      return "sky";
    case "Stored":
      return "brand";
    case "Reserved":
      return "amber";
    case "Used":
      return "mint";
    case "Discarded":
      return "rose";
    default:
      return "neutral";
  }
}

function QuickLink({ to, label, hint }: { to: string; label: string; hint: string }) {
  return (
    <Link
      to={to}
      className="rounded-xl border border-line/60 bg-bg-elevated/40 hover:bg-bg-elevated px-4 py-3 transition flex items-center justify-between"
    >
      <div>
        <div className="text-sm font-medium">{label}</div>
        <div className="text-xs text-ink-muted">{hint}</div>
      </div>
      <ChevronRight className="size-4 text-ink-dim" />
    </Link>
  );
}

function RecommendationBanner({ plan }: { plan: ApheresisPlanResponse }) {
  const { isOptimal, isSufficient, maxDaysReached } = plan;
  const tone = isOptimal ? "mint" : isSufficient ? "amber" : maxDaysReached ? "rose" : "sky";
  const Icon = isOptimal ? Sparkles : isSufficient ? CheckCircle2 : maxDaysReached ? TriangleAlert : Calculator;
  const palette = {
    mint: "from-emerald-500/15 to-transparent border-emerald-500/30",
    amber: "from-amber-500/15 to-transparent border-amber-500/30",
    rose: "from-rose-500/15 to-transparent border-rose-500/30",
    sky: "from-sky-500/15 to-transparent border-sky-500/30",
  }[tone];
  return (
    <div className={`rounded-2xl border bg-gradient-to-r ${palette} p-4 flex items-start gap-3`}>
      <div className="grid place-items-center size-9 rounded-xl bg-bg-card/60 border border-line/60 shrink-0">
        <Icon className="size-4" />
      </div>
      <div className="text-sm text-ink">{plan.recommendation}</div>
    </div>
  );
}

function Timeline({
  plan,
  patientId,
  patientWeight,
  isAutologous,
  onChanged,
}: {
  plan?: ApheresisPlanResponse;
  patientId: string;
  patientWeight?: number;
  isAutologous?: boolean;
  onChanged: () => Promise<void> | void;
}) {
  if (!plan) return <div className="skeleton h-32" />;
  const days = plan.forecastPlan.length ? plan.forecastPlan : plan.completedSessions;
  if (!days.length)
    return (
      <p className="text-sm text-ink-muted">
        Henüz aferez seansı yok. İlk gün için "Aferez seansı ekle"yi kullanın.
      </p>
    );
  return (
    <ol className="relative ml-2 space-y-4 border-l border-line pl-4">
      {days.map((d) => (
        <TimelineDay
          key={`${d.day}-${d.isPlanned ? "p" : "d"}`}
          d={d}
          patientId={patientId}
          patientWeight={patientWeight}
          isAutologous={isAutologous}
          onChanged={onChanged}
        />
      ))}
    </ol>
  );
}

function TimelineDay({
  d,
  patientId,
  patientWeight,
  isAutologous,
  onChanged,
}: {
  d: ApheresisPlanResponse["completedSessions"][number];
  patientId: string;
  patientWeight?: number;
  isAutologous?: boolean;
  onChanged: () => Promise<void> | void;
}) {
  const [expanded, setExpanded] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [delOpen, setDelOpen] = useState(false);
  const planned = d.isPlanned;

  const removeMut = useMutation({
    mutationFn: () => Sessions.remove(d.sessionId!),
    onSuccess: async () => {
      toast.success(`Gün ${d.day} seansı silindi`);
      setDelOpen(false);
      await onChanged();
    },
  });
  const recalc = useMutation({
    mutationFn: () => Sessions.calculate(d.sessionId!),
    onSuccess: async (r) => {
      toast.success(
        isAutologous
          ? `Hesaplandı · CD34 ${formatNumber(r.cd34PerKg, 2)}`
          : `Hesaplandı · CD34 ${formatNumber(r.cd34PerKg, 2)} / CD3 ${formatNumber(r.cd3PerKg, 2)}`,
      );
      await onChanged();
    },
  });
  const hasLabs =
    !planned &&
    (d.wbcPre != null ||
      d.hgb != null ||
      d.hct != null ||
      d.plt != null ||
      d.volumeMl != null ||
      d.wbc != null ||
      d.cd34Percent != null ||
      d.cd45Percent != null ||
      d.cd3Percent != null);
  return (
    <li className="relative">
      <span
        className={`absolute -left-[22px] top-1 grid size-4 place-items-center rounded-full border ${
          planned ? "border-line bg-bg" : "border-brand-500 bg-brand-500/40 shadow-glow"
        }`}
      >
        <span className="size-1.5 rounded-full bg-brand-400" />
      </span>
      <div className="flex items-center justify-between">
        <div className="text-sm font-medium">
          Gün {d.day} <span className="text-ink-dim text-xs">· {formatDate(d.date)}</span>
        </div>
        {planned ? <Badge>Planlı</Badge> : <Badge tone="brand">Tamamlandı</Badge>}
      </div>
      <div className="mt-1 grid grid-cols-2 gap-2 text-xs text-ink-muted">
        <span>CD34/kg: {formatNumber(d.cd34PerKg, 2)}</span>
        {!isAutologous && <span>CD3/kg: {formatNumber(d.cd3PerKg, 2)}</span>}
        <span>Kümülatif CD34: {formatNumber(d.cumulativeCd34, 2)}</span>
        {!isAutologous && <span>Kümülatif CD3: {formatNumber(d.cumulativeCd3, 2)}</span>}
      </div>
      {!planned && d.sessionId && (
        <div className="mt-1.5 flex items-center gap-1 flex-wrap">
          {hasLabs && (
            <button
              type="button"
              onClick={() => setExpanded((x) => !x)}
              className="text-[11px] text-brand-400 hover:text-brand-500"
            >
              {expanded ? "Detayları gizle" : "PK / ÜRÜN detayları"}
            </button>
          )}
          <span className="ml-auto inline-flex items-center gap-1">
            <button
              type="button"
              onClick={() => recalc.mutate()}
              title="Yeniden hesapla"
              className="rounded-md p-1 text-ink-dim hover:text-ink hover:bg-bg-elevated"
            >
              <Calculator className="size-3.5" />
            </button>
            <button
              type="button"
              onClick={() => setEditOpen(true)}
              title="Düzenle"
              className="rounded-md p-1 text-ink-dim hover:text-ink hover:bg-bg-elevated"
            >
              <Pencil className="size-3.5" />
            </button>
            <button
              type="button"
              onClick={() => setDelOpen(true)}
              title="Sil"
              className="rounded-md p-1 text-ink-dim hover:text-accent-rose hover:bg-rose-500/10"
            >
              <Trash2 className="size-3.5" />
            </button>
          </span>
        </div>
      )}
      {hasLabs && expanded && (
        <div className="mt-2 space-y-2">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
            <LabsBlock
              title="PK · Aferez öncesi"
              items={[
                ["WBC", d.wbcPre],
                ["HGB", d.hgb],
                ["HCT", d.hct],
                ["PLT", d.plt],
                ["%CD45 (kan)", d.preCd45Percent],
                ["%CD34 (kan)", d.preCd34Percent],
                ["MHS (kan)", d.preMhs],
              ]}
            />
            <LabsBlock
              title="ÜRÜN · Post-procedure"
              items={
                isAutologous
                  ? [
                      ["Hacim (ml)", d.volumeMl],
                      ["WBC", d.wbc],
                      ["%CD45", d.cd45Percent],
                      ["%CD34", d.cd34Percent],
                      ["Mutlak hücre (kontrol)", d.absoluteCellCount],
                      ["MHS", d.mhs],
                    ]
                  : [
                      ["Hacim (ml)", d.volumeMl],
                      ["WBC", d.wbc],
                      ["%CD45", d.cd45Percent],
                      ["%CD34", d.cd34Percent],
                      ["Mutlak hücre (kontrol)", d.absoluteCellCount],
                      ["%CD3", d.cd3Percent],
                      ["%Lenfosit", d.lymphocytePercent],
                      ["MHS", d.mhs],
                    ]
              }
            />
          </div>

          {d.wbcPost != null || d.hgbPost != null || d.hctPost != null || d.pltPost != null ? (
            <LabsBlock
              title="PK · İşlem sonrası"
              items={[
                ["WBC", d.wbcPost],
                ["HGB", d.hgbPost],
                ["HCT", d.hctPost],
                ["PLT", d.pltPost],
              ]}
            />
          ) : null}

          {d.sessionId && patientWeight && patientWeight > 0 && (
            <ProductVsBagsPanel
              sessionId={d.sessionId}
              patientWeight={patientWeight}
              productVolumeMl={d.volumeMl ?? 0}
              absoluteCellCount={d.absoluteCellCount ?? 0}
              productCd34PerKg={d.cd34PerKg}
              productCd3PerKg={d.cd3PerKg}
              isAutologous={isAutologous}
            />
          )}
        </div>
      )}

      <Modal
        open={editOpen}
        onClose={() => setEditOpen(false)}
        title={`Gün ${d.day} seansını düzenle`}
        size="xl"
      >
        {d.sessionId && (
          <EditSessionByIdForm
            sessionId={d.sessionId}
            patientId={patientId}
            weightKg={patientWeight}
            isAutologous={isAutologous}
            onCancel={() => setEditOpen(false)}
            onSaved={async () => {
              setEditOpen(false);
              await onChanged();
            }}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={delOpen}
        onClose={() => setDelOpen(false)}
        onConfirm={() => removeMut.mutate()}
        loading={removeMut.isPending}
        description={`Gün ${d.day} (${formatDate(d.date)}) aferez seansı silinecek. Bağlı torbalar varsa silinemeyebilir.`}
      />
    </li>
  );
}

function EditSessionByIdForm({
  sessionId,
  patientId,
  weightKg,
  isAutologous,
  onCancel,
  onSaved,
}: {
  sessionId: string;
  patientId: string;
  weightKg?: number;
  isAutologous?: boolean;
  onCancel: () => void;
  onSaved: () => void;
}) {
  const sess = useQuery({ queryKey: ["session", sessionId], queryFn: () => Sessions.byId(sessionId) });
  if (sess.isLoading) return <div className="skeleton h-60" />;
  if (!sess.data) return <p className="text-sm text-ink-muted">Seans bulunamadı.</p>;

  const s = sess.data;
  return (
    <SessionInlineForm
      initial={s}
      patientId={patientId}
      weightKg={weightKg}
      isAutologous={isAutologous}
      onCancel={onCancel}
      onSaved={onSaved}
    />
  );
}

function SessionInlineForm({
  initial,
  patientId,
  weightKg,
  isAutologous,
  onCancel,
  onSaved,
}: {
  initial: {
    id: string;
    day: number;
    date: string;
    wbcPre?: number | null;
    hgb?: number | null;
    hct?: number | null;
    plt?: number | null;
    preCd45Percent?: number | null;
    preCd34Percent?: number | null;
    preMhs?: number | null;
    wbcPost?: number | null;
    hgbPost?: number | null;
    hctPost?: number | null;
    pltPost?: number | null;
    volumeMl: number;
    wbc: number;
    cd34Percent: number;
    cd45Percent: number;
    cd3Percent: number;
    lymphocytePercent?: number | null;
    mhs?: number | null;
    absoluteCellCount?: number | null;
    cd34PerKg: number;
    cd3PerKg: number;
  };
  patientId: string;
  weightKg?: number;
  isAutologous?: boolean;
  onCancel: () => void;
  onSaved: () => void;
}) {
  const qc = useQueryClient();
  const clinical = useQuery({ queryKey: ["clinical-settings"], queryFn: () => ClinicalSettingsApi.get(), staleTime: 0 });
  const sessionDivisor = clinical.data?.sessionCd34Cd3Divisor ?? 10000;
  const { register, handleSubmit, watch, reset, setValue, formState: { isSubmitting } } = useForm<SessionForm>({
    defaultValues: {
      day: initial.day,
      date: initial.date.slice(0, 10),
      wbcPre: initial.wbcPre ?? undefined,
      hgb: initial.hgb ?? undefined,
      hct: initial.hct ?? undefined,
      plt: initial.plt ?? undefined,
      preCd45Percent: initial.preCd45Percent ?? undefined,
      preCd34Percent: initial.preCd34Percent ?? undefined,
      preMhs: initial.preMhs ?? undefined,
      wbcPost: initial.wbcPost ?? undefined,
      hgbPost: initial.hgbPost ?? undefined,
      hctPost: initial.hctPost ?? undefined,
      pltPost: initial.pltPost ?? undefined,
      volumeMl: initial.volumeMl,
      wbc: initial.wbc,
      cd34Percent: initial.cd34Percent,
      cd45Percent: initial.cd45Percent,
      cd3Percent: initial.cd3Percent,
      lymphocytePercent: initial.lymphocytePercent ?? undefined,
      mhs: initial.mhs ?? undefined,
    },
  });

  // Query cache'inden yeni veri gelirse form alanlarını güncel değerlerle senkronla.
  useEffect(() => {
    reset({
      day: initial.day,
      date: initial.date.slice(0, 10),
      wbcPre: initial.wbcPre ?? undefined,
      hgb: initial.hgb ?? undefined,
      hct: initial.hct ?? undefined,
      plt: initial.plt ?? undefined,
      preCd45Percent: initial.preCd45Percent ?? undefined,
      preCd34Percent: initial.preCd34Percent ?? undefined,
      preMhs: initial.preMhs ?? undefined,
      wbcPost: initial.wbcPost ?? undefined,
      hgbPost: initial.hgbPost ?? undefined,
      hctPost: initial.hctPost ?? undefined,
      pltPost: initial.pltPost ?? undefined,
      volumeMl: initial.volumeMl,
      wbc: initial.wbc,
      cd34Percent: initial.cd34Percent,
      cd45Percent: initial.cd45Percent,
      cd3Percent: initial.cd3Percent,
      lymphocytePercent: initial.lymphocytePercent ?? undefined,
      mhs: initial.mhs ?? undefined,
    });
  }, [initial, reset]);

  const values = watch();
  const preview = usePreviewCalculation(values, weightKg ?? 0, sessionDivisor);
  useEffect(() => {
    setValue("preMhs", preview?.absoluteCellCount, { shouldDirty: false, shouldValidate: false });
  }, [preview?.absoluteCellCount, setValue]);

  const onSubmit = async (d: SessionForm) => {
    const updated = await Sessions.update({
      id: initial.id,
      patientId,
      day: Number(d.day),
      date: new Date(d.date).toISOString(),
      wbcPre: numOrNull(d.wbcPre),
      hgb: numOrNull(d.hgb),
      hct: numOrNull(d.hct),
      plt: numOrNull(d.plt),
      preCd45Percent: numOrNull(d.preCd45Percent),
      preCd34Percent: numOrNull(d.preCd34Percent),
      preMhs: preview?.absoluteCellCount ?? numOrNull(d.preMhs),
      wbcPost: numOrNull(d.wbcPost),
      hgbPost: numOrNull(d.hgbPost),
      hctPost: numOrNull(d.hctPost),
      pltPost: numOrNull(d.pltPost),
      volumeMl: Number(d.volumeMl),
      wbc: Number(d.wbc),
      cd34Percent: Number(d.cd34Percent),
      cd45Percent: Number(d.cd45Percent),
      cd3Percent: isAutologous ? 0 : Number(d.cd3Percent),
      lymphocytePercent: isAutologous ? undefined : numOrNull(d.lymphocytePercent),
      mhs: numOrNull(d.mhs),
      absoluteCellCount: initial.absoluteCellCount ?? 0,
      cd34PerKg: initial.cd34PerKg,
      cd3PerKg: isAutologous ? 0 : initial.cd3PerKg,
    });
    await Sessions.calculate(updated.id);
    qc.setQueryData(["session", initial.id], updated);
    qc.invalidateQueries({ queryKey: ["session", initial.id] });
    qc.invalidateQueries({ queryKey: ["apheresis-plan", patientId] });
    qc.invalidateQueries({ queryKey: ["patient", patientId] });
    qc.invalidateQueries({ queryKey: ["patients"] });
    qc.invalidateQueries({ queryKey: ["sessions"] });
    qc.invalidateQueries({ queryKey: ["dashboard"] });
    toast.success("Seans güncellendi");
    onSaved();
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5 pb-2">
      <VariantBanner isAutologous={isAutologous} />
      <section>
        <SectionTitle title="1 · Gün" />
        <div className="grid grid-cols-2 gap-3">
          <Input label="Gün" type="number" {...register("day", { required: true, min: 1 })} />
          <Input label="Tarih" type="date" {...register("date", { required: true })} />
        </div>
      </section>
      <section>
        <SectionTitle title="2 · PK — Aferez öncesi" />
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <Input label="WBC pre" type="number" step="0.01" {...register("wbcPre")} />
          <Input label="HGB" type="number" step="0.01" {...register("hgb")} />
          <Input label="HCT" type="number" step="0.01" {...register("hct")} />
          <Input label="PLT" type="number" step="0.01" {...register("plt")} />
          <Input label="%CD45 (pre)" type="number" step="0.01" {...register("preCd45Percent")} />
          <Input label="%CD34 (pre)" type="number" step="0.01" {...register("preCd34Percent")} />
          <Input label="MHS (pre)" type="number" step="0.01" {...register("preMhs")} readOnly />
        </div>
      </section>
      <section>
        <SectionTitle
          title="3 · ÜRÜN — Aferez sonrası"
          hint={isAutologous ? "Otolog · CD3/Lenfosit takibi uygulanmaz" : undefined}
        />
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <Input label="Hacim (ml)" type="number" step="0.1" {...register("volumeMl", { required: true, min: 0.1 })} />
          <Input label="WBC" type="number" step="0.01" {...register("wbc", { required: true })} />
          <Input label="%CD45" type="number" step="0.01" {...register("cd45Percent", { required: true })} />
          <Input label="%CD34" type="number" step="0.01" {...register("cd34Percent", { required: true })} />
          {!isAutologous && (
            <>
              <Input label="%CD3" type="number" step="0.01" {...register("cd3Percent", { required: true })} />
              <Input label="%Lenfosit" type="number" step="0.01" {...register("lymphocytePercent")} />
            </>
          )}
          <Input label="MHS" type="number" step="0.01" {...register("mhs")} />
        </div>
      </section>
      <div className="rounded-xl border border-brand-500/30 bg-brand-500/5 p-3">
        <div className="text-[11px] uppercase tracking-wide text-ink-dim mb-1">
          Önizleme · kilo {weightKg ?? "—"} kg
        </div>
        <div className="grid grid-cols-2 gap-2 text-sm">
          <div>
            CD34/kg{" "}
            <span className="font-semibold text-brand-400">
              {preview ? formatNumber(preview.cd34, 2) : "—"}
            </span>
          </div>
          {!isAutologous && (
            <div>
              CD3/kg{" "}
              <span className="font-semibold text-accent-mint">
                {preview ? formatNumber(preview.cd3, 2) : "—"}
              </span>
            </div>
          )}
          <div>
            Mutlak hücre{" "}
            <span className="font-semibold text-ink">
              {preview ? formatNumber(preview.absoluteCellCount, 2) : "—"}
            </span>
          </div>
        </div>
      </div>
      <div className="sticky bottom-0 -mx-6 -mb-5 px-6 py-3 bg-bg-card/95 backdrop-blur border-t border-line/60 flex justify-end gap-2">
        <Button variant="soft" type="button" onClick={onCancel}>İptal</Button>
        <Button type="submit" loading={isSubmitting}>Kaydet</Button>
      </div>
    </form>
  );
}

function ProductVsBagsPanel({
  sessionId,
  patientWeight,
  productVolumeMl,
  absoluteCellCount,
  productCd34PerKg,
  productCd3PerKg,
  isAutologous,
}: {
  sessionId: string;
  patientWeight: number;
  productVolumeMl: number;
  absoluteCellCount: number;
  productCd34PerKg: number;
  productCd3PerKg: number;
  isAutologous?: boolean;
}) {
  const bags = useQuery({
    queryKey: ["bags-by-session", sessionId],
    queryFn: () => Bags.bySession(sessionId, 0, 100),
  });

  if (bags.isLoading) return <div className="skeleton h-16 rounded-xl" />;
  const items = bags.data?.items ?? [];
  if (!items.length) return null;

  const totalVolumeMl = items.reduce((s, b) => s + (b.volumeMl || 0), 0);
  const totalBagsCd34PerKg = items.reduce((s, b) => s + (b.cd34PerKg || 0), 0);
  const totalBagsCd3PerKg = items.reduce((s, b) => s + (b.cd3PerKg || 0), 0);

  const deltaCd34 = totalBagsCd34PerKg - productCd34PerKg;
  const deltaCd3 = totalBagsCd3PerKg - productCd3PerKg;
  const productCd34FromAbsolute =
    patientWeight > 0 ? (productVolumeMl * absoluteCellCount) / patientWeight : 0;
  const deltaAbsoluteControl = totalBagsCd34PerKg - productCd34FromAbsolute;

  const tol = 0.05; // %5 tolerans
  const within = (actual: number, target: number) => {
    if (target === 0) return Math.abs(actual) < 0.001;
    return Math.abs(actual - target) / Math.abs(target) <= tol;
  };
  const cd34Match = within(totalBagsCd34PerKg, productCd34PerKg);
  const cd3Match = within(totalBagsCd3PerKg, productCd3PerKg);
  const absoluteControlMatch = within(totalBagsCd34PerKg, productCd34FromAbsolute);

  return (
    <div className="rounded-xl border border-line/60 bg-bg-elevated/40 p-3">
      <div className="flex items-center gap-2 mb-2">
        <Scissors className="size-3.5 text-ink-muted" />
        <div className="text-[10px] uppercase tracking-wider text-ink-dim">
          Ürün vs Torbalar toplamı — {items.length} torba · {formatNumber(totalVolumeMl, 1)} ml
        </div>
      </div>
      <div className={`grid ${isAutologous ? "grid-cols-3" : "grid-cols-5"} gap-2`}>
        <CompareCell
          label="Ürün CD34/kg"
          value={formatNumber(productCd34PerKg, 2)}
          tone="muted"
        />
        <CompareCell
          label="Torbalar Σ CD34/kg"
          value={formatNumber(totalBagsCd34PerKg, 2)}
          hint={`Δ ${deltaCd34 >= 0 ? "+" : ""}${formatNumber(deltaCd34, 2)}`}
          tone={cd34Match ? "ok" : "warn"}
        />
        <CompareCell
          label="Kontrol: Hacim × Mutlak / kg"
          value={formatNumber(productCd34FromAbsolute, 2)}
          hint={`Δ ${deltaAbsoluteControl >= 0 ? "+" : ""}${formatNumber(deltaAbsoluteControl, 2)}`}
          tone={absoluteControlMatch ? "ok" : "warn"}
        />
        {!isAutologous && (
          <>
            <CompareCell
              label="Ürün CD3/kg"
              value={formatNumber(productCd3PerKg, 2)}
              tone="muted"
            />
            <CompareCell
              label="Torbalar Σ CD3/kg"
              value={formatNumber(totalBagsCd3PerKg, 2)}
              hint={`Δ ${deltaCd3 >= 0 ? "+" : ""}${formatNumber(deltaCd3, 2)}`}
              tone={cd3Match ? "ok" : "warn"}
            />
          </>
        )}
      </div>
      {(!cd34Match || !absoluteControlMatch || (!isAutologous && !cd3Match)) && (
        <div className="mt-2 flex items-center gap-1.5 text-[11px] text-accent-amber">
          <TriangleAlert className="size-3" />
          Torbaların toplamı ürün değeriyle %5+ fark veriyor — bölme notunu veya torba yüzdelerini
          gözden geçirin.
        </div>
      )}
    </div>
  );
}

function CompareCell({
  label,
  value,
  hint,
  tone,
}: {
  label: string;
  value: string;
  hint?: string;
  tone: "ok" | "warn" | "muted";
}) {
  const cls =
    tone === "ok"
      ? "border-emerald-500/30 bg-emerald-500/10"
      : tone === "warn"
        ? "border-amber-500/30 bg-amber-500/10"
        : "border-line/60 bg-bg-elevated/40";
  const accent =
    tone === "ok" ? "text-accent-mint" : tone === "warn" ? "text-accent-amber" : "text-ink";
  return (
    <div className={`rounded-lg border px-2.5 py-2 ${cls}`}>
      <div className="text-[10px] uppercase tracking-wide text-ink-dim">{label}</div>
      <div className={`text-sm font-semibold mt-0.5 ${accent}`}>{value}</div>
      {hint && <div className="text-[10px] text-ink-dim mt-0.5">{hint}</div>}
    </div>
  );
}

function LabsBlock({
  title,
  items,
}: {
  title: string;
  items: Array<[string, number | null | undefined]>;
}) {
  return (
    <div className="rounded-xl border border-line/60 bg-bg-elevated/40 p-3">
      <div className="text-[10px] uppercase tracking-wider text-ink-dim mb-1.5">
        {title}
      </div>
      <dl className="grid grid-cols-2 gap-x-3 gap-y-1 text-xs">
        {items.map(([k, v]) => (
          <div key={k} className="flex items-center justify-between gap-2">
            <dt className="text-ink-dim">{k}</dt>
            <dd className="text-ink font-medium">
              {v === null || v === undefined ? "—" : formatNumber(v, 2)}
            </dd>
          </div>
        ))}
      </dl>
    </div>
  );
}

function CumulativeChart({ plan }: { plan?: ApheresisPlanResponse }) {
  if (!plan) return <div className="h-72 skeleton" />;
  const isAutologous = plan.isAutologous;
  const data = (plan.forecastPlan.length ? plan.forecastPlan : plan.completedSessions).map((d) => ({
    name: `G${d.day}`,
    cd34: d.cumulativeCd34,
    cd3: d.cumulativeCd3,
    planned: d.isPlanned ? d.cumulativeCd34 : null,
  }));
  if (!data.length)
    return (
      <div className="grid h-72 place-items-center">
        <p className="text-sm text-ink-muted">Henüz veri yok.</p>
      </div>
    );
  return (
    <div className="h-72">
      <ResponsiveContainer>
        <AreaChart data={data} margin={{ top: 10, right: 16, left: -10, bottom: 0 }}>
          <defs>
            <linearGradient id="g34" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="#6366f1" stopOpacity={0.7} />
              <stop offset="100%" stopColor="#6366f1" stopOpacity={0} />
            </linearGradient>
            <linearGradient id="g3" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="#5eead4" stopOpacity={0.7} />
              <stop offset="100%" stopColor="#5eead4" stopOpacity={0} />
            </linearGradient>
          </defs>
          <CartesianGrid stroke="#1f2138" vertical={false} />
          <XAxis dataKey="name" stroke="#6b7191" fontSize={12} tickLine={false} axisLine={false} />
          <YAxis stroke="#6b7191" fontSize={12} tickLine={false} axisLine={false} />
          <Tooltip
            contentStyle={{
              background: "#13141d",
              border: "1px solid #23253a",
              borderRadius: 12,
              color: "#e6e8f3",
            }}
          />
          <ReferenceLine
            y={plan.targetCd34PerKg}
            stroke="#fbbf24"
            strokeDasharray="4 4"
            label={{ value: `Hedef ${plan.targetCd34PerKg}`, fill: "#fbbf24", fontSize: 11, position: "insideTopRight" }}
          />
          <ReferenceLine
            y={plan.idealCd34PerKg}
            stroke="#5eead4"
            strokeDasharray="4 4"
            label={{ value: `İdeal ${plan.idealCd34PerKg}`, fill: "#5eead4", fontSize: 11, position: "insideTopRight" }}
          />
          <Area type="monotone" dataKey="cd34" stroke="#6366f1" fill="url(#g34)" strokeWidth={2.5} />
          {!isAutologous && (
            <Area type="monotone" dataKey="cd3" stroke="#5eead4" fill="url(#g3)" strokeWidth={2} />
          )}
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
}

interface SessionForm {
  day: number;
  date: string;

  // PK — aferez öncesi kan paneli
  wbcPre?: number;
  hgb?: number;
  hct?: number;
  plt?: number;

  // PK — aferez öncesi flow-cytometry (hastanın kanından ölçülen)
  preCd45Percent?: number;
  preCd34Percent?: number;
  preMhs?: number;

  // İşlem sonrası hemogram (opsiyonel)
  wbcPost?: number;
  hgbPost?: number;
  hctPost?: number;
  pltPost?: number;

  // ÜRÜN — aferez sonrası ölçümler
  volumeMl: number;
  wbc: number;
  cd34Percent: number;
  cd45Percent: number;
  cd3Percent: number;
  lymphocytePercent?: number;
  mhs?: number;
  absoluteCellCount?: number;
}

function CreateSessionForm({
  patientId,
  weightKg,
  defaultDay,
  isAutologous,
  onCancel,
  onCreated,
}: {
  patientId: string;
  weightKg?: number;
  defaultDay: number;
  isAutologous?: boolean;
  onCancel: () => void;
  onCreated: () => void;
}) {
  const clinical = useQuery({ queryKey: ["clinical-settings"], queryFn: () => ClinicalSettingsApi.get(), staleTime: 0 });
  const sessionDivisor = clinical.data?.sessionCd34Cd3Divisor ?? 10000;
  const { register, handleSubmit, watch, setValue, formState: { errors } } = useForm<SessionForm>({
    defaultValues: {
      day: defaultDay,
      date: new Date().toISOString().slice(0, 10),
      volumeMl: undefined,
      wbc: undefined,
      cd34Percent: undefined,
      cd45Percent: undefined,
      cd3Percent: undefined,
    },
  });

  const values = watch();
  const preview = usePreviewCalculation(values, weightKg ?? 0, sessionDivisor);
  useEffect(() => {
    setValue("preMhs", preview?.absoluteCellCount, { shouldDirty: false, shouldValidate: false });
  }, [preview?.absoluteCellCount, setValue]);

  const create = useMutation({
    mutationFn: (data: SessionForm) =>
      Sessions.create({
        patientId,
        day: Number(data.day),
        date: new Date(data.date).toISOString(),

        wbcPre: numOrNull(data.wbcPre),
        hgb: numOrNull(data.hgb),
        hct: numOrNull(data.hct),
        plt: numOrNull(data.plt),
        preCd45Percent: numOrNull(data.preCd45Percent),
        preCd34Percent: numOrNull(data.preCd34Percent),
        preMhs: preview?.absoluteCellCount ?? numOrNull(data.preMhs),
        wbcPost: numOrNull(data.wbcPost),
        hgbPost: numOrNull(data.hgbPost),
        hctPost: numOrNull(data.hctPost),
        pltPost: numOrNull(data.pltPost),

        volumeMl: Number(data.volumeMl),
        wbc: Number(data.wbc),
        cd34Percent: Number(data.cd34Percent),
        cd45Percent: Number(data.cd45Percent),
        cd3Percent: isAutologous ? 0 : Number(data.cd3Percent),
        lymphocytePercent: isAutologous ? undefined : numOrNull(data.lymphocytePercent),
        mhs: numOrNull(data.mhs),
        absoluteCellCount: preview?.absoluteCellCount ?? 0,

        cd34PerKg: 0,
        cd3PerKg: 0,
      }),
    onSuccess: onCreated,
  });

  return (
    <form
      onSubmit={handleSubmit((d) => create.mutate(d))}
      className="space-y-5 pb-2"
    >
      <VariantBanner isAutologous={isAutologous} />
      <section>
        <SectionTitle
          title="1 · Gün bilgisi"
          hint="Hangi aferez gününe ait ve hangi tarihte yapıldı"
        />
        <div className="grid grid-cols-2 gap-3">
          <Input
            label="Gün"
            type="number"
            {...register("day", { required: true, min: 1 })}
            error={errors.day?.message as any}
          />
          <Input label="Tarih" type="date" {...register("date", { required: true })} />
        </div>
      </section>

      <section>
        <SectionTitle
          title="2 · PK — Aferez öncesi (pre-procedure)"
          hint="Hastanın aferez öncesi kan paneli ve hücre yüzdeleri (formdaki PK bloğu)."
        />
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <Input label="WBC (pre)" type="number" step="0.01" {...register("wbcPre")} />
          <Input label="HGB" type="number" step="0.01" {...register("hgb")} />
          <Input label="HCT" type="number" step="0.01" {...register("hct")} />
          <Input label="PLT" type="number" step="0.01" {...register("plt")} />
          <Input label="%CD45 (pre)" type="number" step="0.01" {...register("preCd45Percent")} />
          <Input label="%CD34 (pre)" type="number" step="0.01" {...register("preCd34Percent")} />
          <Input label="MHS (pre)" type="number" step="0.01" {...register("preMhs")} readOnly />
        </div>
      </section>

      <section>
        <SectionTitle
          title="3 · ÜRÜN — Aferez sonrası (post-procedure)"
          hint={
            isAutologous
              ? "Otolog · sadece CD34 takibi yapılır, CD3/Lenfosit uygulanmaz"
              : "Toplanan ürün hacmi ve hücre yüzdeleri"
          }
        />
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <Input
            label="Hacim (ml)"
            type="number"
            step="0.1"
            {...register("volumeMl", { required: true, min: 0.1 })}
          />
          <Input
            label="WBC (10^9/L)"
            type="number"
            step="0.01"
            {...register("wbc", { required: true, min: 0.01 })}
          />
          <Input label="%CD45" type="number" step="0.01" {...register("cd45Percent", { required: true })} />
          <Input label="%CD34" type="number" step="0.01" {...register("cd34Percent", { required: true })} />
          {!isAutologous && (
            <>
              <Input label="%CD3" type="number" step="0.01" {...register("cd3Percent", { required: true })} />
              <Input label="%Lenfosit" type="number" step="0.01" {...register("lymphocytePercent")} />
            </>
          )}
          <Input label="MHS" type="number" step="0.01" {...register("mhs")} />
        </div>
      </section>

      <div className="rounded-xl border border-brand-500/30 bg-brand-500/5 p-3">
        <div className="text-[11px] uppercase tracking-wide text-ink-dim mb-1">
          Canlı hesaplama önizlemesi · kilo {weightKg ?? "—"} kg
        </div>
        <div className="grid grid-cols-2 gap-2 text-sm">
          <div>
            CD34/kg{" "}
            <span className="font-semibold text-brand-400">
              {preview ? formatNumber(preview.cd34, 2) : "—"}
            </span>
          </div>
          {!isAutologous && (
            <div>
              CD3/kg{" "}
              <span className="font-semibold text-accent-mint">
                {preview ? formatNumber(preview.cd3, 2) : "—"}
              </span>
            </div>
          )}
          <div>
            Mutlak hücre{" "}
            <span className="font-semibold text-ink">
              {preview ? formatNumber(preview.absoluteCellCount, 2) : "—"}
            </span>
          </div>
        </div>
        <p className="mt-1 text-[11px] text-ink-dim">
          Hesap: (Hacim × WBC × %CD45 × %CD34) / {formatNumber(sessionDivisor, 0)} / kilo · sunucu ile ayni bolen.
        </p>
      </div>

      <div className="sticky bottom-0 -mx-6 -mb-5 px-6 py-3 bg-bg-card/95 backdrop-blur border-t border-line/60 flex justify-end gap-2">
        <Button variant="soft" type="button" onClick={onCancel}>İptal</Button>
        <Button type="submit" loading={create.isPending} icon={<Calculator className="size-4" />}>
          Kaydet ve hesapla
        </Button>
      </div>
    </form>
  );
}

function SectionTitle({ title, hint }: { title: string; hint?: string }) {
  return (
    <div className="mb-2.5 flex items-baseline justify-between gap-2 border-b border-line/60 pb-1.5">
      <h4 className="text-[11px] uppercase tracking-wider text-ink-muted font-semibold">{title}</h4>
      {hint && <p className="text-[11px] text-ink-dim text-right">{hint}</p>}
    </div>
  );
}

function VariantBanner({ isAutologous }: { isAutologous?: boolean }) {
  const title = isAutologous ? "OTOLOG aferez" : "ALLOJENİK aferez";
  const subtitle = isAutologous
    ? "Hasta kendi hücreleri · CD34 yeterliliği takip edilir · Maks 4 gün"
    : "Donör hücreleri · CD34 + CD3/Lenfosit takibi · Maks 2 gün";
  const tone = isAutologous
    ? "from-sky-500/15 to-sky-500/5 border-sky-500/30 text-accent-sky"
    : "from-brand-500/15 to-emerald-500/5 border-brand-500/30 text-brand-400";
  return (
    <div
      className={`rounded-xl border bg-gradient-to-br ${tone} px-3 py-2 flex items-center justify-between gap-3`}
    >
      <div className="flex items-center gap-2">
        <Sparkles className="size-4 shrink-0" />
        <div>
          <div className="text-xs font-semibold tracking-wide uppercase">{title}</div>
          <div className="text-[11px] text-ink-dim mt-0.5">{subtitle}</div>
        </div>
      </div>
      <div className="text-[10px] uppercase tracking-wider text-ink-dim hidden sm:block">
        form sırası: PK · ÜRÜN
      </div>
    </div>
  );
}

function numOrNull(v: unknown): number | undefined {
  if (v === "" || v === null || v === undefined) return undefined;
  const n = Number(v);
  return Number.isFinite(n) ? n : undefined;
}

function usePreviewCalculation(v: Partial<SessionForm>, weight: number, divisor = 10000) {
  if (!v.wbc) return null;
  const absoluteCellCount = calculateAbsoluteCellCount(
    Number(v.wbc),
    Number(v.cd45Percent ?? 0),
    Number(v.cd34Percent ?? 0),
  );
  if (!weight || !v.volumeMl) return { cd34: 0, cd3: 0, absoluteCellCount };
  const result = calculateCellDose(
    {
      volumeMl: Number(v.volumeMl),
      wbc: Number(v.wbc),
      cd45Percent: Number(v.cd45Percent ?? 0),
      cd34Percent: Number(v.cd34Percent ?? 0),
      cd3Percent: Number(v.cd3Percent ?? 0),
    },
    weight,
    divisor,
  );
  return { cd34: result.cd34PerKg, cd3: result.cd3PerKg, absoluteCellCount };
}

function SplitForm({
  plan,
  onCancel,
  onDone,
}: {
  plan?: ApheresisPlanResponse;
  onCancel: () => void;
  onDone: () => void;
}) {
  const [bagCount, setBagCount] = useState(4);
  const [autoPlace, setAutoPlace] = useState(true);

  const lastSession =
    plan?.completedSessions?.[plan.completedSessions.length - 1];
  const isAutologous = !!plan?.isAutologous;

  const split = useMutation({
    mutationFn: () =>
      Bags.split({
        sessionId: lastSession!.sessionId!,
        bagCount,
        autoPlaceCryo: autoPlace,
        requireCumulativeSufficient: true,
      }),
    onSuccess: (r) => {
      toast.success(
        `${r.bagCount} torba oluşturuldu, Cryo torbası ${
          r.cryoBagCellId ? "hücreye yerleştirildi" : "frozen olarak hazır"
        }.`,
      );
      onDone();
    },
  });

  if (!lastSession?.sessionId) {
    return (
      <p className="text-sm text-ink-muted">
        Bölünecek tamamlanmış aferez seansı bulunamadı.
      </p>
    );
  }

  const perBag = (v: number) => v / bagCount;

  return (
    <div className="space-y-5">
      <div className="rounded-xl border border-line/60 bg-bg-elevated/50 p-4">
        <p className="text-xs uppercase text-ink-dim tracking-wide">Bölünecek seans</p>
        <p className="text-sm mt-1">
          Gün {lastSession.day} · {formatDate(lastSession.date)} · CD34/kg{" "}
          {formatNumber(lastSession.cd34PerKg, 2)}
        </p>
      </div>

      <div>
        <div className="label">Torba sayısı</div>
        <div className="flex gap-2">
          {[2, 3, 4, 5, 6].map((n) => (
            <button
              key={n}
              type="button"
              onClick={() => setBagCount(n)}
              className={`btn-soft ${bagCount === n ? "!border-brand-500/60 !text-brand-400 !bg-brand-500/10" : ""}`}
            >
              {n}
            </button>
          ))}
        </div>
        <p className="mt-2 text-[11px] text-ink-dim">
          Her torba ≈ {formatNumber(perBag(lastSession.cd34PerKg), 2)} CD34/kg
          {!isAutologous && (
            <>
              {" "}ve {formatNumber(perBag(lastSession.cd3PerKg), 2)} CD3/kg
            </>
          )}
          .
        </p>
      </div>

      <label className="flex items-start gap-3 rounded-xl border border-line/60 bg-bg-elevated/40 p-3 cursor-pointer">
        <input
          type="checkbox"
          checked={autoPlace}
          onChange={(e) => setAutoPlace(e.target.checked)}
          className="mt-1 accent-brand-500"
        />
        <div>
          <div className="text-sm font-medium">Cryo torbasını otomatik yerleştir</div>
          <div className="text-xs text-ink-muted">
            Sistem ilk boş slotu bulur ve 1. torbayı oraya store eder. Diğer 3 torba "Reserved" durumda kalır.
          </div>
        </div>
      </label>

      <div className="flex items-center justify-end gap-2">
        <Button variant="soft" type="button" onClick={onCancel}>İptal</Button>
        <Button onClick={() => split.mutate()} loading={split.isPending} icon={<Scissors className="size-4" />}>
          Böl ve cryo'ya yerleştir
        </Button>
      </div>
    </div>
  );
}

/* ------------------------------------------------------------------ */
/*                    Custom split (kullanıcı tanımlı)                */
/* ------------------------------------------------------------------ */

const PURPOSE_OPTIONS: { value: number; label: string }[] = [
  { value: 0, label: "Cryo (dondurma)" },
];

interface CustomBagRow {
  id: string;
  volumeMl: string;
  wbc: string;
  cd45Percent: string;
  cd34Percent: string;
  cd3Percent: string;
  purpose: number;
  compositionNote: string;
}

function makeRow(_session?: { wbc?: number | null; cd45Percent?: number | null; cd34Percent?: number | null; cd3Percent?: number | null }, purpose = 0): CustomBagRow {
  // Her satır boş başlar; her torbanın WBC/CD3 vb. değerleri normalde farklıdır.
  // Kullanıcı isterse manuel girer; session değerlerinden otomatik kopyalanmaz.
  return {
    id: crypto.randomUUID(),
    volumeMl: "",
    wbc: "",
    cd45Percent: "",
    cd34Percent: "",
    cd3Percent: "",
    purpose,
    compositionNote: "",
  };
}

function CustomSplitForm({
  plan,
  onCancel,
  onDone,
}: {
  plan?: ApheresisPlanResponse;
  onCancel: () => void;
  onDone: () => void;
}) {
  const completed = plan?.completedSessions ?? [];
  const defaultSessionId = completed.length
    ? completed[completed.length - 1]!.sessionId ?? ""
    : "";

  const [sessionId, setSessionId] = useState<string>(defaultSessionId);
  const [validateTotal, setValidateTotal] = useState(true);

  const selectedSession = completed.find((s) => s.sessionId === sessionId);
  const weight = plan?.weightKg ?? 0;

  const isAutologous = !!plan?.isAutologous;

  const [rows, setRows] = useState<CustomBagRow[]>(() => [
    makeRow(undefined, 0),
    makeRow(undefined, 0),
  ]);

  // Removed per-row freeze slot selection — handled later via Cryo grid or server-side auto-placement.

  const clinicalQ = useQuery({
    queryKey: ["clinical-settings"],
    queryFn: () => ClinicalSettingsApi.get(),
    staleTime: 0,
  });
  const divisor = clinicalQ.data?.sessionCd34Cd3Divisor ?? 10000;

  const updateRow = (id: string, patch: Partial<CustomBagRow>) =>
    setRows((prev) => prev.map((r) => (r.id === id ? { ...r, ...patch } : r)));

  const removeRow = (id: string) => setRows((prev) => prev.filter((r) => r.id !== id));

  const addRow = () =>
    setRows((prev) => [
      ...prev,
      makeRow(undefined, 0),
    ]);

  // Canlı CD34/kg önizleme (frontend formülü).
  const computed = rows.map((r) => {
    if (!r.volumeMl || !r.wbc || !weight) return { cd34: 0, cd3: 0 };
    const result = calculateCellDose(
      {
        volumeMl: Number(r.volumeMl),
        wbc: Number(r.wbc),
        cd45Percent: Number(r.cd45Percent || 0),
        cd34Percent: Number(r.cd34Percent || 0),
        cd3Percent: Number(r.cd3Percent || 0),
      },
      weight,
      divisor,
    );
    return { cd34: result.cd34PerKg, cd3: result.cd3PerKg };
  });

  const totalVolume = rows.reduce((a, r) => a + (Number(r.volumeMl) || 0), 0);
  const sessionVolume = Number(selectedSession?.volumeMl ?? 0);

  const submit = useMutation({
    mutationFn: async () => {
      if (!sessionId) throw new Error("Önce bir aferez seansı seçin.");
      if (!rows.length) throw new Error("En az bir torba ekleyin.");
      const bags = rows.map((r, i) => {
        const volumeMl = Number(r.volumeMl);
        if (!volumeMl || volumeMl <= 0)
          throw new Error(`${i + 1}. torba için hacim girmelisiniz.`);

        const num = (s: string) => (s === "" ? undefined : Number(s));
        return {
          volumeMl,
          wbc: num(r.wbc),
          cd45Percent: num(r.cd45Percent),
          cd34Percent: num(r.cd34Percent),
          // Otolog torbalarda CD3 toplanmaz/raporlanmaz.
          cd3Percent: isAutologous ? undefined : num(r.cd3Percent),
          purpose: r.purpose,
          compositionNote: r.compositionNote.trim() || undefined,
        };
      });
      return Bags.customSplit({ sessionId, bags, validateTotalVolume: validateTotal });
    },
    onSuccess: (r) => {
      const frozen = r.bags.filter((b) => b.bagCellId).length;
      toast.success(
        `${r.bagCount} torba oluşturuldu${frozen ? `, ${frozen} torba hücreye dondurularak yerleştirildi` : ""}.`,
      );
      onDone();
    },
    onError: (e: Error) => toast.error(e.message),
  });

  if (!completed.length) {
    return (
      <p className="text-sm text-ink-muted">
        Önce en az bir tamamlanmış aferez seansı kaydedilmelidir.
      </p>
    );
  }

  return (
    <div className="space-y-5">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div>
          <div className="label">Aferez seansı</div>
          <Select value={sessionId} onChange={(e) => setSessionId(e.target.value)}>
            {completed.map((s) => (
              <option key={s.sessionId ?? s.day} value={s.sessionId ?? ""}>
                Gün {s.day} · {formatDate(s.date)} · {formatNumber(s.volumeMl ?? 0, 0)} ml ·
                CD34 {formatNumber(s.cd34PerKg, 2)}/kg
              </option>
            ))}
          </Select>
        </div>
        <div className="rounded-xl border border-line/60 bg-bg-elevated/50 p-3 text-xs text-ink-muted">
          <div>Hasta kilosu: <span className="text-ink">{weight} kg</span></div>
          <div>
            Session ürün hacmi:{" "}
            <span className="text-ink">{formatNumber(sessionVolume, 2)} ml</span> · Girilen toplam:{" "}
            <span className={totalVolume > sessionVolume ? "text-danger-400" : "text-ink"}>
              {formatNumber(totalVolume, 2)} ml
            </span>
          </div>
        </div>
      </div>

      <div className="space-y-3">
        {rows.map((r, idx) => {
          const c = computed[idx]!;
          return (
            <div
              key={r.id}
              className="rounded-xl border border-line/60 bg-bg-elevated/40 p-3 grid grid-cols-12 gap-2 items-end"
            >
              <div className="col-span-12 md:col-span-1 text-xs uppercase text-ink-dim tracking-wide pt-1">
                #{idx + 1}
              </div>
              <div className="col-span-6 md:col-span-2 min-w-0">
                <div className="label">Hacim (ml)</div>
                <Input
                  type="number"
                  step="0.1"
                  inputMode="decimal"
                  className="text-right w-full"
                  value={r.volumeMl}
                  onChange={(e) => updateRow(r.id, { volumeMl: e.target.value })}
                  placeholder="32"
                />
              </div>
              <div className="col-span-6 md:col-span-2 min-w-0">
                <div className="label">WBC (x10³/µL)</div>
                <Input
                  type="number"
                  step="0.1"
                  inputMode="decimal"
                  className="text-right w-full"
                  value={r.wbc}
                  onChange={(e) => updateRow(r.id, { wbc: e.target.value })}
                />
              </div>
              <div className="col-span-4 md:col-span-2 min-w-0">
                <div className="label">CD45%</div>
                <Input
                  type="number"
                  step="0.01"
                  inputMode="decimal"
                  className="text-right w-full"
                  value={r.cd45Percent}
                  onChange={(e) => updateRow(r.id, { cd45Percent: e.target.value })}
                />
              </div>
              <div className="col-span-4 md:col-span-2 min-w-0">
                <div className="label">CD34%</div>
                <Input
                  type="number"
                  step="0.01"
                  inputMode="decimal"
                  className="text-right w-full"
                  value={r.cd34Percent}
                  onChange={(e) => updateRow(r.id, { cd34Percent: e.target.value })}
                />
              </div>
              <div className="col-span-6 md:col-span-2 min-w-0">
                <div className="label">Amaç</div>
                <Select
                  value={r.purpose}
                  onChange={(e) => updateRow(r.id, { purpose: Number(e.target.value) })}
                >
                  {PURPOSE_OPTIONS.map((p) => (
                    <option key={p.value} value={p.value}>
                      {p.label}
                    </option>
                  ))}
                </Select>
              </div>
              {/* per-row freeze → cell removed as requested */}
              <div className="col-span-1 flex justify-end">
                <button
                  type="button"
                  onClick={() => removeRow(r.id)}
                  disabled={rows.length <= 1}
                  className="p-2 rounded-lg text-ink-dim hover:text-danger-400 hover:bg-danger-500/10 disabled:opacity-40"
                  title="Torbayı sil"
                >
                  <Trash className="size-4" />
                </button>
              </div>
              <div className="col-span-12 md:col-span-12 grid grid-cols-12 gap-2 items-end">
                <div className="col-span-12 md:col-span-9 min-w-0">
                  <div className="label">Bölünme notu (ör. "32+32=64 ml")</div>
                  <Input
                    value={r.compositionNote}
                    onChange={(e) => updateRow(r.id, { compositionNote: e.target.value })}
                    placeholder="32+32=64 ml"
                  />
                </div>
                {!isAutologous && (
                  <div className="col-span-12 md:col-span-3 min-w-0">
                    <div className="label">CD3%</div>
                    <Input
                      type="number"
                      step="0.01"
                      inputMode="decimal"
                      className="text-right w-full"
                      value={r.cd3Percent}
                      onChange={(e) => updateRow(r.id, { cd3Percent: e.target.value })}
                      placeholder="opsiyonel"
                    />
                  </div>
                )}
              </div>
              <div className="col-span-12 flex items-center gap-3 text-xs text-ink-dim pt-1">
                <span className="inline-flex items-center gap-1">
                  <Calculator className="size-3" />
                  CD34/kg:{" "}
                  <span className="text-ink font-medium">{formatNumber(c.cd34, 0)}</span>
                </span>
                {!isAutologous && c.cd3 > 0 && (
                  <span>
                    CD3/kg: <span className="text-ink">{formatNumber(c.cd3, 0)}</span>
                  </span>
                )}
                {/* per-row freeze indicator removed */}
              </div>
            </div>
          );
        })}
      </div>

      <div className="flex items-center gap-2">
        <Button type="button" variant="soft" onClick={addRow} icon={<Plus className="size-4" />}>
          Torba ekle
        </Button>
        <label className="flex items-center gap-2 text-xs text-ink-muted ml-auto cursor-pointer">
          <input
            type="checkbox"
            checked={validateTotal}
            onChange={(e) => setValidateTotal(e.target.checked)}
            className="accent-brand-500"
          />
          Toplam hacmi session ürün hacmiyle sınırla
        </label>
      </div>

      <div className="sticky bottom-0 -mx-6 -mb-5 px-6 py-3 bg-bg-card/95 backdrop-blur border-t border-line/60 flex items-center justify-end gap-2">
        <Button variant="soft" type="button" onClick={onCancel}>İptal</Button>
        <Button
          onClick={() => submit.mutate()}
          loading={submit.isPending}
          icon={<Snowflake className="size-4" />}
        >
          Torbaları oluştur
        </Button>
      </div>
    </div>
  );
}
