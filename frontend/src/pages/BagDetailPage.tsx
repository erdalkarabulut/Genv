import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate, useParams, Link } from "react-router-dom";
import {
  Bags,
  Dashboard,
  Movements,
  Patients,
  Sessions,
  Slots,
} from "@/lib/api";
import { Card, CardHeader, Stat } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Modal } from "@/components/ui/Modal";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { Select } from "@/components/ui/Input";
import { formatDateTime, formatNumber, shortId } from "@/lib/utils";
import {
  ArrowLeft,
  ArrowRightLeft,
  Beaker,
  Droplet,
  MoveRight,
  PackageCheck,
  Snowflake,
  Trash2,
  UserRound,
} from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";
import type { Bag, BagPurpose, BagStatus } from "@/lib/types";

const statusTone: Record<BagStatus, "sky" | "mint" | "amber" | "neutral" | "rose"> = {
  Frozen: "sky",
  Stored: "mint",
  Reserved: "amber",
  Used: "neutral",
  Discarded: "rose",
};

const purposeTone: Record<BagPurpose, "sky" | "mint" | "amber" | "brand"> = {
  Cryo: "sky",
  Infusion: "mint",
  Backup: "amber",
  QualityControl: "brand",
};

export default function BagDetailPage() {
  const { id } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const nav = useNavigate();

  const bag = useQuery({ queryKey: ["bag", id], queryFn: () => Bags.byId(id!), enabled: !!id });
  const movements = useQuery({
    queryKey: ["bag-movements", id],
    queryFn: () => Movements.byBag(id!),
    enabled: !!id,
  });

  const [storeOpen, setStoreOpen] = useState(false);
  const [moveOpen, setMoveOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ["bag", id] });
    qc.invalidateQueries({ queryKey: ["bag-movements", id] });
    qc.invalidateQueries({ queryKey: ["bags"] });
    qc.invalidateQueries({ queryKey: ["cryo-grid"] });
    qc.invalidateQueries({ queryKey: ["dashboard"] });
    qc.invalidateQueries({ queryKey: ["movements"] });
  };

  const use = useMutation({
    mutationFn: () => Bags.use(id!),
    onSuccess: () => {
      toast.success("Bag kullanıldı");
      invalidate();
    },
  });
  const remove = useMutation({
    mutationFn: () => Bags.remove(id!),
    onSuccess: () => {
      toast.success("Bag silindi");
      nav("/bags");
    },
  });

  if (!id) return null;
  const b = bag.data;

  return (
    <div className="space-y-6">
      <button
        onClick={() => nav("/bags")}
        className="text-xs text-ink-muted hover:text-ink inline-flex items-center gap-1"
      >
        <ArrowLeft className="size-3.5" /> Torbalar
      </button>

      <header className="flex items-end justify-between gap-4 flex-wrap">
        <div className="flex items-center gap-4">
          <div className="grid place-items-center size-14 rounded-2xl bg-gradient-to-br from-brand-500/30 to-emerald-400/20 text-brand-400">
            <Snowflake className="size-6" />
          </div>
          <div>
            <h1 className="text-2xl font-semibold tracking-tight">
              Bag #{b?.bagNumber ?? "…"}
            </h1>
            <div className="mt-1 flex items-center gap-2 flex-wrap text-sm text-ink-muted">
              {b && (
                <>
                  <Badge tone={statusTone[b.status]} dot>{b.status}</Badge>
                  <Badge tone={purposeTone[b.purpose]}>{b.purpose}</Badge>
                  <span className="text-xs text-ink-dim">· {shortId(b.id)}</span>
                </>
              )}
            </div>
          </div>
        </div>
        {b && (
          <div className="flex items-center gap-2 flex-wrap">
            {b.status === "Frozen" && !b.slotId && (
              <Button
                icon={<Snowflake className="size-4" />}
                onClick={() => setStoreOpen(true)}
              >
                Slota yerleştir
              </Button>
            )}
            {b.slotId && b.status !== "Used" && b.status !== "Discarded" && (
              <Button
                variant="soft"
                icon={<ArrowRightLeft className="size-4" />}
                onClick={() => setMoveOpen(true)}
              >
                Başka slota taşı
              </Button>
            )}
            {b.status === "Stored" || b.status === "Reserved" || b.status === "Frozen" ? (
              <Button
                variant="soft"
                icon={<PackageCheck className="size-4" />}
                onClick={() => use.mutate()}
                loading={use.isPending}
              >
                Kullan
              </Button>
            ) : null}
            <Button
              variant="danger"
              icon={<Trash2 className="size-4" />}
              onClick={() => setDeleteOpen(true)}
            >
              Sil
            </Button>
          </div>
        )}
      </header>

      <section className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <Stat
          label="CD34/kg"
          value={b ? formatNumber(b.cd34PerKg, 2) : "—"}
          hint="Hedef ≥ 2 · İdeal 4–5"
          icon={<Droplet className="size-4" />}
        />
        <Stat
          label="CD3/kg"
          value={b ? formatNumber(b.cd3PerKg, 2) : "—"}
          hint="3–8 ideal · >10 GVHD riski"
          icon={<Beaker className="size-4" />}
        />
        <Stat
          label="Hacim"
          value={b ? `${b.volumeMl} ml` : "—"}
          hint={b ? `Kaynak: ${b.sourceVolumeMl} ml` : undefined}
        />
        <Stat
          label="Slot"
          value={b?.slotId ? shortId(b.slotId) : "—"}
          hint={b?.slotId ? "Cryo tankta" : "Yerleştirilmemiş"}
        />
      </section>

      <section className="grid grid-cols-1 xl:grid-cols-3 gap-4">
        <Card className="xl:col-span-2">
          <CardHeader
            title="Movement history"
            subtitle="Bu torba için tüm store, move, use, split aksiyonları"
          />
          {movements.isLoading ? (
            <div className="skeleton h-40" />
          ) : (movements.data?.items ?? []).length === 0 ? (
            <p className="text-sm text-ink-muted">
              Henüz hareket kaydı yok.
            </p>
          ) : (
            <ol className="relative ml-2 space-y-3 border-l border-line pl-4">
              {(movements.data?.items ?? []).map((m) => (
                <li key={m.id} className="relative">
                  <span className="absolute -left-[22px] top-1 grid size-4 place-items-center rounded-full border border-brand-500 bg-brand-500/40 shadow-glow">
                    <span className="size-1.5 rounded-full bg-brand-400" />
                  </span>
                  <div className="flex items-center justify-between gap-3 flex-wrap">
                    <div>
                      <div className="text-sm font-medium">{m.action}</div>
                      <div className="text-[11px] text-ink-dim flex items-center gap-1 flex-wrap">
                        {m.fromSlotId ? shortId(m.fromSlotId) : "—"}
                        <MoveRight className="size-3" />
                        {m.toSlotId ? shortId(m.toSlotId) : "—"}
                      </div>
                    </div>
                    <div className="text-[11px] text-ink-dim">
                      {formatDateTime(m.createdDate)}
                    </div>
                  </div>
                </li>
              ))}
            </ol>
          )}
        </Card>

        <Card>
          <CardHeader title="İlişkili kayıtlar" subtitle="Seans ve hasta bilgileri" />
          {b ? <RelatedBlock bag={b} /> : <div className="skeleton h-32" />}
        </Card>
      </section>

      {b && (
        <Modal open={storeOpen} onClose={() => setStoreOpen(false)} title="Slota yerleştir">
          <StoreForm bag={b} onCancel={() => setStoreOpen(false)} onDone={() => { setStoreOpen(false); invalidate(); }} />
        </Modal>
      )}

      {b && (
        <Modal open={moveOpen} onClose={() => setMoveOpen(false)} title="Başka slota taşı">
          <MoveForm bag={b} onCancel={() => setMoveOpen(false)} onDone={() => { setMoveOpen(false); invalidate(); }} />
        </Modal>
      )}

      <ConfirmDialog
        open={deleteOpen}
        onClose={() => setDeleteOpen(false)}
        onConfirm={() => remove.mutate()}
        loading={remove.isPending}
        description={`Bag #${b?.bagNumber ?? ""} kaydını silmek üzeresiniz. Hareket kayıtları korunur ancak slot serbest bırakılmaz — önce "Kullan" aksiyonu öneririz.`}
      />
    </div>
  );
}

