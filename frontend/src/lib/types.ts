export type TransplantType = "Autologous" | "Allogeneic";
export type BagStatus = "Frozen" | "Stored" | "Reserved" | "Used" | "Discarded";
export type BagPurpose = "Cryo" | "Infusion" | "Backup" | "QualityControl";
export type BagUseReason = "Infusion" | "Disposal" | "Transfer" | "Other";

/* -------------------------- Patient -------------------------- */
export interface Patient {
  id: string;
  fullName: string;
  weightKg: number;
  bloodGroup?: string | null;
  transplantType: TransplantType;
  diagnosis?: string | null;
  protocolNo?: string | null;
  identityNumber?: string | null;
  birthDate?: string | null;
  donorId?: string | null;
  createdDate: string;
}

/* -------------------------- Donor ---------------------------- */
export type DonorType = "Related" | "Unrelated";

export interface Donor {
  id: string;
  fullName: string;
  weightKg: number;
  bloodGroup?: string | null;
  relation?: string | null;
  identityNumber?: string | null;
  donorType: DonorType;
  birthDate?: string | null;
  createdDate: string;
}

/* --------------------- Collection Session -------------------- */
export interface CollectionSession {
  id: string;
  patientId: string;
  day: number;
  date: string;
  // PK (işlem öncesi) hemogram
  wbcPre?: number | null;
  hgb?: number | null;
  hct?: number | null;
  plt?: number | null;
  // PK (işlem öncesi) flow-cytometry
  preCd45Percent?: number | null;
  preCd34Percent?: number | null;
  preMhs?: number | null;
  // İşlem sonrası hemogram
  wbcPost?: number | null;
  hgbPost?: number | null;
  hctPost?: number | null;
  pltPost?: number | null;
  // Ürün
  volumeMl: number;
  wbc: number;
  cd34Percent: number;
  cd45Percent: number;
  cd3Percent: number;
  lymphocytePercent?: number | null;
  mhs?: number | null;
  cd34PerKg: number;
  cd3PerKg: number;
  createdDate: string;
}

/* -------------------------- Bag ------------------------------ */
export interface Bag {
  id: string;
  sessionId: string;
  bagNumber: number;
  volumeMl: number;
  sourceVolumeMl: number;
  wbc?: number | null;
  cd34Percent?: number | null;
  cd45Percent?: number | null;
  cd3Percent?: number | null;
  compositionNote?: string | null;
  cd34PerKg: number;
  cd3PerKg: number;
  status: BagStatus;
  useReason?: BagUseReason | null;
  useNote?: string | null;
  purpose: BagPurpose;
  splitBatchId?: string | null;
  bagCellId?: string | null;
  createdDate: string;
}

/* ---------------------- Bag Movement ------------------------- */
export interface BagMovement {
  id: string;
  bagId: string;
  fromBagCellId?: string | null;
  toBagCellId?: string | null;
  action: string;
  createdDate: string;
}

/* --------------- Cryo: Tank → Rack → RackSlot → Box → BagCell --------- */
export interface Tank {
  id: string;
  name: string;
  createdDate: string;
}
export interface Rack {
  id: string;
  tankId: string;
  name: string;
  createdDate: string;
}
export interface Box {
  id: string;
  /** Raf slotu (RackSlots) id — kutu hangi fiziksel slotta. */
  slotId: string;
  name: string;
  createdDate: string;
}

/** Raf üzeri fiziksel slot (kutu oturur). */
export interface RackSlot {
  id: string;
  rackId: string;
  name: string;
  createdDate: string;
}

/** Kutu içi torba hücresi (grid hücresi). */
export interface BagCell {
  id: string;
  boxId: string;
  position: string;
  isOccupied: boolean;
  version: number;
  createdDate: string;
}

/* ------------------------ DliProduct ------------------------- */
export interface DliProduct {
  id: string;
  patientId: string;
  sessionId?: string | null;
  donorId?: string | null;
  date: string;
  volumeMl: number;
  wbc?: number | null;
  lymphocytePercent?: number | null;
  cd3Percent?: number | null;
  totalCd3: number;
  cd3PerKg: number;
  notes?: string | null;
  createdDate: string;
}

/* --------------------- Apheresis plan ------------------------ */
export interface ApheresisDayDto {
  day: number;
  date: string;
  cd34PerKg: number;
  cd3PerKg: number;
  cumulativeCd34: number;
  cumulativeCd3: number;
  sessionId?: string | null;
  isPlanned: boolean;

  wbcPre?: number | null;
  hgb?: number | null;
  hct?: number | null;
  plt?: number | null;
  preCd45Percent?: number | null;
  preCd34Percent?: number | null;
  preMhs?: number | null;

  wbcPost?: number | null;
  hgbPost?: number | null;
  hctPost?: number | null;
  pltPost?: number | null;

