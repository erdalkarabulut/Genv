import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ClinicalSettingsApi, DliProducts, Donors, Patients, Sessions } from "@/lib/api";
import { Card, CardHeader } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Input, Select } from "@/components/ui/Input";
import { Modal } from "@/components/ui/Modal";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { EmptyState } from "@/components/ui/EmptyState";
import { useEffect, useMemo, useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { Plus, Pencil, Trash2, FlaskConical, Calculator, TriangleAlert } from "lucide-react";
import { toast } from "sonner";
import { formatDate } from "@/lib/utils";
import type { DliProduct, Patient } from "@/lib/types";
import { Pagination } from "@/components/ui/Pagination";

interface DliForm {
  patientId: string;
  sessionId?: string;
  donorId?: string;
  date: string;
  volumeMl: number;
  wbc?: number;
  lymphocytePercent?: number;
  cd3Percent?: number;
  cd3PerKgOverride?: number;
  notes?: string;
}

export default function DliPage() {
  const qc = useQueryClient();
  const patients = useQuery({ queryKey: ["patients"], queryFn: () => Patients.list(0, 500) });
  const donors = useQuery({ queryKey: ["donors"], queryFn: () => Donors.list(0, 500) });
  const sessions = useQuery({ queryKey: ["sessions"], queryFn: () => Sessions.list(0, 500) });
  const clinical = useQuery({
    queryKey: ["clinical-settings"],
    queryFn: () => ClinicalSettingsApi.get(),
    staleTime: 0,
  });
  const dliDivisor = clinical.data?.dliCd3CalculationDivisor ?? 10000;
  const highDoseThreshold = clinical.data?.dliHighDoseCd3PerKgThreshold ?? 10;

  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<DliProduct | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<DliProduct | null>(null);
  const [page, setPage] = useState(0);

  const PAGE_SIZE = 10;

  const list = useQuery({
    queryKey: ["dli-products", page],
    queryFn: () => DliProducts.byDynamic({ sort: [{ field: "date", dir: "desc" }] }, page, PAGE_SIZE),
  });

  const patientById = useMemo(() => {
    const map = new Map<string, Patient>();
    (patients.data?.items ?? []).forEach((p) => map.set(p.id, p));
    return map;
  }, [patients.data]);

  const dliItems = list.data?.items ?? [];
  const paginated = dliItems;

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ["dli-products"] });
  };

  const remove = useMutation({
    mutationFn: (id: string) => DliProducts.remove(id),
    onSuccess: () => {
      toast.success("DLI kaydı silindi");
      setDeleteTarget(null);
      invalidate();
    },
  });

  return (
    <div className="space-y-6">
      <header className="flex items-end justify-between gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">DLI Ürünleri</h1>
          <p className="text-sm text-ink-muted mt-1">
            Donör Lenfosit İnfüzyonu — WBC × Lenfosit% × CD3% ile toplam CD3 hesaplanır.
          </p>
        </div>
        <Button icon={<Plus className="size-4" />} onClick={() => setCreateOpen(true)}>
          Yeni DLI kaydı
        </Button>
      </header>

      <Card className="bg-gradient-to-br from-brand-500/5 to-transparent">
        <div className="flex items-start gap-3">
          <div className="grid place-items-center size-10 rounded-xl bg-brand-500/15 text-brand-400">
            <Calculator className="size-5" />
          </div>
          <div className="flex-1">
            <div className="text-sm font-semibold">Formül</div>
            <div className="text-xs text-ink-muted mt-0.5">
              Toplam CD3 = (Hacim ml × 1000) × WBC × (Lenfosit% / 100) × (CD3% / 100) /{" "}
              <b>{fmtNum(dliDivisor)}</b>
              <span className="mx-1">→</span>
              hasta kilosuna bölündüğünde <b>CD3/kg × 10⁶</b>. Yüksek doz eşiği:{" "}
              <b>{fmtNum(highDoseThreshold)}</b> (API / Klinik eşikleri).
            </div>
          </div>
        </div>
      </Card>

      {list.isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="card h-32 skeleton" />
          ))}
        </div>
      ) : (list.data?.items ?? []).length === 0 ? (
        <EmptyState
          icon={<FlaskConical className="size-10" />}
          title="DLI kaydı yok"
          description="İlk DLI ürününü oluşturup canlı CD3 hesabını deneyin."
          action={
            <Button icon={<Plus className="size-4" />} onClick={() => setCreateOpen(true)}>
              Yeni DLI kaydı
            </Button>
          }
        />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {paginated.map((d) => (
            <DliCard
              key={d.id}
              d={d}
              patient={patientById.get(d.patientId) ?? null}
              highDoseThreshold={highDoseThreshold}
              onEdit={() => setEditing(d)}
              onDelete={() => setDeleteTarget(d)}
            />
          ))}
        </div>
      )}

      <Pagination
        page={page}
        totalPages={list.data?.pages ?? 0}
        totalItems={list.data?.count ?? 0}
        pageSize={PAGE_SIZE}
        onPageChange={setPage}
      />

      <Modal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        title="Yeni DLI kaydı"
        description="Hacim × WBC × Lenfosit% × CD3% canlı hesap"
        size="lg"
      >
        <DliFormView
          patients={patients.data?.items ?? []}
          donors={donors.data?.items ?? []}
          sessions={sessions.data?.items ?? []}
          dliDivisor={dliDivisor}
          onCancel={() => setCreateOpen(false)}
          onSubmit={async (d) => {
            await DliProducts.create({
              patientId: d.patientId,
              sessionId: d.sessionId || undefined,
              donorId: d.donorId || undefined,
              date: d.date ? new Date(d.date).toISOString() : new Date().toISOString(),
              volumeMl: Number(d.volumeMl),
              wbc: d.wbc !== undefined && d.wbc !== null && `${d.wbc}` !== "" ? Number(d.wbc) : undefined,
              lymphocytePercent:
                d.lymphocytePercent !== undefined && `${d.lymphocytePercent}` !== ""
                  ? Number(d.lymphocytePercent)
                  : undefined,
              cd3Percent:
                d.cd3Percent !== undefined && `${d.cd3Percent}` !== "" ? Number(d.cd3Percent) : undefined,
              cd3PerKgOverride:
                d.cd3PerKgOverride !== undefined && `${d.cd3PerKgOverride}` !== ""
                  ? Number(d.cd3PerKgOverride)
                  : undefined,
              notes: d.notes || undefined,
            });
            toast.success("DLI kaydı oluşturuldu");
            invalidate();
            setCreateOpen(false);
          }}
        />
      </Modal>

      <Modal
        open={!!editing}
        onClose={() => setEditing(null)}
        title="DLI kaydı düzenle"
        size="lg"
      >
        {editing && (
          <DliFormView
            initial={editing}
            patients={patients.data?.items ?? []}
            donors={donors.data?.items ?? []}
            sessions={sessions.data?.items ?? []}
            dliDivisor={dliDivisor}
            onCancel={() => setEditing(null)}
            onSubmit={async (d) => {
              await DliProducts.update({
                id: editing.id,
                patientId: d.patientId,
                sessionId: d.sessionId || undefined,
                donorId: d.donorId || undefined,
                date: d.date ? new Date(d.date).toISOString() : editing.date,
                volumeMl: Number(d.volumeMl),
                wbc: d.wbc !== undefined && `${d.wbc}` !== "" ? Number(d.wbc) : undefined,
                lymphocytePercent:
                  d.lymphocytePercent !== undefined && `${d.lymphocytePercent}` !== ""
                    ? Number(d.lymphocytePercent)
                    : undefined,
                cd3Percent:
                  d.cd3Percent !== undefined && `${d.cd3Percent}` !== "" ? Number(d.cd3Percent) : undefined,
                cd3PerKgOverride:
                  d.cd3PerKgOverride !== undefined && `${d.cd3PerKgOverride}` !== ""
                    ? Number(d.cd3PerKgOverride)
                    : undefined,
                notes: d.notes || undefined,
              });
              toast.success("DLI kaydı güncellendi");
              invalidate();
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
        description="Bu DLI kaydını silmek istediğinize emin misiniz?"
      />
    </div>
  );
}

function DliCard({
  d,
  patient,
  highDoseThreshold,
  onEdit,
  onDelete,
}: {
  d: DliProduct;
  patient: Patient | null;
  highDoseThreshold: number;
  onEdit: () => void;
  onDelete: () => void;
}) {
  const highDose = d.cd3PerKg > highDoseThreshold;
  return (
    <div className="card p-5 group">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div className="text-sm font-semibold truncate">
            {patient?.fullName ?? "Bilinmeyen hasta"}
          </div>
          <div className="text-[11px] text-ink-dim mt-0.5">
            {formatDate(d.date)}
            {patient?.protocolNo && <span className="ml-1">· {patient.protocolNo}</span>}
          </div>
        </div>
        <Badge tone={highDose ? "rose" : "brand"} dot>
          {highDose ? "Yüksek doz" : "Düşük doz"}
        </Badge>
      </div>

      <div className="mt-4 grid grid-cols-3 gap-2">
        <Mini label="Hacim" value={`${d.volumeMl} ml`} />
        <Mini label="WBC" value={d.wbc ? `${d.wbc}` : "—"} />
        <Mini label="Lenf%" value={d.lymphocytePercent ? `${d.lymphocytePercent}%` : "—"} />
        <Mini label="CD3%" value={d.cd3Percent ? `${d.cd3Percent}%` : "—"} />
        <Mini label="Toplam CD3" value={`${fmtNum(d.totalCd3)} ×10⁶`} />
        <Mini label="CD3/kg" value={`${fmtNum(d.cd3PerKg)} ×10⁶`} />
      </div>

      {d.notes && (
        <div className="mt-3 rounded-lg bg-bg-elevated/40 border border-line/60 px-3 py-2 text-xs text-ink-muted line-clamp-2">
          {d.notes}
        </div>
      )}

      <div className="mt-4 flex justify-end gap-1">
        <Button size="sm" variant="soft" icon={<Pencil className="size-3.5" />} onClick={onEdit}>
          Düzenle
        </Button>
        <Button size="sm" variant="danger" icon={<Trash2 className="size-3.5" />} onClick={onDelete}>
          Sil
        </Button>
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

function fmtNum(v: number | null | undefined) {
  if (v === null || v === undefined || !isFinite(v)) return "—";
  if (v === 0) return "0";
  if (Math.abs(v) >= 100) return v.toFixed(1);
  if (Math.abs(v) >= 10) return v.toFixed(2);
  return v.toFixed(3);
}

function DliFormView({
  initial,
  patients,
  donors,
  sessions,
  dliDivisor,
  onCancel,
  onSubmit,
}: {
  initial?: Partial<DliProduct>;
  patients: Patient[];
  donors: { id: string; fullName: string }[];
  sessions: { id: string; patientId: string; day: number; date: string }[];
  dliDivisor: number;
  onCancel: () => void;
  onSubmit: (d: DliForm) => Promise<void>;
}) {
  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<DliForm>({
    defaultValues: {
      patientId: initial?.patientId ?? "",
      sessionId: initial?.sessionId ?? "",
      donorId: initial?.donorId ?? "",
      date: initial?.date ? initial.date.slice(0, 10) : new Date().toISOString().slice(0, 10),
      volumeMl: initial?.volumeMl ?? 50,
      wbc: initial?.wbc ?? undefined,
      lymphocytePercent: initial?.lymphocytePercent ?? undefined,
      cd3Percent: initial?.cd3Percent ?? undefined,
      cd3PerKgOverride: undefined,
      notes: initial?.notes ?? "",
    },
  });

  useEffect(() => {
    reset({
      patientId: initial?.patientId ?? "",
      sessionId: initial?.sessionId ?? "",
      donorId: initial?.donorId ?? "",
      date: initial?.date ? initial.date.slice(0, 10) : new Date().toISOString().slice(0, 10),
      volumeMl: initial?.volumeMl ?? 50,
      wbc: initial?.wbc ?? undefined,
      lymphocytePercent: initial?.lymphocytePercent ?? undefined,
      cd3Percent: initial?.cd3Percent ?? undefined,
      cd3PerKgOverride: undefined,
      notes: initial?.notes ?? "",
    });
  }, [initial, reset]);

  const values = useWatch({ control }) as DliForm;
  const selectedPatient = patients.find((p) => p.id === values.patientId);

  // Canlı hesap
  const volumeUl = Number(values.volumeMl || 0) * 1000;
  const wbc = Number(values.wbc || 0);
  const lymph = Number(values.lymphocytePercent || 0) / 100;
  const cd3 = Number(values.cd3Percent || 0) / 100;
  const totalCd3 = (volumeUl * wbc * lymph * cd3) / dliDivisor;
  const weight = selectedPatient?.weightKg ?? 0;
  const autoCd3PerKg = weight > 0 ? totalCd3 / weight : 0;
  const hasOverride =
    values.cd3PerKgOverride !== undefined &&
    values.cd3PerKgOverride !== null &&
    `${values.cd3PerKgOverride}` !== "";
  const effectiveCd3PerKg = hasOverride ? Number(values.cd3PerKgOverride) : autoCd3PerKg;
  const isHighDose = effectiveCd3PerKg > 10;
  const computable = volumeUl > 0 && wbc > 0 && lymph > 0 && cd3 > 0;

  const patientSessions = sessions.filter((s) => s.patientId === values.patientId);

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="grid grid-cols-2 gap-4">
      <div className="col-span-2">
        <Select
          label="Hasta"
          {...register("patientId", { required: "Zorunlu" })}
          options={[
            { value: "", label: "— Seçilmedi —" },
            ...patients.map((p) => ({
              value: p.id,
              label: `${p.fullName}${p.protocolNo ? ` · ${p.protocolNo}` : ""} · ${p.weightKg} kg · ${
                p.transplantType === "Autologous" ? "Otolog" : "Allojenik"
              }`,
            })),
          ]}
        />
        {errors.patientId && (
          <p className="mt-1 text-[11px] text-accent-rose">{errors.patientId.message}</p>
        )}
      </div>

      <Select
        label="Donör (opsiyonel)"
        {...register("donorId")}
        options={[
          { value: "", label: "— Seçilmedi —" },
          ...donors.map((d) => ({ value: d.id, label: d.fullName })),
        ]}
      />
      <Select
        label="Aferez seansı (opsiyonel)"
        {...register("sessionId")}
        options={[
          { value: "", label: "— Seçilmedi —" },
          ...patientSessions.map((s) => ({
            value: s.id,
            label: `Gün ${s.day} · ${formatDate(s.date)}`,
          })),
        ]}
      />

      <Input label="Tarih" type="date" {...register("date")} />
      <Input
        label="Hacim (ml)"
        type="number"
        step="0.1"
        {...register("volumeMl", { required: "Zorunlu", min: 0.1 })}
        error={errors.volumeMl?.message}
      />

      <Input
        label="WBC (×10⁶/ml)"
        type="number"
        step="0.01"
        placeholder="Ör. 80"
        {...register("wbc")}
      />
      <Input
        label="Lenfosit %"
        type="number"
        step="0.1"
        placeholder="0-100"
        {...register("lymphocytePercent")}
      />
      <Input
        label="CD3 %"
        type="number"
        step="0.1"
        placeholder="0-100"
        {...register("cd3Percent")}
      />
      <Input
        label="CD3/kg manuel (opsiyonel)"
        type="number"
        step="0.01"
        placeholder="Hesabı elle gir"
        hint="Boş bırakılırsa otomatik hesaplanır"
        {...register("cd3PerKgOverride")}
      />

      <div className="col-span-2">
        <Input label="Not" placeholder="DLI notu" {...register("notes")} />
      </div>

      {/* Canlı hesap panosu */}
      <div className="col-span-2">
        <div
          className={
            "rounded-xl border p-4 transition " +
            (computable || hasOverride
              ? isHighDose
                ? "border-rose-500/40 bg-rose-500/5"
                : "border-brand-500/40 bg-brand-500/5"
              : "border-line/60 bg-bg-elevated/40")
          }
        >
          <div className="flex items-center gap-2 mb-3">
            <Calculator className="size-4 text-brand-400" />
            <div className="text-sm font-semibold">Canlı hesap</div>
            {isHighDose && (
              <Badge tone="rose" dot>
                <span className="inline-flex items-center gap-1">
                  <TriangleAlert className="size-3" />
                  Yüksek doz (&gt;10 ×10⁶/kg)
                </span>
              </Badge>
            )}
            {hasOverride && <Badge tone="amber">Manuel override aktif</Badge>}
          </div>

          <div className="grid grid-cols-2 md:grid-cols-4 gap-2">
            <CalcCell label="Hacim (µL)" value={`${fmtNum(volumeUl)}`} />
            <CalcCell
              label="WBC × Lenf% × CD3%"
              value={wbc && lymph && cd3 ? `${fmtNum(wbc * lymph * cd3)}` : "—"}
            />
            <CalcCell label="Toplam CD3 (×10⁶)" value={computable ? fmtNum(totalCd3) : "—"} highlight />
            <CalcCell
              label="CD3/kg (×10⁶)"
              value={
                hasOverride
                  ? fmtNum(effectiveCd3PerKg)
                  : computable && weight > 0
                    ? fmtNum(autoCd3PerKg)
                    : "—"
              }
              highlight
              warn={isHighDose}
            />
          </div>

          {selectedPatient ? (
            <div className="mt-3 text-[11px] text-ink-dim">
              Hasta kilosu: <b className="text-ink">{selectedPatient.weightKg} kg</b>
              {" · "}
              {selectedPatient.transplantType === "Autologous" ? "Otolog" : "Allojenik"}
            </div>
          ) : (
            <div className="mt-3 text-[11px] text-accent-amber">
              Hasta seçin — CD3/kg hesabı için kilo bilgisi gerekiyor.
            </div>
          )}
        </div>
      </div>

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

function CalcCell({
  label,
  value,
  highlight,
  warn,
}: {
  label: string;
  value: string;
  highlight?: boolean;
  warn?: boolean;
}) {
  return (
    <div
      className={
        "rounded-lg border px-3 py-2 " +
        (warn
          ? "border-rose-500/40 bg-rose-500/10"
          : highlight
            ? "border-brand-500/40 bg-brand-500/10"
            : "border-line/60 bg-bg-elevated/40")
      }
    >
      <div className="text-[10px] uppercase tracking-wide text-ink-dim">{label}</div>
      <div
        className={
          "text-sm font-semibold mt-0.5 " +
          (warn ? "text-accent-rose" : highlight ? "text-brand-400" : "text-ink")
        }
      >
        {value}
      </div>
    </div>
  );
}