function RelatedBlock({ bag }: { bag: Bag }) {
  const session = useQuery({
    queryKey: ["session", bag.sessionId],
    queryFn: () => Sessions.byId(bag.sessionId),
    enabled: !!bag.sessionId,
  });
  const patient = useQuery({
    queryKey: ["patient", session.data?.patientId],
    queryFn: () => Patients.byId(session.data!.patientId),
    enabled: !!session.data?.patientId,
  });

  return (
    <div className="space-y-3">
      <div className="rounded-xl border border-line/60 bg-bg-elevated/40 p-3">
        <div className="flex items-center gap-2 text-xs text-ink-dim">
          <Beaker className="size-3.5" /> Aferez seansı
        </div>
        {session.isLoading ? (
          <div className="skeleton h-8 mt-2" />
        ) : session.data ? (
          <div className="mt-1.5 text-sm">
            Gün {session.data.day} · CD34/kg {formatNumber(session.data.cd34PerKg, 2)} · CD3/kg{" "}
            {formatNumber(session.data.cd3PerKg, 2)}
            <div className="text-[11px] text-ink-dim">
              {formatDateTime(session.data.date)}
            </div>
          </div>
        ) : (
          <span className="text-sm text-ink-muted">—</span>
        )}
      </div>

      <div className="rounded-xl border border-line/60 bg-bg-elevated/40 p-3">
        <div className="flex items-center gap-2 text-xs text-ink-dim">
          <UserRound className="size-3.5" /> Hasta
        </div>
        {patient.isLoading ? (
          <div className="skeleton h-8 mt-2" />
        ) : patient.data ? (
          <Link
            to={`/patients/${patient.data.id}`}
            className="mt-1.5 block text-sm font-medium hover:text-brand-400"
          >
            {patient.data.fullName}
            <div className="text-[11px] text-ink-dim font-normal">
              {patient.data.protocolNo ?? "—"} · {patient.data.weightKg} kg ·{" "}
              {patient.data.transplantType}
            </div>
          </Link>
        ) : (
          <span className="text-sm text-ink-muted">—</span>
        )}
      </div>
    </div>
  );
}

