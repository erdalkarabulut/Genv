import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Boxes, Racks, Slots, Tanks } from "@/lib/api";
import { Card, CardHeader } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Input, Select } from "@/components/ui/Input";
import { Modal } from "@/components/ui/Modal";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { Plus, Pencil, Trash2, Boxes as BoxesIcon, ChevronRight } from "lucide-react";

/* ---- Types used only locally ---- */
type Entity = "tank" | "rack" | "box" | "slot";

export default function InventoryPage() {
  const qc = useQueryClient();
  const tanks = useQuery({ queryKey: ["tanks"], queryFn: () => Tanks.list(0, 500) });
  const racks = useQuery({ queryKey: ["racks"], queryFn: () => Racks.list(0, 1000) });
  const boxes = useQuery({ queryKey: ["boxes"], queryFn: () => Boxes.list(0, 2000) });
  const slots = useQuery({ queryKey: ["slots"], queryFn: () => Slots.list(0, 5000) });

  const [selectedTank, setSelectedTank] = useState<string | null>(null);
  const [selectedRack, setSelectedRack] = useState<string | null>(null);
  const [selectedBox, setSelectedBox] = useState<string | null>(null);

  const tankItems = tanks.data?.items ?? [];
  const rackItems = useMemo(
    () => (racks.data?.items ?? []).filter((r) => !selectedTank || r.tankId === selectedTank),
    [racks.data, selectedTank],
  );
  const boxItems = useMemo(
    () => (boxes.data?.items ?? []).filter((b) => !selectedRack || b.rackId === selectedRack),
    [boxes.data, selectedRack],
  );
  const slotItems = useMemo(
    () => (slots.data?.items ?? []).filter((s) => !selectedBox || s.boxId === selectedBox),
    [slots.data, selectedBox],
  );

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ["tanks"] });
    qc.invalidateQueries({ queryKey: ["racks"] });
    qc.invalidateQueries({ queryKey: ["boxes"] });
    qc.invalidateQueries({ queryKey: ["slots"] });
    qc.invalidateQueries({ queryKey: ["cryo-grid"] });
    qc.invalidateQueries({ queryKey: ["dashboard"] });
  };

  /* ---- modal state ---- */
  const [modal, setModal] = useState<
    | { kind: Entity; mode: "create" | "edit"; data?: any }
    | null
  >(null);
  const [toDelete, setToDelete] = useState<{ kind: Entity; id: string; label: string } | null>(null);

  const doDelete = useMutation({
    mutationFn: async ({ kind, id }: { kind: Entity; id: string }) => {
      if (kind === "tank") return Tanks.remove(id);
      if (kind === "rack") return Racks.remove(id);
      if (kind === "box") return Boxes.remove(id);
      return Slots.remove(id);
    },
    onSuccess: () => {
      toast.success("Silindi");
      setToDelete(null);
      invalidate();
    },
  });

  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-2xl font-semibold tracking-tight">Cryo envanteri</h1>
        <p className="text-sm text-ink-muted mt-1">
          Tank → Rack → Box → Slot hiyerarşisi. Yapıyı burada oluşturup düzenleyebilirsiniz.
        </p>
      </header>

      <section className="grid grid-cols-1 lg:grid-cols-4 gap-4">
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
            const bxCount = (boxes.data?.items ?? []).filter((b) => b.rackId === r.id).length;
            return (
              <Row
                key={r.id}
                label={r.name}
                hint={`${bxCount} box`}
                active={selectedRack === r.id}
                onClick={() => {
                  setSelectedRack(r.id);
                  setSelectedBox(null);
                }}
                onEdit={() => setModal({ kind: "rack", mode: "edit", data: r })}
                onDelete={() => setToDelete({ kind: "rack", id: r.id, label: r.name })}
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
              data: { rackId: selectedRack ?? rackItems[0]?.id },
            })
          }
          disabledAdd={rackItems.length === 0}
        >
          {boxItems.map((b) => {
            const slCount = (slots.data?.items ?? []).filter((s) => s.boxId === b.id).length;
            return (
              <Row
                key={b.id}
                label={b.name}
                hint={`${slCount} slot`}
                active={selectedBox === b.id}
                onClick={() => setSelectedBox(b.id)}
                onEdit={() => setModal({ kind: "box", mode: "edit", data: b })}
                onDelete={() => setToDelete({ kind: "box", id: b.id, label: b.name })}
              />
            );
          })}
        </Column>

        {/* Slots */}
        <Column
          title="Slotlar"
          count={slotItems.length}
          onAdd={() =>
            setModal({
              kind: "slot",
              mode: "create",
              data: { boxId: selectedBox ?? boxItems[0]?.id },
            })
          }
          disabledAdd={boxItems.length === 0}
        >
          {slotItems.map((s) => (
            <Row
              key={s.id}
              label={s.position}
              hint={
                <span className="inline-flex items-center gap-1.5">
                  <Badge tone={s.isOccupied ? "rose" : "mint"} dot>
                    {s.isOccupied ? "Dolu" : "Boş"}
                  </Badge>
                  <span className="text-[10px] text-ink-dim">v{s.version}</span>
                </span>
              }
              onEdit={() => setModal({ kind: "slot", mode: "edit", data: s })}
              onDelete={() => setToDelete({ kind: "slot", id: s.id, label: s.position })}
            />
          ))}
        </Column>
      </section>

      {/* Create / Edit modals */}
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
            boxes={boxes.data?.items ?? []}
            onCancel={() => setModal(null)}
            onSaved={() => {
              setModal(null);
              invalidate();
            }}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={!!toDelete}
        onClose={() => setToDelete(null)}
        onConfirm={() => toDelete && doDelete.mutate({ kind: toDelete.kind, id: toDelete.id })}
        loading={doDelete.isPending}
        description={`"${toDelete?.label ?? ""}" (${labelFor(toDelete?.kind)}) silinecek. Alt düğümler foreign-key nedeniyle bağlıysa işlem başarısız olabilir.`}
      />
    </div>
  );
}

