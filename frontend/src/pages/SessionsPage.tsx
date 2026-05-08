import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Patients, Sessions } from "@/lib/api";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Input, Select } from "@/components/ui/Input";
import { Modal } from "@/components/ui/Modal";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { EmptyState } from "@/components/ui/EmptyState";
import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { Link } from "react-router-dom";
import { Pencil, Trash2, Calculator, Beaker, Search } from "lucide-react";
import { toast } from "sonner";
import { formatDate, formatNumber } from "@/lib/utils";
import type { CollectionSession, Patient } from "@/lib/types";
import { Pagination } from "@/components/ui/Pagination";
import { useDebounce } from "@/lib/useDebounce";

export default function SessionsPage() {
  const qc = useQueryClient();

  const [q, setQ] = useState("");
  const [patientFilter, setPatientFilter] = useState<string>("all");
  const [editing, setEditing] = useState<CollectionSession | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<CollectionSession | null>(null);
  const [page, setPage] = useState(0);
  const debouncedQ = useDebounce(q, 300);

  useEffect(() => { setPage(0); }, [debouncedQ, patientFilter]);

  const PAGE_SIZE = 10;

  const buildQuery = () => {
    const sort = [{ field: "date", dir: "desc" as const }];
    const dayNum = debouncedQ.trim() ? parseInt(debouncedQ, 10) : NaN;
    const dayFilter = !isNaN(dayNum)
      ? { field: "day", operator: "eq" as const, value: String(dayNum) }
      : null;
    const patientIdFilter = patientFilter !== "all"
      ? { field: "patientId", operator: "eq" as const, value: patientFilter }
      : null;
    const active = [patientIdFilter, dayFilter].filter(Boolean) as any[];
    if (active.length === 0) return { sort };
    let f = active[active.length - 1];
    for (let i = active.length - 2; i >= 0; i--) {
      f = { ...active[i], logic: "and" as const, filters: [f] };
    }
    return { filter: f, sort };
  };

  const sessions = useQuery({
    queryKey: ["sessions", page, PAGE_SIZE, debouncedQ, patientFilter],
    queryFn: () => Sessions.byDynamic(buildQuery(), page, PAGE_SIZE),
  });
  const patients = useQuery({ queryKey: ["patients", "for-sessions"], queryFn: () => Patients.list(0, 500) });

  const pMap = useMemo(() => {
    const m = new Map<string, Patient>();
    (patients.data?.items ?? []).forEach((p) => m.set(p.id, p));
    return m;
  }, [patients.data]);

  const paginated = sessions.data?.items ?? [];

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ["sessions"] });
    qc.invalidateQueries({ queryKey: ["apheresis-plan"] });
    qc.invalidateQueries({ queryKey: ["dashboard"] });
    qc.invalidateQueries({ queryKey: ["patient"] });
  };

  const remove = useMutation({
    mutationFn: (id: string) => Sessions.remove(id),
    onSuccess: () => {
      toast.success("Aferez seansı silindi");
      setDeleteTarget(null);
      invalidate();
    },
  });

  const calculate = useMutation({
    mutationFn: (id: string) => Sessions.calculate(id),
    onSuccess: (r) => {
      toast.success(`Yeniden hesaplandı · CD34 ${formatNumber(r.cd34PerKg, 2)} / CD3 ${formatNumber(r.cd3PerKg, 2)}`);
      invalidate();
    },
  });

  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-2xl font-semibold tracking-tight">Aferez seansları</h1>
        <p className="text-sm text-ink-muted mt-1">
          Tüm hastaların pre/post-procedure kayıtları. Düzenle, sil veya yeniden hesapla.
        </p>
      </header>

      <Card>
        <div className="flex items-center gap-3 flex-wrap">
          <div className="relative flex-1 min-w-[240px]">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-ink-dim" />
            <input
              className="input pl-9"
              placeholder="Gün numarası ile ara…"
              value={q}
              onChange={(e) => setQ(e.target.value)}
            />
          </div>
          <Select
            value={patientFilter}
            onChange={(e) => setPatientFilter(e.target.value)}
            options={[
              { value: "all", label: "Tüm hastalar" },
              ...(patients.data?.items ?? []).map((p) => ({
                value: p.id,
                label: p.fullName,
              })),
            ]}
            className="min-w-[220px]"
          />
        </div>
      </Card>

      {sessions.isLoading ? (
        <div className="card h-40 skeleton" />
      ) : paginated.length === 0 ? (
        <EmptyState
          icon={<Beaker className="size-10" />}
          title="Seans yok"
          description="Seçili filtre için aferez seansı bulunamadı. Yeni seans eklemek için hasta detayına gidin."
        />
      ) : (
        <Card className="!p-0 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="text-xs text-ink-dim uppercase tracking-wide">
                <tr className="border-b border-line/60">
                  <Th>Hasta</Th>
                  <Th>Gün</Th>
                  <Th>Tarih</Th>
                  <Th>Hacim</Th>
                  <Th>CD34/kg</Th>
                  <Th>CD3/kg</Th>
                  <Th>PK (pre)</Th>
                  <Th>{""}</Th>
                </tr>
              </thead>
              <tbody>
                {paginated.length === 0 ? null : paginated.map((s) => {
                  const p = pMap.get(s.patientId);
                  return (
                    <tr
                      key={s.id}
                      className="border-b border-line/40 hover:bg-bg-elevated/40 transition"
                    >
                      <td className="px-4 py-3">
                        {p ? (
                          <Link
                            to={`/patients/${p.id}`}
                            className="text-sm font-medium hover:text-brand-400"
                          >
                            {p.fullName}
                          </Link>
                        ) : (
                          <span className="text-ink-dim">—</span>
                        )}
                        {p && (
                          <div className="text-[11px] text-ink-dim">
                            {p.protocolNo ?? "—"} · {p.transplantType}
                          </div>
                        )}
                      </td>
                      <td className="px-4 py-3">
                        <Badge>Gün {s.day}</Badge>
                      </td>
                      <td className="px-4 py-3 text-ink-muted">{formatDate(s.date)}</td>
                      <td className="px-4 py-3 text-ink-muted">{s.volumeMl} ml</td>
                      <td className="px-4 py-3 font-medium">
                        {formatNumber(s.cd34PerKg, 2)}
                      </td>
                      <td className="px-4 py-3 font-medium">
                        {formatNumber(s.cd3PerKg, 2)}
                      </td>
                      <td className="px-4 py-3 text-[11px] text-ink-dim">
                        WBC {s.wbcPre ?? "—"} · HGB {s.hgb ?? "—"} · HCT {s.hct ?? "—"} · PLT{" "}
                        {s.plt ?? "—"}
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex justify-end gap-1">
                          <Button
                            size="sm"
                            variant="soft"
                            icon={<Calculator className="size-3.5" />}
                            onClick={() => calculate.mutate(s.id)}
                            loading={calculate.isPending && calculate.variables === s.id}
                            title="Yeniden hesapla"
                          >
                            Hesapla
                          </Button>
                          <Button
                            size="sm"
                            variant="soft"
                            icon={<Pencil className="size-3.5" />}
                            onClick={() => setEditing(s)}
                          >
                            Düzenle
                          </Button>
                          <Button
                            size="sm"
                            variant="danger"
                            icon={<Trash2 className="size-3.5" />}
                            onClick={() => setDeleteTarget(s)}
                          >
                            Sil
                          </Button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </Card>
      )}

      <Pagination
        page={page}
        totalPages={sessions.data?.pages ?? 0}
        totalItems={sessions.data?.count ?? 0}
        pageSize={PAGE_SIZE}
        onPageChange={setPage}
      />

      <Modal
        open={!!editing}
        onClose={() => setEditing(null)}
        title="Aferez seansını düzenle"
        description="PK (pre) ve ÜRÜN (post) değerlerini güncelleyebilir, ardından Hesapla ile CD34/CD3/kg değerlerini yeniden üretebilirsiniz."
        size="xl"
      >
        {editing && (
          <SessionEditForm
            session={editing}
            patient={pMap.get(editing.patientId)}
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
        description={`Gün ${deleteTarget?.day ?? "?"} (${
          deleteTarget ? formatDate(deleteTarget.date) : ""
        }) aferez seansını silmek istediğinize emin misiniz?`}
      />
    </div>
  );
}

function Th({ children }: { children: React.ReactNode }) {
  return <th className="text-left px-4 py-3 font-medium">{children}</th>;
}

interface SessionFormVals {
  day: number;
  date: string;
  wbcPre?: number;
  hgb?: number;
  hct?: number;
  plt?: number;
  volumeMl: number;
  wbc: number;
  cd34Percent: number;
  cd45Percent: number;
  cd3Percent: number;
  lymphocytePercent?: number;
  mhs?: number;
  absoluteCellCount: number;
  cd34PerKg: number;
  cd3PerKg: number;
}

function SessionEditForm({
  session,
  patient,
  onCancel,
  onSaved,
}: {
  session: CollectionSession;
  patient?: Patient;
  onCancel: () => void;
  onSaved: () => void;
}) {
  const qc = useQueryClient();
  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<SessionFormVals>({
    defaultValues: {
      day: session.day,
      date: session.date.slice(0, 10),
      wbcPre: session.wbcPre ?? undefined,
      hgb: session.hgb ?? undefined,
      hct: session.hct ?? undefined,
      plt: session.plt ?? undefined,
      volumeMl: session.volumeMl,
      wbc: session.wbc,
      cd34Percent: session.cd34Percent,
      cd45Percent: session.cd45Percent,
      cd3Percent: session.cd3Percent,
      lymphocytePercent: session.lymphocytePercent ?? undefined,
      mhs: session.mhs ?? undefined,
      absoluteCellCount: session.absoluteCellCount,
      cd34PerKg: session.cd34PerKg,
      cd3PerKg: session.cd3PerKg,
    },
  });

  // Modal açıkken farklı seans düzenlemeye geçilirse formu yeni kayıtla senkronla.
  useEffect(() => {
    reset({
      day: session.day,
      date: session.date.slice(0, 10),
      wbcPre: session.wbcPre ?? undefined,
      hgb: session.hgb ?? undefined,
      hct: session.hct ?? undefined,
      plt: session.plt ?? undefined,
      volumeMl: session.volumeMl,
      wbc: session.wbc,
      cd34Percent: session.cd34Percent,
      cd45Percent: session.cd45Percent,
      cd3Percent: session.cd3Percent,
      lymphocytePercent: session.lymphocytePercent ?? undefined,
      mhs: session.mhs ?? undefined,
      absoluteCellCount: session.absoluteCellCount,
      cd34PerKg: session.cd34PerKg,
      cd3PerKg: session.cd3PerKg,
    });
  }, [session, reset]);

  const isAutologous = patient?.transplantType === "Autologous";

  const onSubmit = async (v: SessionFormVals) => {
    const updated = await Sessions.update({
      id: session.id,
      patientId: session.patientId,
      day: Number(v.day),
      date: new Date(v.date).toISOString(),
      wbcPre: numOrNull(v.wbcPre),
      hgb: numOrNull(v.hgb),
      hct: numOrNull(v.hct),
      plt: numOrNull(v.plt),
      volumeMl: Number(v.volumeMl),
      wbc: Number(v.wbc),
      cd34Percent: Number(v.cd34Percent),
      cd45Percent: Number(v.cd45Percent),
      cd3Percent: isAutologous ? 0 : Number(v.cd3Percent),
      lymphocytePercent: isAutologous ? undefined : numOrNull(v.lymphocytePercent),
      mhs: numOrNull(v.mhs),
      absoluteCellCount: session.absoluteCellCount,
      // Keep persisted calculated values stable until backend calculate recomputes.
      cd34PerKg: session.cd34PerKg,
      cd3PerKg: isAutologous ? 0 : session.cd3PerKg,
    });
    await Sessions.calculate(updated.id);
    const fresh = await Sessions.byId(updated.id);

    qc.setQueryData(["session", updated.id], fresh);
    qc.setQueriesData({ queryKey: ["sessions"] }, (old: any) => {
      if (!old?.items) return old;
      return {
        ...old,
        items: old.items.map((it: any) => (it.id === fresh.id ? fresh : it)),
      };
    });

    qc.invalidateQueries({ queryKey: ["sessions"] });
    qc.invalidateQueries({ queryKey: ["patient", session.patientId] });
    qc.invalidateQueries({ queryKey: ["apheresis-plan", session.patientId] });
    qc.invalidateQueries({ queryKey: ["patients"] });
    qc.invalidateQueries({ queryKey: ["dashboard"] });
    toast.success("Seans güncellendi");
    onSaved();
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5 pb-2">
      <div className="text-xs text-ink-muted">
        Hasta: <span className="text-ink font-medium">{patient?.fullName ?? "—"}</span>{" "}
        {patient && <>· {patient.weightKg} kg · {patient.transplantType}</>}
      </div>

      <section>
        <SectionTitle title="Gün bilgisi" />
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
        <SectionTitle title="PK — Aferez öncesi" />
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <Input label="WBC pre" type="number" step="0.01" {...register("wbcPre")} />
          <Input label="HGB" type="number" step="0.01" {...register("hgb")} />
          <Input label="HCT" type="number" step="0.01" {...register("hct")} />
          <Input label="PLT" type="number" step="0.01" {...register("plt")} />
        </div>
      </section>

      <section>
        <SectionTitle
          title="ÜRÜN — Aferez sonrası"
          hint={isAutologous ? "Otolog · CD3/Lenfosit uygulanmaz" : undefined}
        />
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <Input
            label="Hacim (ml)"
            type="number"
            step="0.1"
            {...register("volumeMl", { required: true, min: 0.1 })}
          />
          <Input
            label="WBC"
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

      <section>
        <SectionTitle title="Hesaplanan değerler" hint="Mutlak hücre read-only, diğerleri manuel override edilebilir" />
        <div className="grid grid-cols-2 gap-3">
          <Input label="Mutlak hücre" type="number" step="0.0001" value={session.absoluteCellCount} disabled />
          <Input label="CD34/kg" type="number" step="0.0001" {...register("cd34PerKg")} />
          {!isAutologous && (
            <Input label="CD3/kg" type="number" step="0.0001" {...register("cd3PerKg")} />
          )}
        </div>
      </section>

      <div className="sticky bottom-0 -mx-6 -mb-5 px-6 py-3 bg-bg-card/95 backdrop-blur border-t border-line/60 flex justify-end gap-2">
        <Button variant="soft" type="button" onClick={onCancel}>İptal</Button>
        <Button type="submit" loading={isSubmitting}>Kaydet</Button>
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

function numOrNull(v: unknown): number | undefined {
  if (v === "" || v === null || v === undefined) return undefined;
  const n = Number(v);
  return Number.isFinite(n) ? n : undefined;
}
