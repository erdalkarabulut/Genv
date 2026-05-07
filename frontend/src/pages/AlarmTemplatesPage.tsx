import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AlarmTemplates } from "@/lib/api";
import type { AlarmTemplateDto } from "@/lib/types";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { Modal } from "@/components/ui/Modal";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { EmptyState } from "@/components/ui/EmptyState";
import { Badge } from "@/components/ui/Badge";
import { cn } from "@/lib/utils";
import { useState } from "react";
import { toast } from "sonner";
import { Bell, Plus, Pencil, Trash2, MessageSquare, Mail, ArrowUpDown } from "lucide-react";

const PLACEHOLDERS = [
  { label: "{DeviceName}", desc: "Cihaz adı (örn. BT01)", sample: "BT01" },
  { label: "{DataLabel}", desc: "Ölçüm adı (örn. Sıcaklık)", sample: "Sıcaklık" },
  { label: "{SensorCode}", desc: "Sensör kodu (örn. BT01-S01)", sample: "BT01-S01" },
  { label: "{Value}", desc: "Ölçülen değer", sample: "85.3" },
  { label: "{AlarmLow}", desc: "Alt alarm eşiği", sample: "-10" },
  { label: "{AlarmHigh}", desc: "Üst alarm eşiği", sample: "50" },
];

const SAMPLE_VALUES: Record<string, string> = {
  "{DeviceName}": "BT01",
  "{DataLabel}": "Sıcaklık",
  "{SensorCode}": "BT01-S01",
  "{Value}": "85.3",
  "{AlarmLow}": "-10",
  "{AlarmHigh}": "50",
};

function resolveTemplate(template: string, _placeholders: typeof PLACEHOLDERS): string {
  let result = template;
  for (const [key, val] of Object.entries(SAMPLE_VALUES)) {
    result = result.replaceAll(key, val);
  }
  return result;
}

const emptyTemplate = (): Omit<AlarmTemplateDto, "id"> => ({
  name: "",
  smsTemplate: "⚠️ {DeviceName} {DataLabel} ({SensorCode}) alarm! Değer={Value} (eşik: {AlarmLow}-{AlarmHigh})",
  emailSubjectTemplate: "[Alarm] {DeviceName} - {DataLabel}",
  emailBodyTemplate: "{DeviceName} cihazında {DataLabel} alarmı.\n\nSensör: {SensorCode}\nDeğer: {Value}\nEşik: {AlarmLow} - {AlarmHigh}",
  devicePrefix: null,
  isActive: true,
});

