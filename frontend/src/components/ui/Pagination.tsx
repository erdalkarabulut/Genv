import { ChevronLeft, ChevronRight } from "lucide-react";
import { cn } from "@/lib/utils";
import type { ReactNode } from "react";

interface PaginationProps {
  page: number;          // 0-based
  totalPages: number;
  totalItems: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  middle?: ReactNode;
  className?: string;
}

export function Pagination({
  page,
  totalPages,
  totalItems,
  pageSize,
  onPageChange,
  middle,
  className,
}: PaginationProps) {
  if (totalPages <= 1) return null;

  const from = page * pageSize + 1;
  const to = Math.min((page + 1) * pageSize, totalItems);

  // Page number buttons: show at most 7 slots with ellipsis
  const pages = buildPageNumbers(page, totalPages);

  return (
    <div className={cn("flex items-center justify-between gap-4 flex-wrap", className)}>
      <p className="text-sm text-ink-muted">
        <span className="font-medium text-ink">{from}–{to}</span> / {totalItems} kayıt
      </p>

      {middle}

      <nav className="flex items-center gap-1">
        <button
          disabled={page === 0}
          onClick={() => onPageChange(page - 1)}
          className="btn-ghost px-2 py-1.5 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed"
          aria-label="Önceki sayfa"
        >
          <ChevronLeft className="size-4" />
        </button>

        {pages.map((p, i) =>
          p === "..." ? (
            <span key={`ellipsis-${i}`} className="px-2 py-1.5 text-sm text-ink-dim">…</span>
          ) : (
            <button
              key={p}
              onClick={() => onPageChange(p as number)}
              className={cn(
                "min-w-[32px] px-2 py-1.5 text-sm rounded-lg font-medium transition-colors",
                p === page
                  ? "bg-brand-600 text-white shadow-glow"
                  : "btn-ghost",
              )}
            >
              {(p as number) + 1}
            </button>
          ),
        )}

        <button
          disabled={page >= totalPages - 1}
          onClick={() => onPageChange(page + 1)}
          className="btn-ghost px-2 py-1.5 rounded-lg disabled:opacity-40 disabled:cursor-not-allowed"
          aria-label="Sonraki sayfa"
        >
          <ChevronRight className="size-4" />
        </button>
      </nav>
    </div>
  );
}

function buildPageNumbers(current: number, total: number): (number | "...")[] {
  if (total <= 7) return Array.from({ length: total }, (_, i) => i);

  const result: (number | "...")[] = [];

  if (current <= 3) {
    result.push(0, 1, 2, 3, 4, "...", total - 1);
  } else if (current >= total - 4) {
    result.push(0, "...", total - 5, total - 4, total - 3, total - 2, total - 1);
  } else {
    result.push(0, "...", current - 1, current, current + 1, "...", total - 1);
  }

  return result;
}

/** Utility hook for client-side pagination */
export function usePagination<T>(items: T[], pageSize: number) {
  return { pageSize, totalItems: items.length, totalPages: Math.ceil(items.length / pageSize) };
}

export function paginateItems<T>(items: T[], page: number, pageSize: number): T[] {
  return items.slice(page * pageSize, (page + 1) * pageSize);
}
