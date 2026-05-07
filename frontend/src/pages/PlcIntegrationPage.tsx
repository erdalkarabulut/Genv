import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AlarmTemplates, PlcAlarmContacts, PlcSensorPoints } from "@/lib/api";
import type { AlarmTemplateDto, PlcAlarmContactDto, PlcSensorPointDto } from "@/lib/types";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { Modal } from "@/components/ui/Modal";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { EmptyState } from "@/components/ui/EmptyState";
import { Badge } from "@/components/ui/Badge";
import { cn } from "@/lib/utils";
import { useState, type ReactNode } from "react";
import { toast } from "sonner";
import { Cpu, Plus, Pencil, Trash2, Bell, Radio } from "lucide-react";

type Tab = "sensors" | "contacts";

const emptySensor = (): Omit<PlcSensorPointDto, "id"> => ({
  sensorCode: "",
  deviceName: "",
  devicePrefix: "",
  dataLabel: "",
  modbusHost: "",
  modbusPort: 502,
  slaveId: 1,
  registerAddress: 0,
  registerLength: 1,
  scaleDivisor: 1,
  pollIntervalSeconds: 5,
  alarmLow: null,
  alarmHigh: null,
  alarmActive: false,
});

const emptyContact = (): Omit<PlcAlarmContactDto, "id"> => ({
  devicePrefix: "",
  alarmTemplateId: undefined,
  displayName: "",
  phone: "",
  email: "",
  smsEnabled: true,
  emailEnabled: false,
});

