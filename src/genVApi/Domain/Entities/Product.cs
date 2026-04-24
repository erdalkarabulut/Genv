using Domain.ValueObjects;
using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;
public class Product : Entity<Guid>
{
    public Guid SessionId { get; set; }

    public double TotalVolume { get; set; }
    public double TotalWbc { get; set; }
    public double Cd34Percent { get; set; }
    public double Cd45Percent { get; set; }
    public double Cd3Percent { get; set; }

    public double TotalCd34PerKg { get; set; }

    public virtual CollectionSession Session { get; set; }

    public bool IsSufficient(ClinicalThresholds t)
        => TotalCd34PerKg >= t.ProductMinimumCd34PerKg;
}