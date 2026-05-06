using Domain.Enums;
using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;
public class CollectionSession : Entity<Guid>
{
    public Guid PatientId { get; set; }

    public int Day { get; set; }
    public DateTime Date { get; set; }

    // PK (işlem öncesi) hemogram
    public double? WbcPre { get; set; }
    public double? Hgb { get; set; }
    public double? Hct { get; set; }
    public double? Plt { get; set; }

    // PK (işlem öncesi) hasta kanından ölçülen flow cytometry değerleri
    public double? PreCd45Percent { get; set; }
    public double? PreCd34Percent { get; set; }
    public double? PreMhs { get; set; }

    // İşlem sonrası hemogram (opsiyonel — formda "İşlem Sonrası" el yazısı)
    public double? WbcPost { get; set; }
    public double? HgbPost { get; set; }
    public double? HctPost { get; set; }
    public double? PltPost { get; set; }

    // Ürün
    public double VolumeMl { get; set; }
    public double WBC { get; set; }
    public double Cd34Percent { get; set; }
    public double Cd45Percent { get; set; }
    public double Cd3Percent { get; set; }
    public double? LymphocytePercent { get; set; }
    public double? Mhs { get; set; }

    // Hesap sonucu
    public double Cd34PerKg { get; set; }
    public double Cd3PerKg { get; set; }

    // Navigation
    public virtual Patient Patient { get; set; }
    public virtual ICollection<Bag> Bags { get; set; }
    public virtual Product Product { get; set; }

    public CollectionSession()
    {
        Bags = new HashSet<Bag>();
    }

    public void Calculate(double weight, double sessionCd34Cd3Divisor)
    {
        if (weight <= 0 || sessionCd34Cd3Divisor <= 0)
            return;
        double total = VolumeMl * WBC;

        Cd34PerKg = Math.Floor((total * (Cd45Percent ) * (Cd34Percent )) / sessionCd34Cd3Divisor / weight);
        Cd3PerKg = Math.Floor((total * (Cd3Percent )) / sessionCd34Cd3Divisor / weight);
    }

    public double GetTotalBagCd34()
        => Bags?.Sum(x => x.Cd34PerKg) ?? 0;

    public bool IsValid()
        => VolumeMl > 0 && WBC > 0;
}