export default function PlcIntegrationPage() {
  const qc = useQueryClient();
  const [tab, setTab] = useState<Tab>("sensors");

  const sensors = useQuery({
    queryKey: ["plc-sensors"],
    queryFn: () => PlcSensorPoints.list(),
  });

  const contacts = useQuery({
    queryKey: ["plc-contacts"],
    queryFn: () => PlcAlarmContacts.list(),
  });

  const templates = useQuery({
    queryKey: ["alarm-templates"],
    queryFn: () => AlarmTemplates.list(),
  });

  const invalidatePlc = () => {
    qc.invalidateQueries({ queryKey: ["plc-sensors"] });
    qc.invalidateQueries({ queryKey: ["plc-contacts"] });
    qc.invalidateQueries({ queryKey: ["alarm-templates"] });
  };

  const [sensorModal, setSensorModal] = useState<
    | { mode: "create"; values: Omit<PlcSensorPointDto, "id"> }
    | { mode: "edit"; values: PlcSensorPointDto }
    | null
  >(null);

  const [contactModal, setContactModal] = useState<
    | { mode: "create"; values: Omit<PlcAlarmContactDto, "id"> }
    | { mode: "edit"; values: PlcAlarmContactDto }
    | null
  >(null);

  const [deleteSensor, setDeleteSensor] = useState<PlcSensorPointDto | null>(null);
  const [deleteContact, setDeleteContact] = useState<PlcAlarmContactDto | null>(null);

  const saveSensor = useMutation({
    mutationFn: async () => {
      if (!sensorModal) return;
      if (sensorModal.mode === "create") {
        await PlcSensorPoints.create(sensorModal.values);
      } else {
        await PlcSensorPoints.update(sensorModal.values);
      }
    },
    onSuccess: () => {
      toast.success("Sensör kaydı kaydedildi");
      setSensorModal(null);
      invalidatePlc();
    },
  });

  const delSensor = useMutation({
    mutationFn: (id: string) => PlcSensorPoints.remove(id),
    onSuccess: () => {
      toast.success("Sensör silindi");
      setDeleteSensor(null);
      invalidatePlc();
    },
  });

  const saveContact = useMutation({
    mutationFn: async () => {
      if (!contactModal) return;
      const payload = {
        ...contactModal.values,
        devicePrefix: contactModal.values.devicePrefix?.trim() || null,
      };
      if (contactModal.mode === "create") {
        await PlcAlarmContacts.create(payload);
      } else {
        await PlcAlarmContacts.update({ ...payload, id: contactModal.values.id });
      }
    },
    onSuccess: () => {
      toast.success("Alarm kontağı kaydedildi");
      setContactModal(null);
      invalidatePlc();
    },
  });

  const delContact = useMutation({
    mutationFn: (id: string) => PlcAlarmContacts.remove(id),
    onSuccess: () => {
      toast.success("Kontak silindi");
      setDeleteContact(null);
      invalidatePlc();
    },
  });

  const sensorRows = sensors.data ?? [];
  const contactRows = contacts.data?.items ?? [];

  const tabBtn = (t: Tab, label: string, icon: ReactNode) => (
    <button
      key={t}
      type="button"
      onClick={() => setTab(t)}
      className={cn(
        "inline-flex items-center gap-2 rounded-xl px-4 py-2 text-sm font-medium transition border",
        tab === t
          ? "border-brand-500/40 bg-brand-500/10 text-brand-300"
          : "border-line/60 bg-bg-elevated/40 text-ink-muted hover:text-ink",
      )}
    >
      {icon}
      {label}
    </button>
  );

  return (
    <div className="space-y-6">
      <header className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight flex items-center gap-2">
            <Cpu className="size-7 text-brand-400" />
            PLC entegrasyonu
          </h1>
          <p className="text-sm text-ink-muted mt-1 max-w-2xl">
            Modbus okuyucu servis, buradaki sensör tanımlarını{" "}
            <code className="text-[11px] px-1 py-0.5 rounded bg-bg-subtle border border-line/60">GET /api/plc-integration/sync</code> ile
            alır. Alarm eşikleri ve bildirim kişileri buradan yönetilir; yalnızca{" "}
            <strong className="text-ink">Admin</strong> rolü düzenleyebilir.
          </p>
        </div>
        <div className="flex flex-wrap gap-2 shrink-0">
          {tabBtn("sensors", "Sensörler", <Radio className="size-4" />)}
          {tabBtn("contacts", "Alarm kontakları", <Bell className="size-4" />)}
        </div>
      </header>

      {tab === "sensors" && (
        <Card className="overflow-hidden">
          <div className="flex items-center justify-between px-5 py-4 border-b border-line/60 bg-bg-subtle/40">
            <div>
              <h2 className="text-sm font-semibold">Modbus sensör noktaları</h2>
              <p className="text-[11px] text-ink-dim mt-0.5">
                Kod benzersizdir (örn. BT01-S01). Poll süresi saniye cinsindendir.
              </p>
            </div>
            <Button
              type="button"
              size="sm"
              onClick={() => setSensorModal({ mode: "create", values: emptySensor() })}
            >
              <Plus className="size-3.5" /> Yeni sensör
            </Button>
          </div>

          {sensors.isLoading ? (
            <div className="p-10 text-center text-sm text-ink-muted">Yükleniyor…</div>
          ) : sensorRows.length === 0 ? (
            <EmptyState
              title="Henüz sensör yok"
              description="PLC IP, register adresi ve ölçekleme ile tanım ekleyin. Modbus worker bu listeyi periyodik çeker."
              action={
                <Button type="button" onClick={() => setSensorModal({ mode: "create", values: emptySensor() })}>
                  <Plus className="size-3.5" /> Sensör ekle
                </Button>
              }
            />
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead className="bg-bg-subtle/50 text-ink-dim border-b border-line/60">
                  <tr>
                    <th className="px-4 py-2 font-medium">Kod</th>
                    <th className="px-4 py-2 font-medium">Cihaz</th>
                    <th className="px-4 py-2 font-medium">Veri</th>
                    <th className="px-4 py-2 font-medium">Modbus</th>
                    <th className="px-4 py-2 font-medium">Adres / Uzunluk</th>
                    <th className="px-4 py-2 font-medium">Ölçek</th>
                    <th className="px-4 py-2 font-medium">Poll (sn)</th>
                    <th className="px-4 py-2 font-medium">Alarm</th>
                    <th className="px-4 py-2 font-medium w-24">İşlem</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-line/40">
                  {sensorRows.map((row) => (
                    <tr key={row.id} className="hover:bg-bg-elevated/30">
                      <td className="px-4 py-2 font-mono text-[11px] text-brand-300">{row.sensorCode}</td>
                      <td className="px-4 py-2 max-w-[140px] truncate" title={row.deviceName}>
                        {row.deviceName}
                      </td>
                      <td className="px-4 py-2 text-ink-muted">{row.dataLabel}</td>
                      <td className="px-4 py-2 whitespace-nowrap">
                        {row.modbusHost}:{row.modbusPort}{" "}
                        <span className="text-ink-dim">slave {row.slaveId}</span>
                      </td>
                      <td className="px-4 py-2 whitespace-nowrap">
                        {row.registerAddress} / {row.registerLength}
                      </td>
                      <td className="px-4 py-2">{row.scaleDivisor}</td>
                      <td className="px-4 py-2">{row.pollIntervalSeconds}</td>
                      <td className="px-4 py-2">
                        <div className="flex flex-col gap-0.5">
                          <span className="text-ink-dim">
                            {row.alarmLow ?? "—"} … {row.alarmHigh ?? "—"}
                          </span>
                          {row.alarmActive ? (
                            <Badge tone="mint">Aktif</Badge>
                          ) : (
                            <Badge tone="neutral">Kapalı</Badge>
                          )}
                        </div>
                      </td>
                      <td className="px-4 py-2">
                        <div className="flex items-center gap-1">
                          <button
                            type="button"
                            className="rounded-lg p-1.5 text-ink-muted hover:text-brand-400 hover:bg-brand-500/10"
                            title="Düzenle"
                            onClick={() => setSensorModal({ mode: "edit", values: { ...row } })}
                          >
                            <Pencil className="size-3.5" />
                          </button>
                          <button
                            type="button"
                            className="rounded-lg p-1.5 text-ink-muted hover:text-accent-rose hover:bg-rose-500/10"
                            title="Sil"
                            onClick={() => setDeleteSensor(row)}
                          >
                            <Trash2 className="size-3.5" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>
      )}

      {tab === "contacts" && (
        <Card className="overflow-hidden">
          <div className="flex items-center justify-between px-5 py-4 border-b border-line/60 bg-bg-subtle/40">
            <div>
              <h2 className="text-sm font-semibold">Alarm bildirim kontakları</h2>
              <p className="text-[11px] text-ink-dim mt-0.5">
                Cihaz öneki boş bırakılırsa tüm cihazlar için geçerlidir (örn. yalnızca BT01 için &quot;BT01&quot; girin).
              </p>
            </div>
            <Button
              type="button"
              size="sm"
              onClick={() => setContactModal({ mode: "create", values: emptyContact() })}
            >
              <Plus className="size-3.5" /> Yeni kontak
            </Button>
          </div>

          {contacts.isLoading ? (
            <div className="p-10 text-center text-sm text-ink-muted">Yükleniyor…</div>
          ) : contactRows.length === 0 ? (
            <EmptyState
              title="Henüz kontak yok"
              description="SMS için telefon ve önek tanımlayın. E-posta şimdilik API tarafında opsiyoneldir."
              action={
                <Button type="button" onClick={() => setContactModal({ mode: "create", values: emptyContact() })}>
                  <Plus className="size-3.5" /> Kontak ekle
                </Button>
              }
            />
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead className="bg-bg-subtle/50 text-ink-dim border-b border-line/60">
                  <tr>
                    <th className="px-4 py-2 font-medium">Ad</th>
                    <th className="px-4 py-2 font-medium">Önek</th>
                    <th className="px-4 py-2 font-medium">Şablon</th>
                    <th className="px-4 py-2 font-medium">Telefon</th>
                    <th className="px-4 py-2 font-medium">E-posta</th>
                    <th className="px-4 py-2 font-medium">SMS / E-posta</th>
                    <th className="px-4 py-2 font-medium w-24">İşlem</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-line/40">
                  {contactRows.map((row: typeof contactRows[number]) => (
                    <tr key={row.id} className="hover:bg-bg-elevated/30">
                      <td className="px-4 py-2">{row.displayName}</td>
                      <td className="px-4 py-2 font-mono text-[11px]">{row.devicePrefix || "— (tümü)"}</td>
                      <td className="px-4 py-2">
                        {row.alarmTemplateName ? (
                          <span className="text-[11px] px-1.5 py-0.5 rounded bg-brand-500/10 text-brand-400">
                            {row.alarmTemplateName}
                          </span>
                        ) : (
                          <span className="text-ink-dim text-[11px]">— (otomatik)</span>
                        )}
                      </td>
                      <td className="px-4 py-2">{row.phone}</td>
                      <td className="px-4 py-2 text-ink-muted max-w-[180px] truncate" title={row.email ?? ""}>
                        {row.email || "—"}
                      </td>
                      <td className="px-4 py-2">
                        {row.smsEnabled ? <Badge tone="mint">SMS</Badge> : null}{" "}
                        {row.emailEnabled ? <Badge tone="neutral">E-posta</Badge> : null}
                      </td>
                      <td className="px-4 py-2">
                        <div className="flex items-center gap-1">
                          <button
                            type="button"
                            className="rounded-lg p-1.5 text-ink-muted hover:text-brand-400 hover:bg-brand-500/10"
                            title="Düzenle"
                            onClick={() =>
                              setContactModal({
                                mode: "edit",
                                values: {
                                  ...row,
                                  devicePrefix: row.devicePrefix ?? "",
                                  alarmTemplateId: row.alarmTemplateId || undefined,
                                },
                              })
                            }
                          >
                            <Pencil className="size-3.5" />
                          </button>
                          <button
                            type="button"
                            className="rounded-lg p-1.5 text-ink-muted hover:text-accent-rose hover:bg-rose-500/10"
                            title="Sil"
                            onClick={() => setDeleteContact(row)}
                          >
                            <Trash2 className="size-3.5" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>
      )}

      <SensorEditModal
        open={!!sensorModal}
        state={sensorModal}
        onClose={() => setSensorModal(null)}
        onChange={(values) => {
          if (!sensorModal) return;
          if (sensorModal.mode === "create") setSensorModal({ mode: "create", values });
          else setSensorModal({ mode: "edit", values: values as PlcSensorPointDto });
        }}
        onSubmit={() => saveSensor.mutate()}
        loading={saveSensor.isPending}
      />

      <ContactEditModal
        open={!!contactModal}
        state={contactModal}
        templates={(templates.data?.items ?? []).map((t: AlarmTemplateDto) => ({ id: t.id, name: t.name }))}
        onClose={() => setContactModal(null)}
        onChange={(values) => {
          if (!contactModal) return;
          if (contactModal.mode === "create") setContactModal({ mode: "create", values });
          else setContactModal({ mode: "edit", values: values as PlcAlarmContactDto });
        }}
        onSubmit={() => saveContact.mutate()}
        loading={saveContact.isPending}
      />

      <ConfirmDialog
        open={!!deleteSensor}
        onClose={() => setDeleteSensor(null)}
        onConfirm={() => deleteSensor && delSensor.mutate(deleteSensor.id)}
        loading={delSensor.isPending}
        description={`${deleteSensor?.sensorCode ?? ""} sensörünü silmek istediğinize emin misiniz?`}
      />

      <ConfirmDialog
        open={!!deleteContact}
        onClose={() => setDeleteContact(null)}
        onConfirm={() => deleteContact && delContact.mutate(deleteContact.id)}
        loading={delContact.isPending}
        description={`${deleteContact?.displayName ?? ""} kaydını silmek istediğinize emin misiniz?`}
      />
    </div>
  );
}

function SensorEditModal({
  open,
  state,
  onClose,
  onChange,
  onSubmit,
  loading,
}: {
  open: boolean;
  state:
    | { mode: "create"; values: Omit<PlcSensorPointDto, "id"> }
    | { mode: "edit"; values: PlcSensorPointDto }
    | null;
  onClose: () => void;
  onChange: (v: Omit<PlcSensorPointDto, "id"> | PlcSensorPointDto) => void;
  onSubmit: () => void;
  loading: boolean;
}) {
  const v = state?.values;
  const patch = (partial: Partial<Omit<PlcSensorPointDto, "id">>) => {
    if (!state) return;
    if (state.mode === "create") onChange({ ...state.values, ...partial });
    else onChange({ ...state.values, ...partial });
  };

  const num = (raw: string, fallback: number) => {
    const n = Number(raw);
    return Number.isFinite(n) ? n : fallback;
  };

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={state?.mode === "edit" ? "Sensörü düzenle" : "Yeni sensör"}
      description="Modbus TCP holding register okuması için tanım. Worker bu kayıtları senkronize eder."
      size="xl"
    >
      {v && (
        <div className="space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <Input
              label="Sensör kodu *"
              value={v.sensorCode}
              onChange={(e) => patch({ sensorCode: e.target.value })}
              placeholder="BT01-S01"
            />
            <Input
              label="Cihaz adı *"
              value={v.deviceName}
              onChange={(e) => patch({ deviceName: e.target.value })}
              placeholder="BESLEME TANKI 01"
            />
            <Input
              label="Cihaz öneki *"
              hint="Bildirim eşlemesi (örn. BT01)"
              value={v.devicePrefix}
              onChange={(e) => patch({ devicePrefix: e.target.value })}
            />
            <Input
              label="Veri etiketi *"
              value={v.dataLabel}
              onChange={(e) => patch({ dataLabel: e.target.value })}
              placeholder="SEVİYE"
            />
            <Input
              label="Modbus IP / host *"
              value={v.modbusHost}
              onChange={(e) => patch({ modbusHost: e.target.value })}
            />
            <Input
              label="Port"
              type="number"
              value={v.modbusPort}
              onChange={(e) => patch({ modbusPort: num(e.target.value, 502) })}
            />
            <Input
              label="Slave ID"
              type="number"
              value={v.slaveId}
              onChange={(e) => patch({ slaveId: num(e.target.value, 1) })}
            />
            <Input
              label="Register adresi"
              type="number"
              value={v.registerAddress}
              onChange={(e) => patch({ registerAddress: num(e.target.value, 0) })}
            />
            <Input
              label="Register uzunluğu"
              type="number"
              value={v.registerLength}
              onChange={(e) => patch({ registerLength: Math.max(1, num(e.target.value, 1)) })}
            />
            <Input
              label="Ölçek (kesir)"
              type="number"
              step="any"
              value={v.scaleDivisor}
              onChange={(e) => patch({ scaleDivisor: Math.max(0.0001, num(e.target.value, 1)) })}
            />
            <Input
              label="Poll aralığı (sn)"
              type="number"
              value={v.pollIntervalSeconds}
              onChange={(e) => patch({ pollIntervalSeconds: Math.max(1, num(e.target.value, 5)) })}
            />
            <Input
              label="Alarm düşük (boş = yok)"
              type="number"
              step="any"
              value={v.alarmLow ?? ""}
              onChange={(e) =>
                patch({
                  alarmLow: e.target.value === "" ? null : num(e.target.value, 0),
                })
              }
            />
            <Input
              label="Alarm yüksek (boş = yok)"
              type="number"
              step="any"
              value={v.alarmHigh ?? ""}
              onChange={(e) =>
                patch({
                  alarmHigh: e.target.value === "" ? null : num(e.target.value, 0),
                })
              }
            />
          </div>
          <label className="flex items-center gap-2 text-sm text-ink-muted cursor-pointer">
            <input
              type="checkbox"
              className="rounded border-line/60"
              checked={v.alarmActive}
              onChange={(e) => patch({ alarmActive: e.target.checked })}
            />
            Alarm değerlendirmesi aktif (eşik + bildirimler)
          </label>
          <div className="flex justify-end gap-2 pt-2 border-t border-line/60">
            <Button type="button" variant="soft" onClick={onClose}>
              Vazgeç
            </Button>
            <Button type="button" loading={loading} onClick={onSubmit}>
              Kaydet
            </Button>
          </div>
        </div>
      )}
    </Modal>
  );
}

function ContactEditModal({
  open,
  state,
  templates,
  onClose,
  onChange,
  onSubmit,
  loading,
}: {
  open: boolean;
  state:
    | { mode: "create"; values: Omit<PlcAlarmContactDto, "id"> }
    | { mode: "edit"; values: PlcAlarmContactDto }
    | null;
  templates: { id: string; name: string }[];
  onClose: () => void;
  onChange: (v: Omit<PlcAlarmContactDto, "id"> | PlcAlarmContactDto) => void;
  onSubmit: () => void;
  loading: boolean;
}) {
  const v = state?.values;
  const patch = (partial: Partial<Omit<PlcAlarmContactDto, "id">>) => {
    if (!state) return;
    if (state.mode === "create") onChange({ ...state.values, ...partial });
    else onChange({ ...state.values, ...partial });
  };

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={state?.mode === "edit" ? "Kontağı düzenle" : "Yeni alarm kontağı"}
      description="SMS gönderimi için telefon zorunludur."
      size="md"
    >
      {v && (
        <div className="space-y-4">
          <Input
            label="Görünen ad *"
            value={v.displayName}
            onChange={(e) => patch({ displayName: e.target.value })}
          />
          <Input
            label="Cihaz öneki (opsiyonel)"
            hint="Boş = tüm cihazlar"
            value={v.devicePrefix ?? ""}
            onChange={(e) => patch({ devicePrefix: e.target.value })}
          />
          <div>
            <label className="text-xs font-semibold text-ink mb-1 block">Alarm şablonu</label>
            <select
              className="w-full rounded border border-line/60 bg-bg-subtle px-3 py-2 text-sm"
              value={v.alarmTemplateId ?? ""}
              onChange={(e) => patch({ alarmTemplateId: e.target.value || undefined })}
            >
              <option value="">— Otomatik seç (varsayılan) —</option>
              {templates.map((t) => (
                <option key={t.id} value={t.id}>{t.name}</option>
              ))}
            </select>
            <p className="text-[11px] text-ink-dim mt-1">
              Boş bırakılırsa cihaz önekine göre otomatik şablon seçilir.
            </p>
          </div>
          <Input
            label="Telefon *"
            value={v.phone}
            onChange={(e) => patch({ phone: e.target.value })}
            placeholder="+90..."
          />
          <Input
            label="E-posta"
            type="email"
            value={v.email ?? ""}
            onChange={(e) => patch({ email: e.target.value })}
          />
          <div className="flex flex-wrap gap-4 text-sm text-ink-muted">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                className="rounded border-line/60"
                checked={v.smsEnabled}
                onChange={(e) => patch({ smsEnabled: e.target.checked })}
              />
              SMS
            </label>
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                className="rounded border-line/60"
                checked={v.emailEnabled}
                onChange={(e) => patch({ emailEnabled: e.target.checked })}
              />
              E-posta
            </label>
          </div>
          <div className="flex justify-end gap-2 pt-2 border-t border-line/60">
            <Button type="button" variant="soft" onClick={onClose}>
              Vazgeç
            </Button>
            <Button type="button" loading={loading} onClick={onSubmit}>
              Kaydet
            </Button>
          </div>
        </div>
      )}
    </Modal>
  );
}
