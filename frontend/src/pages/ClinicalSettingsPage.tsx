import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ClinicalSettingsApi } from "@/lib/api";
import type { ClinicalSettingsUpdateBody } from "@/lib/types";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { useForm } from "react-hook-form";
import { useEffect } from "react";
import { toast } from "sonner";
import { SlidersHorizontal, Save } from "lucide-react";

export default function ClinicalSettingsPage() {
  const qc = useQueryClient();
  const snap = useQuery({
    queryKey: ["clinical-settings"],
    queryFn: () => ClinicalSettingsApi.get(),
  });

  const form = useForm<ClinicalSettingsUpdateBody>({
    defaultValues: {
      sessionCd34Cd3Divisor: 10000,
      dliCd3CalculationDivisor: 10000,
      targetCd34AutologousPerKg: 2,
      targetCd34AllogeneicPerKg: 4,
      idealCd34AutologousPerKg: 4,
      idealCd34AllogeneicPerKg: 5,
      maxApheresisDaysAutologous: 4,
      maxApheresisDaysAllogeneic: 2,
      dliHighDoseCd3PerKgThreshold: 10,
      productMinimumCd34PerKg: 2,
      dashboardCd34LowThreshold: 2,
      dashboardCd34ElevatedFloor: 4,
      dashboardCd3HighRiskThreshold: 10,
      dashboardCd3LowImmuneThreshold: 2,
      dashboardCd3OptimalMin: 3,
      dashboardCd3OptimalMax: 8,
    },
  });

  useEffect(() => {
    if (!snap.data) return;
    const { id: _omit, ...rest } = snap.data;
    form.reset(rest);
  }, [snap.data, form]);

  const save = useMutation({
    mutationFn: (body: ClinicalSettingsUpdateBody) => ClinicalSettingsApi.update(body),
    onSuccess: () => {
      toast.success("Klinik eşikleri kaydedildi");
      qc.invalidateQueries({ queryKey: ["clinical-settings"] });
      qc.invalidateQueries({ queryKey: ["dashboard"] });
      qc.invalidateQueries({ queryKey: ["patients"] });
      qc.invalidateQueries({ queryKey: ["dli-products"] });
      qc.invalidateQueries({ queryKey: ["sessions"] });
    },
  });

  if (snap.isLoading && !snap.data)
    return <div className="card h-48 skeleton" />;

  return (
    <div className="space-y-6 max-w-5xl">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Klinik eşikleri</h1>
          <p className="text-sm text-ink-muted mt-1">
            CD34/CD3 hesap bölenleri ve kabul — veritabanında saklanır; hesaplar ve gösterge paneli bundan okur.
          </p>
        </div>
        <Button
          icon={<Save className="size-4" />}
          loading={save.isPending}
          onClick={form.handleSubmit((values) => save.mutate(values))}
        >
          Kaydet
        </Button>
      </header>

      <Card className="p-6 space-y-4">
        <div className="flex items-center gap-2 text-sm font-semibold">
          <SlidersHorizontal className="size-4 text-brand-400" />
          Hesaplama bölenleri
        </div>
        <div className="grid sm:grid-cols-2 gap-4">
          <Input
            label="Aferez CD34/CD3 böleni"
            type="number"
            step="any"
            {...form.register("sessionCd34Cd3Divisor", { valueAsNumber: true })}
          />
          <Input
            label="DLI CD3 toplam böleni"
            type="number"
            step="any"
            {...form.register("dliCd3CalculationDivisor", { valueAsNumber: true })}
          />
        </div>
      </Card>

      <Card className="p-6 space-y-4">
        <div className="text-sm font-semibold">Kümülatif CD34/kg — hedef ve ideal</div>
        <div className="grid sm:grid-cols-2 gap-4">
          <Input
            label="Hedef CD34/kg (otolog)"
            type="number"
            step="any"
            {...form.register("targetCd34AutologousPerKg", { valueAsNumber: true })}
          />
          <Input
            label="Hedef CD34/kg (allogeneik)"
            type="number"
            step="any"
            {...form.register("targetCd34AllogeneicPerKg", { valueAsNumber: true })}
          />
          <Input
            label="İdeal CD34/kg (otolog)"
            type="number"
            step="any"
            {...form.register("idealCd34AutologousPerKg", { valueAsNumber: true })}
          />
          <Input
            label="İdeal CD34/kg (allogeneik)"
            type="number"
            step="any"
            {...form.register("idealCd34AllogeneicPerKg", { valueAsNumber: true })}
          />
        </div>
      </Card>

      <Card className="p-6 space-y-4">
        <div className="text-sm font-semibold">Aferez gün limiti</div>
        <div className="grid sm:grid-cols-2 gap-4">
          <Input
            label="Maks. gün (otolog)"
            type="number"
            {...form.register("maxApheresisDaysAutologous", { valueAsNumber: true })}
          />
          <Input
            label="Maks. gün (allogeneik)"
            type="number"
            {...form.register("maxApheresisDaysAllogeneic", { valueAsNumber: true })}
          />
        </div>
      </Card>

      <Card className="p-6 space-y-4">
        <div className="text-sm font-semibold">DLI ve ürün</div>
        <div className="grid sm:grid-cols-2 gap-4">
          <Input
            label="DLI yüksek doz eşiği (CD3/kg ×10⁶)"
            type="number"
            step="any"
            {...form.register("dliHighDoseCd3PerKgThreshold", { valueAsNumber: true })}
          />
          <Input
            label="Ürün min. CD34/kg"
            type="number"
            step="any"
            {...form.register("productMinimumCd34PerKg", { valueAsNumber: true })}
          />
        </div>
      </Card>

      <Card className="p-6 space-y-4">
        <div className="text-sm font-semibold">Dashboard özet risk (toplam CD34/CD3)</div>
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <Input
            label="CD34 düşük (altında Yetersiz)"
            type="number"
            step="any"
            {...form.register("dashboardCd34LowThreshold", { valueAsNumber: true })}
          />
          <Input
            label="CD34 yükselmiş taban"
            type="number"
            step="any"
            {...form.register("dashboardCd34ElevatedFloor", { valueAsNumber: true })}
          />
          <Input
            label="CD3 GVHD risk üstü"
            type="number"
            step="any"
            {...form.register("dashboardCd3HighRiskThreshold", { valueAsNumber: true })}
          />
          <Input
            label="CD3 düşük bağışıklık altı"
            type="number"
            step="any"
            {...form.register("dashboardCd3LowImmuneThreshold", { valueAsNumber: true })}
          />
          <Input
            label="CD3 optimal min"
            type="number"
            step="any"
            {...form.register("dashboardCd3OptimalMin", { valueAsNumber: true })}
          />
          <Input
            label="CD3 optimal max"
            type="number"
            step="any"
            {...form.register("dashboardCd3OptimalMax", { valueAsNumber: true })}
          />
        </div>
      </Card>

      <div className="flex justify-end">
        <Button
          icon={<Save className="size-4" />}
          loading={save.isPending}
          onClick={form.handleSubmit((values) => save.mutate(values))}
        >
          Kaydet
        </Button>
      </div>
    </div>
  );
}
