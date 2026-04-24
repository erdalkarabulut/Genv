# GenV Frontend — Cryo & Apheresis Suite

React + Vite + TypeScript + Tailwind frontend for the GenV Stem Cell & Cryo Management platform.

## Stack

- **Vite** + **React 18** + **TypeScript**
- **Tailwind CSS** (dark-first design system)
- **TanStack Query** for server state & cache
- **Axios** HTTP client with JWT interceptor (`localStorage["genv:jwt"]`)
- **@microsoft/signalr** for real-time cryo events (`/hubs/cryo`)
- **Recharts** for analytics, **Framer Motion** for subtle motion
- **sonner** for toasts, **lucide-react** for icons

## Development

```bash
cd frontend
npm install
npm run dev        # http://localhost:5173
```

Vite dev-server proxies `/api` and `/hubs` to the .NET API at `http://localhost:5278`.
Start the API separately:

```bash
dotnet run --project src/genVApi/WebAPI/WebAPI.csproj
```

In **Development** the API injects a synthetic `Admin` principal so demo flows work
without authenticating against `/api/Auth`. In production you should log in via
`/api/Auth/Login` and place the returned JWT in `localStorage["genv:jwt"]`.

## Build

```bash
npm run build      # runs tsc -b && vite build
npm run preview
```

## Structure

```
src/
  components/
    Layout.tsx         # app shell, sidebar, live connection pill
    ui/                # Button, Card, Badge, Input, Modal, Drawer, EmptyState
  lib/
    api.ts             # axios client + typed endpoints
    signalr.ts         # singleton Cryo hub connection
    types.ts           # shared DTO types mirroring API contracts
    utils.ts           # cn, formatters
  pages/
    DashboardPage.tsx       # totals, risk gauge, status charts, recent patients
    PatientsPage.tsx        # list + create
    PatientDetailPage.tsx   # apheresis plan, timeline, cumulative chart,
                            # new-session form, 4-bag split + cryo placement
    CryoGridPage.tsx        # tank → rack → box → slot visual + slot drawer
    BagsPage.tsx            # all bags, status/purpose filters + use action
```

## Design principles

- Dark-first surface, glassmorphism cards, soft radial brand glow.
- Data-dense dashboards with muted chrome, colored status accents only.
- Motion is used sparingly for focus (modals, drawer, live pill).
- Medical safety: the UI only presents data; every recommendation line ends with
  "Klinik değerlendirme gerekir" when thresholds require judgement.
