import { useEffect, useState } from "react";
import { Modal } from "@/components/ui/Modal";
import { Button } from "@/components/ui/Button";
import type { BagUseReason } from "@/lib/types";

interface UseBagDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (reason: BagUseReason, note: string | null) => Promise<void> | void;
  bagLabel?: string;
  loading?: boolean;
}

const REASONS: { value: BagUseReason; label: string; hint: string }[] = [
  { value: "Infusion", label: "İnfüzyon", hint: "Hastaya verildi" },
  { value: "Disposal", label: "İmha", hint: "Bertaraf edildi" },
  { value: "Transfer", label: "Transfer", hint: "Başka birime/kuruma aktarıldı" },
  { value: "Other", label: "Diğer", hint: "Açıklama (zorunlu)" },
];

export function UseBagDialog({
  open,
  onClose,
  onSubmit,
  bagLabel,
  loading = false,
}: UseBagDialogProps) {
  const [reason, setReason] = useState<BagUseReason>("Infusion");
  const [note, setNote] = useState("");
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (open) {
      setReason("Infusion");
      setNote("");
      setSubmitting(false);
    }
  }, [open]);

  const trimmedNote = note.trim();
  const otherInvalid = reason === "Other" && trimmedNote.length < 3;

  const handleSubmit = async () => {
    if (otherInvalid || submitting) return;
    setSubmitting(true);
    try {
      await onSubmit(reason, trimmedNote.length > 0 ? trimmedNote : null);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal
      open={open}
      onClose={onClose}
      size="sm"
      title="Torbayı Kullan"
      description={
        bagLabel
          ? `${bagLabel} için kullanım sebebini seç.`
          : "Kullanım sebebini seç."
      }
    >
      <div className="space-y-4">
        <div className="space-y-2">
          {REASONS.map((r) => (
            <label
              key={r.value}
              className={`flex cursor-pointer items-start gap-3 rounded-lg border px-3 py-2 transition ${
                reason === r.value
                  ? "border-accent-cyan/60 bg-accent-cyan/5"
                  : "border-line/60 hover:border-line"
              }`}
            >
              <input
                type="radio"
                name="bag-use-reason"
                checked={reason === r.value}
                onChange={() => setReason(r.value)}
                className="mt-1 accent-accent-cyan"
              />
              <div className="flex-1">
                <div className="text-sm font-medium">{r.label}</div>
                <div className="text-[11px] text-ink-dim">{r.hint}</div>
              </div>
            </label>
          ))}
        </div>

        <div>
          <label className="label" htmlFor="use-bag-note">
            Açıklama
            {reason === "Other" && (
              <span className="ml-1 text-accent-rose">*</span>
            )}
          </label>
          <textarea
            id="use-bag-note"
            className="input min-h-[72px] resize-y"
            placeholder={
              reason === "Other"
                ? "En az 3 karakter, sebebi açıkla"
                : "İsteğe bağlı not"
            }
            value={note}
            onChange={(e) => setNote(e.target.value)}
            maxLength={500}
          />
          {otherInvalid && (
            <p className="mt-1 text-[11px] text-accent-rose">
              Diğer seçildi: en az 3 karakterlik açıklama gir.
            </p>
          )}
        </div>

        <div className="flex items-center justify-end gap-2 pt-2">
          <Button variant="ghost" onClick={onClose} disabled={submitting || loading}>
            Vazgeç
          </Button>
          <Button
            variant="primary"
            onClick={handleSubmit}
            loading={submitting || loading}
            disabled={otherInvalid}
          >
            Kullan
          </Button>
        </div>
      </div>
    </Modal>
  );
}
