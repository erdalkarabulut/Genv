import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Bags, ClinicalSettingsApi, Dashboard, Donors, Patients, Sessions } from "@/lib/api";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { memo, useEffect, useMemo, useRef, useState } from "react";
import type {
  Bag,
  CollectionSession,
  CryoBagCellDto,
  CryoTankDto,
  Donor,
  Patient,
} from "@/lib/types";
import {
  Boxes,
  ChevronLeft,
  ChevronRight,
  Hand,
  Move3d,
  PackageOpen,
  RotateCcw,
  Search,
  Snowflake,
  X,
  ZoomIn,
  ZoomOut,
} from "lucide-react";
import { cn, calculateCellDose, formatNumber } from "@/lib/utils";
import { toast } from "sonner";
import { onCryo } from "@/lib/signalr";

type CellMeta = { bag?: Bag; session?: CollectionSession; patient?: Patient; donor?: Donor };
type FlatBox = {
  rackId: string;
  rackName: string;
  slotName: string;
  boxId: string;
  boxName: string;
  cells: CryoBagCellDto[];
};
type PickUp =
  | { kind: "stored"; bagId: string; fromBagCellId: string; label: string }
  | { kind: "reserved"; bagId: string; label: string }
  | null;
type PatientCellHit = {
  cell: CryoBagCellDto;
  rackId: string;
  rackName: string;
  rackIndex: number;
};

