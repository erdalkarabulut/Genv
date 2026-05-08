/**
 * CD34/CD3 per-kg calculation utilities.
 *
 * All formulas respect the server-configured `sessionCd34Cd3Divisor` via ClinicalSettings.
 * Import `calculateCellDose` from this file wherever a bag/cell dose preview is needed.
 */

export interface DoseInput {
  volumeMl: number;
  wbc: number;
  cd45Percent: number;
  cd34Percent: number;
  cd3Percent: number;
}

/** Live preview used in the custom-split form on PatientDetailPage. */
export function calculateCellDose(input: DoseInput, weightKg: number, divisor: number) {
  const { volumeMl, wbc, cd45Percent, cd34Percent, cd3Percent } = input;
  if (!volumeMl || !wbc || !weightKg) return { cd34PerKg: 0, cd3PerKg: 0 };
  const total = volumeMl * wbc;
  return {
    cd34PerKg: roundDose((total * cd45Percent * cd34Percent) / divisor / weightKg, 0),
    cd3PerKg: cd3Percent ? roundDose((total * cd3Percent) / divisor / weightKg, 0) : 0,
  };
}

/** Absolute cell count preview: WBC x %CD45 x %CD34 / 10000. */
export function calculateAbsoluteCellCount(
  wbc: number,
  cd45Percent: number,
  cd34Percent: number,
): number {
  if (!wbc) return 0;
  return roundDose((wbc * cd45Percent * cd34Percent) / 10000, 0);
}

/**
 * Rounds a dose value for display.  Returns 0 for NaN/Infinity.
 * @param value Raw calculated value
 * @param decimals Number of decimal places (default 0 = no decimals)
 */
export function roundDose(value: number, decimals = 0): number {
  const n = Number(value);
  if (!Number.isFinite(n)) return 0;
  return Math.round(n * 10 ** decimals) / 10 ** decimals;
}
