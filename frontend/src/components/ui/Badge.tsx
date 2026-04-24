import { cn } from "@/lib/utils";
import type { ReactNode } from "react";

type Tone = "neutral" | "brand" | "mint" | "rose" | "amber" | "sky";

const tones: Record<Tone, string> = {
  neutral: "bg-bg-elevated/80 text-ink-muted border-line/80",
  brand: "bg-brand-500/10 text-brand-400 border-brand-500/30",
  mint: "bg-emerald-500/10 text-accent-mint border-emerald-500/30",
  rose: "bg-rose-500/10 text-accent-rose border-rose-500/30",
  amber: "bg-amber-500/10 text-accent-amber border-amber-500/30",
  sky: "bg-sky-500/10 text-accent-sky border-sky-500/30",
};

export function Badge({
  children,
  tone = "neutral",
  className,
  dot,
}: {
  children: ReactNode;
  tone?: Tone;
  className?: string;
  dot?: boolean;
}) {
  return (
    <span className={cn("chip border", tones[tone], className)}>
      {dot && <span className={cn("size-1.5 rounded-full", dotBg(tone))} />}
      {children}
    </span>
  );
}

function dotBg(tone: Tone) {
  switch (tone) {
    case "brand": return "bg-brand-400";
    case "mint": return "bg-accent-mint";
    case "rose": return "bg-accent-rose";
    case "amber": return "bg-accent-amber";
    case "sky": return "bg-accent-sky";
    default: return "bg-ink-dim";
  }
}
