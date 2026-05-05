import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Bags } from "@/lib/api";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Input, Select } from "@/components/ui/Input";
import { Modal } from "@/components/ui/Modal";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { useEffect, useState } from "react";
import { onCryo } from "@/lib/signalr";
import { Pagination } from "@/components/ui/Pagination";
import { useDebounce } from "@/lib/useDebounce";
import type { Bag, BagPurpose, BagStatus } from "@/lib/types";
import { formatDate, formatNumber, shortId } from "@/lib/utils";
import { Search, Trash2, Pencil, PackageCheck, ChevronRight } from "lucide-react";
import { toast } from "sonner";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";

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

export default function BagsPage() {
  const qc = useQueryClient();
  const [q, setQ] = useState("");
  const [status, setStatus] = useState<"all" | BagStatus>("all");
  const [purpose, setPurpose] = useState<"all" | BagPurpose>("all");
  const [page, setPage] = useState(0);
  const [editing, setEditing] = useState<Bag | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Bag | null>(null);
  const debouncedQ = useDebounce(q, 300);

  useEffect(() => { setPage(0); }, [debouncedQ, status, purpose]);

  const PAGE_SIZE = 10;

  const buildQuery = () => {
    const sort = [{ field: "createdDate", dir: "desc" as const }];
    // bagNumber is int — only filter by eq if query is a valid number
    const bagNum = debouncedQ ? parseInt(debouncedQ, 10) : NaN;
    const searchFilter = !isNaN(bagNum)
      ? { field: "bagNumber", operator: "eq" as const, value: String(bagNum) }
      : null;
    const statusFilter = status !== "all" ? { field: "status", operator: "eq" as const, value: status } : null;
    const purposeFilter = purpose !== "all" ? { field: "purpose", operator: "eq" as const, value: purpose } : null;

    const active = [searchFilter, statusFilter, purposeFilter].filter(Boolean) as any[];
    if (active.length === 0) return { sort };
    // Chain with AND: f1 AND (f2 AND (f3))
    let f = active[active.length - 1];
    for (let i = active.length - 2; i >= 0; i--) {
      f = { ...active[i], logic: "and" as const, filters: [f] };
    }
    return { filter: f, sort };
  };

  const list = useQuery({
    queryKey: ["bags", page, PAGE_SIZE, debouncedQ, status, purpose],
    queryFn: () => Bags.byDynamic(buildQuery(), page, PAGE_SIZE),
  });

  const paginated = list.data?.items ?? [];

  useEffect(() => {
    const a = onCryo("BagStored", () => qc.invalidateQueries({ queryKey: ["bags"] }));
    const b = onCryo("BagMoved", () => qc.invalidateQueries({ queryKey: ["bags"] }));
    const c = onCryo("BagUsed", () => qc.invalidateQueries({ queryKey: ["bags"] }));
    return () => { a(); b(); c(); };
  }, [qc]);

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ["bags"] });
    qc.invalidateQueries({ queryKey: ["dashboard"] });
    qc.invalidateQueries({ queryKey: ["cryo-grid"] });
    qc.invalidateQueries({ queryKey: ["movements"] });
  };

  const remove = useMutation({
    mutationFn: (id: string) => Bags.remove(id),
    onSuccess: () => {
      toast.success("Bag silindi");
      setDeleteTarget(null);
      invalidate();
    },
  });

  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-2xl font-semibold tracking-tight">Torbalar</h1>
        <p className="text-sm text-ink-muted mt-1">
          Tüm bag yaşam döngüsü görünümü, gerçek zamanlı senkron. Satıra tıklayarak detay ve movement history açın.
        </p>
      </header>

      <Card>
        <div className="flex items-center gap-3 flex-wrap">
          <div className="relative flex-1 min-w-[240px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-ink-dim" />
            <input
              className="input pl-9"
              placeholder="Bag id veya numara ile ara…"
              value={q}
              onChange={(e) => setQ(e.target.value)}
            />
          </div>
          <Select
            value={status}
            onChange={(e) => setStatus(e.target.value as any)}
            options={[
              { value: "all", label: "Tüm statüler" },
              { value: "Frozen", label: "Frozen" },
              { value: "Stored", label: "Stored" },
              { value: "Reserved", label: "Reserved" },
              { value: "Used", label: "Used" },
              { value: "Discarded", label: "Discarded" },
            ]}
            className="min-w-[180px]"
          />
          <Select
            value={purpose}
            onChange={(e) => setPurpose(e.target.value as any)}
            options={[
              { value: "all", label: "Tüm amaçlar" },
              { value: "Cryo", label: "Cryo" },
              { value: "Infusion", label: "Infusion" },
              { value: "Backup", label: "Backup" },
              { value: "QualityControl", label: "QualityControl" },
            ]}
            className="min-w-[180px]"
          />
        </div>
      </Card>

      <Card className="!p-0 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="text-xs text-ink-dim uppercase tracking-wide">
              <tr className="border-b border-line/60">
                <Th>Bag</Th>
                <Th>Durum</Th>
                <Th>Amaç</Th>
                <Th>CD34/kg</Th>
                <Th>CD3/kg</Th>
                <Th>Hacim</Th>
                <Th>Hücre</Th>
                <Th>Tarih</Th>
                <Th>{""}</Th>
              </tr>
            </thead>
            <tbody>
              {list.isLoading &&
                Array.from({ length: 6 }).map((_, i) => (
                  <tr key={i} className="border-b border-line/40">
                    <td colSpan={9} className="px-4 py-3">
                      <div className="skeleton h-6" />
                    </td>
                  </tr>
                ))}
              {!list.isLoading && paginated.length === 0 && (
                <tr>
                  <td colSpan={9} className="px-4 py-12 text-center text-ink-muted text-sm">
                    Sonuç yok.
                  </td>
                </tr>
              )}
              {paginated.map((b) => (
                <BagRow
                  key={b.id}
                  b={b}
                  onEdit={() => setEditing(b)}
                  onDelete={() => setDeleteTarget(b)}
                  onUseSuccess={invalidate}
                />
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      <Pagination
        page={page}
        totalPages={list.data?.pages ?? 0}
        totalItems={list.data?.count ?? 0}
        pageSize={PAGE_SIZE}
        onPageChange={setPage}
      />

      <Modal open={!!editing} onClose={() => setEditing(null)} title="Bag'i düzenle" size="lg">
        {editing && (
          <BagEditForm
            bag={editing}
            onCancel={() => setEditing(null)}
            onSaved={() => {
              setEditing(null);
              invalidate();
            }}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && remove.mutate(deleteTarget.id)}
        loading={remove.isPending}
        description={`Bag #${deleteTarget?.bagNumber ?? ""} kaydını silmek istediğinize emin misiniz? Hareket geçmişi korunur.`}
      />
    </div>
  );
}

function Th({ children }: { children: React.ReactNode }) {
  return <th className="text-left px-4 py-3 font-medium">{children}</th>;
}

function BagRow({
  b,
  onEdit,
  onDelete,
  onUseSuccess,
}: {
  b: Bag;
  onEdit: () => void;
  onDelete: () => void;
  onUseSuccess: () => void;
}) {
  const nav = useNavigate();
  const useMut = useMutation({
    mutationFn: () => Bags.use(b.id),
    onSuccess: () => {
      toast.success(`Bag #${b.bagNumber} kullanıldı`);
      onUseSuccess();
    },
  });

  return (
    <tr
      className="border-b border-line/40 hover:bg-bg-elevated/40 transition cursor-pointer"
      onClick={() => nav(`/bags/${b.id}`)}
    >
      <td className="px-4 py-3">
        <div className="font-medium">#{b.bagNumber}</div>
        <div className="text-[11px] text-ink-dim">{shortId(b.id)}</div>
      </td>
      <td className="px-4 py-3">
        <Badge tone={statusTone[b.status]} dot>{b.status}</Badge>
      </td>
      <td className="px-4 py-3">
        <Badge tone={purposeTone[b.purpose]}>{b.purpose}</Badge>
      </td>
      <td className="px-4 py-3 text-ink-muted">{formatNumber(b.cd34PerKg, 2)}</td>
      <td className="px-4 py-3 text-ink-muted">{formatNumber(b.cd3PerKg, 2)}</td>
      <td className="px-4 py-3 text-ink-muted">{b.volumeMl} ml</td>
      <td className="px-4 py-3 text-ink-muted">{b.bagCellId ? shortId(b.bagCellId) : "—"}</td>
      <td className="px-4 py-3 text-ink-muted">{formatDate(b.createdDate)}</td>
      <td className="px-4 py-3" onClick={(e) => e.stopPropagation()}>
        <div className="flex justify-end gap-1">
          {b.status !== "Used" && b.status !== "Discarded" && (
            <Button
              size="sm"
              variant="soft"
              icon={<PackageCheck className="size-3.5" />}
              onClick={() => useMut.mutate()}
              loading={useMut.isPending}
              title="Kullan"
            >
              Kullan
            </Button>
          )}
          <Button size="sm" variant="soft" icon={<Pencil className="size-3.5" />} onClick={onEdit}>
            Düzenle
          </Button>
          <Button size="sm" variant="danger" icon={<Trash2 className="size-3.5" />} onClick={onDelete}>
            Sil
          </Button>
          <button
            className="ml-1 rounded-md p-1 text-ink-dim hover:text-ink"
            onClick={() => nav(`/bags/${b.id}`)}
            title="Detay"
          >
            <ChevronRight className="size-4" />
          </button>
        </div>
      </td>
    </tr>
  );
}

interface BagEditVals {
  bagNumber: number;
  volumeMl: number;
  sourceVolumeMl: number;
  cd34PerKg: number;
  cd3PerKg: number;
  status: BagStatus;
  purpose: BagPurpose;
}

function BagEditForm({
  bag,
  onCancel,
  onSaved,
}: {
  bag: Bag;
  onCancel: () => void;
  onSaved: () => void;
}) {
  const { register, handleSubmit, reset, formState: { isSubmitting } } = useForm<BagEditVals>({
    defaultValues: {
      bagNumber: bag.bagNumber,
      volumeMl: bag.volumeMl,
      sourceVolumeMl: bag.sourceVolumeMl,
      cd34PerKg: bag.cd34PerKg,
      cd3PerKg: bag.cd3PerKg,
      status: bag.status,
      purpose: bag.purpose,
    },
  });

  useEffect(() => {
    reset({
      bagNumber: bag.bagNumber,
      volumeMl: bag.volumeMl,
      sourceVolumeMl: bag.sourceVolumeMl,
      cd34PerKg: bag.cd34PerKg,
      cd3PerKg: bag.cd3PerKg,
      status: bag.status,
      purpose: bag.purpose,
    });
  }, [bag, reset]);

  const qc = useQueryClient();

  const onSubmit = async (v: BagEditVals) => {
    await Bags.update({
      id: bag.id,
      sessionId: bag.sessionId,
      bagNumber: Number(v.bagNumber),
      volumeMl: Number(v.volumeMl),
      sourceVolumeMl: Number(v.sourceVolumeMl),
      cd34PerKg: Number(v.cd34PerKg),
      cd3PerKg: Number(v.cd3PerKg),
      status: v.status,
      purpose: v.purpose,
      splitBatchId: bag.splitBatchId,
      bagCellId: bag.bagCellId,
    });
    toast.success("Bag güncellendi");
    qc.setQueryData(["bags"], (old: { items: Bag[] } | undefined) => {
      if (!old) return old;
      return { ...old, items: old.items.map(i => i.id === bag.id ? { ...i, ...v, bagNumber: Number(v.bagNumber), volumeMl: Number(v.volumeMl), sourceVolumeMl: Number(v.sourceVolumeMl), cd34PerKg: Number(v.cd34PerKg), cd3PerKg: Number(v.cd3PerKg) } : i) };
    });
    onSaved();
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="grid grid-cols-2 gap-3">
      <Input label="Bag number" type="number" {...register("bagNumber", { required: true })} />
      <Input label="Hacim (ml)" type="number" step="0.1" {...register("volumeMl", { required: true })} />
      <Input
        label="Kaynak hacim (ml)"
        type="number"
        step="0.1"
        {...register("sourceVolumeMl", { required: true })}
      />
      <div />
      <Input label="CD34/kg" type="number" step="0.0001" {...register("cd34PerKg")} />
      <Input label="CD3/kg" type="number" step="0.0001" {...register("cd3PerKg")} />
      <Select
        label="Status"
        {...register("status")}
        options={[
          { value: "Frozen", label: "Frozen" },
          { value: "Stored", label: "Stored" },
          { value: "Reserved", label: "Reserved" },
          { value: "Used", label: "Used" },
          { value: "Discarded", label: "Discarded" },
        ]}
      />
      <Select
        label="Purpose"
        {...register("purpose")}
        options={[
          { value: "Cryo", label: "Cryo" },
          { value: "Infusion", label: "Infusion" },
          { value: "Backup", label: "Backup" },
          { value: "QualityControl", label: "QualityControl" },
        ]}
      />
      <div className="col-span-2 flex justify-end gap-2 pt-2">
        <Button variant="soft" type="button" onClick={onCancel}>İptal</Button>
        <Button type="submit" loading={isSubmitting}>Kaydet</Button>
      </div>
    </form>
  );
}
