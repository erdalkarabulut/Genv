import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Bags, Dashboard, Patients, Sessions } from "@/lib/api";
import { Card, CardHeader } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Drawer } from "@/components/ui/Modal";
import { useEffect, useMemo, useState } from "react";
import type { Bag, CryoBoxDto, CryoBagCellDto } from "@/lib/types";
import {
  Hand,
  Layers,
  MousePointerClick,
  PackageOpen,
  Snowflake,
  X,
  UserRound,
  Beaker,
  Calendar,
  ExternalLink,
  StickyNote,
} from "lucide-react";
import { onCryo } from "@/lib/signalr";
import { cn, formatDate, formatNumber, shortId } from "@/lib/utils";
import { toast } from "sonner";
import { Link } from "react-router-dom";

const purposeTone: Record<string, string> = {
  Cryo: "bg-sky-500/30 border-sky-400/60",
  Infusion: "bg-emerald-500/30 border-emerald-400/60",
  Backup: "bg-amber-500/30 border-amber-400/60",
  QualityControl: "bg-fuchsia-500/30 border-fuchsia-400/60",
};

type PickUp =
  | { kind: "stored"; bagId: string; fromBagCellId: string; label: string }
  | { kind: "reserved"; bagId: string; label: string }
  | null;

export default function CryoGridPage() {
  const qc = useQueryClient();
  const grid = useQuery({ queryKey: ["cryo-grid"], queryFn: Dashboard.cryoGrid });
  const bagsQ = useQuery({ queryKey: ["bags", "all"], queryFn: () => Bags.list(0, 500) });
  const [activeTank, setActiveTank] = useState<number>(0);
  const [drawerCell, setDrawerCell] = useState<CryoBagCellDto | null>(null);
  const [pickup, setPickup] = useState<PickUp>(null);

  useEffect(() => {
    const unsubA = onCryo("BagStored", () => {
      qc.invalidateQueries({ queryKey: ["cryo-grid"] });
      qc.invalidateQueries({ queryKey: ["bags"] });
    });
    const unsubB = onCryo("BagMoved", () => {
      qc.invalidateQueries({ queryKey: ["cryo-grid"] });
      qc.invalidateQueries({ queryKey: ["bags"] });
    });
    const unsubC = onCryo("BagUsed", () => {
      qc.invalidateQueries({ queryKey: ["cryo-grid"] });
      qc.invalidateQueries({ queryKey: ["bags"] });
    });
    return () => { unsubA(); unsubB(); unsubC(); };
  }, [qc]);

  useEffect(() => {
    const handler = (e: KeyboardEvent) => { if (e.key === "Escape") setPickup(null); };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, []);

  const moveMut = useMutation({
    mutationFn: ({ bagId, bagCellId }: { bagId: string; bagCellId: string }) =>
      Bags.move(bagId, bagCellId),
    onSuccess: () => {
      toast.success("Torba taşındı");
      qc.invalidateQueries({ queryKey: ["cryo-grid"] });
      qc.invalidateQueries({ queryKey: ["bags"] });
      qc.invalidateQueries({ queryKey: ["dashboard"] });
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
      qc.invalidateQueries({ queryKey: ["dashboard"] });
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
      toast.info("Torba zaten bu hücrede.");
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
      /* toast already shown */
    }
  };

  const tank = grid.data?.tanks?.[activeTank];

  const tankStats = useMemo(() => {
    if (!tank) return { boxes: 0, occupied: 0, cells: 0 };
    let boxes = 0;
    let occupied = 0;
    let cells = 0;
    for (const r of tank.racks) {
      for (const slot of r.slots) {
        boxes += slot.boxes.length;
        for (const box of slot.boxes) {
          cells += box.bagCells.length;
          occupied += box.bagCells.filter((c) => c.isOccupied).length;
        }
      }
    }
    return { boxes, occupied, cells };
  }, [tank]);

  const reservedBags: Bag[] = useMemo(
    () =>
      (bagsQ.data?.items ?? []).filter(
        (b) => b.purpose === "Cryo" && b.status === "Reserved" && !b.bagCellId,
      ),
    [bagsQ.data],
  );

  return (
    <div className="space-y-6">
      <header className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Cryo Grid</h1>
          <p className="text-sm text-ink-muted mt-1 flex items-center gap-2 flex-wrap">
            Tank seçin: tüm rack, raf slotu, kutu ve hücreler tek ekranda.
            <span className="inline-flex items-center gap-1 text-[11px] text-ink-dim border border-line/60 rounded-md px-1.5 py-0.5">
              <Hand className="size-3" /> sürükle-bırak
            </span>
            <span className="inline-flex items-center gap-1 text-[11px] text-ink-dim border border-line/60 rounded-md px-1.5 py-0.5">
              <MousePointerClick className="size-3" /> tıkla-seç-boş slota bırak
            </span>
          </p>
        </div>
      </header>

      <div className="grid grid-cols-12 gap-4">
        {/* Tank list */}
        <Card className="col-span-12 lg:col-span-3 p-3">
          <div className="px-2 mb-2 text-xs uppercase tracking-wide text-ink-dim">Tanklar</div>
          {grid.isLoading && (
            <div className="space-y-2 px-2">
              {Array.from({ length: 3 }).map((_, i) => (
                <div key={i} className="skeleton h-12" />
              ))}
            </div>
          )}
          <div className="space-y-1">
            {grid.data?.tanks?.map((t, i) => {
              const total = t.racks.reduce(
                (a, r) =>
                  a +
                  r.slots.reduce(
                    (b, slot) =>
                      b +
                      slot.boxes.reduce((c, box) => c + box.bagCells.length, 0),
                    0,
                  ),
                0,
              );
              const occupied = t.racks.reduce(
                (a, r) =>
                  a +
                  r.slots.reduce(
                    (b, slot) =>
                      b +
                      slot.boxes.reduce(
                        (c, box) => c + box.bagCells.filter((cell) => cell.isOccupied).length,
                        0,
                      ),
                    0,
                  ),
                0,
              );
              return (
                <button
                  key={t.id}
                  onClick={() => setActiveTank(i)}
                  className={cn(
                    "w-full text-left rounded-xl px-3 py-2.5 transition border",
                    activeTank === i
                      ? "border-brand-500/40 bg-brand-500/10 text-ink shadow-glow"
                      : "border-transparent hover:bg-bg-elevated/60 text-ink-muted",
                  )}
                >
                  <div className="flex items-center justify-between">
                    <span className="font-medium text-sm">{t.name}</span>
                    <Snowflake className="size-3.5 text-accent-sky" />
                  </div>
                  <div className="text-[11px] text-ink-dim mt-0.5">
                    {occupied}/{total} hücre dolu · {t.racks.length} rack
                  </div>
                </button>
              );
            })}
          </div>

          {/* Reserved bags — drag to store */}
          <div className="mt-4 border-t border-line/60 pt-3">
            <div className="flex items-center justify-between px-2 mb-2">
              <div className="text-xs uppercase tracking-wide text-ink-dim">Depoya alınabilir</div>
              <Badge tone={reservedBags.length ? "amber" : "mint"} className="text-[10px]">
                {reservedBags.length}
              </Badge>
            </div>
            {reservedBags.length === 0 && (
              <p className="px-2 text-[11px] text-ink-dim">
                Cryo amaçlı, henüz hücreye yerleştirilmemiş torba yok.
              </p>
            )}
            <div className="space-y-1.5">
              {reservedBags.map((b) => {
                const selected = pickup?.kind === "reserved" && pickup.bagId === b.id;
                return (
                  <div
                    key={b.id}
                    draggable
                    onDragStart={(e) => {
                      setPickup({ kind: "reserved", bagId: b.id, label: `Bag #${b.bagNumber}` });
                      e.dataTransfer.setData("text/plain", b.id);
                      e.dataTransfer.effectAllowed = "move";
                    }}
                    onClick={() =>
                      setPickup(
                        selected
                          ? null
                          : { kind: "reserved", bagId: b.id, label: `Bag #${b.bagNumber}` },
                      )
                    }
                    className={cn(
                      "cursor-grab active:cursor-grabbing select-none rounded-lg border px-2.5 py-2 text-xs transition",
                      selected
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
          </div>
        </Card>

        {/* Rack tabs + boxes */}
        <Card className="col-span-12 lg:col-span-9 flex flex-col min-h-0">
          <CardHeader
            title={tank?.name ?? "Seçili tank yok"}
            subtitle={
              tank
                ? `${tank.racks.length} rack · ${tankStats.boxes} kutu · ${tankStats.occupied}/${tankStats.cells} hücre dolu`
                : undefined
            }
            right={<Badge tone="brand" dot>Live</Badge>}
          />

          {!grid.isLoading && tank && tank.racks.length === 0 && (
            <p className="text-sm text-ink-muted">Bu tankta rack yok.</p>
          )}

          {!grid.isLoading &&
            tank &&
            tank.racks.length > 0 &&
            tankStats.boxes === 0 && (
              <p className="text-sm text-ink-muted">
                Bu tanktaki rack&apos;lerde henüz kutu yok; önce envanterden raf slotu ve kutu ekleyin.
              </p>
            )}

          {tank && tankStats.boxes > 0 && (
            <div className="max-h-[min(72vh,calc(100vh-10rem))] overflow-auto pr-1 pb-2 mt-1 grid gap-6 auto-cols-fr grid-flow-col">
              {tank.racks.map((rack) => {
                const rackBoxCount = rack.slots.reduce((n, s) => n + s.boxes.length, 0);
                return (
                  <section key={rack.id} className="space-y-3 min-w-[16rem]">
                    <div className="sticky top-0 z-10 -mx-1 px-2 py-2.5 bg-bg-card/95 backdrop-blur-sm border-b border-line/60 flex flex-wrap items-center justify-between gap-2">
                      <div className="flex items-center gap-2 text-sm font-semibold text-ink">
                        <Layers className="size-4 text-brand-400 shrink-0" />
                        <span>{rack.name}</span>
                      </div>
                      <span className="text-[11px] text-ink-dim">
                        {rack.slots.length} raf slotu · {rackBoxCount} kutu
                      </span>
                    </div>
                    {rack.slots.length === 0 ? (
                      <p className="text-xs text-ink-muted pl-1">Bu rack&apos;te raf slotu yok.</p>
                    ) : (
                      <div className="grid grid-cols-1 gap-4">
                        {rack.slots.flatMap((rackSlot) =>
                          rackSlot.boxes.map((box) => (
                            <BoxGrid
                              key={`${rack.id}-${rackSlot.id}-${box.id}`}
                              rackName={rack.name}
                              rackSlotName={rackSlot.name}
                              box={box}
                              pickup={pickup}
                              onCell={(s) => {
                                if (pickup) {
                                  dropOnCell(s, pickup);
                                } else if (s.isOccupied) {
                                  setDrawerCell(s);
                                } else {
                                  toast.info("Önce bir torba seçin (veya sürükleyin).");
                                }
                              }}
                              onDropOnCell={(s, p) => dropOnCell(s, p)}
                              onPickUpStored={(s) => {
                                if (!s.bagId) return;
                                setPickup({
                                  kind: "stored",
                                  bagId: s.bagId,
                                  fromBagCellId: s.id,
                                  label: `Bag #${s.bagNumber ?? ""} @ ${s.position}`,
                                });
                              }}
                            />
                          )),
                        )}
                      </div>
                    )}
                  </section>
                );
              })}
            </div>
          )}
        </Card>
      </div>

      {/* Floating pickup banner */}
      {pickup && (
        <div className="fixed inset-x-0 bottom-4 z-40 flex justify-center pointer-events-none">
          <div className="pointer-events-auto flex items-center gap-3 rounded-full border border-brand-500/40 bg-bg-card/90 backdrop-blur px-4 py-2 shadow-glow">
            <Hand className="size-4 text-brand-400" />
            <div className="text-sm">
              <span className="text-ink-dim">Seçili:</span>{" "}
              <span className="font-medium">{pickup.label}</span>{" "}
              <span className="text-ink-dim">
                — boş bir hücreye sürükleyin veya tıklayın
                {pickup.kind === "reserved" && " (depoya alınır)"}
                {pickup.kind === "stored" && " (taşınır)"}
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

      <Drawer
        open={!!drawerCell}
        onClose={() => setDrawerCell(null)}
        title={`Hücre ${drawerCell?.position ?? ""}`}
      >
        {drawerCell && (
          <BagCellDetail
            cell={drawerCell}
            onAfter={() => setDrawerCell(null)}
            onMove={(s) => {
              setPickup({
                kind: "stored",
                bagId: s.bagId!,
                fromBagCellId: s.id,
                label: `Bag #${s.bagNumber ?? ""} @ ${s.position}`,
              });
              setDrawerCell(null);
            }}
          />
        )}
      </Drawer>
    </div>
  );
}

function BoxGrid({
  rackName,
  rackSlotName,
  box,
  pickup,
  onCell,
  onDropOnCell,
  onPickUpStored,
}: {
  rackName: string;
  rackSlotName: string;
  box: CryoBoxDto;
  pickup: PickUp;
  onCell: (s: CryoBagCellDto) => void;
  onDropOnCell: (s: CryoBagCellDto, p: PickUp) => void;
  onPickUpStored: (s: CryoBagCellDto) => void;
}) {
  const cols = useMemo(() => {
    const positions = box.bagCells.map((s) => s.position);
    const letters = new Set(positions.map((p) => p.replace(/[0-9]/g, "")));
    const numbers = new Set(positions.map((p) => Number(p.replace(/[A-Za-z]/g, ""))));
    return { letters: [...letters].sort(), numbers: [...numbers].sort((a, b) => a - b) };
  }, [box]);

  const cellMap = useMemo(() => {
    const m = new Map<string, CryoBagCellDto>();
    box.bagCells.forEach((s) => m.set(s.position, s));
    return m;
  }, [box]);

  const occupied = box.bagCells.filter((s) => s.isOccupied).length;

  return (
    <div className="rounded-2xl border border-line/60 bg-bg-elevated/30 p-3 w-fit">
      <div className="mb-2">
        <div className="text-sm font-medium leading-tight">
          {rackName} · {rackSlotName}
        </div>
        <div className="flex items-center gap-2 mt-0.5">
          <span className="text-[10px] text-ink-dim">{box.name}</span>
          <Badge
            tone={occupied === box.bagCells.length ? "rose" : occupied === 0 ? "mint" : "amber"}
            className="text-[10px]"
          >
            {occupied}/{box.bagCells.length}
          </Badge>
        </div>
      </div>
      <div
        className="grid gap-2 w-fit"
        style={{ gridTemplateColumns: `repeat(${cols.letters.length || 1}, 4.5rem)` }}
      >
        {cols.numbers.flatMap((N) =>
          cols.letters.map((L) => {
            const pos = `${L}${N}`;
            const slot = cellMap.get(pos);
            if (!slot)
              return (
                <div
                  key={pos}
                  className="size-[4.5rem] rounded bg-bg-subtle/40 border border-line/40"
                />
              );
            const isPickupSource =
              pickup?.kind === "stored" && pickup.fromBagCellId === slot.id;
            const isValidTarget = !slot.isOccupied && !!pickup;
            const tone = slot.isOccupied
              ? purposeTone[slot.purpose ?? "Cryo"] ?? "bg-rose-500/30 border-rose-400/60"
              : "bg-emerald-500/15 border-emerald-400/30 hover:bg-emerald-500/25";
            return (
              <button
                key={pos}
                draggable={slot.isOccupied}
                onDragStart={(e) => {
                  if (!slot.isOccupied || !slot.bagId) {
                    e.preventDefault();
                    return;
                  }
                  onPickUpStored(slot);
                  e.dataTransfer.setData("text/plain", slot.bagId);
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
                  onDropOnCell(slot, pickup);
                }}
                onClick={() => onCell(slot)}
                title={`${pos} ${slot.isOccupied ? "· dolu" : "· boş"}`}
                className={cn(
                  "relative size-[4.5rem] rounded-md border text-xs font-semibold tracking-tight transition",
                  tone,
                  isValidTarget && "ring-2 ring-brand-400/70 animate-pulseGlow",
                  isPickupSource && "ring-2 ring-amber-400/70",
                  pickup && slot.isOccupied && !isPickupSource && "opacity-40",
                )}
              >
                {pos}
              </button>
            );
          }),
        )}
      </div>
    </div>
  );
}

function BagCellDetail({
  cell,
  onAfter,
  onMove,
}: {
  cell: CryoBagCellDto;
  onAfter: () => void;
  onMove: (s: CryoBagCellDto) => void;
}) {
  const qc = useQueryClient();

  const bagQ = useQuery({
    queryKey: ["bag", cell.bagId],
    queryFn: () => Bags.byId(cell.bagId!),
    enabled: !!cell.bagId && cell.isOccupied,
  });
  const sessionQ = useQuery({
    queryKey: ["session", bagQ.data?.sessionId],
    queryFn: () => Sessions.byId(bagQ.data!.sessionId),
    enabled: !!bagQ.data?.sessionId,
  });
  const patientQ = useQuery({
    queryKey: ["patient", sessionQ.data?.patientId],
    queryFn: () => Patients.byId(sessionQ.data!.patientId),
    enabled: !!sessionQ.data?.patientId,
  });

  const useBag = async () => {
    if (!cell.bagId) return;
    try {
      await Bags.use(cell.bagId);
      toast.success("Torba kullanıldı, hücre boşaldı");
      qc.invalidateQueries({ queryKey: ["cryo-grid"] });
      qc.invalidateQueries({ queryKey: ["dashboard"] });
      qc.invalidateQueries({ queryKey: ["bags"] });
      qc.invalidateQueries({ queryKey: ["movements"] });
      onAfter();
    } catch {
      /* toast already shown */
    }
  };

  if (!cell.isOccupied) {
    return (
      <div className="space-y-3">
        <p className="text-sm text-ink-muted">Bu hücre boş.</p>
        <p className="text-xs text-ink-dim">
          Soldan bir &quot;Depoya alınabilir&quot; torbayı buraya sürükleyebilir ya da tıklayarak seçip boş
          hücreye bırakabilirsiniz. Alternatif: aferez seansını &quot;4 torbaya böl + Cryo&quot; ile otomatik
          yerleştirin.
        </p>
      </div>
    );
  }

  const bag = bagQ.data;
  const session = sessionQ.data;
  const patient = patientQ.data;
  const loading = bagQ.isLoading || sessionQ.isLoading || patientQ.isLoading;

  return (
    <div className="space-y-4">
      {/* Pozisyon + amaç rozeti */}
      <div className="rounded-xl bg-gradient-to-br from-brand-500/10 to-transparent border border-brand-500/30 p-3 flex items-center justify-between gap-3">
        <div>
          <div className="text-xs text-ink-dim">Pozisyon</div>
          <div className="text-lg font-semibold">{cell.position}</div>
        </div>
        <div className="flex items-center gap-1.5">
          <Badge tone={cell.purpose === "Cryo" ? "sky" : "brand"} dot>
            {purposeLabel(cell.purpose)}
          </Badge>
          <Badge tone={statusTone(cell.status)} dot>
            {cell.status ?? "—"}
          </Badge>
        </div>
      </div>

      {/* Hasta bloğu */}
      <div className="rounded-xl border border-line/60 bg-bg-elevated/40 p-3 space-y-2">
        <div className="flex items-center gap-2 text-[11px] uppercase tracking-wide text-ink-dim">
          <UserRound className="size-3.5" />
          Hasta bilgileri
        </div>
        {loading ? (
          <div className="skeleton h-16 rounded-lg" />
        ) : patient ? (
          <div>
            <div className="flex items-center justify-between gap-2">
              <Link
                to={`/patients/${patient.id}`}
                className="text-sm font-semibold text-brand-400 hover:text-brand-500 inline-flex items-center gap-1.5"
              >
                {patient.fullName}
                <ExternalLink className="size-3" />
              </Link>
              <Badge tone={patient.transplantType === "Autologous" ? "sky" : "brand"}>
                {patient.transplantType === "Autologous" ? "Otolog" : "Allojenik"}
              </Badge>
            </div>
            <div className="mt-1.5 grid grid-cols-2 gap-x-3 gap-y-0.5 text-[11px] text-ink-dim">
              <div>
                Protokol: <span className="text-ink">{patient.protocolNo ?? "—"}</span>
              </div>
              <div>
                Kilo: <span className="text-ink">{patient.weightKg} kg</span>
              </div>
              <div>
                Kan grubu: <span className="text-ink">{patient.bloodGroup ?? "—"}</span>
              </div>
              <div>
                Doğum: <span className="text-ink">{formatDate(patient.birthDate)}</span>
              </div>
              {patient.diagnosis && (
                <div className="col-span-2 truncate" title={patient.diagnosis}>
                  Tanı: <span className="text-ink">{patient.diagnosis}</span>
                </div>
              )}
            </div>
          </div>
        ) : (
          <p className="text-xs text-ink-dim">Hasta bilgisi alınamadı.</p>
        )}
      </div>

      {/* Seans bloğu */}
      <div className="rounded-xl border border-line/60 bg-bg-elevated/40 p-3 space-y-2">
        <div className="flex items-center gap-2 text-[11px] uppercase tracking-wide text-ink-dim">
          <Beaker className="size-3.5" />
          Aferez seansı
        </div>
        {loading ? (
          <div className="skeleton h-10 rounded-lg" />
        ) : session ? (
          <div className="grid grid-cols-2 gap-x-3 gap-y-0.5 text-[11px] text-ink-dim">
            <div className="inline-flex items-center gap-1">
              <Calendar className="size-3" />
              <span className="text-ink">
                Gün {session.day} · {formatDate(session.date)}
              </span>
            </div>
            <div>
              Hacim: <span className="text-ink">{formatNumber(session.volumeMl, 1)} ml</span>
            </div>
            <div>
              WBC: <span className="text-ink">{formatNumber(session.wbc, 2)}</span>
            </div>
            <div>
              %CD34: <span className="text-ink">{formatNumber(session.cd34Percent, 2)}</span>
            </div>
            <div>
              Ürün CD34/kg:{" "}
              <span className="text-ink">{formatNumber(session.cd34PerKg, 2)}</span>
            </div>
            {session.cd3PerKg > 0 && (
              <div>
                Ürün CD3/kg:{" "}
                <span className="text-ink">{formatNumber(session.cd3PerKg, 2)}</span>
              </div>
            )}
          </div>
        ) : (
          <p className="text-xs text-ink-dim">Seans bilgisi alınamadı.</p>
        )}
      </div>

      {/* Torba bilgi bloğu */}
      <div className="rounded-xl border border-line/60 bg-bg-elevated/40 p-3 space-y-2">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2 text-[11px] uppercase tracking-wide text-ink-dim">
            <PackageOpen className="size-3.5" />
            Torba bilgileri
          </div>
          {bag?.id && (
            <Link
              to={`/bags/${bag.id}`}
              className="text-[11px] text-brand-400 hover:text-brand-500 inline-flex items-center gap-1"
            >
              Detay <ExternalLink className="size-3" />
            </Link>
          )}
        </div>
        <div className="grid grid-cols-2 gap-2">
          <Field label="Bag #" value={cell.bagNumber?.toString() ?? "—"} />
          <Field label="Bag ID" value={shortId(cell.bagId)} />
          <Field label="Hacim" value={bag ? `${formatNumber(bag.volumeMl, 1)} ml` : "—"} />
          <Field
            label="Kaynak hacim"
            value={bag ? `${formatNumber(bag.sourceVolumeMl, 1)} ml` : "—"}
          />
          <Field label="WBC" value={bag?.wbc ? formatNumber(bag.wbc, 2) : "—"} />
          <Field label="%CD34" value={bag?.cd34Percent ? formatNumber(bag.cd34Percent, 2) : "—"} />
          <Field label="%CD45" value={bag?.cd45Percent ? formatNumber(bag.cd45Percent, 2) : "—"} />
          <Field label="%CD3" value={bag?.cd3Percent ? formatNumber(bag.cd3Percent, 2) : "—"} />
          <Field label="CD34/kg" value={formatNumber(cell.cd34PerKg ?? 0, 2)} />
          <Field label="CD3/kg" value={formatNumber(cell.cd3PerKg ?? 0, 2)} />
        </div>
        {bag?.compositionNote && (
          <div className="mt-1 rounded-lg bg-bg-subtle/60 border border-line/60 px-3 py-2 text-xs flex gap-2">
            <StickyNote className="size-3.5 text-ink-dim shrink-0 mt-0.5" />
            <div>
              <div className="text-[10px] uppercase tracking-wide text-ink-dim mb-0.5">
                Bölme notu
              </div>
              <div className="text-ink-muted">{bag.compositionNote}</div>
            </div>
          </div>
        )}
      </div>

      <div className="flex flex-wrap items-center justify-between gap-2">
        <Button variant="soft" onClick={() => onMove(cell)} icon={<Hand className="size-4" />}>
          Taşımak için seç
        </Button>
        <Button variant="danger" onClick={useBag}>
          Torbayı kullan (hücre boşalt)
        </Button>
      </div>
      <p className="text-[11px] text-ink-dim">
        İpucu: Hücreyi sürükleyerek başka boş bir hücreye de doğrudan taşıyabilirsiniz.
      </p>
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

function statusTone(s?: string | null): "brand" | "mint" | "amber" | "rose" | "sky" | "neutral" {
  switch (s) {
    case "Frozen":
      return "sky";
    case "Stored":
      return "brand";
    case "Reserved":
      return "amber";
    case "Used":
      return "mint";
    case "Discarded":
      return "rose";
    default:
      return "neutral";
  }
}

function Field({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg bg-bg-elevated/40 border border-line/60 px-3 py-2">
      <div className="text-[10px] uppercase tracking-wide text-ink-dim">{label}</div>
      <div className="text-sm font-medium mt-0.5">{value}</div>
    </div>
  );
}
