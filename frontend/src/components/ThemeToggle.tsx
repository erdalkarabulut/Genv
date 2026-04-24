import { Monitor, Moon, Sun } from "lucide-react";
import { cn } from "@/lib/utils";
import { useTheme, type ThemePreference } from "@/lib/theme";

const options: Array<{ value: ThemePreference; icon: typeof Sun; label: string }> = [
  { value: "light", icon: Sun, label: "Açık tema" },
  { value: "dark", icon: Moon, label: "Koyu tema" },
  { value: "system", icon: Monitor, label: "Sistem teması" },
];

export function ThemeToggle({ className }: { className?: string }) {
  const { preference, setPreference } = useTheme();

  return (
    <div
      className={cn(
        "inline-flex rounded-xl border border-line/60 bg-bg-elevated/60 p-0.5 backdrop-blur-sm",
        className,
      )}
      role="group"
      aria-label="Tema seçimi"
    >
      {options.map(({ value, icon: Icon, label }) => (
        <button
          key={value}
          type="button"
          title={label}
          aria-pressed={preference === value}
          onClick={() => setPreference(value)}
          className={cn(
            "rounded-lg p-2 transition outline-none focus-visible:ring-2 focus-visible:ring-brand-500/50",
            preference === value
              ? "bg-brand-500/15 text-brand-600 dark:text-brand-400 shadow-sm"
              : "text-ink-dim hover:text-ink",
          )}
        >
          <Icon className="size-4" aria-hidden />
        </button>
      ))}
    </div>
  );
}
