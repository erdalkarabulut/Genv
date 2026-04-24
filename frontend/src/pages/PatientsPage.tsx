import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Donors, Patients } from "@/lib/api";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Input, Select } from "@/components/ui/Input";
import { Modal } from "@/components/ui/Modal";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { Link } from "react-router-dom";
import { Plus, Search, UserRound, Pencil, Trash2, HeartHandshake } from "lucide-react";
import { formatDate } from "@/lib/utils";
import { toast } from "sonner";
import type { Patient, TransplantType } from "@/lib/types";
import { EmptyState } from "@/components/ui/EmptyState";
import { DonorFormView } from "./DonorsPage";

interface PatientForm {
  fullName: string;
  weightKg: number;
  bloodGroup?: string;
  transplantType: TransplantType;
  diagnosis?: string;
  protocolNo?: string;
  birthDate?: string;
  donorId?: string;
}

export default function PatientsPage() {
  const qc = useQueryClient();
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<Patient | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Patient | null>(null);
  const [q, setQ] = useState("");
  const [filter, setFilter] = useState<"all" | TransplantType>("all");

  const list = useQuery({ queryKey: ["patients"], queryFn: () => Patients.list(0, 200) });
  const donors = useQuery({ queryKey: ["donors"], queryFn: () => Donors.list(0, 500) });

  const filtered = useMemo(() => {
    const items = list.data?.items ?? [];
    return items.filter((p) => {
      const matchesQ =
        !q ||
        p.fullName.toLowerCase().includes(q.toLowerCase()) ||
        p.protocolNo?.toLowerCase().includes(q.toLowerCase());
      const matchesF = filter === "all" || p.transplantType === filter;
      return matchesQ && matchesF;
    });
  }, [list.data, q, filter]);

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ["patients"] });
    qc.invalidateQueries({ queryKey: ["dashboard"] });
  };

  const remove = useMutation({
    mutationFn: (id: string) => Patients.remove(id),
    onSuccess: () => {
      toast.success("Hasta silindi");
      setDeleteTarget(null);
      invalidate();
    },
  });

  return (
    <div className="space-y-6">
      <header className="flex items-end justify-between gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Hastalar</h1>
          <p className="text-sm text-ink-muted mt-1">
            Otolog ve allogeneik hasta kayıtları, aferez planlarına buradan ulaşın.
          </p>
        </div>
        <Button icon={<Plus className="size-4" />} onClick={() => setCreateOpen(true)}>
          Yeni hasta
        </Button>
      </header>

      <Card>
        <div className="flex items-center gap-3 flex-wrap">
          <div className="relative flex-1 min-w-[240px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-ink-dim" />
            <input
              className="input pl-9"
              placeholder="Ad veya protokol no ile ara…"
              value={q}
              onChange={(e) => setQ(e.target.value)}
            />
          </div>
          <Select
            value={filter}
            onChange={(e) => setFilter(e.target.value as any)}
            options={[
              { value: "all", label: "Tüm transplant tipleri" },
              { value: "Autologous", label: "Otolog" },
              { value: "Allogeneic", label: "Allogeneik" },
            ]}
            className="min-w-[200px]"
          />
        </div>
      </Card>

      {list.isLoading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="card h-32 skeleton" />
          ))}
        </div>
      ) : filtered.length === 0 ? (
        <EmptyState
          icon={<UserRound className="size-10" />}
          title="Henüz kayıtlı hasta yok"
          description="İlk hastanızı oluşturun ve aferez planlamasına başlayın."
          action={
            <Button icon={<Plus className="size-4" />} onClick={() => setCreateOpen(true)}>
              Yeni hasta
            </Button>
          }
        />
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {filtered.map((p) => (
            <PatientCard
              key={p.id}
              p={p}
              onEdit={() => setEditing(p)}
              onDelete={() => setDeleteTarget(p)}
            />
          ))}
        </div>
      )}

      <Modal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        title="Yeni hasta"
        description="Aferez planlaması için temel bilgiler"
      >
        <PatientFormView
          donors={donors.data?.items ?? []}
          onCancel={() => setCreateOpen(false)}
          onSubmit={async (d) => {
            await Patients.create({
              ...d,
              weightKg: Number(d.weightKg),
              birthDate: d.birthDate ? new Date(d.birthDate).toISOString() : undefined,
              donorId: d.donorId || undefined,
            });
            toast.success("Hasta oluşturuldu");
            invalidate();
            setCreateOpen(false);
          }}
        />
      </Modal>

      <Modal
        open={!!editing}
        onClose={() => setEditing(null)}
        title="Hastayı düzenle"
      >
        {editing && (
          <PatientFormView
            initial={editing}
            donors={donors.data?.items ?? []}
            onCancel={() => setEditing(null)}
            onSubmit={async (d) => {
              await Patients.update({
                id: editing.id,
                ...d,
                weightKg: Number(d.weightKg),
                birthDate: d.birthDate ? new Date(d.birthDate).toISOString() : undefined,
                donorId: d.donorId || undefined,
              });
              toast.success("Hasta güncellendi");
              invalidate();
              qc.invalidateQueries({ queryKey: ["patient", editing.id] });
              setEditing(null);
            }}
          />
        )}
      </Modal>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && remove.mutate(deleteTarget.id)}
        loading={remove.isPending}
        description={`"${deleteTarget?.fullName ?? ""}" adlı hastayı silmek istediğinize emin misiniz? Bağlı seans ve torba kayıtları varsa işlem başarısız olabilir.`}
      />
    </div>
  );
}

