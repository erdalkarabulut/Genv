import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { BagCells, Boxes, RackSlots, Racks, Tanks } from "@/lib/api";
import { Card, CardHeader } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Input, Select } from "@/components/ui/Input";
import { Modal } from "@/components/ui/Modal";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { useEffect, useMemo, useRef, useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { Plus, Pencil, Trash2, Boxes as BoxesIcon, ChevronRight } from "lucide-react";

type Entity = "tank" | "rack" | "rackSlot" | "box" | "bagCell";

/** Envanter ekranı dış DB değişikliği / silme sonrası her zaman taze veri alsın. */
const inventoryListQuery = {
  staleTime: 0,
  gcTime: 5 * 60 * 1000,
  refetchOnWindowFocus: true,
  refetchOnMount: "always" as const,
};

export default function InventoryPage() {
  const qc = useQueryClient();
  const tanks = useQuery({
    ...inventoryListQuery,
    queryKey: ["tanks"],
    queryFn: () => Tanks.list(0, 500),
  });
  const racks = useQuery({
    ...inventoryListQuery,
    queryKey: ["racks"],
    queryFn: () => Racks.list(0, 1000),
  });
  const rackSlots = useQuery({
    ...inventoryListQuery,
    queryKey: ["rackSlots"],
    queryFn: () => RackSlots.list(0, 5000),
  });
  const boxes = useQuery({
    ...inventoryListQuery,
    queryKey: ["boxes"],
    queryFn: () => Boxes.list(0, 2000),
  });
  const bagCells = useQuery({
    ...inventoryListQuery,
    queryKey: ["bagCells"],
    queryFn: () => BagCells.list(0, 5000),
  });

  const [selectedTank, setSelectedTank] = useState<string | null>(null);
  const [selectedRack, setSelectedRack] = useState<string | null>(null);
  const [selectedRackSlot, setSelectedRackSlot] = useState<string | null>(null);
  const [selectedBox, setSelectedBox] = useState<string | null>(null);

  const selectionRef = useRef({
    tank: null as string | null,
    rack: null as string | null,
    slot: null as string | null,
    box: null as string | null,
  });
  useEffect(() => {
    selectionRef.current = {
      tank: selectedTank,
      rack: selectedRack,
      slot: selectedRackSlot,
      box: selectedBox,
    };
  }, [selectedTank, selectedRack, selectedRackSlot, selectedBox]);

  const tankItems = tanks.data?.items ?? [];
  /** Tank tablosunda hâlâ var olan tank id'leri — silinmiş tanka bağlı yetim rack vb. gizlenir. */
  const knownTankIds = useMemo(() => new Set(tankItems.map((t) => t.id)), [tankItems]);

  const rackItems = useMemo(() => {
    const all = racks.data?.items ?? [];
    if (tankItems.length === 0) return [];
    if (selectedTank) return all.filter((r) => r.tankId === selectedTank);
    return all.filter((r) => knownTankIds.has(r.tankId));
  }, [racks.data, selectedTank, tankItems.length, knownTankIds]);

  const rackSlotItems = useMemo(() => {
    const all = rackSlots.data?.items ?? [];
    const rackIds = new Set(rackItems.map((r) => r.id));
    if (rackIds.size === 0) return [];
    const inRacks = all.filter((s) => rackIds.has(s.rackId));
    if (!selectedRack) return inRacks;
    return inRacks.filter((s) => s.rackId === selectedRack);
  }, [rackSlots.data, rackItems, selectedRack]);

  const boxItems = useMemo(() => {
    const all = boxes.data?.items ?? [];
    const slotIds = new Set(rackSlotItems.map((s) => s.id));
    if (slotIds.size === 0) return [];
    const inSlots = all.filter((b) => slotIds.has(b.slotId));
    if (!selectedRackSlot) return inSlots;
    return inSlots.filter((b) => b.slotId === selectedRackSlot);
  }, [boxes.data, rackSlotItems, selectedRackSlot]);

  const bagCellItems = useMemo(() => {
    const all = bagCells.data?.items ?? [];
    const boxIds = new Set(boxItems.map((b) => b.id));
    if (boxIds.size === 0) return [];
    const inBoxes = all.filter((c) => boxIds.has(c.boxId));
    if (!selectedBox) return inBoxes;
    return inBoxes.filter((c) => c.boxId === selectedBox);
  }, [bagCells.data, boxItems, selectedBox]);

  const refreshCryoInventory = async () => {
    await Promise.all([
      qc.refetchQueries({ queryKey: ["tanks"], type: "active" }),
      qc.refetchQueries({ queryKey: ["racks"], type: "active" }),
      qc.refetchQueries({ queryKey: ["rackSlots"], type: "active" }),
      qc.refetchQueries({ queryKey: ["boxes"], type: "active" }),
      qc.refetchQueries({ queryKey: ["bagCells"], type: "active" }),
      qc.invalidateQueries({ queryKey: ["cryo-grid"] }),
      qc.invalidateQueries({ queryKey: ["dashboard"] }),
    ]);
  };

  const [modal, setModal] = useState<
    | { kind: Entity; mode: "create" | "edit"; data?: unknown }
    | null
  >(null);
  const [toDelete, setToDelete] = useState<{ kind: Entity; id: string; label: string } | null>(null);

  const pruneSelectionAfterDelete = (kind: Entity, id: string) => {
    if (kind === "tank") {
      if (selectionRef.current.tank === id) {
        setSelectedTank(null);
        setSelectedRack(null);
        setSelectedRackSlot(null);
        setSelectedBox(null);
      } else {
        setSelectedTank((cur) => (cur === id ? null : cur));
      }
      return;
    }
    if (kind === "rack") {
      setSelectedRack((cur) => (cur === id ? null : cur));
      setSelectedRackSlot(null);
      setSelectedBox(null);
      return;
    }
    if (kind === "rackSlot") {
      setSelectedRackSlot((cur) => (cur === id ? null : cur));
      setSelectedBox(null);
      return;
    }
    if (kind === "box") {
      setSelectedBox((cur) => (cur === id ? null : cur));
    }
  };

  const doDelete = useMutation({
    mutationFn: async ({ kind, id }: { kind: Entity; id: string }) => {
      if (kind === "tank") return Tanks.remove(id);
      if (kind === "rack") return Racks.remove(id);
      if (kind === "rackSlot") return RackSlots.remove(id);
      if (kind === "box") return Boxes.remove(id);
      return BagCells.remove(id);
    },
    onSuccess: async (_data, variables) => {
      pruneSelectionAfterDelete(variables.kind, variables.id);
      setToDelete(null);
      toast.success("Silindi");
      await refreshCryoInventory();
    },
    // Hata mesajı axios interceptor'da gösteriliyor
  });

  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-2xl font-semibold tracking-tight">Cryo envanteri</h1>
        <p className="text-sm text-ink-muted mt-1">
          Tank → Rack → Raf slotu → Box → Torba hücresi. Yapıyı burada oluşturup düzenleyebilirsiniz.
        </p>
      </header>

      <section className="grid grid-cols-1 xl:grid-cols-5 gap-4 overflow-x-auto">
        {/* Tanks */}
        <Column
          title="Tanklar"
          count={tankItems.length}
          onAdd={() => setModal({ kind: "tank", mode: "create" })}
        >
          {tankItems.map((t) => {
            const rackCount = (racks.data?.items ?? []).filter((r) => r.tankId === t.id).length;
            return (
              <Row
                key={t.id}
                label={t.name}
                hint={`${rackCount} rack`}
                active={selectedTank === t.id}
                onClick={() => {
                  setSelectedTank(t.id);
                  setSelectedRack(null);
                  setSelectedRackSlot(null);
                  setSelectedBox(null);
                }}
                onEdit={() => setModal({ kind: "tank", mode: "edit", data: t })}
                onDelete={() => setToDelete({ kind: "tank", id: t.id, label: t.name })}
              />
            );
          })}
        </Column>

        {/* Racks */}
        <Column
          title="Racklar"
          count={rackItems.length}
          onAdd={() =>
            setModal({
              kind: "rack",
              mode: "create",
              data: { tankId: selectedTank ?? tankItems[0]?.id },
            })
          }
          disabledAdd={tankItems.length === 0}
        >
          {rackItems.map((r) => {
            const slotCount = (rackSlots.data?.items ?? []).filter((s) => s.rackId === r.id).length;
            return (
              <Row
                key={r.id}
                label={r.name}
                hint={`${slotCount} raf slotu`}
                active={selectedRack === r.id}
                onClick={() => {
                  setSelectedRack(r.id);
                  setSelectedRackSlot(null);
                  setSelectedBox(null);
                }}
                onEdit={() => setModal({ kind: "rack", mode: "edit", data: r })}
                onDelete={() => setToDelete({ kind: "rack", id: r.id, label: r.name })}
              />
            );
          })}
        </Column>

        {/* Rack slots */}
        <Column
          title="Raf slotları"
          count={rackSlotItems.length}
          onAdd={() =>
            setModal({
              kind: "rackSlot",
              mode: "create",
              data: { rackId: selectedRack ?? rackItems[0]?.id },
            })
          }
          disabledAdd={rackItems.length === 0}
        >
          {rackSlotItems.map((s) => {
            const bxCount = (boxes.data?.items ?? []).filter((b) => b.slotId === s.id).length;
            return (
              <Row
                key={s.id}
                label={s.name}
                hint={`${bxCount} kutu`}
                active={selectedRackSlot === s.id}
                onClick={() => {
                  setSelectedRackSlot(s.id);
                  setSelectedBox(null);
                }}
                onEdit={() => setModal({ kind: "rackSlot", mode: "edit", data: s })}
                onDelete={() => setToDelete({ kind: "rackSlot", id: s.id, label: s.name })}
              />
            );
          })}
        </Column>

        {/* Boxes */}
        <Column
          title="Boxlar"
          count={boxItems.length}
          onAdd={() =>
            setModal({
              kind: "box",
              mode: "create",
              data: { slotId: selectedRackSlot ?? rackSlotItems[0]?.id },
            })
          }
          disabledAdd={rackSlotItems.length === 0}
        >
          {boxItems.map((b) => {
            const cellCount = (bagCells.data?.items ?? []).filter((c) => c.boxId === b.id).length;
            return (
              <Row
                key={b.id}
                label={b.name}
                hint={`${cellCount} hücre`}
                active={selectedBox === b.id}
                onClick={() => setSelectedBox(b.id)}
                onEdit={() => setModal({ kind: "box", mode: "edit", data: b })}
                onDelete={() => setToDelete({ kind: "box", id: b.id, label: b.name })}
              />
            );
          })}
        </Column>

        {/* Bag cells */}
        <Column
          title="Torba hücreleri"
          count={bagCellItems.length}
          onAdd={() =>
            setModal({
              kind: "bagCell",
              mode: "create",
              data: { boxId: selectedBox ?? boxItems[0]?.id },
            })
          }
          disabledAdd={boxItems.length === 0}
        >
          {bagCellItems.map((c) => (
            <Row
              key={c.id}
              label={c.position}
              hint={
                <span className="inline-flex items-center gap-1.5">
                  <Badge tone={c.isOccupied ? "rose" : "mint"} dot>
                    {c.isOccupied ? "Dolu" : "Boş"}
                  </Badge>
                  <span className="text-[10px] text-ink-dim">v{c.version}</span>
                </span>
              }
              onEdit={() => setModal({ kind: "bagCell", mode: "edit", data: c })}
              onDelete={() => setToDelete({ kind: "bagCell", id: c.id, label: c.position })}
            />
          ))}
        </Column>
      </section>

      <Modal
        open={!!modal}
        onClose={() => setModal(null)}
        title={`${modal?.mode === "edit" ? "Düzenle" : "Yeni"} · ${labelFor(modal?.kind)}`}
      >
        {modal && (
          <EntityForm
            kind={modal.kind}
            mode={modal.mode}
            initial={modal.data}
            tanks={tankItems}
            racks={racks.data?.items ?? []}
            rackSlots={rackSlots.data?.items ?? []}
            boxes={boxes.data?.items ?? []}
            onCancel={() => setModal(null)}
            onSaved={() => {
              setModal(null);
              void refreshCryoInventory();
            }}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={!!toDelete}
        onClose={() => setToDelete(null)}
        onConfirm={() => toDelete && doDelete.mutate({ kind: toDelete.kind, id: toDelete.id })}
        loading={doDelete.isPending}
        description={`"${toDelete?.label ?? ""}" (${labelFor(toDelete?.kind)}) silinecek. Alt kayıtlar bağlıysa işlem başarısız olabilir.`}
      />
    </div>
  );
}

function labelFor(k?: Entity) {
  if (k === "tank") return "Tank";
  if (k === "rack") return "Rack";
  if (k === "rackSlot") return "Raf slotu";
  if (k === "box") return "Box";
  if (k === "bagCell") return "Torba hücresi";
  return "";
}

function Column({
  title,
  count,
  children,
  onAdd,
  disabledAdd,
}: {
  title: string;
  count: number;
  children: React.ReactNode;
  onAdd: () => void;
  disabledAdd?: boolean;
}) {
  return (
    <Card className="!p-0 min-w-[180px]">
      <CardHeader
        className="px-4 pt-4 pb-3"
        title={
          <span className="inline-flex items-center gap-2">
            <BoxesIcon className="size-4 text-ink-muted" /> {title}
            <span className="text-[11px] font-normal text-ink-dim">({count})</span>
          </span>
        }
        right={
          <Button size="sm" variant="soft" disabled={disabledAdd} onClick={onAdd} icon={<Plus className="size-3.5" />}>
            Ekle
          </Button>
        }
      />
      <div className="max-h-[60vh] overflow-y-auto px-2 pb-3 space-y-1">
        {count === 0 && <p className="text-xs text-ink-dim px-3 py-2">Kayıt yok.</p>}
        {children}
      </div>
    </Card>
  );
}

function Row({
  label,
  hint,
  active,
  onClick,
  onEdit,
  onDelete,
}: {
  label: React.ReactNode;
  hint?: React.ReactNode;
  active?: boolean;
  onClick?: () => void;
  onEdit?: () => void;
  onDelete?: () => void;
}) {
  return (
    <div
      className={`group flex items-center justify-between rounded-lg px-3 py-2 transition cursor-pointer border ${
        active
          ? "bg-brand-500/10 border-brand-500/30"
          : "border-transparent hover:bg-bg-elevated/60"
      }`}
      onClick={onClick}
    >
      <div className="min-w-0">
        <div className="text-sm font-medium truncate">{label}</div>
        {hint && <div className="text-[11px] text-ink-dim truncate">{hint}</div>}
      </div>
      <div
        className="flex items-center gap-1 opacity-70 group-hover:opacity-100"
        onClick={(e) => e.stopPropagation()}
      >
        {onEdit && (
          <button
            className="rounded-md p-1 text-ink-dim hover:text-ink hover:bg-bg-elevated"
            onClick={onEdit}
            title="Düzenle"
          >
            <Pencil className="size-3.5" />
          </button>
        )}
        {onDelete && (
          <button
            className="rounded-md p-1 text-ink-dim hover:text-accent-rose hover:bg-rose-500/10"
            onClick={onDelete}
            title="Sil"
          >
            <Trash2 className="size-3.5" />
          </button>
        )}
        {onClick && <ChevronRight className="size-3.5 text-ink-dim" />}
      </div>
    </div>
  );
}

interface FormVals {
  name?: string;
  tankId?: string;
  rackId?: string;
  slotId?: string;
  boxId?: string;
  position?: string;
  isOccupied?: boolean;
  version?: number;
}

function EntityForm({
  kind,
  mode,
  initial,
  tanks,
  racks,
  rackSlots,
  boxes,
  onCancel,
  onSaved,
}: {
  kind: Entity;
  mode: "create" | "edit";
  initial?: unknown;
  tanks: { id: string; name: string }[];
  racks: { id: string; name: string; tankId: string }[];
  rackSlots: { id: string; name: string; rackId: string }[];
  boxes: { id: string; name: string; slotId: string }[];
  onCancel: () => void;
  onSaved: () => void;
}) {
  const i = (initial ?? {}) as Record<string, unknown>;
  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormVals>({
    defaultValues: {
      name: (i.name as string) ?? "",
      tankId: (i.tankId as string) ?? "",
      rackId: (i.rackId as string) ?? "",
      slotId: (i.slotId as string) ?? "",
      boxId: (i.boxId as string) ?? "",
      position: (i.position as string) ?? "",
      isOccupied: (i.isOccupied as boolean) ?? false,
      version: (i.version as number) ?? 0,
    },
  });

  useEffect(() => {
    const iv = (initial ?? {}) as Record<string, unknown>;
    reset({
      name: (iv.name as string) ?? "",
      tankId: (iv.tankId as string) ?? "",
      rackId: (iv.rackId as string) ?? "",
      slotId: (iv.slotId as string) ?? "",
      boxId: (iv.boxId as string) ?? "",
      position: (iv.position as string) ?? "",
      isOccupied: (iv.isOccupied as boolean) ?? false,
      version: (iv.version as number) ?? 0,
    });
  }, [initial, reset]);
  const editId = i.id as string | undefined;

  const onSubmit = async (v: FormVals) => {
    try {
      if (kind === "tank") {
        if (mode === "create") await Tanks.create({ name: v.name });
        else await Tanks.update({ id: editId!, name: v.name! });
      } else if (kind === "rack") {
        if (mode === "create") await Racks.create({ tankId: v.tankId, name: v.name });
        else await Racks.update({ id: editId!, tankId: v.tankId!, name: v.name! });
      } else if (kind === "rackSlot") {
        if (mode === "create") await RackSlots.create({ rackId: v.rackId!, name: v.name! });
        else await RackSlots.update({ id: editId!, rackId: v.rackId!, name: v.name! });
      } else if (kind === "box") {
        if (mode === "create") await Boxes.create({ slotId: v.slotId, name: v.name });
        else await Boxes.update({ id: editId!, slotId: v.slotId!, name: v.name! });
      } else {
        if (mode === "create")
          await BagCells.create({
            boxId: v.boxId,
            position: v.position,
            isOccupied: false,
            version: 0,
          });
        else
          await BagCells.update({
            id: editId!,
            boxId: v.boxId!,
            position: v.position!,
            isOccupied: !!v.isOccupied,
            version: Number(v.version ?? 0),
          });
      }
      toast.success("Kaydedildi");
      onSaved();
    } catch {
      /* interceptor */
    }
  };

  const rackSlotOptions = rackSlots.map((s) => {
    const r = racks.find((x) => x.id === s.rackId);
    return { value: s.id, label: `${r?.name ?? "?"}/${s.name}` };
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      {(kind === "tank" || kind === "rack" || kind === "rackSlot" || kind === "box") && (
        <Input
          label="Ad"
          placeholder={
            kind === "tank" ? "TankA" : kind === "rack" ? "R1" : kind === "rackSlot" ? "S1" : "B1"
          }
          {...register("name", { required: "Zorunlu" })}
          error={errors.name?.message}
        />
      )}
      {kind === "rack" && (
        <Select
          label="Tank"
          {...register("tankId", { required: "Zorunlu" })}
          options={tanks.map((t) => ({ value: t.id, label: t.name }))}
        />
      )}
      {kind === "rackSlot" && (
        <Select
          label="Rack"
          {...register("rackId", { required: "Zorunlu" })}
          options={racks.map((r) => ({ value: r.id, label: r.name }))}
        />
      )}
      {kind === "box" && (
        <Select
          label="Raf slotu"
          {...register("slotId", { required: "Zorunlu" })}
          options={rackSlotOptions}
        />
      )}
      {kind === "bagCell" && (
        <>
          <Select
            label="Box"
            {...register("boxId", { required: "Zorunlu" })}
            options={boxes.map((b) => ({ value: b.id, label: b.name }))}
          />
          <Input
            label="Pozisyon"
            placeholder="A1, B2, C3..."
            {...register("position", { required: "Zorunlu" })}
            error={errors.position?.message}
          />
          {mode === "edit" && (
            <>
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" {...register("isOccupied")} className="accent-brand-500" />
                Dolu (manuel değişiklik önerilmez)
              </label>
              <Input
                label="Version (concurrency token)"
                type="number"
                {...register("version")}
              />
            </>
          )}
        </>
      )}

      <div className="flex justify-end gap-2 pt-2">
        <Button variant="soft" type="button" onClick={onCancel}>İptal</Button>
        <Button type="submit" loading={isSubmitting}>
          {mode === "edit" ? "Güncelle" : "Oluştur"}
        </Button>
      </div>
    </form>
  );
}