export default function Cryo3DPage() {
  const qc = useQueryClient();
  const grid = useQuery({ queryKey: ["cryo-grid"], queryFn: Dashboard.cryoGrid });
  const bagsQ = useQuery({ queryKey: ["bags", "all"], queryFn: () => Bags.list(0, 500) });
  const sessionsQ = useQuery({ queryKey: ["sessions", "for-cryo"], queryFn: () => Sessions.list(0, 1000) });
  const patientsQ = useQuery({ queryKey: ["patients", "for-cryo"], queryFn: () => Patients.list(0, 1000) });
  const donorsQ = useQuery({ queryKey: ["donors", "for-cryo"], queryFn: () => Donors.list(0, 1000) });
  const clinicalQ = useQuery({ queryKey: ["clinical-settings"], queryFn: () => ClinicalSettingsApi.get(), staleTime: 0 });
  const divisor = clinicalQ.data?.sessionCd34Cd3Divisor ?? 10000;

  const [activeTank, setActiveTank] = useState(0);
  const tank: CryoTankDto | undefined = grid.data?.tanks?.[activeTank];

  const [search, setSearch] = useState("");
  const [selectedPatientId, setSelectedPatientId] = useState<string | null>(null);
  const [pickup, setPickup] = useState<PickUp>(null);

  useEffect(() => {
    const offs = [
      onCryo("BagStored", () => {
        qc.invalidateQueries({ queryKey: ["cryo-grid"] });
        qc.invalidateQueries({ queryKey: ["bags"] });
      }),
      onCryo("BagMoved", () => {
        qc.invalidateQueries({ queryKey: ["cryo-grid"] });
        qc.invalidateQueries({ queryKey: ["bags"] });
      }),
      onCryo("BagUsed", () => {
        qc.invalidateQueries({ queryKey: ["cryo-grid"] });
        qc.invalidateQueries({ queryKey: ["bags"] });
      }),
    ];
    return () => offs.forEach((off) => off());
  }, [qc]);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") setPickup(null);
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, []);

  const moveMut = useMutation({
    mutationFn: ({ bagId, bagCellId }: { bagId: string; bagCellId: string }) =>
      Bags.move(bagId, bagCellId),
    onSuccess: () => {
      toast.success("Torba taşındı");
      qc.invalidateQueries({ queryKey: ["cryo-grid"] });
      qc.invalidateQueries({ queryKey: ["bags"] });
      qc.invalidateQueries({ queryKey: ["movements"] });
    },
  });
  const storeMut = useMutation({
    mutationFn: ({ bagId, bagCellId }: { bagId: string; bagCellId: string }) =>
      Bags.store(bagId, bagCellId),
    onSuccess: () => {
      toast.success("Torba depoya alındı");
      qc.invalidateQueries({ queryKey: ["cryo-grid"] });
      qc.invalidateQueries({ queryKey: ["bags"] });
      qc.invalidateQueries({ queryKey: ["movements"] });
    },
  });

  const dropOnCell = async (target: CryoBagCellDto, p: PickUp) => {
    if (!p) return;
    if (target.isOccupied) {
      toast.error("Bu hücre dolu — boş bir hücreye bırakın.");
      return;
    }
    if (p.kind === "stored" && p.fromBagCellId === target.id) {
      setPickup(null);
      return;
    }
    try {
      if (p.kind === "stored") {
        await moveMut.mutateAsync({ bagId: p.bagId, bagCellId: target.id });
      } else {
        await storeMut.mutateAsync({ bagId: p.bagId, bagCellId: target.id });
      }
      setPickup(null);
    } catch {
      /* toast handled */
    }
  };

  const cellMetaById = useMemo(() => {
    const meta = new Map<string, CellMeta>();
    const bagById = new Map<string, Bag>();
    (bagsQ.data?.items ?? []).forEach((b) => bagById.set(b.id, b));
    const sessionById = new Map<string, CollectionSession>();
    (sessionsQ.data?.items ?? []).forEach((s) => sessionById.set(s.id, s));
    const patientById = new Map<string, Patient>();
    (patientsQ.data?.items ?? []).forEach((p) => patientById.set(p.id, p));
    const donorById = new Map<string, Donor>();
    (donorsQ.data?.items ?? []).forEach((d) => donorById.set(d.id, d));
    if (!tank) return meta;
    for (const r of tank.racks) {
      for (const slot of r.slots) {
        for (const box of slot.boxes) {
          for (const c of box.bagCells) {
            if (!c.isOccupied || !c.bagId) continue;
            const bag = bagById.get(c.bagId);
            const session = bag?.sessionId ? sessionById.get(bag.sessionId) : undefined;
            const patient = session?.patientId ? patientById.get(session.patientId) : undefined;
            const donor = patient?.donorId ? donorById.get(patient.donorId) : undefined;
            meta.set(c.id, { bag, session, patient, donor });
          }
        }
      }
    }
    return meta;
  }, [tank, bagsQ.data, sessionsQ.data, patientsQ.data, donorsQ.data]);

  // 3D scene state
  const [yaw, setYaw] = useState(0);
  const [pitch, setPitch] = useState(-6);
  const [zoom, setZoom] = useState(1);
  const sceneRef = useRef<HTMLDivElement>(null);
  const dragRef = useRef<{ x: number; y: number; yaw: number; pitch: number } | null>(null);
  const autoSpinRef = useRef<number | null>(null);
  const [autoSpin, setAutoSpin] = useState(false);

  const onMouseDown = (e: React.MouseEvent) => {
    dragRef.current = { x: e.clientX, y: e.clientY, yaw, pitch };
    setAutoSpin(false);
  };
  useEffect(() => {
    const onMove = (e: MouseEvent) => {
      const d = dragRef.current;
      if (!d) return;
      const dx = e.clientX - d.x;
      const dy = e.clientY - d.y;
      setYaw(d.yaw + dx * 0.4);
      setPitch(Math.max(-75, Math.min(75, d.pitch - dy * 0.3)));
    };
    const onUp = () => {
      dragRef.current = null;
    };
    window.addEventListener("mousemove", onMove);
    window.addEventListener("mouseup", onUp);
    return () => {
      window.removeEventListener("mousemove", onMove);
      window.removeEventListener("mouseup", onUp);
    };
  }, []);

  useEffect(() => {
    if (!autoSpin) {
      if (autoSpinRef.current) cancelAnimationFrame(autoSpinRef.current);
      return;
    }
    let last = performance.now();
    const tick = (now: number) => {
      const dt = (now - last) / 1000;
      last = now;
      setYaw((y) => y + dt * 8);
      autoSpinRef.current = requestAnimationFrame(tick);
    };
    autoSpinRef.current = requestAnimationFrame(tick);
    return () => {
      if (autoSpinRef.current) cancelAnimationFrame(autoSpinRef.current);
    };
  }, [autoSpin]);

  const onWheel = (e: React.WheelEvent) => {
    e.preventDefault();
    setZoom((z) => Math.max(0.4, Math.min(2.4, z - e.deltaY * 0.0015)));
  };

  const flatBoxes: FlatBox[] = useMemo(() => {
    if (!tank) return [];
    const out: FlatBox[] = [];
    for (const r of tank.racks) {
      for (const s of r.slots) {
        for (const b of s.boxes) {
          out.push({
            rackId: r.id,
            rackName: r.name,
            slotName: s.name,
            boxId: b.id,
            boxName: b.name,
            cells: b.bagCells,
          });
        }
      }
    }
    return out;
  }, [tank]);

  // Build per-rack vertical stacks
  const rackColumns = useMemo(() => {
    if (!tank) return [];
    return tank.racks.map((r) => ({
      id: r.id,
      name: r.name,
      boxes: r.slots.flatMap((s) =>
        s.boxes.map((b) => ({
          slotName: s.name,
          boxId: b.id,
          boxName: b.name,
          cells: b.bagCells,
        })),
      ),
    }));
  }, [tank]);

  const stats = useMemo(() => {
    let cells = 0;
    let occupied = 0;
    for (const fb of flatBoxes) {
      cells += fb.cells.length;
      occupied += fb.cells.filter((c) => c.isOccupied).length;
    }
    return { boxes: flatBoxes.length, cells, occupied };
  }, [flatBoxes]);

  const [selected, setSelected] = useState<{ cell: CryoBagCellDto; meta?: CellMeta } | null>(null);

  // Patient → cells haritası (yalnızca aktif tank)
  const patientCells = useMemo(() => {
    const map = new Map<string, PatientCellHit[]>();
    if (!tank) return map;
    tank.racks.forEach((r, rackIndex) => {
      for (const slot of r.slots) {
        for (const box of slot.boxes) {
          for (const c of box.bagCells) {
            if (!c.isOccupied) continue;
            const meta = cellMetaById.get(c.id);
            const pid = meta?.patient?.id;
            if (!pid) continue;
            const arr = map.get(pid) ?? [];
            arr.push({ cell: c, rackId: r.id, rackName: r.name, rackIndex });
            map.set(pid, arr);
          }
        }
      }
    });
    return map;
  }, [tank, cellMetaById]);

  const searchText = search.trim().toLowerCase();
  const matchedPatients = useMemo(() => {
    const all = patientsQ.data?.items ?? [];
    if (!searchText) return [];
    return all
      .filter((p) => {
        const hay = `${p.fullName ?? ""} ${p.protocolNo ?? ""}`.toLowerCase();
        return hay.includes(searchText) && patientCells.has(p.id);
      })
      .slice(0, 20);
  }, [patientsQ.data, searchText, patientCells]);

  // Otomatik: arama tek hasta bulursa, onu seçili yap
  useEffect(() => {
    if (matchedPatients.length === 1) setSelectedPatientId(matchedPatients[0].id);
    else if (!searchText) setSelectedPatientId(null);
  }, [matchedPatients, searchText]);

  const selectedPatient = useMemo(
    () =>
      selectedPatientId
        ? (patientsQ.data?.items ?? []).find((p) => p.id === selectedPatientId)
        : undefined,
    [patientsQ.data, selectedPatientId],
  );
  const selectedPatientHits = selectedPatientId ? patientCells.get(selectedPatientId) ?? [] : [];
  const highlightedSet = useMemo(
    () => (selectedPatientId ? new Set(selectedPatientHits.map((h) => h.cell.id)) : null),
    [selectedPatientId, selectedPatientHits],
  );

  const reservedBags: Bag[] = useMemo(
    () =>
      (bagsQ.data?.items ?? []).filter(
        (b) => b.purpose === "Cryo" && b.status === "Reserved" && !b.bagCellId,
      ),
    [bagsQ.data],
  );

  const nRacks = rackColumns.length;
  // Sabit, geniş bir açı adımı: aynı anda ortada ~3 rack net görünür,
  // komşular biraz dönmüş şekilde durur, gerisi render edilmez.
  const angleStep = 16;
  const radius = 640;
  // Aktif rack'in iki yanına 2'şer rack daha → toplam 5 rack görünür.
  const windowSize = 2;
  // Carousel: yaw sürekli artıyor; fraksiyonel ofset rack pozisyonlarına uygulanır.
  // Böylece nRacks * angleStep 360'ın katı olmasa bile rack'ler asla çakışmaz.
  const fractional = -yaw / Math.max(0.001, angleStep);
  const rawIdx = Math.round(fractional);
  const currentIdx = nRacks > 0 ? ((rawIdx % nRacks) + nRacks) % nRacks : 0;
  const cellW = 130;
  const cellH = 64;
  const cellGap = 6;

  const reset = () => {
    setYaw(0);
    setPitch(-6);
    setZoom(1);
  };

  // En yakın temsile dönerek snap yap; -idx*angleStep'in yaw'a en yakın
  // (mod 360) eşdeğerini seç ki kısa yoldan dönsün.
  const rotateToRack = (idx: number) => {
    if (nRacks === 0) return;
    const targetBase = -idx * angleStep;
    const k = Math.round((yaw - targetBase) / 360);
    setYaw(targetBase + k * 360);
    setAutoSpin(false);
  };

  return (
    <div className="h-[calc(100vh-8.75rem)] flex flex-col gap-3 overflow-hidden">
      <div className="flex flex-wrap items-center justify-between gap-3 px-1">
        <div className="min-w-0">
          <h1 className="text-2xl font-semibold tracking-tight flex items-center gap-2">
            <Move3d className="size-5 text-brand-400" />
            Cryo 3D Görünümü
          </h1>
          <p className="text-xs text-ink-dim mt-0.5">
            Hasta ara, hücreye tıklayarak detay gör, torbayı sürükleyerek başka hücreye taşı.
          </p>
        </div>
        <div className="flex items-center gap-2 flex-wrap">
          <div className="relative">
            <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 size-3.5 text-ink-dim" />
            <input
              className="input h-9 pl-8 text-xs w-64"
              placeholder="Hasta adı veya protokol no..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          {grid.data?.tanks?.map((t, i) => (
            <button
              key={t.id}
              onClick={() => setActiveTank(i)}
              className={cn(
                "rounded-full px-3 py-1.5 text-xs border transition flex items-center gap-1.5",
                activeTank === i
                  ? "bg-brand-500/15 border-brand-500/40 text-ink shadow-glow"
                  : "border-line/60 text-ink-muted hover:border-brand-500/30",
              )}
            >
              <Snowflake className="size-3 text-accent-sky" />
              {t.name}
            </button>
          ))}
        </div>
      </div>

      {/* Hasta arama eşleşmeleri */}
      {searchText && matchedPatients.length > 0 && (
        <div className="flex flex-wrap items-center gap-1.5 px-1">
          {matchedPatients.map((p) => {
            const hits = patientCells.get(p.id) ?? [];
            const active = selectedPatientId === p.id;
            return (
              <button
                key={p.id}
                onClick={() => setSelectedPatientId(p.id)}
                className={cn(
                  "rounded-full px-2.5 py-1 text-[11px] border transition",
                  active
                    ? "bg-brand-500/15 border-brand-500/40 text-ink shadow-glow"
                    : "border-line/60 text-ink-muted hover:border-brand-500/30",
                )}
              >
                {p.fullName}{" "}
                <span className="text-ink-dim">({hits.length} hücre)</span>
              </button>
            );
          })}
        </div>
      )}

      {/* Seçili hastanın hücre listesi — tıklayınca rack öne gelir */}
      {selectedPatient && selectedPatientHits.length > 0 && (
        <div className="flex flex-wrap items-center gap-1.5 px-1">
          <span className="text-[11px] text-ink-dim">
            {selectedPatient.fullName} hücreleri:
          </span>
          {selectedPatientHits.map((h) => (
            <button
              key={h.cell.id}
              onClick={() => {
                rotateToRack(h.rackIndex);
                setSelected({ cell: h.cell, meta: cellMetaById.get(h.cell.id) });
              }}
              className="rounded-md px-2 py-1 text-[11px] border border-emerald-500/40 bg-emerald-500/10 text-emerald-200 hover:bg-emerald-500/20"
              title="Rack'i öne getir ve detayı aç"
            >
              {h.rackName} · {h.cell.position}
              {h.cell.bagNumber != null && (
                <span className="text-ink-dim"> · #{h.cell.bagNumber}</span>
              )}
            </button>
          ))}
        </div>
      )}


      <div className="grid grid-cols-12 gap-3 min-h-0 flex-1">
        {/* Sol panel: Depoya alınabilir torbalar */}
        <div className="col-span-12 lg:col-span-3 min-h-0 flex flex-col">
          <Card className="p-3 min-h-0 flex-1 overflow-auto">
            <div className="flex items-center justify-between px-1 mb-2">
              <div className="text-xs uppercase tracking-wide text-ink-dim">
                Depoya alınabilir
              </div>
              <Badge tone={reservedBags.length ? "amber" : "mint"} className="text-[10px]">
                {reservedBags.length}
              </Badge>
            </div>
            {reservedBags.length === 0 && (
              <p className="px-1 text-[11px] text-ink-dim">
                Cryo amaçlı, henüz hücreye yerleştirilmemiş torba yok.
              </p>
            )}
            <div className="space-y-1.5">
              {reservedBags.map((b) => {
                const sel = pickup?.kind === "reserved" && pickup.bagId === b.id;
                return (
                  <div
                    key={b.id}
                    draggable
                    onDragStart={(e) => {
                      setPickup({
                        kind: "reserved",
                        bagId: b.id,
                        label: `Bag #${b.bagNumber}`,
                      });
                      e.dataTransfer.setData("text/plain", b.id);
                      e.dataTransfer.effectAllowed = "move";
                    }}
                    onClick={() =>
                      setPickup(
                        sel
                          ? null
                          : {
                              kind: "reserved",
                              bagId: b.id,
                              label: `Bag #${b.bagNumber}`,
                            },
                      )
                    }
                    className={cn(
                      "cursor-grab active:cursor-grabbing select-none rounded-lg border px-2.5 py-2 text-xs transition",
                      sel
                        ? "border-sky-500/50 bg-sky-500/10 shadow-glow"
                        : "border-line/60 bg-bg-elevated/40 hover:border-sky-500/30",
                    )}
                    title="Sürükleyip boş bir hücreye bırakın"
                  >
                    <div className="flex items-center justify-between">
                      <span className="font-medium">Bag #{b.bagNumber}</span>
                      <PackageOpen className="size-3.5 text-accent-sky" />
                    </div>
                    <div className="text-[10px] text-ink-dim">
                      CD34 {formatNumber(b.cd34PerKg, 2)} · {b.volumeMl} mL
                    </div>
                  </div>
                );
              })}
            </div>
          </Card>
        </div>

        {/* Sağ: 3D sahne */}
        <Card className="col-span-12 lg:col-span-9 relative min-h-0 overflow-hidden p-0">
        {/* Backdrop grid */}
        <div className="absolute inset-0 pointer-events-none cryo3d-backdrop" />

        {/* HUD */}
        <div className="absolute z-20 top-3 left-3 flex flex-col gap-1.5">
          <Badge tone="brand" dot>
            {tank?.name ?? "Tank yok"}
          </Badge>
          <div className="rounded-lg border border-line/60 bg-bg-card/70 backdrop-blur px-2.5 py-1.5 text-[11px] text-ink-dim">
            <div>
              {nRacks} rack · {stats.boxes} kutu
            </div>
            <div>
              <span className="text-emerald-400">{stats.occupied}</span>
              <span className="text-ink-dim"> / {stats.cells} hücre dolu</span>
            </div>
          </div>
        </div>

        <div className="absolute z-20 top-3 right-3 flex flex-col items-end gap-2">
          <div className="flex items-center gap-1.5">
            <Button
              size="sm"
              variant="soft"
              onClick={() => setAutoSpin((s) => !s)}
              icon={<RotateCcw className="size-3.5" />}
            >
              {autoSpin ? "Dönmeyi durdur" : "Otomatik döndür"}
            </Button>
            <button
              onClick={() => setZoom((z) => Math.min(2.4, z + 0.15))}
              className="size-8 grid place-items-center rounded-lg border border-line/60 bg-bg-elevated/60 hover:border-brand-500/40 text-ink-muted hover:text-ink"
              title="Yakınlaştır"
            >
              <ZoomIn className="size-4" />
            </button>
            <button
              onClick={() => setZoom((z) => Math.max(0.4, z - 0.15))}
              className="size-8 grid place-items-center rounded-lg border border-line/60 bg-bg-elevated/60 hover:border-brand-500/40 text-ink-muted hover:text-ink"
              title="Uzaklaştır"
            >
              <ZoomOut className="size-4" />
            </button>
            <button
              onClick={reset}
              className="size-8 grid place-items-center rounded-lg border border-line/60 bg-bg-elevated/60 hover:border-brand-500/40 text-ink-muted hover:text-ink"
              title="Görünümü sıfırla"
            >
              <Boxes className="size-4" />
            </button>
          </div>
          <div className="rounded-lg border border-line/60 bg-bg-card/70 backdrop-blur px-2.5 py-1 text-[10px] text-ink-dim">
            Sürükle: döndür · Tekerlek: zoom
          </div>
        </div>

        {/* Rack navigator (alt orta) — döngüsel */}
        {nRacks > 0 && (
          <div className="absolute z-20 bottom-3 left-1/2 -translate-x-1/2 flex items-center gap-2">
            <button
              onClick={() => {
                setYaw((y) => y + angleStep);
                setAutoSpin(false);
              }}
              className="size-9 grid place-items-center rounded-full border border-line/60 bg-bg-card/80 backdrop-blur text-ink-muted hover:text-ink hover:border-brand-500/40"
              title="Önceki rack"
            >
              <ChevronLeft className="size-4" />
            </button>
            <div className="rounded-full border border-brand-500/30 bg-bg-card/80 backdrop-blur px-3 py-1.5 text-xs text-ink">
              <span className="font-semibold text-brand-300">
                {rackColumns[currentIdx]?.name ?? "—"}
              </span>
              <span className="text-ink-dim"> · {currentIdx + 1}/{nRacks}</span>
            </div>
            <button
              onClick={() => {
                setYaw((y) => y - angleStep);
                setAutoSpin(false);
              }}
              className="size-9 grid place-items-center rounded-full border border-line/60 bg-bg-card/80 backdrop-blur text-ink-muted hover:text-ink hover:border-brand-500/40"
              title="Sonraki rack"
            >
              <ChevronRight className="size-4" />
            </button>
          </div>
        )}

        {/* 3D scene */}
        <div
          ref={sceneRef}
          onMouseDown={onMouseDown}
          onWheel={onWheel}
          className="absolute inset-0 cursor-grab active:cursor-grabbing select-none"
          style={{ perspective: "1600px" }}
        >
          <div
            className="absolute left-1/2 top-1/2"
            style={{
              transformStyle: "preserve-3d",
              transform: `translate(-50%, -50%) scale(${zoom}) rotateX(${pitch}deg)`,
            }}
          >
            {/* Tank core cylinder (faux) */}
            <TankCore radius={radius} columns={rackColumns} />

            {/* Carousel: her rack'in fraksiyonel index'e olan döngüsel uzaklığı
                hesaplanır; ofset * angleStep ile sahneye yerleştirilir. */}
            {rackColumns.map((rack, i) => {
              if (nRacks === 0) return null;
              let off = i - fractional;
              const halfN = nRacks / 2;
              off = ((off + halfN) % nRacks + nRacks) % nRacks - halfN;
              if (Math.abs(off) > windowSize + 0.5) return null;
              const angle = off * angleStep;
              const isActive = Math.abs(off) < 0.5;
              return (
                <RackColumn
                  key={rack.id}
                  rack={rack}
                  angle={angle}
                  radius={radius}
                  isActive={isActive}
                  cellW={cellW}
                  cellH={cellH}
                  cellGap={cellGap}
                  cellMetaById={cellMetaById}
                  pickup={pickup}
                  highlightedCellIds={highlightedSet}
                  onCellClick={(cell, meta) => {
                    if (!isActive) rotateToRack(i);
                    if (pickup) {
                      dropOnCell(cell, pickup);
                    } else if (cell.isOccupied) {
                      setSelected({ cell, meta });
                    } else if (isActive) {
                      toast.info("Önce bir torba seçin (veya sürükleyin).");
                    }
                  }}
                  onDropOnCell={(cell, p) => dropOnCell(cell, p)}
                  onPickUpStored={(cell) => {
                    if (!cell.bagId) return;
                    setPickup({
                      kind: "stored",
                      bagId: cell.bagId,
                      fromBagCellId: cell.id,
                      label: `Bag #${cell.bagNumber ?? ""} @ ${cell.position}`,
                    });
                  }}
                />
              );
            })}

            {/* Floor disc */}
            <div
              className="absolute"
              style={{
                width: radius * 2 + 200,
                height: radius * 2 + 200,
                left: -(radius + 100),
                top: -(radius + 100),
                transform: "rotateX(90deg) translateZ(-300px)",
                background:
                  "radial-gradient(circle, rgba(56,189,248,0.18) 0%, rgba(56,189,248,0.05) 40%, transparent 70%)",
                borderRadius: "50%",
                border: "1px solid rgba(56,189,248,0.15)",
              }}
            />
          </div>
        </div>

        {grid.isLoading && (
          <div className="absolute inset-0 grid place-items-center text-ink-dim text-sm">
            Yükleniyor...
          </div>
        )}
        {!grid.isLoading && nRacks === 0 && (
          <div className="absolute inset-0 grid place-items-center text-ink-muted text-sm">
            Bu tankta gösterilecek rack yok.
          </div>
        )}
        </Card>
      </div>

      {/* Pickup banner */}
      {pickup && (
        <div className="fixed inset-x-0 bottom-4 z-40 flex justify-center pointer-events-none">
          <div className="pointer-events-auto flex items-center gap-3 rounded-full border border-brand-500/40 bg-bg-card/90 backdrop-blur px-4 py-2 shadow-glow">
            <Hand className="size-4 text-brand-400" />
            <div className="text-sm">
              <span className="text-ink-dim">Seçili:</span>{" "}
              <span className="font-medium">{pickup.label}</span>{" "}
              <span className="text-ink-dim">
                — boş bir hücreye sürükleyin veya tıklayın
              </span>
            </div>
            <button
              type="button"
              onClick={() => setPickup(null)}
              className="rounded-md p-1 text-ink-dim hover:text-ink hover:bg-bg-elevated"
              title="Vazgeç (Esc)"
            >
              <X className="size-4" />
            </button>
          </div>
        </div>
      )}

      {/* Hücre detay kartı — CryoGrid'deki hint stilinde, sahnenin sağında */}
      {selected && (
        <div className="fixed z-[60] top-[7.5rem] right-6 w-[380px] max-w-[92vw]">
          <div className="rounded-xl border border-brand-500/30 bg-bg-card/95 backdrop-blur p-2.5 shadow-glow">
            <div className="flex items-center justify-between px-1 pb-1.5">
              <div className="text-[11px] uppercase tracking-wide text-ink-dim">
                Hücre detayı
              </div>
              <div className="flex items-center gap-1">
                <button
                  onClick={() => {
                    if (!selected.cell.bagId) return;
                    setPickup({
                      kind: "stored",
                      bagId: selected.cell.bagId,
                      fromBagCellId: selected.cell.id,
                      label: `Bag #${selected.cell.bagNumber ?? ""} @ ${selected.cell.position}`,
                    });
                    setSelected(null);
                  }}
                  className="text-[10px] rounded-md px-2 py-1 border border-line/60 bg-bg-elevated/60 text-ink-muted hover:text-ink hover:border-brand-500/40"
                  title="Bu torbayı taşımak üzere seç"
                >
                  Taşı
                </button>
                <button
                  onClick={() => setSelected(null)}
                  className="rounded-md p-1 text-ink-dim hover:text-ink hover:bg-bg-elevated"
                  title="Kapat"
                >
                  <X className="size-3.5" />
                </button>
              </div>
            </div>
            <HoverHintCard cell={selected.cell} meta={selected.meta} divisor={divisor} />
          </div>
        </div>
      )}
    </div>
  );
}