function labelFor(k?: Entity) {
  return k === "tank" ? "Tank" : k === "rack" ? "Rack" : k === "box" ? "Box" : k === "slot" ? "Slot" : "";
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
    <Card className="!p-0">
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
        {count === 0 && (
          <p className="text-xs text-ink-dim px-3 py-2">Kayıt yok.</p>
        )}
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
  onEdit: () => void;
  onDelete: () => void;
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
        <button
          className="rounded-md p-1 text-ink-dim hover:text-ink hover:bg-bg-elevated"
          onClick={onEdit}
          title="Düzenle"
        >
          <Pencil className="size-3.5" />
        </button>
        <button
          className="rounded-md p-1 text-ink-dim hover:text-accent-rose hover:bg-rose-500/10"
          onClick={onDelete}
          title="Sil"
        >
          <Trash2 className="size-3.5" />
        </button>
        {onClick && <ChevronRight className="size-3.5 text-ink-dim" />}
      </div>
    </div>
  );
}

interface FormVals {
  name?: string;
  tankId?: string;
  rackId?: string;
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
  boxes,
  onCancel,
  onSaved,
}: {
  kind: Entity;
  mode: "create" | "edit";
  initial: any;
  tanks: any[];
  racks: any[];
  boxes: any[];
  onCancel: () => void;
  onSaved: () => void;
}) {
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormVals>({
    defaultValues: {
      name: initial?.name ?? "",
      tankId: initial?.tankId ?? "",
      rackId: initial?.rackId ?? "",
      boxId: initial?.boxId ?? "",
      position: initial?.position ?? "",
      isOccupied: initial?.isOccupied ?? false,
      version: initial?.version ?? 0,
    },
  });

  const onSubmit = async (v: FormVals) => {
    try {
      if (kind === "tank") {
        if (mode === "create") await Tanks.create({ name: v.name });
        else await Tanks.update({ id: initial.id, name: v.name! });
      } else if (kind === "rack") {
        if (mode === "create") await Racks.create({ tankId: v.tankId, name: v.name });
        else await Racks.update({ id: initial.id, tankId: v.tankId!, name: v.name! });
      } else if (kind === "box") {
        if (mode === "create") await Boxes.create({ rackId: v.rackId, name: v.name });
        else await Boxes.update({ id: initial.id, rackId: v.rackId!, name: v.name! });
      } else {
        if (mode === "create")
          await Slots.create({
            boxId: v.boxId,
            position: v.position,
            isOccupied: false,
            version: 0,
          });
        else
          await Slots.update({
            id: initial.id,
            boxId: v.boxId!,
            position: v.position!,
            isOccupied: !!v.isOccupied,
            version: Number(v.version ?? 0),
          });
      }
      toast.success("Kaydedildi");
      onSaved();
    } catch {
      /* toast handled by interceptor */
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      {kind !== "slot" && (
        <Input
          label="Ad"
          placeholder={kind === "tank" ? "TankA" : kind === "rack" ? "R1" : "B1"}
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
      {kind === "box" && (
        <Select
          label="Rack"
          {...register("rackId", { required: "Zorunlu" })}
          options={racks.map((r) => ({ value: r.id, label: r.name }))}
        />
      )}
      {kind === "slot" && (
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
                Dolu (concurrency güvenliği için manuel değişiklik önerilmez)
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
