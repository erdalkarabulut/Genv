import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Bags, ClinicalSettingsApi, Dashboard, Donors, Patients, Sessions } from "@/lib/api";
import { Card, CardHeader } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Drawer } from "@/components/ui/Modal";
import { Pagination } from "@/components/ui/Pagination";
import { UseBagDialog } from "@/components/bags/UseBagDialog";
import { useEffect, useMemo, useState } from "react";
import type {
  Bag,
  BagUseReason,
  CollectionSession,
  CryoBagCellDto,
  CryoBoxDto,
  CryoTankDto,
  Donor,
  Patient,
} from "@/lib/types";
import {
  Hand,
  Layers,
  PackageOpen,
  Snowflake,
  X,
  UserRound,
  Beaker,
  Calendar,
  ExternalLink,
  Search,
  StickyNote,
  Plus,
} from "lucide-react";
import { onCryo } from "@/lib/signalr";
import { cn, formatDate, formatNumber, shortId, calculateCellDose } from "@/lib/utils";
import { toast } from "sonner";
import { Link, useNavigate } from "react-router-dom";

type PickUp =
  | { kind: "stored"; bagId: string; fromBagCellId: string; label: string }
  | { kind: "reserved"; bagId: string; label: string }
  | null;

type CellMeta = { bag?: Bag; session?: CollectionSession; patient?: Patient; donor?: Donor };

type HoverHintState = {
  cell: CryoBagCellDto;
  meta?: CellMeta;
  left: number;
  top: number;
};