function TankCore({
  radius,
  columns,
}: {
  radius: number;
  columns: Array<{ boxes: Array<{ cells: CryoBagCellDto[] }> }>;
}) {
  const height = Math.max(
    260,
    columns.reduce((m, c) => {
      const cells = c.boxes.reduce((a, b) => a + b.cells.length, 0);
      return Math.max(m, cells * 4 + c.boxes.length * 12);
    }, 0) + 80,
  );

  // Vertical cylinder using 12 flat panels (lighter for perf)
  const panels = 12;
  const inner = radius - 30;
  return (
    <div style={{ transformStyle: "preserve-3d" }}>
      {Array.from({ length: panels }).map((_, i) => {
        const angle = (i / panels) * 360;
        const w = (2 * Math.PI * inner) / panels + 2;
        return (
          <div
            key={i}
            className="absolute"
            style={{
              width: w,
              height,
              left: -w / 2,
              top: -height / 2,
              transform: `rotateY(${angle}deg) translateZ(${inner}px)`,
              background:
                "linear-gradient(180deg, rgba(56,189,248,0.07), rgba(56,189,248,0.02))",
              border: "1px solid rgba(125,211,252,0.10)",
            }}
          />
        );
      })}
      {/* Top cap */}
      <div
        className="absolute"
        style={{
          width: inner * 2,
          height: inner * 2,
          left: -inner,
          top: -inner,
          transform: `rotateX(90deg) translateZ(${height / 2}px)`,
          borderRadius: "50%",
          background:
            "radial-gradient(circle, rgba(125,211,252,0.18), rgba(125,211,252,0.04))",
          border: "1px solid rgba(125,211,252,0.25)",
        }}
      />
      {/* Bottom cap */}
      <div
        className="absolute"
        style={{
          width: inner * 2,
          height: inner * 2,
          left: -inner,
          top: -inner,
          transform: `rotateX(-90deg) translateZ(${height / 2}px)`,
          borderRadius: "50%",
          background:
            "radial-gradient(circle, rgba(56,189,248,0.20), rgba(56,189,248,0.02))",
          border: "1px solid rgba(56,189,248,0.20)",
        }}
      />
    </div>
  );
}

