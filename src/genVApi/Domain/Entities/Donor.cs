using Domain.Enums;
using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;
public class Donor : Entity<Guid>
{
    public string FullName { get; set; } = default!;
    public double WeightKg { get; set; }
    public string? BloodGroup { get; set; }

    /// <summary>Hasta ile ilişki açıklaması (ör. "Kardeş", "Anne"). Akraba-dışı donörlerde boş kalabilir.</summary>
    public string? Relation { get; set; }
    public string? IdentityNumber { get; set; }

    /// <summary>Akraba / Akraba-dışı ayrımı. Formdaki Allojenik kısmında açıkça belirtiliyor.</summary>
    public DonorType DonorType { get; set; } = DonorType.Related;

    /// <summary>Donörün doğum tarihi (formdaki D.TARİHİ alanı).</summary>
    public DateTime? BirthDate { get; set; }

    public bool IsValid()
        => !string.IsNullOrWhiteSpace(FullName);
}