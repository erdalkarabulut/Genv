import { cn } from "@/lib/utils";
import type { HTMLAttributes, ReactNode } from "react";

export function Card({ className, ...rest }: HTMLAttributes<HTMLDivElement>) {
  return <div className={cn("card p-5", className)} {...rest} />;
}

export function CardHeader({
  title,
  subtitle,
  right,
  className,
}: {
  title: ReactNode;
  subtitle?: ReactNode;
  right?: ReactNode;
  className?: string;
}) {
  return (
    <div className={cn("flex items-start justify-between gap-4 mb-4", className)}>
      <div>
        <h3 className="text-base font-semibold text-ink">{title}</h3>
        {subtitle && <p className="mt-0.5 text-xs text-ink-muted">{subtitle}</p>}
      </div>
      {right}
    </div>
  );
}

export function Stat({
  label,
  value,
  hint,
  icon,
  trend,
}: {
  label: string;
  value: ReactNode;
  hint?: ReactNode;
  icon?: ReactNode;
  trend?: "up" | "down" | "flat";
}) {
  return (
    <Card className="relative overflow-hidden">
      <div className="absolute -right-8 -top-8 size-32 rounded-full bg-brand-500/10 blur-2xl" />
      <div className="flex items-center justify-between">
        <p className="text-xs uppercase tracking-wide text-ink-dim">{label}</p>
        {icon && <div className="text-ink-muted">{icon}</div>}
      </div>
      <div className="mt-2 text-3xl font-semibold tracking-tight">{value}</div>
      {hint && <p className="mt-1 text-xs text-ink-muted">{hint}</p>}
    </Card>
  );
}
