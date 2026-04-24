import type { ReactNode } from "react";

export function EmptyState({
  icon,
  title,
  description,
  action,
}: {
  icon?: ReactNode;
  title: string;
  description?: string;
  action?: ReactNode;
}) {
  return (
    <div className="card flex flex-col items-center justify-center gap-2 py-14 text-center">
      {icon && <div className="mb-2 text-ink-dim">{icon}</div>}
      <h4 className="text-base font-semibold">{title}</h4>
      {description && (
        <p className="max-w-sm text-sm text-ink-muted">{description}</p>
      )}
      {action && <div className="mt-3">{action}</div>}
    </div>
  );
}
