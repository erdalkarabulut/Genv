import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import { toast } from "sonner";

/** auth.tsx ile aynı anahtar — taşımayın ayrı tutun */
const JWT_STORAGE_KEY = "genv:jwt";

/** Sunucu cookie + GET /api/Auth/RefreshToken yanıtı */
interface AccessTokenDto {
  token: string;
  expirationDate: string;
}
import type {
  ApheresisPlanResponse,
  Bag,
  BagMovement,
  Box,
  CollectionSession,
  CryoGridResponse,
  DashboardResponse,
  DliProduct,
  Donor,
  PageResponse,
  Patient,
  PlcAlarmContactDto,
  PlcSensorPointDto,
  ClinicalSettingsSnapshot,
  ClinicalSettingsUpdateBody,
  Rack,
  Slot,
  SplitResponse,
  Tank,
} from "./types";

export const api = axios.create({
  baseURL: "/",
  headers: { "Content-Type": "application/json" },
  withCredentials: true,
});

/** Authorization header yok — yalnızca refresh çağrısı (çerez ile) */
const apiRefresh = axios.create({
  baseURL: "/",
  withCredentials: true,
});

let refreshPromise: Promise<string | null> | null = null;

function refreshAccessToken(): Promise<string | null> {
  if (!refreshPromise) {
    refreshPromise = apiRefresh
      .get<AccessTokenDto>("/api/Auth/RefreshToken")
      .then((r) => {
        const t = r.data?.token;
        if (t) {
          localStorage.setItem(JWT_STORAGE_KEY, t);
          window.dispatchEvent(new Event("genv-token-update"));
        }
        return t ?? null;
      })
      .catch(() => null)
      .finally(() => {
        refreshPromise = null;
      });
  }
  return refreshPromise;
}

function shouldSkipRefreshOn401(url?: string): boolean {
  if (!url) return false;
  return (
    url.includes("/api/Auth/Login") ||
    url.includes("/api/Auth/Register") ||
    url.includes("/api/Auth/RefreshToken")
  );
}

