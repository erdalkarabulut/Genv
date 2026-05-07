import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Donors, Patients } from "@/lib/api";
import { Card, CardHeader } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Input, Select } from "@/components/ui/Input";
import { Modal } from "@/components/ui/Modal";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { EmptyState } from "@/components/ui/EmptyState";
import { useEffect, useState } from "react";
import { useForm, Controller } from "react-hook-form";
import { Pagination } from "@/components/ui/Pagination";
import { useDebounce } from "@/lib/useDebounce";
import { Plus, Search, Pencil, Trash2, HeartHandshake, Link2, Users } from "lucide-react";
import { toast } from "sonner";
import { formatDate, BLOOD_GROUP_OPTIONS } from "@/lib/utils";
import type { Donor, DonorType } from "@/lib/types";

export interface DonorFormValues {
  fullName: string;
  weightKg: number;
  bloodGroup?: string;
  relation?: string;
  identityNumber?: string;
  donorType: DonorType;
  birthDate?: string;
}

export default function DonorsPage() {
  const qc = useQueryClient();

  const [q, setQ] = useState("");
  const [page, setPage] = useState(0);
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<Donor | null>(null);
  const [detailId, setDetailId] = useState<string | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Donor | null>(null);
  const debouncedQ = useDebounce(q, 300);

  useEffect(() => { setPage(0); }, [debouncedQ]);

  const PAGE_SIZE = 10;

  const buildQuery = () => {
    const sort = [{ field: "createdDate", dir: "desc" as const }];
    if (!debouncedQ) return { sort };
    const filter = {
      field: "fullName", operator: "contains" as const, value: debouncedQ,
      logic: "or" as const,
      filters: [
        { field: "bloodGroup", operator: "contains" as const, value: debouncedQ,
          logic: "or" as const,
          filters: [{ field: "relation", operator: "contains" as const, value: debouncedQ }] },
      ],
    };
    return { filter, sort };
  };

  const list = useQuery({
    queryKey: ["donors", page, PAGE_SIZE, debouncedQ],
    queryFn: () => Donors.byDynamic(buildQuery(), page, PAGE_SIZE),
  });
  const patients = useQuery({
    queryKey: ["patients", "for-donors"],
    queryFn: () => Patients.list(0, 500),
  });

  const paginated = list.data?.items ?? [];

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ["donors"] });
    qc.invalidateQueries({ queryKey: ["patients"] });
  };

  const remove = useMutation({
    mutationFn: (id: string) => Donors.remove(id),
    onSuccess: () => {
      toast.success("Donor silindi");
      setDeleteTarget(null);
      invalidate();
    },
  });

  return (
    <div className="space-y-6">
      <header className="flex items-end justify-between gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Donorlar</h1>
          <p className="text-sm text-ink-muted mt-1">
            Allogeneik transplantasyon için donör kayıtları.
          </p>
        </div>
        <Button icon={<Plus className="size-4" />} onClick={() => setCreateOpen(true)}>
          Yeni donor
        </Button>
      </header>

      <Card>
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-ink-dim" />
          <input
            className="input pl-9"
            placeholder="Ad, kan grubu veya yakınlık ile ara…"
            value={q}
            onChange={(e) => setQ(e.target.value)}
          />
        </div>
      </Card>

      {list.isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="card h-28 skeleton" />
          ))}
        </div>
      ) : paginated.length === 0 ? (
        <EmptyState
          icon={<HeartHandshake className="size-10" />}
          title="Donor kaydı yok"
          description="Allogeneik nakil için bir donor oluşturun."
          action={
            <Button icon={<Plus className="size-4" />} onClick={() => setCreateOpen(true)}>
              Yeni donor
            </Button>
          }
        />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {paginated.map((d) => {
            const linkedPatients = (patients.data?.items ?? []).filter((p) => p.donorId === d.id);
            return (
              <div
                key={d.id}
                className="card p-5 transition hover:border-brand-500/40 hover:shadow-glow cursor-pointer"
                onClick={() => setDetailId(d.id)}
              >
                <div className="flex items-start justify-between gap-3">
                  <div className="flex items-center gap-3 min-w-0">
                    <div className="grid place-items-center size-11 rounded-xl bg-gradient-to-br from-rose-500/30 to-orange-400/20 text-rose-300 font-semibold">
                      {d.fullName.slice(0, 2).toUpperCase()}
                    </div>
                    <div className="min-w-0">
                      <div className="text-base font-semibold truncate">{d.fullName}</div>
                      <div className="text-xs text-ink-muted truncate">
                        {d.relation ?? "Yakınlık —"}
                      </div>
                    </div>
                  </div>
                  <div className="flex flex-col items-end gap-1">
                    <Badge tone={d.donorType === "Unrelated" ? "sky" : "brand"} dot>
                      {d.donorType === "Unrelated" ? "Akraba-dışı" : "Akraba"}
                    </Badge>
                    <Badge tone="rose" dot>{d.bloodGroup ?? "—"}</Badge>
                  </div>
                </div>
                <div className="mt-4 grid grid-cols-3 gap-2">
                  <Mini label="Kilo" value={`${d.weightKg} kg`} />
                  <Mini label="Hasta" value={`${linkedPatients.length}`} />
                  <Mini label="Kayıt" value={formatDate(d.createdDate)} />
                </div>
                <div className="mt-4 flex items-center justify-end gap-1" onClick={(e) => e.stopPropagation()}>
                  <Button
                    size="sm"
                    variant="soft"
                    icon={<Pencil className="size-3.5" />}
                    onClick={() => setEditing(d)}
                  >
                    Düzenle
                  </Button>
                  <Button
                    size="sm"
                    variant="danger"
                    icon={<Trash2 className="size-3.5" />}
                    onClick={() => setDeleteTarget(d)}
                  >
                    Sil
                  </Button>
                </div>
              </div>
            );
          })}
        </div>
      )}

      <Pagination
        page={page}
        totalPages={list.data?.pages ?? 0}
        totalItems={list.data?.count ?? 0}
        pageSize={PAGE_SIZE}
        onPageChange={setPage}
      />

      {/* Create */}
      <Modal open={createOpen} onClose={() => setCreateOpen(false)} title="Yeni donor">
        <DonorFormView
          onCancel={() => setCreateOpen(false)}
          onSubmit={async (d) => {
            await Donors.create({
              ...d,
              weightKg: Number(d.weightKg),
              birthDate: d.birthDate ? new Date(d.birthDate).toISOString() : undefined,
            });
            toast.success("Donor oluşturuldu");
            invalidate();
            setCreateOpen(false);
          }}
        />
      </Modal>

      {/* Edit */}
      <Modal open={!!editing} onClose={() => setEditing(null)} title="Donor düzenle">
        {editing && (
          <DonorFormView
            initial={editing}
            onCancel={() => setEditing(null)}
            onSubmit={async (d) => {
              await Donors.update({
                id: editing.id,
                ...d,
                weightKg: Number(d.weightKg),
                birthDate: d.birthDate ? new Date(d.birthDate).toISOString() : undefined,
              });
              toast.success("Donor güncellendi");
              qc.setQueryData(["donors"], (old: { items: Donor[] } | undefined) => {
                if (!old) return old;
                return { ...old, items: old.items.map(i => i.id === editing.id ? { ...i, ...d, weightKg: Number(d.weightKg), birthDate: d.birthDate ? new Date(d.birthDate).toISOString() : i.birthDate } : i) };
              });
              invalidate();
              setEditing(null);
            }}
          />
        )}
      </Modal>

      {/* Detail — bağlı hastalar */}
      <Modal
        open={!!detailId}
        onClose={() => setDetailId(null)}
        title="Donor detayı"
        size="lg"
      >
        {detailId && (
          <DonorDetail donorId={detailId} onClose={() => setDetailId(null)} />
        )}
      </Modal>

      {/* Delete */}
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && remove.mutate(deleteTarget.id)}
        loading={remove.isPending}
        description={`"${deleteTarget?.fullName ?? ""}" adlı donoru silmek istediğinize emin misiniz? Bu donora bağlı hasta kayıtları varsa silinemez.`}
      />
    </div>
  );
}

