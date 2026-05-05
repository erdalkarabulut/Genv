using Domain.Enums;
using Domain.ValueObjects;
using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;

public class Patient : Entity<Guid>
{
    public string FullName { get; set; } = default!;
    public double WeightKg { get; set; }
    public string? BloodGroup { get; set; }
    public TransplantType TransplantType { get; set; }

    public string? Diagnosis { get; set; }
    public string? ProtocolNo { get; set; }
    public string? IdentityNumber { get; set; }
    public DateTime? BirthDate { get; set; }

    public Guid? DonorId { get; set; }

    // Navigation
    public virtual Donor? Donor { get; set; }
    public virtual ICollection<CollectionSession> Sessions { get; set; }
    public virtual ICollection<DliProduct> DliProducts { get; set; }

    public Patient()
    {
        Sessions = new HashSet<CollectionSession>();
        DliProducts = new HashSet<DliProduct>();
    }

    public double GetTotalCd34() => Sessions?.Sum(x => x.Cd34PerKg) ?? 0;
    public double GetTotalCd3() => Sessions?.Sum(x => x.Cd3PerKg) ?? 0;

    /// <summary>
    /// Aferez başına izin verilen maksimum gün sayısı.
    /// Otolog (hasta kendi)  -> 4 gün
    /// Allogeneik (donor)    -> 2 ardışık gün
    /// </summary>
    public int GetMaxCollectionDays(ClinicalThresholds t)
        => IsAutologous() ? t.MaxApheresisDaysAutologous : t.MaxApheresisDaysAllogeneic;

    /// <summary>
    /// Hedef kümülatif CD34/kg değeri (klinik minimum).
    /// </summary>
    public double GetTargetCd34(ClinicalThresholds t)
        => IsAutologous() ? t.TargetCd34AutologousPerKg : t.TargetCd34AllogeneicPerKg;

    /// <summary>
    /// Klinik ideal kümülatif CD34/kg değeri (optimal kabul eşiği).
    /// </summary>
    public double GetIdealCd34(ClinicalThresholds t)
        => IsAutologous() ? t.IdealCd34AutologousPerKg : t.IdealCd34AllogeneicPerKg;

    public bool IsSufficient(ClinicalThresholds t) => GetTotalCd34() >= GetTargetCd34(t);

    public int GetCurrentDay() => Sessions?.Count ?? 0;

    public int GetAge()
    {
        if (BirthDate == null) return 0;
        return DateTime.Now.Year - BirthDate.Value.Year;
    }

    /// <summary>
    /// Bir sonraki aferez günü gerekli mi?
    /// </summary>
    /// <param name="currentDay">Tamamlanan en son aferez günü (1..N)</param>
    public bool ShouldContinueCollection(int currentDay, ClinicalThresholds t)
    {
        if (IsSufficient(t))
            return false;
        if (currentDay >= GetMaxCollectionDays(t))
            return false;
        return true;
    }

    public bool IsAutologous() => TransplantType == TransplantType.Autologous;
    public bool IsAllogeneic() => TransplantType == TransplantType.Allogeneic;

    public string GetStatus(ClinicalThresholds t)
    {
        double total = GetTotalCd34();
        double min = GetTargetCd34(t);
        double ideal = GetIdealCd34(t);

        if (total < min)
            return "Yetersiz";
        if (total < ideal)
            return "Sınırda";
        return "Yeterli";
    }

    public bool IsValid()
        => !string.IsNullOrWhiteSpace(FullName) && WeightKg > 0;
}