api.interceptors.request.use((config) => {
  const token = localStorage.getItem(JWT_STORAGE_KEY);
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (r) => r,
  async (err: AxiosError<any>) => {
    const status = err.response?.status;
    const original = err.config as (InternalAxiosRequestConfig & { _retry?: boolean }) | undefined;
    const url = original?.url ?? "";

    const data = err.response?.data;
    const message =
      (typeof data === "string" && data) ||
      data?.detail ||
      data?.title ||
      data?.Message ||
      err.message ||
      "Bilinmeyen hata";

    if (status === 401 && original && !original._retry && !shouldSkipRefreshOn401(url)) {
      original._retry = true;
      const newToken = await refreshAccessToken();
      if (newToken) {
        original.headers.Authorization = `Bearer ${newToken}`;
        return api.request(original);
      }
    }

    if (status === 401 && shouldSkipRefreshOn401(url)) {
      toast.error(message);
      return Promise.reject(err);
    }

    if (status === 401) {
      if (localStorage.getItem(JWT_STORAGE_KEY)) {
        localStorage.removeItem(JWT_STORAGE_KEY);
        toast.error("Oturumunuz sona erdi, lütfen tekrar giriş yapın.");
      }
      if (!location.pathname.startsWith("/login")) {
        const next = encodeURIComponent(location.pathname + location.search);
        location.replace(`/login?next=${next}`);
      }
    } else {
      toast.error(message);
    }
    return Promise.reject(err);
  },
);

/* --------------------------------------------------------------- */
/*                       Generic helpers                           */
/* --------------------------------------------------------------- */

const pageUrl = (path: string, page = 0, size = 50) =>
  `${path}?PageIndex=${page}&PageSize=${size}`;

export interface DynamicFilter {
  field: string;
  operator:
    | "eq"
    | "neq"
    | "lt"
    | "lte"
    | "gt"
    | "gte"
    | "isnull"
    | "isnotnull"
    | "startswith"
    | "endswith"
    | "contains"
    | "doesnotcontain";
  value?: string | number | boolean | null;
  logic?: "and" | "or";
  filters?: DynamicFilter[];
}

export interface DynamicSort {
  field: string;
  dir: "asc" | "desc";
}

export interface DynamicQuery {
  filter?: DynamicFilter;
  sort?: DynamicSort[];
}

const dynamic = <T>(path: string, page: number, size: number, q?: DynamicQuery) =>
  api
    .post<PageResponse<T>>(`${path}/by-dynamic?PageIndex=${page}&PageSize=${size}`, q ?? {})
    .then((r) => r.data);

/* --------------------------------------------------------------- */
/*                          Dashboard                              */
/* --------------------------------------------------------------- */
export const Dashboard = {
  get: () => api.get<DashboardResponse>("/api/Dashboard").then((r) => r.data),
  cryoGrid: () => api.get<CryoGridResponse>("/api/Dashboard/cryo-grid").then((r) => r.data),
};

/* --------------------------------------------------------------- */
/*                           Patients                              */
/* --------------------------------------------------------------- */
export const Patients = {
  list: (page = 0, size = 200) =>
    api.get<PageResponse<Patient>>(pageUrl("/api/Patients", page, size)).then((r) => r.data),
  byId: (id: string) => api.get<Patient>(`/api/Patients/${id}`).then((r) => r.data),
  create: (body: Partial<Patient>) => api.post<Patient>("/api/Patients", body).then((r) => r.data),
  update: (body: Partial<Patient> & { id: string }) =>
    api.put<Patient>("/api/Patients", body).then((r) => r.data),
  remove: (id: string) => api.delete(`/api/Patients/${id}`).then((r) => r.data),
  byDynamic: (q: DynamicQuery, page = 0, size = 200) =>
    dynamic<Patient>("/api/Patients", page, size, q),
};

/* --------------------------------------------------------------- */
/*                           Donors                                */
/* --------------------------------------------------------------- */
export const Donors = {
  list: (page = 0, size = 200) =>
    api.get<PageResponse<Donor>>(pageUrl("/api/Donors", page, size)).then((r) => r.data),
  byId: (id: string) => api.get<Donor>(`/api/Donors/${id}`).then((r) => r.data),
  create: (body: Partial<Donor>) => api.post<Donor>("/api/Donors", body).then((r) => r.data),
  update: (body: Partial<Donor> & { id: string }) =>
    api.put<Donor>("/api/Donors", body).then((r) => r.data),
  remove: (id: string) => api.delete(`/api/Donors/${id}`).then((r) => r.data),
};

/* --------------------------------------------------------------- */
/*                      Apheresis Plans                            */
/* --------------------------------------------------------------- */
export const ApheresisPlans = {
  byPatient: (patientId: string) =>
    api
      .get<ApheresisPlanResponse>(`/api/ApheresisPlans/${patientId}`)
      .then((r) => r.data),
};

/* --------------------------------------------------------------- */
/*                      Collection Sessions                        */
/* --------------------------------------------------------------- */
export const Sessions = {
  list: (page = 0, size = 200) =>
    api
      .get<PageResponse<CollectionSession>>(pageUrl("/api/CollectionSessions", page, size))
      .then((r) => r.data),
  byId: (id: string) =>
    api.get<CollectionSession>(`/api/CollectionSessions/${id}`).then((r) => r.data),
  create: (body: Partial<CollectionSession>) =>
    api.post<CollectionSession>("/api/CollectionSessions", body).then((r) => r.data),
  update: (body: Partial<CollectionSession> & { id: string }) =>
    api.put<CollectionSession>("/api/CollectionSessions", body).then((r) => r.data),
  calculate: (id: string) =>
    api
      .post<{ sessionId: string; cd34PerKg: number; cd3PerKg: number; patientWeightKg: number }>(
        `/api/CollectionSessions/${id}/calculate`,
        {},
      )
      .then((r) => r.data),
  remove: (id: string) =>
    api.delete(`/api/CollectionSessions/${id}`).then((r) => r.data),
  byPatient: (patientId: string, page = 0, size = 50) =>
    dynamic<CollectionSession>("/api/CollectionSessions", page, size, {
      filter: { field: "patientId", operator: "eq", value: patientId },
      sort: [{ field: "day", dir: "asc" }],
    }),
};

/* --------------------------------------------------------------- */
/*                            Bags                                 */
/* --------------------------------------------------------------- */
export const Bags = {
  list: (page = 0, size = 200) =>
    api.get<PageResponse<Bag>>(pageUrl("/api/Bags", page, size)).then((r) => r.data),
  byId: (id: string) => api.get<Bag>(`/api/Bags/${id}`).then((r) => r.data),
  create: (body: Partial<Bag>) => api.post<Bag>("/api/Bags", body).then((r) => r.data),
  update: (body: Partial<Bag> & { id: string }) =>
    api.put<Bag>("/api/Bags", body).then((r) => r.data),
  remove: (id: string) => api.delete(`/api/Bags/${id}`).then((r) => r.data),
  store: (bagId: string, slotId: string) =>
    api.post(`/api/Bags/store`, { bagId, slotId }).then((r) => r.data),
  move: (bagId: string, toSlotId: string) =>
    api
      .post(`/api/Bags/move`, { bagId, targetSlotId: toSlotId })
      .then((r) => r.data),
  use: (bagId: string) => api.post(`/api/Bags/use`, { bagId }).then((r) => r.data),
  split: (body: {
    sessionId: string;
    bagCount?: number;
    cryoSlotId?: string;
    autoPlaceCryo?: boolean;
    requireCumulativeSufficient?: boolean;
  }) => api.post<SplitResponse>(`/api/Bags/split`, body).then((r) => r.data),
  customSplit: (body: {
    sessionId: string;
    bags: Array<{
      volumeMl: number;
      wbc?: number;
      cd45Percent?: number;
      cd34Percent?: number;
      cd3Percent?: number;
      purpose: number; // BagPurpose: 0 Cryo, 1 Infusion, 2 Backup, 3 QC
      compositionNote?: string;
      freezeIntoSlotId?: string;
    }>;
    validateTotalVolume?: boolean;
  }) =>
    api
      .post<{
        sessionId: string;
        patientId: string;
        splitBatchId: string;
        totalVolumeMl: number;
        bagCount: number;
        bags: Array<{
          bagId: string;
          bagNumber: number;
          volumeMl: number;
          cd34PerKg: number;
          cd3PerKg: number;
          purpose: string;
          status: string;
          slotId?: string;
        }>;
      }>(`/api/Bags/custom-split`, body)
      .then((r) => r.data),
  freeze: (bagId: string, slotId?: string) =>
    api
      .post<{ bagId: string; slotId?: string; status: string }>(`/api/Bags/freeze`, {
        bagId,
        slotId,
      })
      .then((r) => r.data),
  bySession: (sessionId: string, page = 0, size = 50) =>
    dynamic<Bag>("/api/Bags", page, size, {
      filter: { field: "sessionId", operator: "eq", value: sessionId },
      sort: [{ field: "bagNumber", dir: "asc" }],
    }),
};

/* --------------------------------------------------------------- */
/*                       BagMovements                              */
/* --------------------------------------------------------------- */
export const Movements = {
  list: (page = 0, size = 200) =>
    api
      .get<PageResponse<BagMovement>>(pageUrl("/api/BagMovements", page, size))
      .then((r) => r.data),
  byBag: (bagId: string, page = 0, size = 100) =>
    dynamic<BagMovement>("/api/BagMovements", page, size, {
      filter: { field: "bagId", operator: "eq", value: bagId },
      sort: [{ field: "createdDate", dir: "desc" }],
    }),
  remove: (id: string) => api.delete(`/api/BagMovements/${id}`).then((r) => r.data),
};

/* --------------------------------------------------------------- */
/*                      Tanks / Racks / Boxes / Slots              */
/* --------------------------------------------------------------- */
export const Tanks = {
  list: (page = 0, size = 100) =>
    api.get<PageResponse<Tank>>(pageUrl("/api/Tanks", page, size)).then((r) => r.data),
  byId: (id: string) => api.get<Tank>(`/api/Tanks/${id}`).then((r) => r.data),
  create: (body: Partial<Tank>) => api.post<Tank>("/api/Tanks", body).then((r) => r.data),
  update: (body: Partial<Tank> & { id: string }) =>
    api.put<Tank>("/api/Tanks", body).then((r) => r.data),
  remove: (id: string) => api.delete(`/api/Tanks/${id}`).then((r) => r.data),
};

export const Racks = {
  list: (page = 0, size = 200) =>
    api.get<PageResponse<Rack>>(pageUrl("/api/Racks", page, size)).then((r) => r.data),
  create: (body: Partial<Rack>) => api.post<Rack>("/api/Racks", body).then((r) => r.data),
  update: (body: Partial<Rack> & { id: string }) =>
    api.put<Rack>("/api/Racks", body).then((r) => r.data),
  remove: (id: string) => api.delete(`/api/Racks/${id}`).then((r) => r.data),
};

export const Boxes = {
  list: (page = 0, size = 500) =>
    api.get<PageResponse<Box>>(pageUrl("/api/Boxes", page, size)).then((r) => r.data),
  create: (body: Partial<Box>) => api.post<Box>("/api/Boxes", body).then((r) => r.data),
  update: (body: Partial<Box> & { id: string }) =>
    api.put<Box>("/api/Boxes", body).then((r) => r.data),
  remove: (id: string) => api.delete(`/api/Boxes/${id}`).then((r) => r.data),
};

export const Slots = {
  list: (page = 0, size = 2000) =>
    api.get<PageResponse<Slot>>(pageUrl("/api/Slots", page, size)).then((r) => r.data),
  byId: (id: string) => api.get<Slot>(`/api/Slots/${id}`).then((r) => r.data),
  create: (body: Partial<Slot>) => api.post<Slot>("/api/Slots", body).then((r) => r.data),
  update: (body: Partial<Slot> & { id: string }) =>
    api.put<Slot>("/api/Slots", body).then((r) => r.data),
  remove: (id: string) => api.delete(`/api/Slots/${id}`).then((r) => r.data),
};

/* --------------------------------------------------------------- */
/*                        DLI Products                             */
/* --------------------------------------------------------------- */
export const DliProducts = {
  list: (page = 0, size = 200) =>
    api
      .get<PageResponse<DliProduct>>(pageUrl("/api/DliProducts", page, size))
      .then((r) => r.data),
  byId: (id: string) => api.get<DliProduct>(`/api/DliProducts/${id}`).then((r) => r.data),
  create: (body: Partial<DliProduct> & { cd3PerKgOverride?: number | null }) =>
    api.post<DliProduct>("/api/DliProducts", body).then((r) => r.data),
  update: (body: Partial<DliProduct> & { id: string; cd3PerKgOverride?: number | null }) =>
    api.put<DliProduct>("/api/DliProducts", body).then((r) => r.data),
  remove: (id: string) => api.delete(`/api/DliProducts/${id}`).then((r) => r.data),
};

/* --------------------------------------------------------------- */
/*                  Clinical thresholds (Admin PUT)               */
/* --------------------------------------------------------------- */
export const ClinicalSettingsApi = {
  get: () => api.get<ClinicalSettingsSnapshot>("/api/ClinicalSettings").then((r) => r.data),
  update: (body: ClinicalSettingsUpdateBody) =>
    api.put<ClinicalSettingsSnapshot>("/api/ClinicalSettings", body).then((r) => r.data),
};

/* --------------------------------------------------------------- */
/*                      PLC integration (Admin JWT)                */
/* --------------------------------------------------------------- */
export const PlcSensorPoints = {
  list: () => api.get<PlcSensorPointDto[]>("/api/PlcSensorPoints").then((r) => r.data),
  create: (body: Omit<PlcSensorPointDto, "id">) =>
    api.post<{ id: string }>("/api/PlcSensorPoints", body).then((r) => r.data),
  update: (body: PlcSensorPointDto) =>
    api.put<{ id: string }>("/api/PlcSensorPoints", body).then((r) => r.data),
  remove: (id: string) => api.delete(`/api/PlcSensorPoints/${id}`).then((r) => r.data),
};

export const PlcAlarmContacts = {
  list: () => api.get<PlcAlarmContactDto[]>("/api/PlcAlarmContacts").then((r) => r.data),
  create: (body: Omit<PlcAlarmContactDto, "id">) =>
    api.post<{ id: string }>("/api/PlcAlarmContacts", body).then((r) => r.data),
  update: (body: PlcAlarmContactDto) =>
    api.put<{ id: string }>("/api/PlcAlarmContacts", body).then((r) => r.data),
  remove: (id: string) => api.delete(`/api/PlcAlarmContacts/${id}`).then((r) => r.data),
};

/* --------------------------------------------------------------- */
/*                             Auth                                */
/* --------------------------------------------------------------- */
export interface LoginResponse {
  accessToken: { token: string; expirationDate: string };
  requiredAuthenticatorType?: string | null;
}

export const Auth = {
  login: (email: string, password: string) =>
    api
      .post<LoginResponse>(`/api/Auth/Login`, { email, password })
      .then((r) => r.data),
  register: (body: { email: string; password: string; firstName: string; lastName: string }) =>
    api.post(`/api/Auth/Register`, { userForRegisterDto: body }).then((r) => r.data),
};