function PatientCard({
  p,
  onEdit,
  onDelete,
}: {
  p: Patient;
  onEdit: () => void;
  onDelete: () => void;
}) {
  return (
    <div className="card group p-5 transition hover:border-brand-500/40 hover:shadow-glow">
      <Link to={`/patients/${p.id}`} className="block">
        <div className="flex items-start justify-between gap-3">
          <div className="flex items-center gap-3 min-w-0">
            <div className="grid place-items-center size-11 rounded-xl bg-gradient-to-br from-brand-500/30 to-emerald-400/20 text-brand-400 font-semibold">
              {p.fullName?.slice(0, 2).toUpperCase()}
            </div>
            <div className="min-w-0">
              <div className="text-base font-semibold truncate">{p.fullName}</div>
              <div className="text-xs text-ink-muted truncate">
                {p.protocolNo ? `Protokol ${p.protocolNo}` : "Protokol —"}
              </div>
            </div>
          </div>
          <Badge tone={p.transplantType === "Autologous" ? "sky" : "brand"} dot>
            {p.transplantType === "Autologous" ? "Otolog" : "Allogeneik"}
          </Badge>
        </div>

        <div className="mt-4 grid grid-cols-3 gap-2">
          <Mini label="Kilo" value={`${p.weightKg} kg`} />
          <Mini label="Kan grubu" value={p.bloodGroup ?? "—"} />
          <Mini label="Kayıt" value={formatDate(p.createdDate)} />
        </div>

        {p.diagnosis && (
          <div className="mt-4 rounded-lg bg-bg-elevated/50 border border-line/60 px-3 py-2 text-xs text-ink-muted line-clamp-2">
            {p.diagnosis}
          </div>
        )}
      </Link>

      <div className="mt-4 flex items-center justify-between gap-2">
        <Link
          to={`/patients/${p.id}`}
          className="text-xs text-brand-400 group-hover:text-brand-500 transition"
        >
          Aferez planını aç &rarr;
        </Link>
        <div className="flex items-center gap-1">
          <Button
            size="sm"
            variant="soft"
            icon={<Pencil className="size-3.5" />}
            onClick={onEdit}
          >
            Düzenle
          </Button>
          <Button
            size="sm"
            variant="danger"
            icon={<Trash2 className="size-3.5" />}
            onClick={onDelete}
          >
            Sil
          </Button>
        </div>
      </div>
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

export function PatientFormView({
  initial,
  donors,
  onCancel,
  onSubmit,
}: {
  initial?: Partial<Patient>;
  donors: { id: string; fullName: string }[];
  onCancel: () => void;
  onSubmit: (d: PatientForm) => Promise<void>;
}) {
  const qc = useQueryClient();
  const [donorCreateOpen, setDonorCreateOpen] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<PatientForm>({
    defaultValues: {
      fullName: initial?.fullName ?? "",
      weightKg: initial?.weightKg ?? 70,
      bloodGroup: initial?.bloodGroup ?? "",
      transplantType: initial?.transplantType ?? "Autologous",
      diagnosis: initial?.diagnosis ?? "",
      protocolNo: initial?.protocolNo ?? "",
      birthDate: initial?.birthDate ? initial.birthDate.slice(0, 10) : "",
      donorId: initial?.donorId ?? "",
    },
  });

  const type = watch("transplantType");
  const donorId = watch("donorId");

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="grid grid-cols-2 gap-4">
      <div className="col-span-2">
        <Input
          label="Ad Soyad"
          placeholder="Ör. Ayşe Yılmaz"
          {...register("fullName", { required: "Zorunlu" })}
          error={errors.fullName?.message}
        />
      </div>
      <Input
        label="Kilo (kg)"
        type="number"
        step="0.1"
        {...register("weightKg", { required: "Zorunlu", min: 1 })}
        error={errors.weightKg?.message}
      />
      <Select
        label="Transplant Tipi"
        {...register("transplantType")}
        options={[
          { value: "Autologous", label: "Otolog (hasta kendi) — 4 gün" },
          { value: "Allogeneic", label: "Allogeneik (donör) — 2 gün" },
        ]}
      />
      <Input label="Kan grubu" placeholder="A+, B-, …" {...register("bloodGroup")} />
      <Input label="Protokol no" {...register("protocolNo")} />
      <div className="col-span-2">
        <Input label="Tanı" placeholder="Multipl Miyelom, AML, …" {...register("diagnosis")} />
      </div>
      <Input label="Doğum tarihi" type="date" {...register("birthDate")} />
      {type === "Allogeneic" && (
        <div>
          <label className="label">Donor</label>
          <div className="flex items-stretch gap-2">
            <select
              className="input pr-8 flex-1"
              {...register("donorId")}
            >
              <option value="" className="bg-bg-card">— Seçilmedi —</option>
              {donors.map((d) => (
                <option key={d.id} value={d.id} className="bg-bg-card">
                  {d.fullName}
                </option>
              ))}
            </select>
            <button
              type="button"
              onClick={() => setDonorCreateOpen(true)}
              title="Yeni donör oluştur"
              className="shrink-0 inline-flex items-center justify-center rounded-xl border border-brand-500/40 bg-brand-500/10 px-3 text-brand-400 hover:bg-brand-500/20 hover:border-brand-500/60 transition"
            >
              <Plus className="size-4" />
            </button>
          </div>
          {!donorId && (
            <p className="mt-1 text-[11px] text-ink-dim">
              Kayıtlı bir donör seçin ya da{" "}
              <button
                type="button"
                onClick={() => setDonorCreateOpen(true)}
                className="text-brand-400 hover:text-brand-500 inline-flex items-center gap-0.5"
              >
                <HeartHandshake className="size-3" /> yeni donör oluşturun
              </button>
              .
            </p>
          )}
        </div>
      )}

      <div className="col-span-2 flex justify-end gap-2 mt-2">
        <Button variant="soft" type="button" onClick={onCancel}>
          İptal
        </Button>
        <Button type="submit" loading={isSubmitting}>
          Kaydet
        </Button>
      </div>

      <Modal
        open={donorCreateOpen}
        onClose={() => setDonorCreateOpen(false)}
        title="Yeni donör"
        description="Oluşturulan donör otomatik olarak bu hastaya atanır"
      >
        <DonorFormView
          onCancel={() => setDonorCreateOpen(false)}
          onSubmit={async (d) => {
            const created = await Donors.create({
              ...d,
              weightKg: Number(d.weightKg),
              birthDate: d.birthDate ? new Date(d.birthDate).toISOString() : undefined,
            });
            await qc.invalidateQueries({ queryKey: ["donors"] });
            setValue("donorId", created.id, { shouldDirty: true, shouldValidate: true });
            toast.success(`Donör "${created.fullName}" oluşturuldu ve bu hastaya atandı`);
            setDonorCreateOpen(false);
          }}
        />
      </Modal>
    </form>
  );
}
