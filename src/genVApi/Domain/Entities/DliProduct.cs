using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;

/// <summary>
/// Donor Lymphocyte Infusion (DLI) kaydı.
/// Allojenik aferez formundaki DLI satırına karşılık gelir:
///   Hacim (µL) × WBC (/µL) × %Lenfosit × %CD3 / 10000 = toplam × 10⁷ CD3
///   toplam / kilo = × 10⁷/kg CD3
/// </summary>
public class DliProduct : Entity<Guid>
{
    public Guid PatientId { get; set; }

    /// <summary>Bu DLI hangi aferez seansından ayrıldı (opsiyonel — bazen ayrı bir işlem olur).</summary>
    public Guid? SessionId { get; set; }

    /// <summary>DLI'nın ayrıldığı donör. Allojenik zorunludur.</summary>
    public Guid? DonorId { get; set; }

    /// <summary>DLI tarihi.</summary>
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public double VolumeMl { get; set; }
    public double? Wbc { get; set; }
    public double? LymphocytePercent { get; set; }
    public double? Cd3Percent { get; set; }

    /// <summary>Hesaplanan toplam CD3 (× 10⁶ hücre).</summary>
    public double TotalCd3 { get; set; }

    /// <summary>Hastanın kilosuna göre CD3/kg (× 10⁶).</summary>
    public double Cd3PerKg { get; set; }

    public string? Notes { get; set; }

    public virtual Patient Patient { get; set; } = default!;
    public virtual CollectionSession? Session { get; set; }
    public virtual Donor? Donor { get; set; }

    /// <summary>
    /// Formül: (Volume µL × WBC × Lenfosit% × CD3%) / 10000 = toplam
    /// Toplam / weight = /kg
    /// </summary>
    public void Calculate(double patientWeightKg, double dliCd3CalculationDivisor)
    {
        if (patientWeightKg <= 0 || dliCd3CalculationDivisor <= 0)
            return;
        double volumeUl = VolumeMl * 1000d; // ml → µL
        double total = volumeUl * (Wbc ?? 0);
        double lymph = (LymphocytePercent ?? 0) / 100d;
        double cd3 = (Cd3Percent ?? 0) / 100d;
        TotalCd3 = total * lymph * cd3 / dliCd3CalculationDivisor;
        Cd3PerKg = TotalCd3 / patientWeightKg;
    }

    public bool IsHighDose(double dliHighDoseCd3PerKgThreshold) => Cd3PerKg > dliHighDoseCd3PerKgThreshold;
}