const RackColumn = memo(RackColumnImpl);

function RackColumnImpl({
  rack,
  angle,
  radius,
  isActive,
  cellW,
  cellH,
  cellGap,
  cellMetaById,
  pickup,
  highlightedCellIds,
  onCellClick,
  onDropOnCell,
  onPickUpStored,
}: {
  rack: { id: string; name: string; boxes: Array<{ slotName: string; boxId: string; boxName: string; cells: CryoBagCellDto[] }> };
  angle: number;
  radius: number;
  isActive: boolean;
  cellW: number;
  cellH: number;
  cellGap: number;
  cellMetaById: Map<string, CellMeta>;
  pickup: PickUp;
  highlightedCellIds: Set<string> | null;
  onCellClick: (cell: CryoBagCellDto, meta: CellMeta | undefined) => void;
  onDropOnCell: (cell: CryoBagCellDto, p: PickUp) => void;
  onPickUpStored: (cell: CryoBagCellDto) => void;
}) {
  // Layout: 1 raf -> 1 box -> 4 hücre, hepsi alt alta tek sütunda.
  const cols = 1;
  const boxBlocks = rack.boxes;
  const blockGap = 14;
  const headerH = 28;

  const blockHeights = boxBlocks.map((b) => {
    const rows = Math.max(1, Math.ceil(b.cells.length / cols));
    return rows * (cellH + cellGap) + headerH + 6;
  });
  const totalHeight =
    blockHeights.reduce((a, b) => a + b, 0) +
    Math.max(0, boxBlocks.length - 1) * blockGap +
    20;

  // Iç ızgara + box yan boşluğu (px-2 → 16) + rack iç boşluğu (p-3 → 24)
  const width = cols * (cellW + cellGap) + 16 + 24;

  return (
    <div
      className="absolute"
      style={{
        left: -width / 2,
        top: -totalHeight / 2,
        width,
        height: totalHeight,
        transformStyle: "preserve-3d",
        transform: `rotateY(${angle}deg) translateZ(${radius}px)`,
      }}
    >
      {/* Rack arka paneli */}
      <div
        className="absolute inset-0 rounded-xl"
        style={{
          background: isActive
            ? "linear-gradient(180deg, rgba(15,23,42,0.94), rgba(15,23,42,0.82))"
            : "linear-gradient(180deg, rgba(15,23,42,0.55), rgba(15,23,42,0.40))",
          border: `1px solid ${isActive ? "rgba(125,211,252,0.45)" : "rgba(125,211,252,0.15)"}`,
          boxShadow: isActive
            ? "0 0 36px rgba(56,189,248,0.25), inset 0 0 36px rgba(2,6,23,0.65)"
            : "inset 0 0 24px rgba(2,6,23,0.55)",
          transform: "translateZ(-3px)",
          opacity: isActive ? 1 : 0.55,
        }}
      />
      {/* Rack ismi — yalnız ortadaki (aktif) rack için */}
      {isActive && (
        <div
          className="absolute left-0 right-0 -top-9 text-center text-base font-semibold tracking-wide text-sky-300 px-2"
          style={{ textShadow: "0 0 10px rgba(56,189,248,0.7)" }}
        >
          {rack.name}
        </div>
      )}

      {/* Box plakaları */}
      <div className="absolute inset-0 p-3 flex flex-col gap-3">
        {boxBlocks.map((b) => {
          const rows = Math.max(1, Math.ceil(b.cells.length / cols));
          const blockHeight = rows * (cellH + cellGap) + headerH + 4;
          const occupied = b.cells.filter((c) => c.isOccupied).length;
          return (
            <div
              key={b.boxId}
              className="relative rounded-lg"
              style={{
                height: blockHeight,
                background:
                  "linear-gradient(180deg, rgba(30,41,59,0.9), rgba(15,23,42,0.75))",
                border: "1px solid rgba(125,211,252,0.25)",
                boxShadow: "0 0 10px rgba(56,189,248,0.12)",
              }}
            >
              <div className="flex items-center justify-between px-2 pt-1.5 text-[11px] text-ink-dim">
                <span className="truncate font-medium">
                  {b.slotName} · {b.boxName}
                </span>
                <span
                  className={cn(
                    "px-1.5 py-0.5 rounded text-[10px] font-semibold",
                    occupied === b.cells.length
                      ? "bg-rose-500/25 text-rose-200"
                      : occupied === 0
                        ? "bg-emerald-500/25 text-emerald-200"
                        : "bg-amber-500/25 text-amber-200",
                  )}
                >
                  {occupied}/{b.cells.length}
                </span>
              </div>
              <div
                className="grid px-2 pb-2 pt-1.5"
                style={{
                  gridTemplateColumns: `repeat(${cols}, ${cellW}px)`,
                  gap: cellGap,
                }}
              >
                {b.cells.map((c) => {
                  const meta = cellMetaById.get(c.id);
                  const name = meta?.patient?.fullName;
                  const isPickupSource =
                    pickup?.kind === "stored" && pickup.fromBagCellId === c.id;
                  const isValidTarget = !c.isOccupied && !!pickup;
                  const isHighlighted = highlightedCellIds?.has(c.id) ?? false;
                  return (
                    <div
                      key={c.id}
                      draggable={c.isOccupied}
                      onMouseDown={(e) => e.stopPropagation()}
                      onDragStart={(e) => {
                        if (!c.isOccupied || !c.bagId) {
                          e.preventDefault();
                          return;
                        }
                        onPickUpStored(c);
                        e.dataTransfer.setData("text/plain", c.bagId);
                        e.dataTransfer.effectAllowed = "move";
                      }}
                      onDragOver={(e) => {
                        if (isValidTarget) {
                          e.preventDefault();
                          e.dataTransfer.dropEffect = "move";
                        }
                      }}
                      onDrop={(e) => {
                        if (!isValidTarget) return;
                        e.preventDefault();
                        onDropOnCell(c, pickup);
                      }}
                      onClick={(e) => {
                        e.stopPropagation();
                        onCellClick(c, meta);
                      }}
                      title={c.isOccupied ? name ?? `Bag #${c.bagNumber ?? ""}` : c.position}
                      className={cn(
                        "rounded-md flex items-center justify-center text-center select-none",
                        c.isOccupied
                          ? "bg-gradient-to-br from-emerald-500/85 to-emerald-700/90 border border-emerald-300/70 text-white shadow-[0_0_10px_rgba(16,185,129,0.35)] hover:shadow-[0_0_18px_rgba(16,185,129,0.75)] cursor-grab active:cursor-grabbing"
                          : "bg-slate-700/30 border border-slate-500/30 cursor-pointer",
                        isValidTarget && "ring-2 ring-brand-400/80",
                        isPickupSource && "ring-2 ring-amber-400/80",
                        pickup && c.isOccupied && !isPickupSource && "opacity-50",
                        isHighlighted && "ring-2 ring-fuchsia-400/80",
                      )}
                      style={{
                        width: cellW,
                        height: cellH,
                        padding: 6,
                      }}
                    >
                      {c.isOccupied && (
                        <span
                          className="font-semibold leading-tight px-1 break-words"
                          style={{ fontSize: 13, lineHeight: 1.2 }}
                        >
                          {name ?? (c.bagNumber != null ? `Bag #${c.bagNumber}` : "—")}
                        </span>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

function HoverHintCard({
  cell,
  meta,
  divisor,
}: {
  cell: CryoBagCellDto;
  meta?: CellMeta;
  divisor: number;
}) {
  const patientName = meta?.patient?.fullName ?? "Bilinmiyor";
  const donorName = meta?.donor?.fullName ?? "—";
  const day = meta?.session?.day ?? "—";
  const bagNo = cell.bagNumber ?? meta?.bag?.bagNumber ?? "—";
  const volume = meta?.bag?.volumeMl != null ? `${formatNumber(meta.bag.volumeMl, 1)} ml` : "—";
  const sourceVolume =
    meta?.bag?.sourceVolumeMl != null ? `${formatNumber(meta.bag.sourceVolumeMl, 1)} ml` : "—";
  const wbc = meta?.bag?.wbc != null ? formatNumber(meta.bag.wbc, 2) : "—";
  const cd34Pct = meta?.bag?.cd34Percent != null ? formatNumber(meta.bag.cd34Percent, 2) : "—";
  const cd45Pct = meta?.bag?.cd45Percent != null ? formatNumber(meta.bag.cd45Percent, 2) : "—";
  const cd3Pct = meta?.bag?.cd3Percent != null ? formatNumber(meta.bag.cd3Percent, 2) : "—";
  const purpose = purposeLabel(cell.purpose);
  const status = cell.status ?? "—";
  const note = meta?.bag?.compositionNote?.trim();

  const weightKg = meta?.patient?.weightKg ?? 0;
  const rawCd34 =
    cell.cd34PerKg != null
      ? cell.cd34PerKg
      : weightKg > 0
        ? calculateCellDose(
            {
              volumeMl: meta?.bag?.volumeMl ?? 0,
              wbc: meta?.bag?.wbc ?? 0,
              cd45Percent: meta?.bag?.cd45Percent ?? 0,
              cd34Percent: meta?.bag?.cd34Percent ?? 0,
              cd3Percent: meta?.bag?.cd3Percent ?? 0,
            },
            weightKg,
            divisor,
          ).cd34PerKg
        : 0;
  const rawCd3 =
    cell.cd3PerKg != null
      ? cell.cd3PerKg
      : weightKg > 0
        ? calculateCellDose(
            {
              volumeMl: meta?.bag?.volumeMl ?? 0,
              wbc: meta?.bag?.wbc ?? 0,
              cd45Percent: meta?.bag?.cd45Percent ?? 0,
              cd34Percent: meta?.bag?.cd34Percent ?? 0,
              cd3Percent: meta?.bag?.cd3Percent ?? 0,
            },
            weightKg,
            divisor,
          ).cd3PerKg
        : 0;
  const cd34 = rawCd34 > 0 ? formatNumber(rawCd34, 0) : "—";
  const cd3 = rawCd3 > 0 ? formatNumber(rawCd3, 0) : "—";

  return (
    <div className="space-y-1.5">
      <div className="rounded-lg border border-brand-500/30 bg-gradient-to-br from-brand-500/10 to-transparent p-1.5">
        <div className="text-[10px] text-ink-dim">Konum</div>
        <div className="text-xs font-semibold">{cell.locationCode ?? cell.position}</div>
      </div>

      <div className="grid grid-cols-3 gap-1.5">
        <Field label="Hasta" value={patientName} />
        <Field label="Donor" value={donorName} />
        <Field label="Ürün" value={`Bag #${bagNo} · Gün ${day}`} />
        <Field label="Durum / Amaç" value={`${status} · ${purpose}`} />
        <Field label="Hacim" value={volume} />
        <Field label="Kaynak Hacim" value={sourceVolume} />
        <Field label="WBC" value={wbc} />
        <Field label="%CD34 / %CD45" value={`${cd34Pct} / ${cd45Pct}`} />
        <Field label="%CD3" value={cd3Pct} />
        <Field label="CD34/kg" value={cd34} />
        <Field label="CD3/kg" value={cd3} />
      </div>

      {note && (
        <div className="rounded-lg border border-line/60 bg-bg-elevated/50 px-2 py-1">
          <div className="text-[10px] uppercase tracking-wide text-ink-dim">Not</div>
          <div className="text-[11px] text-ink-muted line-clamp-2">{note}</div>
        </div>
      )}
    </div>
  );
}

function purposeLabel(p?: string | null) {
  switch (p) {
    case "Cryo":
      return "Cryo";
    case "Infusion":
      return "Infüzyon";
    case "Backup":
      return "Yedek";
    case "QualityControl":
      return "QC";
    default:
      return "—";
  }
}

function Field({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg bg-bg-elevated/40 border border-line/60 px-2 py-1.5">
      <div className="text-[10px] uppercase tracking-wide text-ink-dim">{label}</div>
      <div className="text-xs font-medium text-ink mt-0.5 leading-tight">{value}</div>
    </div>
  );
}
