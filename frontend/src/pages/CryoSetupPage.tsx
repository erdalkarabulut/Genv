import { useState, useMemo } from "react";
import { useMutation, useQueryClient, useQuery } from "@tanstack/react-query";
import { Tanks, Dashboard } from "@/lib/api";
import { Card, CardHeader } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { Layers, Snowflake, PackageOpen, Grid3X3, Plus, ArrowLeft } from "lucide-react";
import { toast } from "sonner";
import { useNavigate } from "react-router-dom";
import { cn } from "@/lib/utils";
import type { CryoTankDto } from "@/lib/types";

export default function CryoSetupPage() {
  const qc = useQueryClient();
  const navigate = useNavigate();

  const gridQ = useQuery({ queryKey: ["cryo-grid"], queryFn: Dashboard.cryoGrid });

  const [mode, setMode] = useState<"new" | "existing">("new");
  const [selectedTankId, setSelectedTankId] = useState<string>("");
  const [tankName, setTankName] = useState("");
  const [rackCount, setRackCount] = useState(10);
  const [slotsPerRack, setSlotsPerRack] = useState(1);
  const [boxesPerSlot, setBoxesPerSlot] = useState(1);
  const [cellsPerBox, setCellsPerBox] = useState(4);

  // Prefix fields
  const [rackPrefix, setRackPrefix] = useState("R");
  const [slotPrefix, setSlotPrefix] = useState("S");
  const [boxPrefix, setBoxPrefix] = useState("B");
  const [cellPrefix, setCellPrefix] = useState("A");

  const totalCells = useMemo(
    () => rackCount * slotsPerRack * boxesPerSlot * cellsPerBox,
    [rackCount, slotsPerRack, boxesPerSlot, cellsPerBox]
  );

  const totalBoxes = useMemo(
    () => rackCount * slotsPerRack * boxesPerSlot,
    [rackCount, slotsPerRack, boxesPerSlot]
  );

  const totalSlots = useMemo(
    () => rackCount * slotsPerRack,
    [rackCount, slotsPerRack]
  );

  const selectedTank: CryoTankDto | undefined = useMemo(() => {
    return gridQ.data?.tanks?.find((t) => t.id === selectedTankId);
  }, [gridQ.data, selectedTankId]);

  const createMut = useMutation({
    mutationFn: () =>
      Tanks.bulkCreate({
        existingTankId: mode === "existing" && selectedTankId ? selectedTankId : undefined,
        tankName: mode === "new" ? tankName : selectedTank?.name ?? "",
        rackCount,
        slotsPerRack,
        boxesPerSlot,
        cellsPerBox,
        rackPrefix,
        slotPrefix,
        boxPrefix,
        cellPrefix,
      }),
    onSuccess: (data) => {
      toast.success(
        <div>
          <div className="font-semibold">{data.tankName} güncellendi!</div>
          <div className="text-xs opacity-80">
            {data.totalRacks} rack · {data.totalSlots} slot · {data.totalBoxes} kutu ·{" "}
            {data.totalCells} hücre eklendi
          </div>
        </div>
      );
      qc.invalidateQueries({ queryKey: ["cryo-grid"] });
      navigate("/cryo");
    },
    onError: (err: any) => {
      toast.error(err?.message || "Tank oluşturulurken hata oluştu.");
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (mode === "new" && !tankName.trim()) {
      toast.error("Lütfen bir tank ismi girin.");
      return;
    }
    if (mode === "existing" && !selectedTankId) {
      toast.error("Lütfen bir tank seçin.");
      return;
    }
    if (rackCount < 1 || slotsPerRack < 1 || boxesPerSlot < 1 || cellsPerBox < 1) {
      toast.error("Tüm adet alanları en az 1 olmalıdır.");
      return;
    }
    createMut.mutate();
  };

  const inputClass =
    "input w-full text-sm [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none";

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div className="flex items-center gap-3">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => navigate("/cryo")}
          className="text-ink-dim hover:text-ink"
        >
          <ArrowLeft className="size-4 mr-1" />
          Cryo Grid
        </Button>
      </div>

      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Toplu Tank Oluştur</h1>
        <p className="text-sm text-ink-dim mt-1">
          Tank, rack, raf slotu, kutu ve hücre yapısını tek seferde oluştur veya mevcut tanka ekle.
        </p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Tank Seçimi */}
        <Card>
          <CardHeader title="Tank Seçimi" subtitle="Yeni tank oluştur veya mevcut tanka ekle" />
          <div className="px-4 pb-4 space-y-4">
            <div className="flex gap-3">
              <button
                type="button"
                onClick={() => setMode("new")}
                className={cn(
                  "flex-1 rounded-lg border px-4 py-3 text-sm font-medium transition",
                  mode === "new"
                    ? "border-brand-500/40 bg-brand-500/10 text-brand-400"
                    : "border-line/60 bg-bg-elevated/30 text-ink-dim hover:border-brand-500/20"
                )}
              >
                Yeni Tank Oluştur
              </button>
              <button
                type="button"
                onClick={() => setMode("existing")}
                className={cn(
                  "flex-1 rounded-lg border px-4 py-3 text-sm font-medium transition",
                  mode === "existing"
                    ? "border-brand-500/40 bg-brand-500/10 text-brand-400"
                    : "border-line/60 bg-bg-elevated/30 text-ink-dim hover:border-brand-500/20"
                )}
              >
                Mevcut Tanka Ekle
              </button>
            </div>

            {mode === "new" ? (
              <div>
                <label className="block text-xs font-medium text-ink-dim mb-1.5">
                  Tank İsmi <span className="text-rose-500">*</span>
                </label>
                <input
                  type="text"
                  className={inputClass}
                  placeholder="Örn: Tank-XL-200"
                  value={tankName}
                  onChange={(e) => setTankName(e.target.value)}
                />
              </div>
            ) : (
              <div>
                <label className="block text-xs font-medium text-ink-dim mb-1.5">
                  Tank Seçin <span className="text-rose-500">*</span>
                </label>
                <select
                  className={inputClass}
                  value={selectedTankId}
                  onChange={(e) => setSelectedTankId(e.target.value)}
                >
                  <option value="">— Tank seçin —</option>
                  {gridQ.data?.tanks?.map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.name} ({t.racks.length} rack)
                    </option>
                  ))}
                </select>
                {selectedTank && (
                  <p className="mt-1.5 text-xs text-ink-dim">
                    Mevcut: {selectedTank.racks.length} rack ·{" "}
                    {selectedTank.racks.reduce(
                      (a, r) =>
                        a +
                        r.slots.reduce(
                          (b, s) =>
                            b +
                            s.boxes.reduce((c, box) => c + box.bagCells.length, 0),
                          0
                        ),
                      0
                    )}{" "}
                    hücre
                  </p>
                )}
              </div>
            )}
          </div>
        </Card>

        {/* Prefix Ayarları */}
        <Card>
          <CardHeader
            title="İsimlendirme"
            subtitle="Rack, slot, kutu ve hücre isimleri için önek belirleyin"
          />
          <div className="px-4 pb-4 space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-medium text-ink-dim mb-1.5">
                  <div className="flex items-center gap-1.5">
                    <Layers className="size-3.5 text-brand-400" />
                    Rack Önek
                  </div>
                </label>
                <input
                  type="text"
                  className={inputClass}
                  placeholder="R, Raf, Shelf..."
                  value={rackPrefix}
                  onChange={(e) => setRackPrefix(e.target.value)}
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-ink-dim mb-1.5">
                  <div className="flex items-center gap-1.5">
                    <Grid3X3 className="size-3.5 text-brand-400" />
                    Slot Önek
                  </div>
                </label>
                <input
                  type="text"
                  className={inputClass}
                  placeholder="S, Slot, Raf..."
                  value={slotPrefix}
                  onChange={(e) => setSlotPrefix(e.target.value)}
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-ink-dim mb-1.5">
                  <div className="flex items-center gap-1.5">
                    <PackageOpen className="size-3.5 text-brand-400" />
                    Kutu Önek
                  </div>
                </label>
                <input
                  type="text"
                  className={inputClass}
                  placeholder="B, Kutu, Box..."
                  value={boxPrefix}
                  onChange={(e) => setBoxPrefix(e.target.value)}
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-ink-dim mb-1.5">
                  <div className="flex items-center gap-1.5">
                    <Snowflake className="size-3.5 text-accent-sky" />
                    Hücre Önek (sayı için boş bırakın)
                  </div>
                </label>
                <input
                  type="text"
                  className={inputClass}
                  placeholder="A, Hücre, Cell... (boş = sayı)"
                  value={cellPrefix}
                  onChange={(e) => setCellPrefix(e.target.value)}
                />
              </div>
            </div>
          </div>
        </Card>

        {/* Yapı Parametreleri */}
        <Card>
          <CardHeader
            title="Yapı Parametreleri"
            subtitle="Her seviye için adet belirleyin"
          />
          <div className="px-4 pb-4 space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-medium text-ink-dim mb-1.5">
                  <div className="flex items-center gap-1.5">
                    <Layers className="size-3.5 text-brand-400" />
                    Rack Sayısı
                  </div>
                </label>
                <input
                  type="number"
                  min={1}
                  max={500}
                  className={inputClass}
                  value={rackCount}
                  onChange={(e) => setRackCount(Number(e.target.value))}
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-ink-dim mb-1.5">
                  <div className="flex items-center gap-1.5">
                    <Grid3X3 className="size-3.5 text-brand-400" />
                    Rack Başına Slot
                  </div>
                </label>
                <input
                  type="number"
                  min={1}
                  max={20}
                  className={inputClass}
                  value={slotsPerRack}
                  onChange={(e) => setSlotsPerRack(Number(e.target.value))}
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-ink-dim mb-1.5">
                  <div className="flex items-center gap-1.5">
                    <PackageOpen className="size-3.5 text-brand-400" />
                    Slot Başına Kutu
                  </div>
                </label>
                <input
                  type="number"
                  min={1}
                  max={50}
                  className={inputClass}
                  value={boxesPerSlot}
                  onChange={(e) => setBoxesPerSlot(Number(e.target.value))}
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-ink-dim mb-1.5">
                  <div className="flex items-center gap-1.5">
                    <Snowflake className="size-3.5 text-accent-sky" />
                    Kutu Başına Hücre
                  </div>
                </label>
                <input
                  type="number"
                  min={1}
                  max={100}
                  className={inputClass}
                  value={cellsPerBox}
                  onChange={(e) => setCellsPerBox(Number(e.target.value))}
                />
              </div>
            </div>
          </div>
        </Card>

        {/* Önizleme */}
        <Card>
          <CardHeader title="Önizleme" subtitle="Oluşturulacak yapı" />
          <div className="px-4 pb-4">
            <div className="rounded-lg border border-line/60 bg-bg-elevated/30 p-4">
              <div className="flex items-center gap-2 mb-3">
                <Snowflake className="size-4 text-accent-sky" />
                <span className="font-semibold text-sm">
                  {mode === "new" ? tankName || "—" : selectedTank?.name || "—"}
                </span>
                {mode === "existing" && selectedTank && (
                  <Badge tone="amber" className="text-[10px]">
                    + Yeni eklenecek
                  </Badge>
                )}
              </div>
              <div className="grid grid-cols-4 gap-2 text-center">
                <div className="rounded-lg border border-brand-500/20 bg-brand-500/5 p-2">
                  <div className="text-lg font-bold text-brand-400">{rackCount}</div>
                  <div className="text-[10px] text-ink-dim uppercase tracking-wide">Rack</div>
                  <div className="text-[9px] text-ink-dim mt-0.5">
                    {rackPrefix || "R"}001...
                  </div>
                </div>
                <div className="rounded-lg border border-brand-500/20 bg-brand-500/5 p-2">
                  <div className="text-lg font-bold text-brand-400">{totalSlots}</div>
                  <div className="text-[10px] text-ink-dim uppercase tracking-wide">Slot</div>
                  <div className="text-[9px] text-ink-dim mt-0.5">
                    {slotPrefix || "S"}1...
                  </div>
                </div>
                <div className="rounded-lg border border-brand-500/20 bg-brand-500/5 p-2">
                  <div className="text-lg font-bold text-brand-400">{totalBoxes}</div>
                  <div className="text-[10px] text-ink-dim uppercase tracking-wide">Kutu</div>
                  <div className="text-[9px] text-ink-dim mt-0.5">
                    {boxPrefix || "B"}1...
                  </div>
                </div>
                <div className="rounded-lg border border-brand-500/20 bg-brand-500/5 p-2">
                  <div className="text-lg font-bold text-brand-400">{totalCells.toLocaleString()}</div>
                  <div className="text-[10px] text-ink-dim uppercase tracking-wide">Hücre</div>
                  <div className="text-[9px] text-ink-dim mt-0.5">
                    {cellPrefix || ""}1...
                  </div>
                </div>
              </div>
            </div>
          </div>
        </Card>

        <div className="flex items-center justify-end gap-3">
          <Button variant="soft" type="button" onClick={() => navigate("/cryo")}>
            İptal
          </Button>
          <Button
            type="submit"
            icon={<Plus className="size-4" />}
            disabled={
              createMut.isPending ||
              (mode === "new" && !tankName.trim()) ||
              (mode === "existing" && !selectedTankId)
            }
          >
            {createMut.isPending ? "İşleniyor..." : "Yapıyı Oluştur"}
          </Button>
        </div>
      </form>
    </div>
  );
}