function Mini({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg bg-bg-elevated/60 border border-line/60 px-2.5 py-2">
      <div className="text-[10px] uppercase text-ink-dim tracking-wide">{label}</div>
      <div className="text-sm font-medium mt-0.5">{value}</div>
    </div>
  );
}

export function DonorFormView({
  initial,
  onCancel,
  onSubmit,
}: {
  initial?: Partial<Donor>;
  onCancel: () => void;
  onSubmit: (d: DonorFormValues) => Promise<void>;
}) {
  const { register, handleSubmit, control, watch, reset, formState: { errors, isSubmitting } } =
    useForm<DonorFormValues>({
      defaultValues: {
        fullName: initial?.fullName ?? "",
        weightKg: initial?.weightKg ?? 70,
        bloodGroup: initial?.bloodGroup ?? "",
        relation: initial?.relation ?? "",
        identityNumber: initial?.identityNumber ?? "",
        donorType: initial?.donorType ?? "Related",
        birthDate: initial?.birthDate ? initial.birthDate.slice(0, 10) : "",
      },
    });

  useEffect(() => {
    reset({
      fullName: initial?.fullName ?? "",
      weightKg: initial?.weightKg ?? 70,
      bloodGroup: initial?.bloodGroup ?? "",
      relation: initial?.relation ?? "",
      identityNumber: initial?.identityNumber ?? "",
      donorType: initial?.donorType ?? "Related",
      birthDate: initial?.birthDate ? initial.birthDate.slice(0, 10) : "",
    });
  }, [initial, reset]);

  const donorType = watch("donorType");

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="grid grid-cols-2 gap-4">
      <div className="col-span-2">
        <Input
          label="Ad Soyad"
          placeholder="Ör. Murat Kara"
          {...register("fullName", { required: "Zorunlu" })}
          error={errors.fullName?.message}
        />
      </div>

      <div className="col-span-2">
        <div className="label">Donör tipi</div>
        <Controller
          control={control}
          name="donorType"
          render={({ field }) => (
            <div className="grid grid-cols-2 gap-2">
              <DonorTypeOption
                label="Akraba"
                description="Kardeş, ebeveyn, diğer kan bağı olan donör"
                active={field.value === "Related"}
                onClick={() => field.onChange("Related")}
              />
              <DonorTypeOption
                label="Akraba-dışı"
                description="MUD / havuzdan eşleşmiş donör"
                active={field.value === "Unrelated"}
                onClick={() => field.onChange("Unrelated")}
              />
            </div>
          )}
        />
      </div>

      <Input
        label="Kilo (kg)"
        type="number"
        step="0.1"
        {...register("weightKg", { required: "Zorunlu", min: 1 })}
        error={errors.weightKg?.message}
      />
      <Select label="Kan grubu" {...register("bloodGroup")} options={BLOOD_GROUP_OPTIONS} />

      <Input
        label={donorType === "Related" ? "Yakınlık (ör. Kardeş, Anne)" : "Yakınlık/not (opsiyonel)"}
        placeholder={donorType === "Related" ? "Sibling, Parent..." : "MUD, Unrelated..."}
        {...register("relation")}
      />
      <Input label="Kimlik No (opsiyonel)" {...register("identityNumber")} />
      <Input label="Doğum tarihi" type="date" {...register("birthDate")} />

      <div className="col-span-2 flex justify-end gap-2 mt-1">
        <Button variant="soft" type="button" onClick={onCancel}>
          İptal
        </Button>
        <Button type="submit" loading={isSubmitting}>
          Kaydet
        </Button>
      </div>
    </form>
  );
}