export default function CryoGridPage() {
  const qc = useQueryClient();
  const navigate = useNavigate();
  const grid = useQuery({ queryKey: ["cryo-grid"], queryFn: Dashboard.cryoGrid });
  const bagsQ = useQuery({ queryKey: ["bags", "all"], queryFn: () => Bags.list(0, 500) });
  const sessionsQ = useQuery({ queryKey: ["sessions", "for-cryo"], queryFn: () => Sessions.list(0, 1000) });
  const patientsQ = useQuery({ queryKey: ["patients", "for-cryo"], queryFn: () => Patients.list(0, 1000) });
  const donorsQ = useQuery({ queryKey: ["donors", "for-cryo"], queryFn: () => Donors.list(0, 1000) });
  const clinicalQ = useQuery({ queryKey: ["clinical-settings"], queryFn: () => ClinicalSettingsApi.get(), staleTime: 0 });
  const divisor = clinicalQ.data?.sessionCd34Cd3Divisor ?? 10000;
  const [activeTank, setActiveTank] = useState<number>(0);
  const [rackPage, setRackPage] = useState(0);
  const [drawerCell, setDrawerCell] = useState<CryoBagCellDto | null>(null);
  const [pickup, setPickup] = useState<PickUp>(null);
  const [hoverHint, setHoverHint] = useState<HoverHintState | null>(null);
  const [search, setSearch] = useState("");

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
  const searchText = search.trim().toLowerCase();

  const bagById = useMemo(() => {
    const m = new Map<string, Bag>();
    (bagsQ.data?.items ?? []).forEach((b) => m.set(b.id, b));
    return m;
  }, [bagsQ.data]);

  const sessionById = useMemo(() => {
    const m = new Map<string, CollectionSession>();
    (sessionsQ.data?.items ?? []).forEach((s) => m.set(s.id, s));
    return m;
  }, [sessionsQ.data]);

  const patientById = useMemo(() => {
    const m = new Map<string, Patient>();
    (patientsQ.data?.items ?? []).forEach((p) => m.set(p.id, p));
    return m;
  }, [patientsQ.data]);

  const donorById = useMemo(() => {
    const m = new Map<string, Donor>();
    (donorsQ.data?.items ?? []).forEach((d) => m.set(d.id, d));
    return m;
  }, [donorsQ.data]);

  const cellMetaById = useMemo(() => {
    const meta = new Map<string, CellMeta>();
    if (!grid.data?.tanks) return meta;
    for (const t of grid.data.tanks) {
      for (const r of t.racks) {
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
    }
    return meta;
  }, [grid.data, bagById, sessionById, patientById, donorById]);

  const matchedCellIds = useMemo(() => {
    if (!searchText) return null;
    const ids = new Set<string>();
    for (const [cellId, m] of cellMetaById.entries()) {
      const bagNo = m.bag?.bagNumber ? String(m.bag.bagNumber) : "";
      const hay = [
        m.patient?.fullName ?? "",
        m.patient?.protocolNo ?? "",
        m.donor?.fullName ?? "",
        m.donor?.relation ?? "",
        bagNo,
      ]
        .join(" ")
        .toLowerCase();
      if (hay.includes(searchText)) ids.add(cellId);
    }
    return ids;
  }, [searchText, cellMetaById]);

  const visibleTank: CryoTankDto | undefined = useMemo(() => {
    if (!tank) return undefined;
    if (!matchedCellIds) return tank;
    return {
      ...tank,
      racks: tank.racks
        .map((r) => ({
          ...r,
          slots: r.slots
            .map((s) => ({
              ...s,
              boxes: s.boxes
                .map((b) => ({
                  ...b,
                  bagCells: b.bagCells.filter((c) => matchedCellIds.has(c.id)),
                }))
                .filter((b) => b.bagCells.length > 0),
            }))
            .filter((s) => s.boxes.length > 0),
        }))
        .filter((r) => r.slots.length > 0),
    };
  }, [tank, matchedCellIds]);

  const visibleStats = useMemo(() => {
    if (!visibleTank) return { boxes: 0, occupied: 0, cells: 0 };
    let boxes = 0;
    let occupied = 0;
    let cells = 0;
    for (const r of visibleTank.racks) {
      for (const slot of r.slots) {
        boxes += slot.boxes.length;
        for (const box of slot.boxes) {
          cells += box.bagCells.length;
          occupied += box.bagCells.filter((c) => c.isOccupied).length;
        }
      }
    }
    return { boxes, occupied, cells };
  }, [visibleTank]);

  const RACKS_PER_PAGE = 20;
  const pagedRacks = useMemo(() => {
    const racks = visibleTank?.racks ?? [];
    const start = rackPage * RACKS_PER_PAGE;
    return racks.slice(start, start + RACKS_PER_PAGE);
  }, [visibleTank, rackPage]);

  useEffect(() => {
    setRackPage(0);
  }, [activeTank, searchText]);

  useEffect(() => {
    setHoverHint(null);
  }, [activeTank, rackPage, searchText, pickup, drawerCell]);

  const showHoverHint = (cell: CryoBagCellDto, rect: DOMRect) => {
    const hintWidth = 380;
    const hintHeight = 260;
    const gap = 10;
    const margin = 8;

    let left = rect.right + gap;
    if (left + hintWidth > window.innerWidth - margin) {
      left = rect.left - hintWidth - gap;
    }
    left = Math.max(margin, Math.min(left, window.innerWidth - hintWidth - margin));

    let top = rect.top + rect.height / 2 - hintHeight / 2;
    top = Math.max(margin, Math.min(top, window.innerHeight - hintHeight - margin));

    setHoverHint({
      cell,
      meta: cellMetaById.get(cell.id),
      left,
      top,
    });
  };

  const reservedBags: Bag[] = useMemo(
    () =>
      (bagsQ.data?.items ?? []).filter(
        (b) => b.purpose === "Cryo" && b.status === "Reserved" && !b.bagCellId,
      ),
    [bagsQ.data],
  );

  const showPagination = !!visibleTank && visibleTank.racks.length > RACKS_PER_PAGE;

  return (
    <div className="h-[calc(100vh-8.75rem)] flex flex-col gap-4 overflow-hidden">
      <div className="grid grid-cols-12 gap-4 min-h-0 flex-1">
        {/* Tank list */}
        <div className="col-span-12 lg:col-span-3 min-h-0 flex flex-col gap-2">
          <div className="flex items-center justify-between px-1">
            <h1 className="text-2xl font-semibold tracking-tight">Cryo Grid</h1>
            <Button
              variant="soft"
              size="sm"
              onClick={() => navigate("/cryo-setup")}
              icon={<Plus className="size-3.5" />}
              className="text-xs"
            >
              Yeni Tank
            </Button>
          </div>
          <Card className="p-3 min-h-0 flex-1 overflow-auto">
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
        </div>

        {/* Rack tabs + boxes */}
        <Card className="col-span-12 lg:col-span-9 flex flex-col min-h-0 overflow-hidden">
          <CardHeader
            title={tank?.name ?? "Seçili tank yok"}
            subtitle={
              visibleTank
                ? `${visibleTank.racks.length} rack · ${visibleStats.boxes} kutu · ${visibleStats.occupied}/${visibleStats.cells} hucre dolu`
                : undefined
            }
            right={<Badge tone="brand" dot>Live</Badge>}
          />

          {showPagination && (
            <div className="mb-3">
              <Pagination
                page={rackPage}
                totalPages={Math.ceil(visibleTank.racks.length / RACKS_PER_PAGE)}
                totalItems={visibleTank.racks.length}
                pageSize={RACKS_PER_PAGE}
                onPageChange={setRackPage}
                middle={
                  <div className="relative w-full max-w-sm md:flex-1 md:max-w-md">
                    <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 size-3.5 text-ink-dim" />
                    <input
                      className="input h-9 pl-8 text-xs"
                      placeholder="Hasta veya donor adindan hucre ara..."
                      value={search}
                      onChange={(e) => setSearch(e.target.value)}
                    />
                  </div>
                }
              />
            </div>
          )}

          {!showPagination && (
            <div className="mb-2 px-0">
            <div className="relative max-w-sm">
              <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 size-3.5 text-ink-dim" />
              <input
                className="input h-9 pl-8 text-xs"
                placeholder="Hasta veya donor adindan hucre ara..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>
            {searchText && (
              <p className="mt-1.5 text-xs text-ink-dim">
                {matchedCellIds && matchedCellIds.size > 0
                  ? `${matchedCellIds.size} hucre eslesti. Sadece eslesen hucreler gosteriliyor.`
                  : "Eslesen hucre bulunamadi."}
              </p>
            )}
            </div>
          )}

          {showPagination && searchText && (
            <p className="mb-2 text-xs text-ink-dim">
              {matchedCellIds && matchedCellIds.size > 0
                ? `${matchedCellIds.size} hucre eslesti. Sadece eslesen hucreler gosteriliyor.`
                : "Eslesen hucre bulunamadi."}
            </p>
          )}

          {!grid.isLoading && visibleTank && visibleTank.racks.length === 0 && (
            <p className="text-sm text-ink-muted">
              {searchText ? "Arama ile eslesen hucre bulunamadi." : "Bu tankta rack yok."}
            </p>
          )}

          {!grid.isLoading &&
            visibleTank &&
            visibleTank.racks.length > 0 &&
            visibleStats.boxes === 0 && (
              <p className="text-sm text-ink-muted">
                Bu tanktaki rack&apos;lerde henüz kutu yok; önce envanterden raf slotu ve kutu ekleyin.
              </p>
            )}

          {visibleTank && visibleStats.boxes > 0 && (
            <div className="min-h-0 flex-1 overflow-y-auto overflow-x-visible pr-1 pb-2 mt-1 grid gap-1.5 grid-cols-2 sm:grid-cols-4 md:grid-cols-6 lg:grid-cols-8 xl:grid-cols-10">
              {pagedRacks.map((rack) => {
                return (
                  <section key={rack.id} className="space-y-1.5 min-w-0">
                    <div className="sticky top-0 z-10 -mx-1 px-1 py-1 bg-bg-card/95 backdrop-blur-sm border-b border-line/60 flex flex-wrap items-center justify-between gap-1">
                      <div className="flex items-center gap-1 text-[11px] font-semibold text-ink">
                        <Layers className="size-3 text-brand-400 shrink-0" />
                        <span>{rack.name}</span>
                      </div>
                    </div>
                    {rack.slots.length === 0 ? (
                      <p className="text-xs text-ink-muted pl-1">Bu rack&apos;te raf slotu yok.</p>
                    ) : (
                      <div className="grid grid-cols-1 gap-1.5">
                        {rack.slots.flatMap((rackSlot) =>
                          rackSlot.boxes.map((box) => (
                            <BoxGrid
                              key={`${rack.id}-${rackSlot.id}-${box.id}`}
                              rackName={rack.name}
                              rackSlotName={rackSlot.name}
                              box={box}
                              cellMetaById={cellMetaById}
                              pickup={pickup}
                              onHoverCell={showHoverHint}
                              onHoverLeave={() => setHoverHint(null)}
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

      {hoverHint && !pickup && (
        <div
          className="pointer-events-none fixed z-[120]"
          style={{ left: hoverHint.left, top: hoverHint.top, width: 380 }}
        >
          <div className="rounded-xl border border-brand-500/30 bg-bg-card/95 p-2 text-left shadow-glow">
            <HoverHintCard cell={hoverHint.cell} meta={hoverHint.meta} divisor={divisor} />
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
  cellMetaById,
  pickup,
  onHoverCell,
  onHoverLeave,
  onCell,
  onDropOnCell,
  onPickUpStored,
}: {
  rackName: string;
  rackSlotName: string;
  box: CryoBoxDto;
  cellMetaById: Map<string, CellMeta>;
  pickup: PickUp;
  onHoverCell: (s: CryoBagCellDto, rect: DOMRect) => void;
  onHoverLeave: () => void;
  onCell: (s: CryoBagCellDto) => void;
  onDropOnCell: (s: CryoBagCellDto, p: PickUp) => void;
  onPickUpStored: (s: CryoBagCellDto) => void;
}) {
  const orderedCells = useMemo(() => {
    const parse = (pos: string) => {
      const letter = pos.replace(/[0-9]/g, "");
      const number = Number(pos.replace(/[A-Za-z]/g, ""));
      return { letter, number };
    };
    return [...box.bagCells].sort((a, b) => {
      const pa = parse(a.position);
      const pb = parse(b.position);
      if (pa.letter !== pb.letter) return pa.letter.localeCompare(pb.letter);
      return pa.number - pb.number;
    });
  }, [box.bagCells]);

  const bagColumnCount = Math.floor(box.bagCells.length / 10) + 1;
  const bagRowCount = Math.min(10, Math.max(box.bagCells.length, 1));

  const occupied = box.bagCells.filter((s) => s.isOccupied).length;

  return (
    <div className="rounded-lg border border-line/60 bg-bg-elevated/30 p-1.5 w-fit">
      <div className="mb-1">
        <div className="text-[11px] font-medium leading-tight truncate" title={`${rackName} · ${rackSlotName}`}>
            {rackName} · {rackSlotName}
        </div>
        <div className="flex items-center gap-1 mt-0.5">
          <span className="text-[10px] text-ink-dim">{box.name}</span>
          <Badge
            tone={occupied === box.bagCells.length ? "rose" : occupied === 0 ? "mint" : "amber"}
            className="text-[9px]"
          >
            {occupied}/{box.bagCells.length}
          </Badge>
        </div>
      </div>
      <div
        className="grid gap-1.5 w-fit"
        style={{
          gridAutoFlow: "column",
          gridTemplateColumns: `repeat(${bagColumnCount}, 3rem)`,
          gridTemplateRows: `repeat(${bagRowCount}, 3rem)`,
        }}
      >
        {orderedCells.map((slot) => {
            const pos = slot.position;
            const isPickupSource =
              pickup?.kind === "stored" && pickup.fromBagCellId === slot.id;
            const isValidTarget = !slot.isOccupied && !!pickup;
            const tone = slot.isOccupied
              ? "bg-emerald-500/30 border-emerald-500/70"
              : "bg-white border-slate-300 hover:bg-slate-50";
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
                onMouseEnter={(e) => {
                  if (slot.isOccupied && !pickup) {
                    onHoverCell(slot, e.currentTarget.getBoundingClientRect());
                  }
                }}
                onMouseLeave={onHoverLeave}
                className={cn(
                  "group relative size-12 rounded-md border text-[9px] font-semibold tracking-tight transition",
                  tone,
                  isValidTarget && "ring-2 ring-brand-400/70 animate-pulseGlow",
                  isPickupSource && "ring-2 ring-amber-400/70",
                  pickup && slot.isOccupied && !isPickupSource && "opacity-40",
                )}
              >
                {pos}
              </button>
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
  const sourceVolume = meta?.bag?.sourceVolumeMl != null ? `${formatNumber(meta.bag.sourceVolumeMl, 1)} ml` : "—";
  const wbc = meta?.bag?.wbc != null ? formatNumber(meta.bag.wbc, 2) : "—";
  const cd34Pct = meta?.bag?.cd34Percent != null ? formatNumber(meta.bag.cd34Percent, 2) : "—";
  const cd45Pct = meta?.bag?.cd45Percent != null ? formatNumber(meta.bag.cd45Percent, 2) : "—";
  const cd3Pct = meta?.bag?.cd3Percent != null ? formatNumber(meta.bag.cd3Percent, 2) : "—";
  const purpose = purposeLabel(cell.purpose);
  const status = cell.status ?? "—";
  const note = meta?.bag?.compositionNote?.trim();

  // Recalculate from raw bag values if cell dose is missing
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
        <Field label="Hasta" value={patientName} compact />
        <Field label="Donor" value={donorName} compact />
        <Field label="Ürün" value={`Bag #${bagNo} · Gün ${day}`} compact />
        <Field label="Durum / Amaç" value={`${status} · ${purpose}`} compact />
        <Field label="Hacim" value={volume} compact />
        <Field label="Kaynak Hacim" value={sourceVolume} compact />
        <Field label="WBC" value={wbc} compact />
        <Field label="%CD34 / %CD45" value={`${cd34Pct} / ${cd45Pct}`} compact />
        <Field label="%CD3" value={cd3Pct} compact />
        <Field label="CD34/kg" value={cd34} compact />
        <Field label="CD3/kg" value={cd3} compact />
      </div>

      {note && (
        <div className="rounded-lg border border-line/60 bg-bg-elevated/50 px-2 py-1">
          <div className="text-[10px] uppercase tracking-wide text-ink-dim">Not</div>
          <div className="text-[11px] text-ink-muted line-clamp-1">{note}</div>
        </div>
      )}
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

  const [useDialogOpen, setUseDialogOpen] = useState(false);
  const [useSubmitting, setUseSubmitting] = useState(false);

  const submitUse = async (reason: BagUseReason, note: string | null) => {
    if (!cell.bagId) return;
    setUseSubmitting(true);
    try {
      await Bags.use(cell.bagId, reason, note);
      toast.success("Torba kullanıldı, hücre boşaldı");
      qc.invalidateQueries({ queryKey: ["cryo-grid"] });
      qc.invalidateQueries({ queryKey: ["dashboard"] });
      qc.invalidateQueries({ queryKey: ["bags"] });
      qc.invalidateQueries({ queryKey: ["movements"] });
      setUseDialogOpen(false);
      onAfter();
    } finally {
      setUseSubmitting(false);
    }
  };

  if (!cell.isOccupied) {
    return (
      <div className="space-y-3">
        <p className="text-sm text-ink-muted">Bu hücre boş.</p>
        <p className="text-xs text-ink-dim">
          Soldan bir &quot;Depoya alınabilir&quot; torbayı buraya sürükleyebilir ya da tıklayarak seçip boş
          hücreye bırakabilirsiniz.
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
        <Button variant="danger" onClick={() => setUseDialogOpen(true)} loading={useSubmitting}>
          Torbayı kullan (hücre boşalt)
        </Button>
      </div>
      <p className="text-[11px] text-ink-dim">
        İpucu: Hücreyi sürükleyerek başka boş bir hücreye de doğrudan taşıyabilirsiniz.
      </p>

      <UseBagDialog
        open={useDialogOpen}
        onClose={() => setUseDialogOpen(false)}
        bagLabel={cell.bagNumber ? `Bag #${cell.bagNumber}` : undefined}
        loading={useSubmitting}
        onSubmit={submitUse}
      />
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

function Field({ label, value, compact = false }: { label: string; value: string; compact?: boolean }) {
  return (
    <div
      className={cn(
        "rounded-lg bg-bg-elevated/40 border border-line/60",
        compact ? "px-2 py-1.5" : "px-3 py-2",
      )}
    >
      <div className="text-[10px] uppercase tracking-wide text-ink-dim">{label}</div>
      <div className={cn("font-medium mt-0.5", compact ? "text-xs leading-tight" : "text-sm")}>{value}</div>
    </div>
  );
}