function StoreForm({ bag, onCancel, onDone }: { bag: Bag; onCancel: () => void; onDone: () => void }) {
  const cryo = useQuery({ queryKey: ["cryo-grid"], queryFn: () => Dashboard.cryoGrid() });
  const emptySlots = (cryo.data?.tanks ?? []).flatMap((t) =>
    t.racks.flatMap((r) =>
      r.boxes.flatMap((b) =>
        b.slots
          .filter((s) => !s.isOccupied)
          .map((s) => ({
            id: s.id,
            label: `${t.name}-${r.name}-${b.name}-${s.position}`,
          })),
      ),
    ),
  );

  const [slotId, setSlotId] = useState(emptySlots[0]?.id ?? "");
  const mut = useMutation({
    mutationFn: () => Bags.store(bag.id, slotId),
    onSuccess: () => {
      toast.success("Slota yerleştirildi");
      onDone();
    },
  });

  return (
    <div className="space-y-4">
      <Select
        label="Hedef slot"
        value={slotId}
        onChange={(e) => setSlotId(e.target.value)}
        options={emptySlots.length === 0
          ? [{ value: "", label: "Boş slot bulunamadı" }]
          : emptySlots.map((s) => ({ value: s.id, label: s.label }))}
      />
      <div className="flex justify-end gap-2">
        <Button variant="soft" onClick={onCancel}>İptal</Button>
        <Button disabled={!slotId} loading={mut.isPending} onClick={() => mut.mutate()}>
          Yerleştir
        </Button>
      </div>
    </div>
  );
}

function MoveForm({ bag, onCancel, onDone }: { bag: Bag; onCancel: () => void; onDone: () => void }) {
  const cryo = useQuery({ queryKey: ["cryo-grid"], queryFn: () => Dashboard.cryoGrid() });
  const emptySlots = (cryo.data?.tanks ?? []).flatMap((t) =>
    t.racks.flatMap((r) =>
      r.boxes.flatMap((bx) =>
        bx.slots
          .filter((s) => !s.isOccupied && s.id !== bag.slotId)
          .map((s) => ({
            id: s.id,
            label: `${t.name}-${r.name}-${bx.name}-${s.position}`,
          })),
      ),
    ),
  );
  const [slotId, setSlotId] = useState(emptySlots[0]?.id ?? "");
  const mut = useMutation({
    mutationFn: () => Bags.move(bag.id, slotId),
    onSuccess: () => {
      toast.success("Torba taşındı");
      onDone();
    },
  });
  return (
    <div className="space-y-4">
      <Select
        label="Yeni slot"
        value={slotId}
        onChange={(e) => setSlotId(e.target.value)}
        options={emptySlots.length === 0
          ? [{ value: "", label: "Boş slot bulunamadı" }]
          : emptySlots.map((s) => ({ value: s.id, label: s.label }))}
      />
      <div className="flex justify-end gap-2">
        <Button variant="soft" onClick={onCancel}>İptal</Button>
        <Button disabled={!slotId} loading={mut.isPending} onClick={() => mut.mutate()}>
          Taşı
        </Button>
      </div>
    </div>
  );
}