export default function AlarmTemplatesPage() {
  const qc = useQueryClient();

  const templates = useQuery({
    queryKey: ["alarm-templates", 0, 50],
    queryFn: () => AlarmTemplates.list(0, 50),
    refetchOnMount: true,
    staleTime: 0,
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ["alarm-templates"] });

  const [editModal, setEditModal] = useState<
    | { mode: "create"; values: Omit<AlarmTemplateDto, "id"> }
    | { mode: "edit"; values: AlarmTemplateDto }
    | null
  >(null);

  const [deleteTarget, setDeleteTarget] = useState<AlarmTemplateDto | null>(null);

  const save = useMutation({
    mutationFn: async () => {
      if (!editModal) return;
      if (editModal.mode === "create") {
        await AlarmTemplates.create(editModal.values);
      } else {
        await AlarmTemplates.update(editModal.values);
      }
    },
    onSuccess: () => {
      toast.success("Şablon kaydedildi");
      setEditModal(null);
      invalidate();
    },
  });

  const del = useMutation({
    mutationFn: (id: string) => AlarmTemplates.remove(id),
    onSuccess: () => {
      toast.success("Şablon silindi");
      setDeleteTarget(null);
      invalidate();
    },
  });

  const rows = templates.data?.items ?? [];

  const insertPlaceholder = (field: "smsTemplate" | "emailSubjectTemplate" | "emailBodyTemplate", ph: string) => {
    if (!editModal) return;
    setEditModal({
      ...editModal,
      values: {
        ...editModal.values,
        [field]: editModal.values[field] + ph,
      } as AlarmTemplateDto,
    });
  };

  if (templates.isError) {
    return (
      <div className="p-8 text-center">
        <Bell className="size-12 text-red-400 mx-auto mb-4" />
        <h2 className="text-lg font-semibold text-red-400">Şablonlar yüklenemedi</h2>
        <p className="text-sm text-ink-muted mt-2">
          {String((templates.error as any)?.message ?? "Bilinmeyen hata")}
        </p>
        <Button className="mt-4" onClick={() => templates.refetch()}>Tekrar dene</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <header className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight flex items-center gap-2">
            <Bell className="size-7 text-brand-400" />
            Alarm şablonları
          </h1>
          <p className="text-sm text-ink-muted mt-1 max-w-2xl">
            PLC alarm tetiklendiğinde gönderilecek SMS ve e-posta mesajlarının şablonlarını tanımlayın.
            Placeholder&apos;lar otomatik olarak alarm verisiyle değiştirilir.
          </p>
        </div>
        <Button
          type="button"
          onClick={() => setEditModal({ mode: "create", values: emptyTemplate() })}
        >
          <Plus className="size-3.5" /> Yeni şablon
        </Button>
      </header>

      {/* Placeholder reference card */}
      <Card className="p-4">
        <div className="flex items-center gap-2 text-xs font-semibold text-ink-muted mb-3">
          <ArrowUpDown className="size-3.5" />
          Kullanılabilir placeholder&apos;lar
        </div>
        <div className="flex flex-wrap gap-2">
          {PLACEHOLDERS.map((p) => (
            <span
              key={p.label}
              className="inline-flex items-center gap-1.5 px-2 py-1 rounded bg-bg-subtle border border-line/60 text-[11px] font-mono"
            >
              <code className="text-brand-400">{p.label}</code>
              <span className="text-ink-dim">— {p.desc}</span>
            </span>
          ))}
        </div>
      </Card>

      {/* Template list */}
      {templates.isLoading ? (
        <div className="card h-48 skeleton" />
      ) : rows.length === 0 ? (
        <EmptyState
          title="Henüz şablon yok"
          description="SMS ve e-posta alarm şablonları tanımlayın. Her şablon benzersiz bir cihaz önekine atanabilir."
          action={
            <Button type="button" onClick={() => setEditModal({ mode: "create", values: emptyTemplate() })}>
              <Plus className="size-3.5" /> Şablon ekle
            </Button>
          }
        />
      ) : (
        <div className="space-y-3">
          {rows.map((tmpl: AlarmTemplateDto) => (
            <Card key={tmpl.id} className="p-5">
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap">
                    <span className="font-semibold text-ink">{tmpl.name}</span>
                    {tmpl.devicePrefix && (
                      <Badge tone="neutral" className="text-[11px]">{tmpl.devicePrefix}</Badge>
                    )}
                    <Badge tone={tmpl.isActive ? "mint" : "rose"} className="text-[11px]">
                      {tmpl.isActive ? "Aktif" : "Pasif"}
                    </Badge>
                  </div>

                  <div className="mt-3 grid sm:grid-cols-2 gap-4 text-xs">
                    <div className="flex items-start gap-2 p-2.5 rounded bg-bg-subtle border border-line/40">
                      <MessageSquare className="size-3.5 text-cyan-400 mt-0.5 shrink-0" />
                      <div>
                        <div className="text-[10px] text-ink-dim mb-1 uppercase tracking-wider">SMS Şablonu</div>
                        <div className="text-ink-muted font-mono leading-relaxed break-all">{tmpl.smsTemplate}</div>
                      </div>
                    </div>
                    <div className="flex items-start gap-2 p-2.5 rounded bg-bg-subtle border border-line/40">
                      <Mail className="size-3.5 text-amber-400 mt-0.5 shrink-0" />
                      <div>
                        <div className="text-[10px] text-ink-dim mb-1 uppercase tracking-wider">E-posta</div>
                        <div className="text-ink-muted font-mono leading-relaxed break-all">
                          <span className="font-semibold text-ink">{tmpl.emailSubjectTemplate ?? "(yok)"}</span>
                          {tmpl.emailBodyTemplate && (
                            <span className="block mt-1 text-ink-dim">{tmpl.emailBodyTemplate}</span>
                          )}
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Preview */}
                  <div className="mt-3 p-3 rounded border border-brand-500/20 bg-brand-500/5">
                    <div className="text-[10px] text-brand-400 font-semibold mb-1.5 uppercase tracking-wider">Önizleme (örnek değerlerle)</div>
                    <div className="text-xs text-ink-muted font-mono leading-relaxed break-all">
                      <span className="text-cyan-300">SMS:</span> {resolveTemplate(tmpl.smsTemplate, PLACEHOLDERS)}
                    </div>
                    {tmpl.emailSubjectTemplate && (
                      <div className="text-xs text-ink-muted font-mono mt-1 leading-relaxed break-all">
                        <span className="text-amber-300">E-posta:</span> {resolveTemplate(tmpl.emailSubjectTemplate, PLACEHOLDERS)}
                        {tmpl.emailBodyTemplate && (
                          <span className="block ml-6 mt-0.5 text-ink-dim">{resolveTemplate(tmpl.emailBodyTemplate, PLACEHOLDERS)}</span>
                        )}
                      </div>
                    )}
                  </div>
                </div>

                <div className="flex items-center gap-1 shrink-0">
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={() => setEditModal({ mode: "edit", values: tmpl })}
                  >
                    <Pencil className="size-3.5" />
                  </Button>
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    onClick={() => setDeleteTarget(tmpl)}
                  >
                    <Trash2 className="size-3.5 text-red-400" />
                  </Button>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}

      {/* Create / Edit Modal */}
      {editModal && (
        <Modal
          open
          onClose={() => setEditModal(null)}
          title={editModal.mode === "create" ? "Yeni alarm şablonu" : "Şablonu düzenle"}
        >
          <div className="space-y-4">
            <div className="grid sm:grid-cols-3 gap-4">
              <Input
                label="Şablon adı"
                value={editModal.values.name}
                onChange={(e) =>
                  setEditModal({ ...editModal, values: { ...editModal.values, name: e.target.value } as AlarmTemplateDto })
                }
                placeholder="örn. Kritik sıcaklık"
              />
              <Input
                label="Cihaz öneki (opsiyonel)"
                value={editModal.values.devicePrefix ?? ""}
                onChange={(e) =>
                  setEditModal({
                    ...editModal,
                    values: { ...editModal.values, devicePrefix: e.target.value || null } as AlarmTemplateDto,
                  })
                }
                placeholder="örn. BT01 — boş = tümü"
              />
              <div className="flex items-end">
                <label className="flex items-center gap-2 cursor-pointer text-sm">
                  <input
                    type="checkbox"
                    checked={editModal.values.isActive}
                    onChange={(e) =>
                      setEditModal({
                        ...editModal,
                        values: { ...editModal.values, isActive: e.target.checked } as AlarmTemplateDto,
                      })
                    }
                    className="accent-brand-500 size-4"
                  />
                  Aktif
                </label>
              </div>
            </div>

            <div>
              <div className="flex items-center justify-between mb-1.5">
                <label className="text-xs font-semibold text-ink">SMS Şablonu</label>
                <div className="flex gap-1">
                  {PLACEHOLDERS.map((p) => (
                    <button
                      key={p.label}
                      type="button"
                      onClick={() => insertPlaceholder("smsTemplate", p.label)}
                      className="text-[10px] px-1.5 py-0.5 rounded bg-brand-500/10 text-brand-400 hover:bg-brand-500/20 font-mono"
                      title={p.desc}
                    >
                      {p.label}
                    </button>
                  ))}
                </div>
              </div>
              <textarea
                className="w-full rounded border border-line/60 bg-bg-subtle px-3 py-2 text-sm font-mono resize-y min-h-[72px]"
                value={editModal.values.smsTemplate}
                onChange={(e) =>
                  setEditModal({
                    ...editModal,
                    values: { ...editModal.values, smsTemplate: e.target.value } as AlarmTemplateDto,
                  })
                }
                placeholder="⚠️ {DeviceName} {DataLabel} alarm!"
              />
            </div>

            <div>
              <label className="text-xs font-semibold text-ink mb-1.5 block">E-posta konu</label>
              <div className="flex gap-1 mb-1.5">
                {PLACEHOLDERS.map((p) => (
                  <button
                    key={p.label}
                    type="button"
                    onClick={() => insertPlaceholder("emailSubjectTemplate", p.label)}
                    className="text-[10px] px-1.5 py-0.5 rounded bg-brand-500/10 text-brand-400 hover:bg-brand-500/20 font-mono"
                    title={p.desc}
                  >
                    {p.label}
                  </button>
                ))}
              </div>
              <Input
                value={editModal.values.emailSubjectTemplate ?? ""}
                onChange={(e) =>
                  setEditModal({
                    ...editModal,
                    values: { ...editModal.values, emailSubjectTemplate: e.target.value || null } as AlarmTemplateDto,
                  })
                }
                placeholder="[Alarm] {DeviceName} - {DataLabel}"
              />
            </div>

            <div>
              <label className="text-xs font-semibold text-ink mb-1.5 block">E-posta gövde</label>
              <div className="flex gap-1 mb-1.5">
                {PLACEHOLDERS.map((p) => (
                  <button
                    key={p.label}
                    type="button"
                    onClick={() => insertPlaceholder("emailBodyTemplate", p.label)}
                    className="text-[10px] px-1.5 py-0.5 rounded bg-brand-500/10 text-brand-400 hover:bg-brand-500/20 font-mono"
                    title={p.desc}
                  >
                    {p.label}
                  </button>
                ))}
              </div>
              <textarea
                className="w-full rounded border border-line/60 bg-bg-subtle px-3 py-2 text-sm font-mono resize-y min-h-[100px]"
                value={editModal.values.emailBodyTemplate ?? ""}
                onChange={(e) =>
                  setEditModal({
                    ...editModal,
                    values: { ...editModal.values, emailBodyTemplate: e.target.value || null } as AlarmTemplateDto,
                  })
                }
                placeholder="Alarm detayları..."
              />
            </div>

            <div className="flex justify-end gap-2 pt-2">
              <Button type="button" variant="soft" onClick={() => setEditModal(null)}>
                İptal
              </Button>
              <Button type="button" loading={save.isPending} onClick={() => save.mutate()}>
                Kaydet
              </Button>
            </div>
          </div>
        </Modal>
      )}

      {/* Delete confirm */}
      {deleteTarget && (
        <ConfirmDialog
          open
          onClose={() => setDeleteTarget(null)}
          onConfirm={() => del.mutate(deleteTarget.id)}
          title="Şablonu sil"
          description={`"${deleteTarget.name}" şablonunu silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`}
          confirmText="Sil"
        />
      )}
    </div>
  );
}