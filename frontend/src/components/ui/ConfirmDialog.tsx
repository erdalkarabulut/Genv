import { AlertTriangle } from "lucide-react";
import { Modal } from "./Modal";
import { Button } from "./Button";

export function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title = "Silme onayı",
  description = "Bu kaydı silmek üzeresiniz. Bu işlem geri alınamaz.",
  confirmText = "Sil",
  cancelText = "Vazgeç",
  loading,
  tone = "danger",
}: {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title?: string;
  description?: string;
  confirmText?: string;
  cancelText?: string;
  loading?: boolean;
  tone?: "danger" | "primary";
}) {
  return (
    <Modal open={open} onClose={onClose} title={title} size="sm">
      <div className="flex items-start gap-3 text-sm text-ink-muted">
        <div
          className={`grid size-9 place-items-center rounded-xl border shrink-0 ${
            tone === "danger"
              ? "border-rose-500/30 bg-rose-500/10 text-accent-rose"
              : "border-brand-500/30 bg-brand-500/10 text-brand-400"
          }`}
        >
          <AlertTriangle className="size-4" />
        </div>
        <p className="pt-1.5 leading-relaxed">{description}</p>
      </div>
      <div className="mt-5 flex items-center justify-end gap-2">
        <Button variant="soft" type="button" onClick={onClose} disabled={loading}>
          {cancelText}
        </Button>
        <Button
          variant={tone === "danger" ? "danger" : "primary"}
          type="button"
          loading={loading}
          onClick={onConfirm}
        >
          {confirmText}
        </Button>
      </div>
    </Modal>
  );
}