  volumeMl?: number | null;
  wbc?: number | null;
  cd34Percent?: number | null;
  cd45Percent?: number | null;
  cd3Percent?: number | null;
  lymphocytePercent?: number | null;
  mhs?: number | null;
}

export interface ApheresisPlanResponse {
  patientId: string;
  patientName: string;
  weightKg: number;
  transplantType: TransplantType;
  isAutologous: boolean;
  maxCollectionDays: number;
  targetCd34PerKg: number;
  idealCd34PerKg: number;
  completedDays: number;
  remainingDays: number;
  cumulativeCd34PerKg: number;
  cumulativeCd3PerKg: number;
  isSufficient: boolean;
  isOptimal: boolean;
  maxDaysReached: boolean;
  shouldContinue: boolean;
  nextDay?: number | null;
  nextPlannedDate?: string | null;
  status: string;
  recommendation: string;
  completedSessions: ApheresisDayDto[];
  forecastPlan: ApheresisDayDto[];
}

/* --------------------- Dashboard / Cryo ---------------------- */
export interface DashboardResponse {
  totalPatients: number;
  totalSessions: number;
  totalBags: number;
  storedBags: number;
  usedBags: number;
  frozenBags: number;
  reservedBags: number;
  discardedBags: number;
  totalSlots: number;
  occupiedSlots: number;
  emptySlots: number;
  totalCd34PerKg: number;
  totalCd3PerKg: number;
  riskStatus: string;
}

export interface CryoBagCellDto {
  id: string;
  position: string;
  isOccupied: boolean;
  bagId?: string | null;
  bagNumber?: number | null;
  status?: BagStatus | null;
  purpose?: BagPurpose | null;
  cd34PerKg?: number | null;
  cd3PerKg?: number | null;
  locationCode?: string | null;
}
export interface CryoBoxDto {
  id: string;
  name: string;
  bagCells: CryoBagCellDto[];
}
/** Raf slotu — altında kutular. */
export interface CryoRackSlotDto {
  id: string;
  name: string;
  boxes: CryoBoxDto[];
}
export interface CryoRackDto {
  id: string;
  name: string;
  slots: CryoRackSlotDto[];
}
export interface CryoTankDto {
  id: string;
  name: string;
  racks: CryoRackDto[];
}
export interface CryoGridResponse {
  tanks: CryoTankDto[];
}

/* ------------------------- Paging ---------------------------- */
export interface PageResponse<T> {
  items: T[];
  index: number;
  size: number;
  count: number;
  pages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

/* ------------------------- Split ----------------------------- */
/** Tek satır klinik eşikleri (API: GET/PUT /api/ClinicalSettings) */
export interface ClinicalSettingsSnapshot {
  id: string;
  sessionCd34Cd3Divisor: number;
  dliCd3CalculationDivisor: number;
  targetCd34AutologousPerKg: number;
  targetCd34AllogeneicPerKg: number;
  idealCd34AutologousPerKg: number;
  idealCd34AllogeneicPerKg: number;
  maxApheresisDaysAutologous: number;
  maxApheresisDaysAllogeneic: number;
  dliHighDoseCd3PerKgThreshold: number;
  productMinimumCd34PerKg: number;
  dashboardCd34LowThreshold: number;
  dashboardCd34ElevatedFloor: number;
  dashboardCd3HighRiskThreshold: number;
  dashboardCd3LowImmuneThreshold: number;
  dashboardCd3OptimalMin: number;
  dashboardCd3OptimalMax: number;
}

export type ClinicalSettingsUpdateBody = Omit<ClinicalSettingsSnapshot, "id">;

/* -------------------------- PLC / Modbus --------------------- */
export interface PlcSensorPointDto {
  id: string;
  sensorCode: string;
  deviceName: string;
  devicePrefix: string;
  dataLabel: string;
  modbusHost: string;
  modbusPort: number;
  slaveId: number;
  registerAddress: number;
  registerLength: number;
  scaleDivisor: number;
  pollIntervalSeconds: number;
  alarmLow?: number | null;
  alarmHigh?: number | null;
  alarmActive: boolean;
}

export interface PlcAlarmContactDto {
  id: string;
  devicePrefix?: string | null;
  displayName: string;
  phone: string;
  email?: string | null;
  smsEnabled: boolean;
  emailEnabled: boolean;
}

export interface SplitResponse {
  sessionId: string;
  patientId: string;
  splitBatchId: string;
  bagCount: number;
  perBagVolumeMl: number;
  perBagCd34PerKg: number;
  perBagCd3PerKg: number;
  cryoBagId: string;
  cryoBagCellId?: string | null;
  bags: Array<{
    bagId: string;
    bagNumber: number;
    volumeMl: number;
    cd34PerKg: number;
    cd3PerKg: number;
    purpose: BagPurpose;
    status: BagStatus;
    bagCellId?: string | null;
  }>;
}