function DonorTypeOption({
  label,
  description,
  active,
  onClick,
}: {
  label: string;
  description: string;
  active: boolean;
  onClick: () => void;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={
        "text-left rounded-xl border px-3 py-2.5 transition " +
        (active
          ? "border-brand-500/60 bg-brand-500/10 shadow-glow"
          : "border-line/60 bg-bg-elevated/40 hover:border-line")
      }
    >
      <div className="text-sm font-semibold flex items-center gap-2">
        <Users className={"size-3.5 " + (active ? "text-brand-400" : "text-ink-dim")} />
        {label}
      </div>
      <div className="text-[11px] text-ink-dim mt-0.5 leading-snug">{description}</div>
    </button>
  );
}

function DonorDetail({ donorId, onClose }: { donorId: string; onClose: () => void }) {
  const donor = useQuery({ queryKey: ["donor", donorId], queryFn: () => Donors.byId(donorId) });
  const patients = useQuery({
    queryKey: ["patients", "all-for-donor-detail"],
    queryFn: () => Patients.list(0, 500),
  });
  const linked = (patients.data?.items ?? []).filter((p) => p.donorId === donorId);

  if (donor.isLoading) return <div className="skeleton h-40" />;
  if (!donor.data) return <p className="text-sm text-ink-muted">Donor bulunamadı.</p>;
  const d = donor.data;

  return (
    <div className="space-y-5">
      <div className="grid grid-cols-2 md:grid-cols-4 gap-2">
        <Mini label="Ad" value={d.fullName} />
        <Mini label="Tip" value={d.donorType === "Unrelated" ? "Akraba-dışı" : "Akraba"} />
        <Mini label="Kilo" value={`${d.weightKg} kg`} />
        <Mini label="Kan grubu" value={d.bloodGroup ?? "—"} />
        <Mini label="Yakınlık" value={d.relation ?? "—"} />
        <Mini label="Kimlik No" value={d.identityNumber ?? "—"} />
        <Mini
          label="Doğum tarihi"
          value={d.birthDate ? formatDate(d.birthDate) : "—"}
        />
      </div>

      <div>
        <CardHeader
          title="Bağlı hastalar"
          subtitle={`${linked.length} hasta bu donor ile eşleşmiş`}
        />
        {linked.length === 0 ? (
          <p className="text-sm text-ink-muted">
            Bu donora henüz bağlı hasta yok.
          </p>
        ) : (
          <ul className="space-y-2">
            {linked.map((p) => (
              <li
                key={p.id}
                className="flex items-center justify-between rounded-xl border border-line/60 bg-bg-elevated/40 px-3 py-2.5"
              >
                <div>
                  <div className="text-sm font-medium">{p.fullName}</div>
                  <div className="text-[11px] text-ink-dim">
                    {p.protocolNo ?? "—"} · {p.weightKg} kg · {p.transplantType}
                  </div>
                </div>
                <a
                  href={`/patients/${p.id}`}
                  className="inline-flex items-center gap-1 text-xs text-brand-400 hover:text-brand-500"
                >
                  <Link2 className="size-3.5" /> Aç
                </a>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="flex justify-end">
        <Button variant="soft" onClick={onClose}>
          Kapat
        </Button>
      </div>
    </div>
  );
}
