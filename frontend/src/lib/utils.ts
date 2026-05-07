import clsx, { type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";
import { calculateCellDose } from "./calculations";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatNumber(n: number | null | undefined, digits = 2) {
  if (n === null || n === undefined || Number.isNaN(n)) return "—";
  return n.toLocaleString("tr-TR", { maximumFractionDigits: digits, minimumFractionDigits: 0 });
}

export function formatDate(d?: string | Date | null) {
  if (!d) return "—";
  const date = typeof d === "string" ? new Date(d) : d;
  return date.toLocaleDateString("tr-TR", { day: "2-digit", month: "short", year: "numeric" });
}

export function formatDateTime(d?: string | Date | null) {
  if (!d) return "—";
  const date = typeof d === "string" ? new Date(d) : d;
  return date.toLocaleString("tr-TR", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function shortId(id?: string | null) {
  if (!id) return "—";
  return `${id.slice(0, 4)}…${id.slice(-4)}`;
}

// Re-export dose calculation utilities for convenient imports
export { calculateCellDose, roundDose } from "./calculations";
export type { DoseInput } from "./calculations";

/** ABO/Rh standart kan grubu seçenekleri (Patient/Donor formlarında ortak kullanılır). */
export const BLOOD_GROUP_OPTIONS: { value: string; label: string }[] = [
  { value: "", label: "Seçilmedi" },
  { value: "A Rh+", label: "A Rh+" },
  { value: "A Rh-", label: "A Rh−" },
  { value: "B Rh+", label: "B Rh+" },
  { value: "B Rh-", label: "B Rh−" },
  { value: "AB Rh+", label: "AB Rh+" },
  { value: "AB Rh-", label: "AB Rh−" },
  { value: "0 Rh+", label: "0 Rh+" },
  { value: "0 Rh-", label: "0 Rh−" },
  { value: "Bilinmiyor", label: "Bilinmiyor" },
];
